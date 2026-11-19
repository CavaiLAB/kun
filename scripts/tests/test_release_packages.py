"""scripts/release_packages.py 的自测。只依赖标准库，不调 dotnet：名单用构造的数据，制品用临时目录里拼出来的包。

运行：python3 -m unittest discover -s scripts/tests -p 'test_release_packages.py' -v
"""
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
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


def kernel(name, role="contract", symbols=True, **kw):
    return Package(id=name, tier="kernel", role=role, project=f"src/{name}/{name}.csproj", symbols=symbols, **kw)


def platform(name, role="provider", symbols=True, **kw):
    return Package(id=name, tier="platform", role=role, project=f"src/{name}/{name}.csproj", symbols=symbols, **kw)


def write(root, name, version=VERSION, deps=(), *, license="MIT", repository=EXPECTED["RepositoryUrl"], commit=COMMIT,
          authors=EXPECTED["Authors"], copyright=EXPECTED["Copyright"], project_url=EXPECTED["PackageProjectUrl"],
          description="Fixture", suffix=".nupkg", filename=None, nuspec=None):
    """在 root 里拼一个只有根 nuspec 的包；元数据默认与仓库的 src/Directory.Build.props 一致。"""
    meta = (f"<id>{name}</id><version>{version}</version><authors>{authors}</authors><description>{description}</description>"
            + (f"<copyright>{copyright}</copyright>" if copyright else "") + (f"<projectUrl>{project_url}</projectUrl>" if project_url else ""))
    if license:
        meta += f'<license type="expression">{license}</license>'
    if repository is not None:
        meta += f'<repository type="git" url="{repository}"' + (f' commit="{commit}"' if commit else "") + " />"
    meta += ('<dependencies><group targetFramework="net10.0">'
             + "".join(f'<dependency id="{d}" version="{v}" exclude="Build,Analyzers" />' for d, v in deps) + "</group></dependencies>")
    text = nuspec or f'<?xml version="1.0" encoding="utf-8"?><package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"><metadata>{meta}</metadata></package>'
    with zipfile.ZipFile(root / (filename or f"{name}.{version}{suffix}"), "w") as archive:
        archive.writestr(f"{name}.nuspec", text)
        archive.writestr("lib/net10.0/placeholder.txt", "")


def full_set(root, packages):
    """每个包都出包与符号包、依赖照名单里的写法。"""
    for p in packages:
        write(root, p.id)
        write(root, p.id, suffix=".snupkg")


class ListTests(unittest.TestCase):
    def test_reads_project_properties(self):
        data = {"Properties": {"IsPackable": "true", "PackageId": "Kun.Data", "KunTier": "platform", "KunRole": "provider",
                               "IsAotCompatible": "false", "IncludeSymbols": "true", "NoWarn": "1701;1702;NU5104"},
                "Items": {"KunTierException": [{"Identity": "Kun.X", "Rule": "K", "Until": "B7", "Reason": "r"}]}}
        package = release.package_from(release.ROOT / "src/Kun.Data/Kun.Data.csproj", data)
        self.assertEqual(Package("Kun.Data", "platform", "provider", "src/Kun.Data/Kun.Data.csproj", False, True, True, (("Kun.X", "K"),)), package)
        self.assertIsNone(release.package_from(release.ROOT / "tests/T/T.csproj", {"Properties": {"IsPackable": "false"}}))
        unnamed = release.package_from(release.ROOT / "src/Kun.Y/Kun.Y.csproj", {"Properties": {"IsPackable": "True", "NoWarn": "NU1903"}})
        self.assertEqual("Kun.Y", unnamed.id)
        self.assertFalse(unnamed.prerelease_ok)

    def test_declarations_duplicates_and_transitional_items(self):
        packages = [kernel("Kun.Core"), Package("Kun.Core", "kernel", "contract", "src/Copy/Kun.Core.csproj"),
                    Package("Kun.Bad", "", "provider", "src/Kun.Bad/Kun.Bad.csproj"),
                    platform("Other.Thing", exceptions=(("Kun.Core", "D"),))]
        strict = release.check_list(packages, transitional=False)
        self.assertTrue(any("两个项目的包名都是" in p for p in strict.problems))
        self.assertTrue(any("Kun.Bad" in p and "档位或角色" in p for p in strict.problems))
        self.assertTrue(any("Other.Thing" in p and "Kun. 开头" in p for p in strict.problems))
        self.assertTrue(any("Other.Thing → Kun.Core" in p for p in strict.problems))
        relaxed = release.check_list(packages, transitional=True)
        self.assertEqual(2, len(relaxed.problems), relaxed.problems)  # 重名与档位没写对两种模式都判失败
        self.assertEqual({"prefix": ["Other.Thing"], "exception": ["Other.Thing → Kun.Core（规则 D）"]}, relaxed.allowed)
        self.assertEqual(["解决方案里没有可打包的项目"], release.check_list([], transitional=True).problems)

    def test_list_round_trips_through_json(self):
        packages = [kernel("Kun.Core", aot=True, exceptions=(("Kun.X", "K"),))]
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "list.json"
            path.write_text(json.dumps([release.asdict(p) for p in packages]), encoding="utf-8")
            self.assertEqual(packages, release._load_list(path))


