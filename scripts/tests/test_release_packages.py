"""scripts/release_packages.py 的自测。只依赖标准库，不调 dotnet：名单用构造的数据，制品用临时目录里拼出来的包。

运行：python3 -m unittest discover -s scripts/tests -p 'test_release_packages.py' -v
"""
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest import mock
import xml.etree.ElementTree as ET
import zipfile


SCRIPT = Path(__file__).resolve().parents[1] / "release_packages.py"
SPEC = importlib.util.spec_from_file_location("release_packages", SCRIPT)
release = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = release  # dataclass 处理字符串注解时要在 sys.modules 里找到所在的模块
SPEC.loader.exec_module(release)

VERSION = "2026.11.30"
COMMIT = "0123456789abcdef0123456789abcdef01234567"
EXPECTED = release.expected_metadata()  # 仓库里 src/Directory.Build.props 写的作者、版权、项目地址、仓库地址
LEGACY = ["legacy"]
Package = release.Package


def project(name):
    return f"src/{name}/{name}.csproj"


def kernel(name, role="contract", symbols=True, **kw):
    return Package(id=name, tier="kernel", role=role, project=project(name), symbols=symbols, **kw)


def platform(name, role="provider", symbols=True, **kw):
    return Package(id=name, tier="platform", role=role, project=project(name), symbols=symbols, **kw)


def write(root, name, version=VERSION, deps=(), *, license="MIT", repository=EXPECTED["RepositoryUrl"], commit=COMMIT,
          authors=EXPECTED["Authors"], copyright=EXPECTED["Copyright"], project_url=EXPECTED["PackageProjectUrl"],
          description="Fixture", suffix=".nupkg", filename=None, nuspec=None, files=(), development=False, placeholder=True):
    """在 root 里拼一个包：根 nuspec、lib/ 下一个占位文件（不是 PE 文件），另加 files 里列的空文件；
    元数据默认与仓库的 src/Directory.Build.props 一致。"""
    meta = (f"<id>{name}</id><version>{version}</version><authors>{authors}</authors><description>{description}</description>"
            + (f"<copyright>{copyright}</copyright>" if copyright else "") + (f"<projectUrl>{project_url}</projectUrl>" if project_url else ""))
    if development:
        meta += "<developmentDependency>true</developmentDependency>"
    if license:
        meta += f'<license type="expression">{license}</license>'
    if repository is not None:
        meta += f'<repository type="git" url="{repository}"' + (f' commit="{commit}"' if commit else "") + " />"
    meta += ('<dependencies><group targetFramework="net10.0">'
             + "".join(f'<dependency id="{d}" version="{v}" exclude="Build,Analyzers" />' for d, v in deps) + "</group></dependencies>")
    text = nuspec or f'<?xml version="1.0" encoding="utf-8"?><package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"><metadata>{meta}</metadata></package>'
    with zipfile.ZipFile(root / (filename or f"{name}.{version}{suffix}"), "w") as archive:
        archive.writestr(f"{name}.nuspec", text)
        if placeholder:
            archive.writestr("lib/net10.0/placeholder.txt", "")
        for entry in files:
            archive.writestr(entry, "")


def full_set(root, packages):
    """每个包都出包与符号包，元数据都对、没有依赖。"""
    for p in packages:
        write(root, p.id)
        write(root, p.id, suffix=".snupkg")


