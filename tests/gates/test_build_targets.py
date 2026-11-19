"""仓库根 Directory.Build.targets 里的公共 API 门禁、AOT 守卫与清单、禁用符号与平台分析器的作用域的自测（S1-KUN-020）。

运行（仓库根目录）：python3 -m unittest discover -s tests/gates -v
- AOT 清单核对逻辑（scripts/aot_projects.py 的 check）与冒烟宿主压警告的判定用合成数据验证，不需要 dotnet；
- AOT 守卫用 tests/gates/aot 下的样例项目，只跑守卫目标，不编译；
- 冒烟宿主不得压掉或放过裁剪、AOT、单文件警告：读它求值后的属性，反例是 tests/gates/aot 下的 Gate.AotHostMuted；
- 其余读 Kun.slnx 里每个项目求值后的属性与项（dotnet msbuild -getProperty / -getItem，不需要还原）。
  包换了档位、宿主漏写 KunHost、冒烟宿主没跟上清单时这里失败。
要 dotnet 的几组在 CI 上缺 dotnet 直接失败而不是跳过。
"""
from __future__ import annotations

import json
import os
import re
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
HOSTS = {"Qian.Platform.Host", "Qian.Gateway.Host"}
# ILC 按冒烟宿主的这些属性与 IlcArg 项决定报哪些警告、警告算不算错误（ILCompiler 包的 Microsoft.NETCore.Native.targets）。
SMOKE_PROPERTIES = ("PublishAot", "NoWarn", "WarningsNotAsErrors", "SuppressTrimAnalysisWarnings", "SuppressAotAnalysisWarnings",
                    "TreatWarningsAsErrors", "IlcTreatWarningsAsErrors", "TrimmerSingleWarn",
                    "EnableTrimAnalyzer", "EnableAotAnalyzer", "EnableSingleFileAnalyzer")
# SDK 自己加进 NoWarn 的：ILLink 包的 Microsoft.NET.ILLink.targets 对 PublishTrimmed 的项目压掉 IL2121（多余的
# UnconditionalSuppressMessage），不是分析结果。那个文件还原之后才导入，没还原时 NoWarn 里没有它。
SDK_IL_NOWARN = {"IL2121"}
ILC_MUTING_SWITCHES = ("--nowarn", "--nowarnaserr", "--notrimwarn", "--noaotwarn", "--singlewarn", "--singlewarnassembly")


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


def msbuild_targets(project: Path, targets: str, *args: str, env: dict | None = None):
    result = subprocess.run(["dotnet", "msbuild", str(project), f"-t:{targets}", "-nologo", *args],
                            capture_output=True, text=True, env={**check_deps._env(), **(env or {})})
    return result.returncode, result.stdout + result.stderr


def il_codes(value: str) -> list:
    """属性值里的 IL 警告号。ILC 按分号、逗号与空白切分，只认 IL 开头的号；这里不分大小写，宁可多报。"""
    return sorted({code.upper() for code in re.split(r"[;,\s]+", value) if re.fullmatch(r"(?i)IL\d+", code)})


def ilc_argument(item: str) -> str:
    """IlcArg 一项是 ILC 参数文件（rsp）里的一行，ILC 把一行当一个参数，先去掉两端的空白与双引号。"""
    return item.strip().strip('"')


def ilc_switch(item: str) -> str:
    """IlcArg 项里的开关名。开关与值用冒号或等号连写（--nowarn:IL2026、--nowarn=IL2026），或分写两项（--nowarn、IL2026，
    前一项就是开关）。同一行里用空白隔开的写法 ILC 不认（整行当成输入文件，编译失败），这里也按开关算，宁可多报。"""
    return re.split(r"[:=\s]", ilc_argument(item), maxsplit=1)[0]