class VerifyTests(unittest.TestCase):
    packages = [kernel("Kun.Core"), kernel("Kun.Data.Native", role="provider"), platform("Kun.Data")]

    def verify(self, root, packages=None, transitional=False, version=VERSION):
        return release.verify(packages or self.packages, root, version, COMMIT, transitional, EXPECTED, LEGACY)

    def test_complete_lockstep_set_passes_in_strict_mode(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Data.Native", deps=[("Kun.Core", VERSION), ("Microsoft.Extensions.Options", "10.0.0")])
            write(root, "Kun.Data", deps=[("kun.core", f"[{VERSION}, )"), ("Kun.Data.Native", f"[{VERSION}]")])
            report = self.verify(root)
            self.assertEqual([], report.problems)
            self.assertEqual({}, report.allowed)

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

    def test_dependencies_must_be_listed_present_and_pinned(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Data.Native", deps=[("Kun.Core", "2026.11.29"), ("Kun.Gone", VERSION)])
            write(root, "Kun.Data", deps=[("Kun.Core", f"(, {VERSION}]")])
            problems = "\n".join(self.verify(root).problems)
            self.assertIn("Kun.Data.Native 对 Kun.Core 的依赖是 2026.11.29", problems)
            self.assertIn("Kun.Data 对 Kun.Core 的依赖是 (,", problems)
            self.assertIn("Kun.Gone", problems)
            self.assertIn("不在名单里", problems)

    def test_family_prefixes_come_from_the_list(self):
        packages = [kernel("Kun.Core"), platform("Other.Thing")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Other.Thing", deps=[("Other.Missing", "1.0.0"), ("Unrelated.Lib", "1.0.0")])
            problems = self.verify(root, packages, transitional=True).problems
            self.assertEqual(1, len(problems), problems)
            self.assertIn("Other.Missing", problems[0])

    def test_kernel_must_not_depend_on_platform_unless_excepted(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, self.packages)
            write(root, "Kun.Data.Native", deps=[("Kun.Data", VERSION)])
            self.assertTrue(any("内核档的 Kun.Data.Native 依赖平台档的 Kun.Data，项目文件里没有记档位例外" in p
                                for p in self.verify(root, transitional=True).problems))
            excepted = [kernel("Kun.Core"), kernel("Kun.Data.Native", role="provider", exceptions=(("Kun.Data", "K"),)), platform("Kun.Data")]
            self.assertTrue(any("内核档的 Kun.Data.Native 依赖平台档的 Kun.Data" in p for p in self.verify(root, excepted).problems))
            relaxed = self.verify(root, excepted, transitional=True)
            self.assertEqual([], relaxed.problems)
            self.assertIn("内核档的 Kun.Data.Native 依赖平台档的 Kun.Data", relaxed.allowed["exception"])

    def test_symbols_license_and_commit(self):
        no_symbols = [kernel("Kun.Core"), Package("Kun.Lite", "kernel", "provider", "src/Kun.Lite/Kun.Lite.csproj")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            write(root, "Kun.Core", license="Apache-2.0", commit="f" * 40)
            write(root, "Kun.Lite")
            strict = "\n".join(self.verify(root, no_symbols).problems)
            self.assertIn("Kun.Core 的项目写了 IncludeSymbols=true，却没出符号包", strict)
            self.assertIn("Kun.Lite（没出符号包）", strict)
            self.assertIn("许可不是 MIT", strict)
            self.assertIn("不是本次检出的", strict)
            relaxed = self.verify(root, no_symbols, transitional=True)
            self.assertEqual(["Kun.Lite"], relaxed.allowed["symbols"])
            self.assertEqual(3, len(relaxed.problems), relaxed.problems)  # 写了 IncludeSymbols、许可、提交号两种模式都判失败

    def test_repository_and_prefix_are_relaxed_only_in_transitional_mode(self):
        packages = [kernel("Kun.Core"), platform("Other.Thing")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Kun.Core", repository="https://example.com/elsewhere.git")
            write(root, "Other.Thing", commit="")
            strict = "\n".join(self.verify(root, packages).problems)
            self.assertIn("Kun.Core（仓库地址不是本仓库）", strict)
            self.assertIn("Other.Thing（没带提交号）", strict)
            self.assertNotIn("example.com", strict)
            relaxed = self.verify(root, packages, transitional=True)
            self.assertEqual([], relaxed.problems)
            self.assertEqual({"prefix", "repository"}, set(relaxed.allowed))

    def test_metadata_and_description_are_relaxed_only_in_transitional_mode(self):
        packages = [kernel("Kun.Core"), kernel("Kun.Lib", role="provider"), kernel("Kun.Bare", role="provider"), kernel("Kun.Default", role="provider")]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            write(root, "Kun.Lib", authors="Someone Else", copyright="", project_url="", description="Wraps the Legacy runtime")
            write(root, "Kun.Core", description="Reads the legacy.sid cookie")  # 小写的运行期字面不算
            write(root, "Kun.Bare", description="")
            write(root, "Kun.Default", description="Package Description")
            strict = "\n".join(self.verify(root, packages).problems)
            self.assertIn("Kun.Lib（作者、版权、项目地址不对）", strict)
            self.assertIn("Kun.Lib（描述里写着过渡目录的名字）", strict)
            self.assertIn("Kun.Bare（描述是空的）", strict)
            self.assertIn("Kun.Default（描述是空的）", strict)
            relaxed = self.verify(root, packages, transitional=True)
            self.assertEqual([], relaxed.problems)
            self.assertEqual(["Kun.Lib（作者、版权、项目地址不对）"], relaxed.allowed["metadata"])
            self.assertEqual(3, len(relaxed.allowed["description"]))

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
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            full_set(root, packages)
            beta = [("Upstream.Instrumentation", "[1.15.1-beta.1, )")]
            write(root, "Kun.Observability", deps=beta)
            write(root, "Kun.Gate", deps=beta)
            strict = self.verify(root, packages).problems
            self.assertEqual(1, len(strict), strict)
            self.assertIn("Kun.Gate → Upstream.Instrumentation", strict[0])
            self.assertEqual({"prerelease": ["Kun.Gate → Upstream.Instrumentation [1.15.1-beta.1,)"]}, self.verify(root, packages, transitional=True).allowed)
        with tempfile.TemporaryDirectory() as folder:  # 版本本身是预发布时不查
            root = Path(folder)
            for p in packages:
                write(root, p.id, version=f"{VERSION}-beta.1", deps=beta if p.id == "Kun.Gate" else ())
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
        return subprocess.run([sys.executable, str(SCRIPT), *args], capture_output=True, text=True)

    def test_verify_packages_exit_codes(self):
        packages = [kernel("Kun.Core"), platform("Other.Thing", symbols=False)]
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            listing = root / "list.json"
            listing.write_text(json.dumps([release.asdict(p) for p in packages]), encoding="utf-8")
            out = root / "out"
            out.mkdir()
            write(out, "Kun.Core")
            write(out, "Kun.Core", suffix=".snupkg")
            write(out, "Other.Thing")
            common = ["verify-packages", "--version", VERSION, "--packages", str(out), "--list", str(listing), "--commit", COMMIT]
            relaxed = self.run_cli(*common, "--transitional")
            self.assertEqual(0, relaxed.returncode, relaxed.stdout + relaxed.stderr)
            self.assertIn("制品核对通过（过渡模式）", relaxed.stdout)
            self.assertIn("[prefix]", relaxed.stdout)
            self.assertIn("[symbols]", relaxed.stdout)
            strict = self.run_cli(*common)
            self.assertEqual(1, strict.returncode, strict.stdout + strict.stderr)
            bad_version = self.run_cli("verify-packages", "--version", "2026.11.31", "--packages", str(out), "--list", str(listing))
            self.assertEqual(2, bad_version.returncode)
            missing_dir = self.run_cli(*[a if a != str(out) else str(root / "nope") for a in common])
            self.assertEqual(2, missing_dir.returncode)


if __name__ == "__main__":
    unittest.main()