class ReleaseDocumentsTests(unittest.TestCase):
    packages = [platform("Kun.Data"), kernel("Kun.Data.Native"), platform("Kun.Registry")]

    def run_command(self, command, version, packages=None):
        packages = self.packages if packages is None else packages
        with tempfile.TemporaryDirectory() as directory:
            package_list = Path(directory) / "release-packages.json"
            package_list.write_text(json.dumps([release.asdict(p) for p in packages]), encoding="utf-8")
            out, err = io.StringIO(), io.StringIO()
            with contextlib.redirect_stdout(out), contextlib.redirect_stderr(err):
                status = release.main([command, "--list", str(package_list), "--version", version])
        return status, out.getvalue(), err.getvalue()

    def test_consumer_mapping_uses_exact_ids_for_each_release_kind(self):
        for suffix, expected in [("", ["Kun.Data", "Kun.Registry"]), ("-hotfix.1", ["Kun.Data", "Kun.Registry"]),
                                 ("-beta.1", ["Kun.Data", "Kun.Data.Native", "Kun.Registry"])]:
            with self.subTest(suffix=suffix):
                status, content, err = self.run_command("nuget-config", VERSION + suffix)
                self.assertEqual(0, status, err)
                config = ET.fromstring(content)
                mappings = {node.get("key"): [p.get("pattern") for p in node.findall("package")]
                            for node in config.findall("packageSourceMapping/packageSource")}
                self.assertEqual({"kun-platform": expected, "nuget.org": ["*"]}, mappings)
                self.assertEqual("%KUN_PLATFORM_FEED_TOKEN%",
                                 config.find("packageSourceCredentials/kun-platform/add[@key='ClearTextPassword']").get("value"))

    def test_notes_preserve_unreleased_markdown_and_use_the_same_mapping(self):
        changelog = "# 历史\n\n## [Unreleased]\n\n### 租户\n\n- 支持 UUID。\n\n## [2026.10.25] - 2026-10-25\n\n- 已发布。\n"
        notes = release.release_notes(self.packages, VERSION, changelog)
        self.assertIn("### 租户\n\n- 支持 UUID。", notes)
        self.assertNotIn("已发布。", notes)
        self.assertIn("| `Kun.Data.Native` | 内核 | nuget.org |", notes)
        beta = release.release_notes(self.packages, VERSION + "-beta.1", changelog)
        self.assertIn("| `Kun.Data.Native` | 内核 | GitHub Packages |", beta)
        self.assertIn(release.nuget_config(self.packages, VERSION + "-beta.1"), beta)

    def test_first_merged_release_includes_each_documented_package_replacement(self):
        notes = release.release_notes(self.packages, VERSION, "## [Unreleased]\n\n<!-- package-migration-table -->")
        guide = (release.ROOT / "docs/guides/package-migration.md").read_text()
        rows = [line for line in guide.splitlines() if line.startswith("| `Qian.")]
        self.assertEqual(54, len(rows))
        self.assertTrue(all(row in notes for row in rows))
        self.assertNotIn("<!-- package-migration-table -->", notes)

    def test_missing_or_placeholder_notes_and_invalid_lists_fail_without_emitting_a_document(self):
        for changelog in ["", "## [Unreleased]\n\n（尚无新变更。）\n\n## [2026.10.25] - 2026-10-25\nolder\n"]:
            with self.assertRaisesRegex(release.Failure, "Unreleased"):
                release.release_notes(self.packages, VERSION, changelog)
        for packages in [[], [platform("Kun.*")], [kernel("Kun.Data"), platform("Kun.Data")]]:
            status, document, err = self.run_command("nuget-config", VERSION, packages)
            self.assertNotEqual(0, status)
            self.assertEqual("", document)
            self.assertTrue(err)

    def test_verify_feed_selects_the_release_kind_and_tier(self):
        with (tempfile.TemporaryDirectory() as directory, mock.patch.object(release, "verify_feed") as verify,
              mock.patch.dict(release.os.environ, {"GITHUB_TOKEN": "test-value"}), contextlib.redirect_stdout(io.StringIO())):
            package_list = Path(directory) / "list.json"
            package_list.write_text(json.dumps([release.asdict(p) for p in self.packages]))
            for version, feed, expected, token in [
                (VERSION, "nuget", ["Kun.Data.Native"], None),
                (VERSION + "-hotfix.2", "github", ["Kun.Data", "Kun.Registry"], "test-value"),
                (VERSION + "-beta.1", "github", [p.id for p in self.packages], "test-value"),
            ]:
                self.assertEqual(0, release.main(["verify-feed", "--feed", feed, "--version", version,
                                                   "--list", str(package_list), "--wait-seconds", "0"]))
                verify.assert_called_with(feed, expected, version, 0, token)