def smoke_host_muting(props: dict, items: dict) -> list:
    """冒烟宿主压掉或放过裁剪、AOT、单文件警告的写法，没有即空列表。

    ILC 是 AOT 清单的最后一道关：清单里的库自己写的 NoWarn 之类不传给它，KUNAOT003 也只查库有没有关掉分析器；
    冒烟宿主自己的设置却原样交给 ILC。CI 的发布命令另给了 IlcTreatWarningsAsErrors 与 TrimmerSingleWarn，
    这里照样查，本机发布用的是项目文件里的值。
    """
    problems = []
    if not is_true(props, "PublishAot"):
        problems.append("PublishAot 不是 true：不经 ILC 原生编译")
    muted = [code for code in il_codes(props.get("NoWarn", "")) if code not in SDK_IL_NOWARN]
    if muted:
        problems.append(f"NoWarn 压掉了 {'、'.join(muted)}")
    downgraded = il_codes(props.get("WarningsNotAsErrors", ""))
    if downgraded:
        problems.append(f"WarningsNotAsErrors 让 {'、'.join(downgraded)} 不算错误")
    problems += [f"写了 {name}=true" for name in ("SuppressTrimAnalysisWarnings", "SuppressAotAnalysisWarnings") if is_true(props, name)]
    # 不写时 ILC 取 TreatWarningsAsErrors（Microsoft.NETCore.Native.targets）。
    if (props.get("IlcTreatWarningsAsErrors") or props.get("TreatWarningsAsErrors", "")).lower() != "true":
        problems.append("IlcTreatWarningsAsErrors 不是 true：ILC 的警告不算错误")
    if props.get("TrimmerSingleWarn", "").lower() != "false":
        problems.append("TrimmerSingleWarn 不是 false：警告按程序集合并成一条")
    problems += [f"{name} 不是 true" for name in ("EnableTrimAnalyzer", "EnableAotAnalyzer", "EnableSingleFileAnalyzer")
                 if not is_true(props, name)]
    arguments = [i["Identity"] for i in items.get("IlcArg", [])]
    switches = sorted({a for a in arguments if ilc_switch(a) in ILC_MUTING_SWITCHES})
    if switches:
        problems.append(f"直接给 ILC 加了 {'、'.join(switches)}")
    # ILC 会展开参数文件里的 @文件，那个文件里的开关这里看不到。
    nested = sorted({a for a in arguments if ilc_argument(a).startswith("@")})
    if nested:
        problems.append(f"让 ILC 另读参数文件 {'、'.join(nested)}，里面的开关查不到")
    return problems


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


class SmokeHostMutingCheckTests(unittest.TestCase):
    """smoke_host_muting 的判定，合成数据。"""

    CLEAN = {"PublishAot": "true", "NoWarn": ";CS1591;IL2121", "WarningsNotAsErrors": ";CS1591", "TreatWarningsAsErrors": "true",
             "IlcTreatWarningsAsErrors": "", "TrimmerSingleWarn": "false",
             "EnableTrimAnalyzer": "true", "EnableAotAnalyzer": "true", "EnableSingleFileAnalyzer": "true"}

    def test_sdk_defaults_pass(self):
        # IL2121 是 SDK 自己压的；IlcTreatWarningsAsErrors 不写时 ILC 取 TreatWarningsAsErrors。
        # --nosinglewarnassembly 把某个程序集的警告展开，--warnaserr 把警告算成错误，都不放过警告。
        allowed = ("--nosinglewarnassembly:Kun.Core", "--nosinglewarnassembly=Kun.Core", "--warnaserr:IL2026")
        self.assertEqual([], smoke_host_muting(self.CLEAN, {"IlcArg": [{"Identity": a} for a in allowed]}))

    def test_switches_are_read_like_ilc_does(self):
        for item in ("--nowarn:IL2026", "--nowarn=IL2026", "--nowarn IL2026", "--nowarn", '"--nowarn=IL2026"',
                     "--singlewarnassembly:Kun.Core", "--singlewarnassembly=Kun.Core"):
            with self.subTest(item=item):
                self.assertEqual([f"直接给 ILC 加了 {item}"], smoke_host_muting(self.CLEAN, {"IlcArg": [{"Identity": item}]}))

    def test_nested_response_file_is_reported(self):
        self.assertEqual(["让 ILC 另读参数文件 @extra.rsp，里面的开关查不到"],
                         smoke_host_muting(self.CLEAN, {"IlcArg": [{"Identity": "@extra.rsp"}]}))

    def test_codes_are_split_like_ilc_does(self):
        props = dict(self.CLEAN, NoWarn="CS1591,il2026 IL3050;IL2121", WarningsNotAsErrors="IL2104")
        self.assertEqual(["NoWarn 压掉了 IL2026、IL3050", "WarningsNotAsErrors 让 IL2104 不算错误"], smoke_host_muting(props, {}))

    def test_treat_warnings_as_errors_off_is_reported(self):
        self.assertEqual(["IlcTreatWarningsAsErrors 不是 true：ILC 的警告不算错误"],
                         smoke_host_muting(dict(self.CLEAN, TreatWarningsAsErrors="false"), {}))


