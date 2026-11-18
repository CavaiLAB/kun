"""仓库根 Directory.Build.targets 里的公共 API 门禁、AOT 守卫与清单、禁用符号与平台分析器的作用域的自测（S1-KUN-020）。

运行（仓库根目录）：python3 -m unittest discover -s tests/gates -v
- AOT 清单核对逻辑（scripts/aot_projects.py 的 check）用合成数据验证，不需要 dotnet；
- AOT 守卫用 tests/gates/aot 下的样例项目，只跑守卫目标，不编译；
- 其余读 Kun.slnx 里每个项目求值后的属性与项（dotnet msbuild -getProperty / -getItem，不需要还原）。
  包换了档位、宿主漏写 KunHost、冒烟宿主没跟上清单时这里失败。
要 dotnet 的几组在 CI 上缺 dotnet 直接失败而不是跳过。
"""
from __future__ import annotations

import json
import os
import shutil
import subprocess
import sys
import unittest
from concurrent.futures import ThreadPoolExecutor
from functools import lru_cache
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

import aot_projects  # noqa: E402
import check_deps  # noqa: E402

AOT_SAMPLES = ROOT / "tests" / "gates" / "aot"
TRANSITIONAL = ROOT / "_import" / "qian"
ANALYZERS = TRANSITIONAL / "tools" / "Qian.Build.Analyzers" / "Qian.Build.Analyzers.csproj"
NEEDS_DOTNET = unittest.skipUnless(shutil.which("dotnet") or os.environ.get("CI") == "true", "本机没有 dotnet")
PROPERTIES = ("IsPackable", "KunTier", "KunHost", "_KunPlatformAnalyzers", "IsAotCompatible", "IsAotCandidate", "HasPublicApi",
              "IsTrimmable", "EnableTrimAnalyzer", "EnableAotAnalyzer", "EnableSingleFileAnalyzer")
ITEMS = ("PackageReference", "ProjectReference", "AdditionalFiles")

# 去内核档的 Qian 包（合并蓝图 D23.9，B3 方案附录 A）：不接禁用符号与平台分析器（D23.14 第 4 点）。
# 它们现在零命中；被禁的类型定义在内核档的模块里。这份名单变了，说明有包换了档位，要复核它该不该受检。
KERNEL_QIAN_PACKAGES = {
    "Qian.Audit.Abstractions", "Qian.Audit.Native", "Qian.Config.Secrets", "Qian.Data.Native.Abstractions",
    "Qian.Data.Native.Postgres", "Qian.Events", "Qian.Events.Abstractions", "Qian.Gateway", "Qian.Gateway.Abstractions",
    "Qian.Plugin.Sdk", "Qian.Plugin.Sdk.Generators", "Qian.Runtime", "Qian.Runtime.Errors", "Qian.Runtime.Errors.AspNetCore",
}
HOSTS = {"Qian.Platform.Host", "Qian.Gateway.Host"}


def msbuild_evaluate(project: Path, properties=(), items=()) -> dict:
    cmd = (["dotnet", "msbuild", str(project), "-nologo"] + [f"-getProperty:{p}" for p in properties]
           + [f"-getItem:{i}" for i in items])
    result = subprocess.run(cmd, capture_output=True, text=True, env=check_deps._env())
    if result.returncode != 0:
        raise AssertionError(f"读取项目失败：{project}\n{result.stdout}{result.stderr}")
    return json.loads(result.stdout)


@lru_cache(maxsize=None)
def evaluated() -> dict:
    """Kun.slnx 里每个项目求值后的属性与项：{项目文件: (属性, 项)}。"""
    def read(path: Path):
        data = msbuild_evaluate(path, PROPERTIES, ITEMS)
        return path, (data.get("Properties") or {}, data.get("Items") or {})
    paths = check_deps.solution_projects(ROOT / "Kun.slnx")
    with ThreadPoolExecutor(max_workers=min(8, os.cpu_count() or 4)) as pool:
        return dict(pool.map(read, paths))


def is_true(props: dict, name: str) -> bool:
    return props.get(name, "").lower() == "true"


def package_ids(items: dict) -> set:
    return {i["Identity"] for i in items.get("PackageReference", [])}


def additional_files(items: dict) -> set:
    return {Path(i.get("FullPath") or i["Identity"]).resolve() for i in items.get("AdditionalFiles", [])}