class ListTests(unittest.TestCase):
    def test_friend_assemblies_resolve_to_package_ids_and_require_a_reason(self):
        owner, friend = kernel("Kun.Api"), kernel("Kun.Client")
        data = {"Properties": {"AssemblyName": "Api"}, "Items": {"InternalsVisibleTo": [
            {"Identity": "Client", "KunPackageFriendReason": "组件事务管道"}, {"Identity": "Api.Tests"}]}}
        client = {"Properties": {"AssemblyName": "Client"}}
        result = release.with_friend_dependencies([(owner, data), (friend, client)])
        self.assertEqual((), result[0].friend_dependencies)
        self.assertEqual(("Kun.Api",), result[1].friend_dependencies)
        data["Items"]["InternalsVisibleTo"][0].pop("KunPackageFriendReason")
        with self.assertRaisesRegex(release.GateError, "KunPackageFriendReason"):
            release.with_friend_dependencies([(owner, data), (friend, client)])

    def test_friend_declaration_must_use_the_project_item(self):
        data = {"Properties": {}, "Items": {"AssemblyAttribute": [
            {"Identity": "System.Runtime.CompilerServices.InternalsVisibleToAttribute"}]}}
        with self.assertRaisesRegex(release.GateError, "须用 InternalsVisibleTo"):
            release.with_friend_dependencies([(kernel("Kun.Api"), data)])

    def test_checks_assembly_attributes_in_every_target_framework(self):
        api = kernel("Kun.Api", aot=True, assembly="Api", declared_friends=("Api.Tests",))
        files = {"api.nupkg": api}
        first = dict(package="api.nupkg", entry="lib/net10.0/Api.dll", assembly="Api",
                     friends=["Api.Tests"], trimmable=True, version=VERSION + ".0")
        self.assertEqual([], release.compare_assembly_metadata(files, [first], VERSION))
        prerelease = VERSION + "-beta.1"
        self.assertEqual([], release.compare_assembly_metadata(files, [first], prerelease))
        stale = dict(first, version="0.1.0.0")
        self.assertIn("AssemblyVersion", release.compare_assembly_metadata(files, [stale], prerelease)[0])
        dependency = dict(stale, assembly="Bundled.Dependency")
        self.assertEqual([], release.compare_assembly_metadata(files, [first, dependency], prerelease))
        other = dict(first, entry="lib/net9.0/Api.dll", friends=["Api.Tests", "Manual"], trimmable=False)
        problems = release.compare_assembly_metadata(files, [first, other], VERSION)
        self.assertEqual(2, len(problems))
        self.assertTrue(all("lib/net9.0/Api.dll" in p for p in problems))
        self.assertIn("缺少项目的程序集", release.compare_assembly_metadata(files, [dict(first, assembly="Other")], VERSION)[0])

    def test_reads_project_properties(self):
        data = {"Properties": {"IsPackable": "true", "PackageId": "Kun.Data", "KunTier": "platform", "KunRole": "provider",
                               "IsAotCompatible": "false", "IncludeSymbols": "true", "NoWarn": "1701;1702;NU5104"},
                "Items": {"KunTierException": [{"Identity": "Kun.X", "Rule": "K", "Until": "B7", "Reason": "r"}]}}
        package = release.package_from(release.ROOT / "src/Kun.Data/Kun.Data.csproj", data)
        self.assertEqual(Package("Kun.Data", "platform", "provider", "src/Kun.Data/Kun.Data.csproj", False, True, True, (("Kun.X", "K"),)), package)
        self.assertTrue(package.build_output)  # 没写 IncludeBuildOutput 就是带编译输出
        generator = release.package_from(release.ROOT / "src/Kun.Gen/Kun.Gen.csproj",
                                         {"Properties": {"IsPackable": "true", "IncludeBuildOutput": "False", "IncludeSymbols": "false"}})
        self.assertFalse(generator.build_output)
        self.assertFalse(generator.symbols)
        self.assertIsNone(release.package_from(release.ROOT / "tests/T/T.csproj", {"Properties": {"IsPackable": "false"}}))
        unnamed = release.package_from(release.ROOT / "src/Kun.Y/Kun.Y.csproj", {"Properties": {"IsPackable": "True", "NoWarn": "NU1903"}})
        self.assertEqual("Kun.Y", unnamed.id)
        self.assertFalse(unnamed.prerelease_ok)

    def test_declarations_duplicates_prefixes_and_tier_exceptions(self):
        packages = [kernel("Kun.Core"), Package("Kun.Core", "kernel", "contract", "src/Copy/Kun.Core.csproj"),
                    Package("Kun.Bad", "", "provider", "src/Kun.Bad/Kun.Bad.csproj"),
                    platform("Other.Thing", exceptions=(("Kun.Core", "D"),)),
                    platform("Stray.Thing"), kernel("Kun.Edge", role="provider", exceptions=(("Kun.Core", "K"),))]
        problems = release.check_list(packages).problems
        self.assertEqual(6, len(problems), problems)
        self.assertTrue(any("两个项目的包名都是" in p for p in problems))
        self.assertTrue(any("Kun.Bad" in p and "档位或角色" in p for p in problems))
        self.assertIn("Other.Thing（包名不以 Kun. 开头）", problems)
        self.assertIn("Stray.Thing（包名不以 Kun. 开头）", problems)
        # 档位例外不分档位与规则，记着就判失败
        self.assertTrue(any(p.startswith("Other.Thing → Kun.Core（规则 D）（项目文件里记着档位例外") for p in problems))
        self.assertTrue(any(p.startswith("Kun.Edge → Kun.Core（规则 K）（项目文件里记着档位例外") for p in problems))
        self.assertEqual(["解决方案里没有可打包的项目"], release.check_list([]).problems)

    def test_final_inventory_refuses_retired_package_ids_even_with_different_casing(self):
        packages = [kernel("Kun.aUdiT"), kernel("Kun.Audit.Abstractions")]
        self.assertEqual(["Kun.aUdiT 已停发，不能出现在合并版中"], release.check_list(packages).problems)

    def test_list_checks_both_expected_tier_counts_in_text_and_json(self):
        packages = [kernel("Kun.Core"), platform("Kun.Registry")]
        with mock.patch.object(release, "discover", return_value=packages):
            for flags in ([], ["--json"]):
                with self.subTest(flags=flags), contextlib.redirect_stdout(io.StringIO()) as out, contextlib.redirect_stderr(io.StringIO()) as err:
                    self.assertEqual(0, release.main(["list", *flags, "--expect", "platform=1,kernel=1"]))
                    self.assertEqual(1, release.main(["list", *flags, "--expect", "kernel=0,platform=2"]))
                    result = err.getvalue() if flags else out.getvalue()
                    self.assertIn("kernel 档有 1 个包，预期 0 个", result)
                    self.assertIn("platform 档有 1 个包，预期 2 个", result)

    def test_list_rejects_missing_duplicate_unknown_or_invalid_tier_counts(self):
        for spec in ["kernel=1", "kernel=1,kernel=2", "kernel=1,build=2", "kernel=-1,platform=2", ""]:
            with self.subTest(spec=spec), self.assertRaises(release.argparse.ArgumentTypeError):
                release.tier_counts(spec)

    def test_list_round_trips_through_json(self):
        packages = [kernel("Kun.Core", aot=True, exceptions=(("Kun.X", "K"),)),
                    kernel("Kun.Gen", role="provider", symbols=False, build_output=False)]
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "list.json"
            path.write_text(json.dumps([release.asdict(p) for p in packages]), encoding="utf-8")
            self.assertEqual(packages, release._load_list(path))
            # 这个字段加上之前导出的名单没有 build_output，按带编译输出读：照旧要符号包，不会因为少个字段就免
            path.write_text(json.dumps([{"id": "Kun.Core", "tier": "kernel", "role": "contract", "project": "src/Kun.Core/Kun.Core.csproj"}]),
                            encoding="utf-8")
            self.assertTrue(release._load_list(path)[0].build_output)

    def test_summary_counts_symbols_by_the_same_rule_as_verify(self):
        # 「要出符号包的」不算不带编译输出的包（它们免不免按制品判），名单不会说要、核对却说免。
        packages = [kernel("Kun.Core"), kernel("Kun.Gen", role="provider", symbols=False, build_output=False),
                    kernel("Kun.Gen.Declared", role="provider", build_output=False), kernel("Kun.Old", symbols=False)]
        self.assertIn("要出符号包的 1、不带编译输出的 2）", release._summary(packages))
        out = io.StringIO()
        with mock.patch.object(release, "discover", return_value=packages), contextlib.redirect_stdout(out):
            self.assertEqual(0, release.main(["list"]))
        self.assertIn("要出符号包的 1、不带编译输出的 2）", out.getvalue())

    def test_list_json_reports_problems_on_stderr(self):
        duplicated = [kernel("Kun.Core"), Package("Kun.Core", "kernel", "contract", "src/Copy/Kun.Core.csproj")]
        out, err = io.StringIO(), io.StringIO()
        with mock.patch.object(release, "discover", return_value=duplicated), contextlib.redirect_stdout(out), contextlib.redirect_stderr(err):
            code = release.main(["list", "--json"])
        self.assertEqual(1, code)
        self.assertEqual(2, len(json.loads(out.getvalue())))
        self.assertIn("两个项目的包名都是 Kun.Core", err.getvalue())