@NEEDS_DOTNET
class AotGuardTests(unittest.TestCase):
    """仓库根 Directory.Build.targets 的 KunAotGuard，用 tests/gates/aot 下的样例项目。"""

    def guard(self, name: str, *args: str, env: dict | None = None):
        return msbuild_targets(AOT_SAMPLES / name / f"{name}.csproj", "KunAotGuard", *args, env=env)

    def test_guard_rejects_old_switch_publish_aot_on_packages_and_muted_analyzers(self):
        for name, code in {"Gate.AotLegacy": "KUNAOT001", "Gate.AotPackagePublish": "KUNAOT002", "Gate.AotMuted": "KUNAOT003"}.items():
            with self.subTest(sample=name):
                returncode, output = self.guard(name)
                self.assertNotEqual(0, returncode, output)
                self.assertIn(f"error {code}", output)

    def test_guard_allows_compatible_libraries_hosts_and_publish_aot_from_the_command_line(self):
        # 命令行上的 PublishAot 会作为全局属性传给每个被引用的库（如 tests/Kun.Integration.AotTests 发布样例时），不能拦。
        # 同名环境变量也对每个项目生效，守卫同样放行：MSBuild 不记属性的来处，见仓库根 Directory.Build.props。
        for name, args, env in (("Gate.AotLibrary", (), None), ("Gate.AotLibrary", ("-p:PublishAot=true",), None),
                                ("Gate.AotLibrary", (), {"PublishAot": "true"}), ("Gate.AotHost", (), None)):
            with self.subTest(sample=name, args=args, env=env):
                returncode, output = self.guard(name, *args, env=env)
                self.assertEqual(0, returncode, output)
                self.assertNotIn("error KUNAOT", output)

    def test_projects_in_both_trees_import_the_root_guards(self):
        for project in ("src/Kun.Core/Kun.Core.csproj", "src/Kun.Errors/Kun.Errors.csproj"):
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
        self.assertTrue({"Qian.Gateway", "Qian.Identity.Client", "Kun.Errors"} <= names)

    def test_compatible_projects_keep_the_analyzers_on_and_nobody_writes_the_old_switch(self):
        for path, (props, _) in evaluated().items():
            with self.subTest(project=path.stem):
                self.assertEqual("", props.get("IsAotCandidate", ""))
                if is_true(props, "IsAotCompatible"):
                    for name in ("IsTrimmable", "EnableTrimAnalyzer", "EnableAotAnalyzer", "EnableSingleFileAnalyzer"):
                        self.assertTrue(is_true(props, name), name)