def analyzer_references(items: dict, project: Path) -> set:
    return {(project.parent / i["Identity"]).resolve() for i in items.get("ProjectReference", [])
            if i.get("OutputItemType") == "Analyzer"}


def msbuild_targets(project: Path, targets: str, *args: str):
    result = subprocess.run(["dotnet", "msbuild", str(project), f"-t:{targets}", "-nologo", *args],
                            capture_output=True, text=True, env=check_deps._env())
    return result.returncode, result.stdout + result.stderr


class AotListCheckTests(unittest.TestCase):
    """scripts/aot_projects.py 的核对逻辑，合成数据。"""

    def project(self, name, compatible=True, packable=True):
        return aot_projects.Project((ROOT / "src" / name / f"{name}.csproj").resolve(), compatible, packable, name)

    def test_matching_smoke_host_passes(self):
        alpha, beta, other = self.project("Gate.Alpha"), self.project("Gate.Beta"), self.project("Gate.Other", compatible=False)
        self.assertEqual([], aot_projects.check([alpha, beta, other], [alpha.path, beta.path], ["Gate.Alpha", "Gate.Beta"]))

    def test_missing_extra_and_unpackable_entries_are_reported(self):
        alpha, beta = self.project("Gate.Alpha"), self.project("Gate.Beta", packable=False)
        other = self.project("Gate.Other", compatible=False)
        problems = aot_projects.check([alpha, beta, other], [alpha.path, other.path], ["Gate.Alpha", "Gate.Other"])
        for text in ("src/Gate.Beta/Gate.Beta.csproj 写了 IsAotCompatible=true，却不可打包",
                     "冒烟宿主没有引用清单里的 src/Gate.Beta/Gate.Beta.csproj",
                     "冒烟宿主引用了不在清单里的 src/Gate.Other/Gate.Other.csproj",
                     "冒烟宿主没有扎根程序集 Gate.Beta",
                     "冒烟宿主扎根了不在清单里的程序集 Gate.Other"):
            self.assertTrue(any(text in p for p in problems), (text, problems))
        self.assertEqual(5, len(problems), problems)


@NEEDS_DOTNET
class AotGuardTests(unittest.TestCase):
    """仓库根 Directory.Build.targets 的 KunAotGuard，用 tests/gates/aot 下的样例项目。"""

    def guard(self, name: str, *args: str):
        return msbuild_targets(AOT_SAMPLES / name / f"{name}.csproj", "KunAotGuard", *args)

    def test_guard_rejects_old_switch_publish_aot_on_packages_and_muted_analyzers(self):
        for name, code in {"Gate.AotLegacy": "KUNAOT001", "Gate.AotPackagePublish": "KUNAOT002", "Gate.AotMuted": "KUNAOT003"}.items():
            with self.subTest(sample=name):
                returncode, output = self.guard(name)
                self.assertNotEqual(0, returncode, output)
                self.assertIn(f"error {code}", output)

    def test_guard_allows_compatible_libraries_hosts_and_publish_aot_from_the_command_line(self):
        # 命令行上的 PublishAot 会作为全局属性传给每个被引用的库（如 tests/Kun.Integration.AotTests 发布样例时），不能拦。
        for name, args in (("Gate.AotLibrary", ()), ("Gate.AotLibrary", ("-p:PublishAot=true",)), ("Gate.AotHost", ())):
            with self.subTest(sample=name, args=args):
                returncode, output = self.guard(name, *args)
                self.assertEqual(0, returncode, output)
                self.assertNotIn("error KUNAOT", output)

    def test_projects_in_both_trees_import_the_root_guards(self):
        for project in ("src/Kun.Core/Kun.Core.csproj", "_import/qian/packages/Qian.Runtime.Errors/Qian.Runtime.Errors.csproj"):
            with self.subTest(project=project):
                returncode, output = msbuild_targets(ROOT / project, "KunAotGuard;KunRequirePublicApiFiles")
                self.assertEqual(0, returncode, output)