class VerifyTests(unittest.TestCase):
    packages = [kernel("Kun.Core"), kernel("Kun.Data.Native", role="provider"), platform("Kun.Data")]

    def verify(self, root, packages=None, version=VERSION):
        return release.verify(packages or self.packages, root, version, COMMIT, EXPECTED, LEGACY)

    def test_only_packaged_friends_require_an_exact_dependency(self):
        packages = [kernel("Kun.Core"), kernel("Kun.Client", friend_dependencies=("Kun.Core",))]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            for dependencies, valid in [([], False), ([("Kun.Core", VERSION)], False),
                                         ([("Kun.Core", f"[{VERSION},)")], False),
                                         ([("Kun.Core", f"[{VERSION}]")], True)]:
                with self.subTest(dependencies=dependencies):
                    write(root, "Kun.Client", deps=dependencies)
                    report = self.verify(root, packages)
                    self.assertEqual(valid, not report.problems, report.problems)

    def test_complete_lockstep_set_passes(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Data.Native", deps=[("Kun.Core", VERSION), ("Microsoft.Extensions.Options", "10.0.0")])
            write(root, "Kun.Data", deps=[("kun.core", f"[{VERSION}, )"), ("Kun.Data.Native", f"[{VERSION}]")])
            self.assertEqual([], self.verify(root).problems)

    def test_missing_extra_duplicate_and_mismatched_artifacts(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages[:2])
            write(root, "Kun.Extra")
            write(root, "Kun.Core", version="2026.11.29")
            write(root, "Kun.Core", filename="Kun.Core.copy.nupkg")
            write(root, "Kun.Orphan", suffix=".snupkg")
            write(root, "Kun.Legacy", filename=f"Kun.Legacy.{VERSION}.symbols.nupkg")
            problems = "\n".join(self.verify(root).problems)
            for expected in ("缺少名单上的包：Kun.Data", "多出名单以外的包：Kun.Extra", "版本是 2026.11.29", "不止一个包",
                             "Kun.Core.copy.nupkg 与 nuspec", "符号包 Kun.Orphan", "旧格式的符号包"):
                self.assertIn(expected, problems)

    def test_dependencies_must_be_listed_present_and_pinned_exactly(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Data.Native", deps=[("Kun.Core", "2026.11.29"), ("Kun.Core", f"(, {VERSION}]"), ("Kun.Gone", VERSION)])
            write(root, "Kun.Data", deps=[("Kun.Core", f"[{VERSION}, 2027.1.1)"), ("Kun.Data.Native", f"({VERSION}, )")])
            problems = "\n".join(self.verify(root).problems)
            self.assertIn("Kun.Data.Native 对 Kun.Core 的依赖写成 2026.11.29", problems)
            self.assertIn(f"Kun.Data.Native 对 Kun.Core 的依赖写成 (,{VERSION}]", problems)
            # 下限是 V 也不够：带上限的、不含 V 的都不认
            self.assertIn(f"Kun.Data 对 Kun.Core 的依赖写成 [{VERSION},2027.1.1)", problems)
            self.assertIn(f"Kun.Data 对 Kun.Data.Native 的依赖写成 ({VERSION},)", problems)
            self.assertIn("Kun.Gone", problems)
            self.assertIn("不在名单里", problems)

    def test_family_prefixes_come_from_the_list(self):
        packages = [kernel("Kun.Core"), platform("Other.Thing")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Other.Thing", deps=[("Other.Missing", "1.0.0"), ("Unrelated.Lib", "1.0.0")])
            problems = self.verify(root, packages).problems
            # 包名前缀一处；与名单同前缀、却不在名单里的依赖一处；别家的依赖不管
            self.assertEqual(2, len(problems), problems)
            self.assertIn("Other.Thing（包名不以 Kun. 开头）", problems)
            self.assertTrue(any("Other.Missing" in p and "不在名单里" in p for p in problems), problems)

    def test_retired_dependencies_are_rejected_even_when_none_are_listed(self):
        packages = [kernel("Kun.Core")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Kun.Core", deps=[("qIaN.Legacy", VERSION), ("Unrelated.Library", "1.0")])
            problems = self.verify(root, packages).problems
            self.assertEqual(["Kun.Core 依赖已停止使用的包 qIaN.Legacy，请引用现行包"], problems)

    def test_kernel_must_not_depend_on_platform_even_with_a_recorded_exception(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Data.Native", deps=[("Kun.Data", VERSION)])
            self.assertIn("内核档的 Kun.Data.Native 依赖平台档的 Kun.Data，项目文件里没有记档位例外", self.verify(root).problems)
            excepted = [kernel("Kun.Core"), kernel("Kun.Data.Native", role="provider", exceptions=(("Kun.Data", "K"),)), platform("Kun.Data")]
            problems = self.verify(root, excepted).problems
            self.assertTrue(any(p.startswith("内核档的 Kun.Data.Native 依赖平台档的 Kun.Data（项目文件里记着档位例外") for p in problems), problems)

    def test_symbols_license_and_commit(self):
        packages = [kernel("Kun.Core"), kernel("Kun.Lite", role="provider", symbols=False),
                    kernel("Kun.Old", role="provider", symbols=False)]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            write(root, "Kun.Core", license="Apache-2.0", commit="f" * 40)
            write(root, "Kun.Lite")
            write(root, "Kun.Old")
            strict = "\n".join(self.verify(root, packages).problems)
            self.assertIn("Kun.Core 的项目写了 IncludeSymbols=true，却没出符号包", strict)
            self.assertIn("Kun.Lite（没出符号包）", strict)
            self.assertIn("Kun.Old（没出符号包）", strict)
            self.assertIn("许可不是 MIT", strict)
            self.assertIn("不是本次检出的", strict)

    # 只带分析器的包：项目不带编译输出（IncludeBuildOutput=false），包里 lib/、runtimes/ 下也没有 dll、exe
    # （每个拼出来的包 lib/ 下都有个占位文件，不是 PE 文件，不算）。
    ANALYZER = ("analyzers/dotnet/cs/Kun.Gen.dll",)

    def test_shipped_generator_payload_and_development_dependency(self):
        generator = kernel("Kun.Plugin.Sdk.Generators", role="provider", symbols=False, build_output=False)
        analyzer = f"analyzers/dotnet/cs/{generator.id}.dll"
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            for files, development, deps, expected in [
                ([analyzer, "README.md", "package/services/metadata/core-properties/nuget.psmdcp"], True, (), []),
                ([analyzer], False, (), ["developmentDependency"]),
                ([], True, (), ["只能分发"]),
                ([analyzer, "build/extra.targets"], True, (), ["只能分发"]),
                ([analyzer], True, [("Roslyn", "4.0")], ["只能分发"]),
            ]:
                with self.subTest(files=files, development=development, deps=deps):
                    write(root, generator.id, files=files, deps=deps, development=development, placeholder=False)
                    problems = self.verify(root, [generator]).problems
                    self.assertEqual(len(expected), len(problems), problems)
                    for message, problem in zip(expected, problems):
                        self.assertIn(message, problem)

    def test_analyzer_only_package_without_symbols_passes(self):
        gen = kernel("Kun.Gen", role="provider", symbols=False, build_output=False)
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            write(root, gen.id, files=self.ANALYZER)
            self.assertEqual([], self.verify(root, [gen]).problems)

    def test_analyzer_only_package_must_not_carry_symbols(self):
        # 免的包出了符号包、或项目写了 IncludeSymbols=true，都报。
        gen = kernel("Kun.Gen", role="provider", symbols=False, build_output=False)
        declared = kernel("Kun.Gen", role="provider", symbols=True, build_output=False)
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            write(root, gen.id, files=self.ANALYZER)
            self.assertEqual(["Kun.Gen 只带分析器、免符号包，项目却写了 IncludeSymbols=true：改写 IncludeSymbols=false"],
                             self.verify(root, [declared]).problems)
            write(root, gen.id, suffix=".snupkg")
            problems = self.verify(root, [gen]).problems
            self.assertEqual(1, len(problems), problems)
            self.assertTrue(problems[0].startswith(f"Kun.Gen 只带分析器、免符号包，却出了符号包 Kun.Gen.{VERSION}.snupkg："), problems)

    def test_build_output_off_with_binaries_in_the_package_still_needs_symbols(self):
        # 只满足项目一头：IncludeBuildOutput=false，包里 lib/ 或 runtimes/ 下却有 dll、exe（不分大小写），照旧要符号包。
        for entry in ("lib/net10.0/Kun.Gen.dll", "runtimes/win-x64/native/Kun.Gen.Native.DLL", "Lib/net10.0/kun-gen.exe"):
            gen = kernel("Kun.Gen", role="provider", symbols=False, build_output=False)
            message = f"Kun.Gen（项目写了 IncludeBuildOutput=false，可包里有 {entry}，不算只带分析器的包）"
            with self.subTest(entry=entry), tempfile.TemporaryDirectory() as folder:
                root = Path(folder)
                write(root, gen.id, files=self.ANALYZER + (entry,))
                self.assertEqual([f"{message}（没出符号包）"], self.verify(root, [gen]).problems)
                write(root, gen.id, suffix=".snupkg")
                self.assertEqual([], self.verify(root, [gen]).problems)  # 出了符号包就通过

    def test_normal_package_without_symbols_is_still_reported(self):
        # 带编译输出的包没出符号包照旧判失败；包里有没有 dll 不影响（单看制品一头不算免）。
        lite = kernel("Kun.Lite", role="provider", symbols=False)
        for files in ((), ("lib/net10.0/Kun.Lite.dll",)):
            with self.subTest(files=files), tempfile.TemporaryDirectory() as folder:
                root = Path(folder)
                write(root, lite.id, files=files)
                self.assertEqual(["Kun.Lite（没出符号包）"], self.verify(root, [lite]).problems)

    def test_repository_must_be_this_repository_with_a_commit(self):
        packages = [kernel("Kun.Core"), platform("Other.Thing"), kernel("Kun.Moved", role="provider")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Other.Thing", repository="https://example.com/elsewhere.git", commit="")
            write(root, "Kun.Moved", repository="https://example.com/elsewhere.git")
            problems = self.verify(root, packages).problems
            strict = "\n".join(problems)
            self.assertIn("Other.Thing（包名不以 Kun. 开头）", problems)
            self.assertIn("Other.Thing（仓库地址不是本仓库、没带提交号）", strict)
            self.assertIn("Kun.Moved（仓库地址不是本仓库）", strict)
            self.assertNotIn("example.com", strict)

    def test_metadata_and_description_must_match_the_repository(self):
        packages = [kernel("Kun.Core"), kernel("Kun.Lib", role="provider"), kernel("Kun.Bare", role="provider"),
                    kernel("Kun.Default", role="provider"), kernel("Kun.Moved", role="provider")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Kun.Lib", authors="Someone Else", copyright="", project_url="", description="Wraps the Legacy runtime")
            write(root, "Kun.Core", description="Reads the legacy.sid cookie")  # 小写的运行期字面不算
            write(root, "Kun.Bare", description="")
            write(root, "Kun.Default", description="Package Description")
            write(root, "Kun.Moved", authors="Someone Else", description="Moved from the Legacy runtime")
            strict = "\n".join(self.verify(root, packages).problems)
            self.assertIn("Kun.Lib（作者、版权、项目地址不对）", strict)
            self.assertIn("Kun.Lib（描述里写着过渡目录的名字）", strict)
            self.assertIn("Kun.Bare（描述是空的）", strict)
            self.assertIn("Kun.Default（描述是空的）", strict)
            self.assertIn("Kun.Moved（作者不对）", strict)
            self.assertIn("Kun.Moved（描述里写着过渡目录的名字）", strict)
            self.assertNotIn("Kun.Core", strict)

    def test_expected_metadata_and_legacy_names_come_from_the_repository(self):
        self.assertTrue(EXPECTED["RepositoryUrl"].endswith(".git"))
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            props = root / "Directory.Build.props"
            props.write_text("<Project><PropertyGroup><Authors>A</Authors><Copyright>C</Copyright>"
                             "<PackageProjectUrl>https://x</PackageProjectUrl><RepositoryUrl>https://x.git</RepositoryUrl>"
                             "</PropertyGroup><PropertyGroup><Authors>Later</Authors></PropertyGroup></Project>", encoding="utf-8")
            self.assertEqual({"Authors": "A", "Copyright": "C", "PackageProjectUrl": "https://x", "RepositoryUrl": "https://x.git"},
                             release.expected_metadata(props))
            props.write_text("<Project><PropertyGroup><Authors>A</Authors></PropertyGroup></Project>", encoding="utf-8")
            with self.assertRaisesRegex(release.GateError, "Copyright"):
                release.expected_metadata(props)
            for name in ("kun", "old", "Kun"):
                (root / "imports" / name).mkdir(parents=True, exist_ok=True)
            (root / "imports" / "file.txt").write_text("", encoding="utf-8")
            self.assertEqual(["old"], release.legacy_names(root / "imports"))
            self.assertEqual([], release.legacy_names(root / "missing"))

    def test_prerelease_dependencies_of_a_stable_version(self):
        packages = [kernel("Kun.Core"), kernel("Kun.Observability", role="provider", prerelease_ok=True), platform("Kun.Gate")]
        beta = [("Upstream.Instrumentation", "[1.15.1-beta.1, )")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            for name in ("Kun.Observability", "Kun.Gate"):
                write(root, name, deps=beta)
            problems = self.verify(root, packages).problems
            self.assertEqual(1, len(problems), problems)  # 写了 NoWarn NU5104 的 Kun.Observability 放过
            self.assertTrue(problems[0].startswith("Kun.Gate → Upstream.Instrumentation [1.15.1-beta.1,)（稳定版带预发布依赖"), problems)
        with tempfile.TemporaryDirectory() as folder:  # 版本本身是预发布时不查
            root = Path(folder)
            for p in packages:
                write(root, p.id, version=f"{VERSION}-beta.1", deps=beta if p.id != "Kun.Core" else ())
                write(root, p.id, version=f"{VERSION}-beta.1", suffix=".snupkg")
            self.assertEqual([], self.verify(root, packages, version=f"{VERSION}-beta.1").problems)

    def test_unsafe_or_broken_nuspec_is_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Core", nuspec='<?xml version="1.0"?><!DOCTYPE x [<!ENTITY e "x">]><package><metadata><id>&e;</id></metadata></package>')
            (root / f"Kun.Data.{VERSION}.nupkg").write_bytes(b"not a zip")
            problems = "\n".join(self.verify(root).problems)
            self.assertIn("DTD", problems)
            self.assertIn("读不出合法的 nuspec", problems)

    def test_lower_bound(self):
        for text, bound in (("1.0.0", "1.0.0"), ("[1.0.0, )", "1.0.0"), ("[1.0.0]", "1.0.0"), ("[1.0.0,2.0.0)", "1.0.0"), ("(,1.0.0]", "")):
            with self.subTest(text=text):
                self.assertEqual(bound, release.lower_bound(text))


class CommandLineTests(unittest.TestCase):
    def run_cli(self, *args):
        stdout, stderr = io.StringIO(), io.StringIO()
        # 这些占位包只用于测命令的状态码；程序集元数据另有逐框架的对照用例。
        with contextlib.redirect_stdout(stdout), contextlib.redirect_stderr(stderr), \
                mock.patch.object(release, "check_assembly_metadata", return_value=[]):
            code = release.main(list(args))
        return subprocess.CompletedProcess(args, code, stdout.getvalue(), stderr.getvalue())

    def test_verify_packages_exit_codes(self):
        packages = [kernel("Kun.Core"), platform("Other.Thing", symbols=False)]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            listing = root / "list.json"
            listing.write_text(json.dumps([release.asdict(p) for p in packages[:1]]), encoding="utf-8")
            out = root / "out"
            out.mkdir()
            write(out, "Kun.Core")
            write(out, "Kun.Core", suffix=".snupkg")
            common = ["verify-packages", "--version", VERSION, "--packages", str(out), "--list", str(listing), "--commit", COMMIT]
            passing = self.run_cli(*common)
            self.assertEqual(0, passing.returncode, passing.stdout + passing.stderr)
            self.assertIn("制品核对通过", passing.stdout)
            listing.write_text(json.dumps([release.asdict(p) for p in packages]), encoding="utf-8")
            write(out, "Other.Thing")
            failing = self.run_cli(*common)
            self.assertEqual(1, failing.returncode, failing.stdout + failing.stderr)
            self.assertIn("Other.Thing（包名不以 Kun. 开头）", failing.stdout)
            self.assertIn("Other.Thing（没出符号包）", failing.stdout)
            bad_version = self.run_cli("verify-packages", "--version", "2026.11.31", "--packages", str(out), "--list", str(listing))
            self.assertEqual(2, bad_version.returncode)
            missing_dir = self.run_cli(*[a if a != str(out) else str(root / "nope") for a in common])
            self.assertEqual(2, missing_dir.returncode)


if __name__ == "__main__":
    unittest.main()