@NEEDS_DOTNET
class SmokeHostMutingTests(unittest.TestCase):
    """冒烟宿主 tests/Kun.AotSmoke 不压掉、不放过裁剪、AOT、单文件警告；反例是 tests/gates/aot 下的 Gate.AotHostMuted。"""

    @staticmethod
    def muting(project: Path) -> list:
        data = msbuild_evaluate(project, SMOKE_PROPERTIES, ("IlcArg",))
        return smoke_host_muting(data.get("Properties") or {}, data.get("Items") or {})

    def test_smoke_host_keeps_every_trim_aot_and_single_file_warning(self):
        self.assertEqual([], self.muting(aot_projects.SMOKE_HOST))

    def test_muted_sample_is_reported(self):
        problems = self.muting(AOT_SAMPLES / "Gate.AotHostMuted" / "Gate.AotHostMuted.csproj")
        for text in ("NoWarn 压掉了 IL2026、IL3000、IL3050", "WarningsNotAsErrors 让 IL2104 不算错误",
                     "写了 SuppressTrimAnalysisWarnings=true", "写了 SuppressAotAnalysisWarnings=true",
                     "IlcTreatWarningsAsErrors 不是 true", "TrimmerSingleWarn 不是 false", "EnableSingleFileAnalyzer 不是 true",
                     "直接给 ILC 加了 --noaotwarn"):
            self.assertTrue(any(text in p for p in problems), (text, problems))


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
    """禁用符号与平台分析器只接平台档的包、宿主与 _import/qian/ 下的全部项目。"""

    def test_scope_follows_tier_host_and_transitional_directory(self):
        banned = (ROOT / "BannedSymbols.txt").resolve()
        for path, (props, items) in evaluated().items():
            expected = props.get("KunTier") == "platform" or is_true(props, "KunHost") or TRANSITIONAL in path.parents
            with self.subTest(project=path.stem):
                self.assertEqual(expected, is_true(props, "_KunPlatformAnalyzers"))
                self.assertEqual(expected, "Microsoft.CodeAnalysis.BannedApiAnalyzers" in package_ids(items))
                self.assertEqual(expected, banned in additional_files(items))
                if path.resolve() != ANALYZERS.resolve():
                    self.assertEqual(expected, ANALYZERS.resolve() in analyzer_references(items, path))

    def test_hosts_transitional_directory_and_kun_projects(self):
        projects = evaluated()
        self.assertEqual(HOSTS, {p.stem for p, (props, _) in projects.items() if is_true(props, "KunHost")})
        # 过渡期 _import/qian/ 下的项目全部受检，内核档的包也在内：它们不定义被禁的类型，却够得着被禁的入口。
        # 有人按档位或 IsPackable 把过渡目录下的包放出去，这里列出漏接的项目。各域批次把包搬出过渡目录时再按归属收窄。
        transitional = {p: props for p, (props, _) in projects.items() if TRANSITIONAL in p.parents}
        self.assertTrue(any(is_true(props, "IsPackable") and props.get("KunTier") == "kernel" for props in transitional.values()),
                        "_import/qian/ 下已没有内核档的包：按归属重定作用域，再改这里")
        self.assertEqual(set(), {p.stem for p, props in transitional.items() if not is_true(props, "_KunPlatformAnalyzers")})
        # 仓库根上的项目：接入的正好是平台档的包与宿主，搬出过渡目录的内核档包与测试不接（D27）。
        outside = {p: props for p, (props, _) in projects.items() if TRANSITIONAL not in p.parents}
        self.assertTrue(any(props.get("KunTier") == "platform" for props in outside.values()),
                        "仓库根上已没有平台档的项目：下面这条测不到平台档那一支，改这里")
        self.assertEqual({p.stem for p, props in outside.items() if props.get("KunTier") == "platform" or is_true(props, "KunHost")},
                         {p.stem for p, props in outside.items() if is_true(props, "_KunPlatformAnalyzers")})

    def test_banned_symbols_list_lives_at_the_root(self):
        self.assertTrue((ROOT / "BannedSymbols.txt").is_file())
        self.assertFalse((TRANSITIONAL / "BannedSymbols.txt").exists())


if __name__ == "__main__":
    unittest.main()