@NEEDS_DOTNET
class AotListTests(unittest.TestCase):
    """真实的 AOT 清单与冒烟宿主 tests/Kun.AotSmoke。"""

    @classmethod
    def setUpClass(cls):
        cls.projects = [aot_projects.Project(path, is_true(props, "IsAotCompatible"), is_true(props, "IsPackable"), path.stem)
                        for path, (props, _) in evaluated().items()]

    def test_smoke_host_references_and_roots_exactly_the_list(self):
        references, roots = aot_projects.smoke_host()
        self.assertEqual([], aot_projects.check(self.projects, references, roots))

    def test_list_content(self):
        names = {p.name for p in aot_projects.aot_projects(self.projects)}
        self.assertIn("Kun.Identity.AspNetCore", names)  # 旧的四份清单都漏了它（S1-KUN-015）
        self.assertNotIn("Kun.Auditing", names)  # EF 拦截器带出 IL2091；B6 并入平台档，不做 AOT（D23.9）
        self.assertTrue({"Qian.Gateway", "Qian.Identity.Client", "Qian.Runtime.Errors"} <= names)

    def test_compatible_projects_keep_the_analyzers_on_and_nobody_writes_the_old_switch(self):
        for path, (props, _) in evaluated().items():
            with self.subTest(project=path.stem):
                self.assertEqual("", props.get("IsAotCandidate", ""))
                if is_true(props, "IsAotCompatible"):
                    for name in ("IsTrimmable", "EnableTrimAnalyzer", "EnableAotAnalyzer", "EnableSingleFileAnalyzer"):
                        self.assertTrue(is_true(props, name), name)


@NEEDS_DOTNET
class PublicApiTests(unittest.TestCase):
    """可打包的项目都接公共 API 门禁，不再按 HasPublicApi 或包名判定。"""

    def test_packable_projects_have_both_files_and_the_analyzer(self):
        packable = {path: v for path, v in evaluated().items() if is_true(v[0], "IsPackable")}
        self.assertTrue(any(p.stem.startswith("Kun.") for p in packable) and any(p.stem.startswith("Qian.") for p in packable))
        for path, (props, items) in packable.items():
            with self.subTest(project=path.stem):
                self.assertTrue((path.parent / "PublicAPI.Shipped.txt").is_file())
                self.assertTrue((path.parent / "PublicAPI.Unshipped.txt").is_file())
                self.assertIn("Microsoft.CodeAnalysis.PublicApiAnalyzers", package_ids(items))

    def test_other_projects_do_not_get_it_and_nobody_writes_has_public_api(self):
        for path, (props, items) in evaluated().items():
            with self.subTest(project=path.stem):
                self.assertEqual("", props.get("HasPublicApi", ""))
                if not is_true(props, "IsPackable"):
                    self.assertNotIn("Microsoft.CodeAnalysis.PublicApiAnalyzers", package_ids(items))


@NEEDS_DOTNET
class AnalyzerScopeTests(unittest.TestCase):
    """禁用符号与平台分析器只接平台档的包、宿主与 _import/qian/ 下不打包的项目。"""

    def test_scope_follows_tier_host_and_transitional_directory(self):
        banned = (ROOT / "BannedSymbols.txt").resolve()
        for path, (props, items) in evaluated().items():
            expected = (props.get("KunTier") == "platform" or is_true(props, "KunHost")
                        or (TRANSITIONAL in path.parents and not is_true(props, "IsPackable")))
            with self.subTest(project=path.stem):
                self.assertEqual(expected, is_true(props, "_KunPlatformAnalyzers"))
                self.assertEqual(expected, "Microsoft.CodeAnalysis.BannedApiAnalyzers" in package_ids(items))
                self.assertEqual(expected, banned in additional_files(items))
                if path.resolve() != ANALYZERS.resolve():
                    self.assertEqual(expected, ANALYZERS.resolve() in analyzer_references(items, path))

    def test_hosts_kernel_packages_and_kun_projects(self):
        projects = evaluated()
        self.assertEqual(HOSTS, {p.stem for p, (props, _) in projects.items() if is_true(props, "KunHost")})
        unchecked = {p.stem for p, (props, _) in projects.items()
                     if TRANSITIONAL in p.parents and is_true(props, "IsPackable") and not is_true(props, "_KunPlatformAnalyzers")}
        self.assertEqual(KERNEL_QIAN_PACKAGES, unchecked)
        self.assertEqual(set(), {p.stem for p, (props, _) in projects.items()
                                 if TRANSITIONAL not in p.parents and is_true(props, "_KunPlatformAnalyzers")})

    def test_banned_symbols_list_lives_at_the_root(self):
        self.assertTrue((ROOT / "BannedSymbols.txt").is_file())
        self.assertFalse((TRANSITIONAL / "BannedSymbols.txt").exists())


if __name__ == "__main__":
    unittest.main()
