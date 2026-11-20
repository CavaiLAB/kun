"""scripts/dotnet_ci.py 的自测（S1-QA-021、S1-KUN-020、S1-KUN-028）：触发过滤自检认得的各类写法，verify 对各分片（含分片 k）结果的核对与
对 Qian.slnf 外测试项目的告警，latest-attempts 按构件名里的尝试号取每个分片最新的一份（「重跑失败的作业」之后），
分片 k 的筛选文件与覆盖率份数核对，images 列不出镜像即工具出错、prefetch 拉取失败只记录，registration 核对每个测试项目都有着落。

运行：python3 -m unittest discover -s tests/gates -v
在临时目录里搭一个 git 跟踪的最小仓库：仓库根放 ci.yml、区域表 scripts/ci_changes.py 与 tests/kun-unit.slnf，_import/qian/ 下放
Qian.slnf、pm/、docs/、根目录 Markdown 与样例源码；把脚本的这些位置换成它再跑。每类写法都有一条会被拦下的样例（去掉对应的识别规则，
用例即失败），并配不该拦的对照：区域表重新纳入了该文件、路径不在本目录的根上、或者根本不是读取。只依赖标准库与 git。
"""
from __future__ import annotations

import contextlib
import io
import json
import os
import shutil
import subprocess
import sys
import tempfile
import textwrap
import unittest
from pathlib import Path
from unittest import mock

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

import dotnet_ci  # noqa: E402

# 与仓库根的 ci.yml 同形：dotnet 区域的作业带 needs.changes.outputs.dotnet == 'true'，有的在 _import/qian 运行，有的在仓库根运行；
# 别的区域的作业运行的脚本不归 dotnet 区域管。
WORKFLOW = """\
name: ci
on:
  pull_request:
jobs:
  changes:
    runs-on: ubuntu-24.04
    steps:
      - run: python3 scripts/ci_changes.py detect --all
  dotnet-build:
    needs: changes
    if: needs.changes.outputs.dotnet == 'true'
    runs-on: ubuntu-24.04
    steps:
      - working-directory: _import/qian
        run: python3 scripts/gate.py
      - working-directory: _import/qian
        run: python3 -m unittest discover -s tests/gates -v
  dotnet-test:
    needs: changes
    if: ${{ !cancelled() && needs.changes.outputs.dotnet == 'true' }}
    runs-on: ubuntu-24.04
    steps:
      - run: python3 _import/qian/scripts/root_gate.py
  native-aot:
    needs: changes
    if: needs.changes.outputs.dotnet == 'true'
    runs-on: ubuntu-24.04
    steps:
      - working-directory: _import/qian
        run: dotnet test --project tests/Sample.AotTests -c Release --report-xunit-trx
  images-selfcheck:
    needs: changes
    if: needs.changes.outputs.images == 'true'
    runs-on: ubuntu-24.04
    steps:
      - working-directory: _import/qian
        run: python3 scripts/images_gate.py
"""

# 与仓库根的区域表同形：dotnet 区域管 _import/qian/ 的一组以 ** 开头、排除 pm/、docs/ 与根目录 Markdown，另有仓库根路径的一组。
AREAS = """\
from typing import NamedTuple, Tuple


class Area(NamedTuple):
    title: str
    groups: Tuple[Tuple[str, ...], ...]


AREAS = {{
    "dotnet": Area("构建", ({groups})),
    "images": Area("镜像", (("_import/qian/deploy/**",),)),
}}
"""

KUN_UNIT = '{"solution": {"path": "../Kun.slnx", "projects": ["tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj"]}}\n'
KUN_INTEGRATION = "tests/Kun.Sample.IntegrationTests/Kun.Sample.IntegrationTests.csproj"

# 照仓库里测试的写法从 AppContext.BaseDirectory 向上找 Qian.slnf：根目录文件名只在这样定位了仓库根的文件里算引用。
LOCATOR = """\
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Qian.slnf"))) directory = directory.Parent;
        return directory!.FullName;
    }
"""

XUNIT_PROJECT = '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><PackageReference Include="xunit.v3" /></ItemGroup></Project>\n'

# 与 tests/Kun.Testing.Containers 同形的容器夹具：只引断言库与扩展点（不是测试项目），镜像写在 ContainerImages 里，夹具类型按属性取镜像。
CONTAINERS_PROJECT = ('<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><PackageReference Include="xunit.v3.assert" />'
                      '<PackageReference Include="xunit.v3.extensibility.core" /></ItemGroup></Project>\n')
CONTAINER_IMAGES = """\
namespace Kun.Testing.Containers;

public static class ContainerImages
{
    public static string Postgres => FromEnvironment("QIAN_TEST_IMAGE_POSTGRES", "postgres:16-alpine");

    public static string Redis => FromEnvironment("QIAN_TEST_IMAGE_REDIS", "redis:7-alpine");

    private static string FromEnvironment(string variable, string fallback) => Environment.GetEnvironmentVariable(variable) ?? fallback;
}
"""
POSTGRES_FIXTURE = "namespace Kun.Testing.Containers;\n\npublic sealed class PostgresFixture\n{\n    public string Image => ContainerImages.Postgres;\n}\n"
# 照仓库里的写法用反斜杠引用夹具项目
FIXTURE_USER = XUNIT_PROJECT.replace("</ItemGroup>", '<ProjectReference Include="..\\Kun.Testing.Containers\\Kun.Testing.Containers.csproj" /></ItemGroup>')


def csharp(*statements: str, locator: bool = True) -> str:
    """一个 C# 类，Read 方法体依次是 statements：第 i 条（从 0 数）在第 7 + i 行。"""
    body = "\n".join("        " + statement for statement in statements)
    return ("namespace Sample;\n\npublic sealed class Probe\n{\n    public void Read(string root, string name, string id)\n    {\n"
            f"{body}\n    }}\n{LOCATOR if locator else ''}}}\n")


def qian(*patterns: str) -> tuple:
    return tuple("!_import/qian/" + pattern[1:] if pattern.startswith("!") else "_import/qian/" + pattern for pattern in patterns)


class Sandbox:
    """临时仓库：scripts/dotnet_ci.py 的 ROOT 换成其中的 _import/qian/，REPO、SOLUTION、WORKFLOW、CI_CHANGES、KUN_UNIT、
    KUN_INTEGRATION 与 CONTAINERS 换成它里面的对应文件，NOT_IN_CI 换成 not_in_ci（缺省为空）；write 写在 _import/qian/ 下，
    write_repo 写在仓库根；写进去的文件在核对前 git add。"""

    def __init__(self, case: unittest.TestCase) -> None:
        self.case = case
        self.repo = Path(tempfile.mkdtemp(prefix="qian-dotnet-ci-"))
        case.addCleanup(shutil.rmtree, self.repo, True)
        self.root = self.repo / "_import" / "qian"
        self.not_in_ci: dict = {}
        subprocess.run(["git", "init", "-q", str(self.repo)], check=True)
        # 与仓库里的 Qian.slnf 同形：筛选一个 Kun.slnx（临时仓库里不必真有），项目路径相对它所在的目录。
        self.write("Qian.slnf", '{"solution": {"path": "Kun.slnx", "projects": ["tests/Sample.Tests/Sample.Tests.csproj"]}}\n')
        self.write_repo("tests/kun-platform.slnf", '{"solution": {"path": "../Kun.slnx", "projects": []}}\n')
        self.write("tests/Sample.Tests/Sample.Tests.csproj", XUNIT_PROJECT)
        self.write("AGENTS.md", "# 约定\n")
        self.write("README.md", "# 仓库\n")
        self.write("docs/prd/14-quality.md", "# 质量\n")
        self.write("docs/prd/15-delivery.md", "# 交付\n")
        self.write("pm/tasks/S1-QA-001.md", "---\nid: S1-QA-001\n---\n")
        self.write("contracts/v0/sample.schema.json", "{}\n")
        self.write("scripts/gate.py", "print('门禁')\n")
        self.write_repo(".github/workflows/ci.yml", WORKFLOW)
        self.write_repo("tests/kun-unit.slnf", KUN_UNIT)
        self.write_repo("tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj", XUNIT_PROJECT)
        self.write_repo(KUN_INTEGRATION, XUNIT_PROJECT)
        self.workflow()

    def write(self, path: str, text: str) -> None:
        self.write_repo(f"_import/qian/{path}", text)

    def write_repo(self, path: str, text: str) -> None:
        target = self.repo / path
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(text, encoding="utf-8")

    def workflow(self, *reincluded: str) -> None:
        """dotnet 区域管 _import/qian/ 的一组：排除 pm/、docs/ 与根目录 Markdown，再重新纳入 reincluded。"""
        groups = [qian("**", "!pm/**", "!docs/**", "!*.md", *reincluded), ("src/**", "tests/**", "!**/*.md")]
        self.areas(*groups)

    def areas(self, *groups: tuple) -> None:
        self.write_repo("scripts/ci_changes.py", AREAS.format(groups="".join(repr(tuple(group)) + ", " for group in groups)))

    def patched(self) -> contextlib.AbstractContextManager:
        subprocess.run(["git", "-C", str(self.repo), "add", "-A"], check=True)
        return mock.patch.multiple(dotnet_ci, ROOT=self.root, REPO=self.repo, SOLUTION=self.root / "Qian.slnf",
                                   WORKFLOW=self.repo / ".github" / "workflows" / "ci.yml",
                                   CI_CHANGES=self.repo / "scripts" / "ci_changes.py",
                                   KUN_UNIT=self.repo / "tests" / "kun-unit.slnf", KUN_INTEGRATION=[KUN_INTEGRATION],
                                   CONTAINERS=self.root / "tests" / "Kun.Testing.Containers",
                                   PLATFORM_FIXTURES=self.repo / "tests/Kun.Testing.PlatformFixtures", NOT_IN_CI=dict(self.not_in_ci))

    def passes(self):
        with self.patched():
            return dotnet_ci.paths_check()

    def blocked(self) -> str:
        with self.patched(), self.case.assertRaises(dotnet_ci.CheckFailed) as caught:
            dotnet_ci.paths_check()
        return str(caught.exception)


class CsharpReferenceTests(unittest.TestCase):
    def setUp(self) -> None:
        self.box = Sandbox(self)

    def source(self, *statements: str, locator: bool = True, path: str = "tests/Sample.Tests/Sample.cs") -> None:
        self.box.write(path, csharp(*statements, locator=locator))

    def test_root_test_inputs_keep_the_platform_document_trigger(self) -> None:
        self.box.write_repo("tests/Moved.Tests/Case.cs", 'class Case { string path = "_import/qian/docs/prd/14-quality.md"; }')
        self.box.write_repo("tests/Moved.Tests/Moved.Tests.csproj",
                            '<Project><ItemGroup><None Include="../../_import/qian/pm/tasks/S1-QA-001.md" /></ItemGroup></Project>')
        message = self.box.blocked()
        self.assertIn("tests/Moved.Tests/Case.cs", message)
        self.assertIn("tests/Moved.Tests/Moved.Tests.csproj", message)
        self.box.workflow("docs/prd/14-quality.md", "pm/tasks/S1-QA-001.md")
        self.box.passes()

    def test_single_literal_is_blocked_until_the_filter_reincludes_the_file(self) -> None:
        self.source('var text = File.ReadAllText(Path.Combine(root, "docs/prd/14-quality.md"));')
        self.assertIn("tests/Sample.Tests/Sample.cs:7 读取 docs/prd/14-quality.md，但只改它的 PR 不触发 dotnet 区域的作业", self.box.blocked())
        self.box.workflow("docs/prd/14-quality.md")
        counts = self.box.passes()
        self.assertEqual(1, counts["重新纳入"])

    def test_path_combine_segments_starting_with_docs_or_pm(self) -> None:
        self.source('var text = File.ReadAllText(Path.Combine(root, "docs", "prd", "14-quality.md"));',
                    'var tasks = Directory.GetFiles(Path.Join(root, "pm", "tasks"));',
                    'var task = Path.Combine(root, "pm", "tasks", name);')
        message = self.box.blocked()
        self.assertIn("Sample.cs:7 读取 docs/prd/14-quality.md，", message)
        self.assertIn("Sample.cs:8 读取 pm/tasks/S1-QA-001.md，", message)
        self.assertIn("Sample.cs:9 读取 pm/tasks/S1-QA-001.md，", message)  # 后面跟着变量：按 pm/tasks/ 前缀算
        self.assertNotIn("15-delivery", message)  # 拼完整的路径只算那一个文件，不算整个目录
        self.box.workflow("docs/prd/14-quality.md", "pm/tasks/**")
        self.box.passes()

    def test_relative_paths_climbing_back_to_the_repository_root(self) -> None:
        self.source('var a = File.ReadAllText("../docs/prd/14-quality.md");',
                    r'var b = File.ReadAllText(@"..\..\..\pm\tasks\S1-QA-001.md");',
                    'var c = Path.Combine(AppContext.BaseDirectory, "..", "..", "docs", "prd", "15-delivery.md");')
        message = self.box.blocked()
        self.assertIn("Sample.cs:7 读取 docs/prd/14-quality.md，", message)
        self.assertIn("Sample.cs:8 读取 pm/tasks/S1-QA-001.md，", message)
        self.assertIn("Sample.cs:9 读取 docs/prd/15-delivery.md，", message)

    def test_interpolated_prefix_and_segments_after_a_hole(self) -> None:
        self.source('var a = File.ReadAllText(Path.Combine(root, $"docs/prd/{name}.md"));',
                    'var b = File.ReadAllText($"{root}/docs/prd/14-quality.md");',
                    'var c = File.ReadAllText(root + "/pm/tasks/" + name + ".md");',
                    'var d = File.ReadAllText($@"{root}\\docs\\prd\\{name}");',
                    'var e = $"docs/prd/15-{name}";',
                    'var f = File.ReadAllText(Path.Combine(root, "docs/prd/14-" + name + ".md"));')
        message = self.box.blocked()
        self.assertIn("Sample.cs:7 读取 docs/prd/…，其中 2 个文件只改它们的 PR 不触发 dotnet 区域的作业", message)
        self.assertIn("Sample.cs:8 读取 docs/prd/14-quality.md，", message)
        self.assertIn("Sample.cs:9 读取 pm/tasks/S1-QA-001.md，", message)
        self.assertIn("Sample.cs:10 读取 docs/prd/…，", message)
        self.assertIn("Sample.cs:11 读取 docs/prd/15-delivery.md，", message)  # 常量前缀按字符串前缀对文件
        self.assertIn("Sample.cs:12 读取 docs/prd/14-quality.md，", message)  # 后面紧跟 + 也只是前缀

    def test_root_markdown_counts_where_the_file_locates_the_repository_root(self) -> None:
        self.source('var a = File.ReadAllText(Path.Combine(RepositoryRoot(), "AGENTS.md"));')
        self.assertIn("Sample.cs:7 读取 AGENTS.md，", self.box.blocked())
        self.box.workflow("AGENTS.md")
        self.box.passes()

    def test_bare_root_file_names_elsewhere_are_not_repository_reads(self) -> None:
        # 照 Lifecycle 测试的写法：临时部署仓库里的 README.md、签名清单里的文件名，本文件不定位仓库根
        self.source('File.WriteAllText(Path.Combine(root, "README.md"), "部署仓库");',
                    'var files = new Dictionary<string, string> { ["README.md"] = "x" };', locator=False)
        self.box.passes()
        self.source('var a = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../../README.md"));', locator=False)
        self.assertIn("Sample.cs:7 读取 README.md，", self.box.blocked())  # 带 ../ 往上爬的照样算

    def test_comments_and_later_segments_are_not_repository_reads(self) -> None:
        self.source('// 读取 "docs/prd/14-quality.md" 的用例见别处',
                    '/* "pm/tasks/S1-QA-001.md" */ var x = 1;',
                    'var plugin = Path.Combine(root, "plugins", id, "docs", "README.md");',
                    'var guide = Path.Combine(root, id, "docs");',
                    'var note = "见 docs/prd/14-quality.md";',
                    'var schema = File.ReadAllText(Path.Combine(root, "contracts/v0/sample.schema.json"));')
        counts = self.box.passes()
        self.assertEqual(0, counts["重新纳入"])
        self.assertEqual(2, counts[".NET 源码"])  # Qian.slnf 与 contracts 下的 schema

    def test_strings_and_characters_before_the_literal_do_not_derail_the_lexer(self) -> None:
        read = 'File.ReadAllText(Path.Combine(root, "{}"));'.format
        self.source('var a = @"x ""docs/prd/14-quality.md"" y";',  # 逐字字符串里的 "" 是引号本身，整串一个字面量
                    'var b = $"{(id == "a" ? "{" : "x")}"; var c = ' + read("docs/prd/15-delivery.md"),  # 洞里字符串的括号不算
                    "var d = '\"'; var e = " + read("pm/tasks/S1-QA-001.md"),  # 字符字面量里的引号
                    'var f = """x"y"""; var g = ' + read("docs/prd/14-quality.md"),  # 原始字面量里的单个引号
                    'var h = $$"""{{name}} "docs/prd/15-delivery.md" """;',  # 原始插值字符串的内容
                    'var i = $$"""{{root}}/pm/tasks/S1-QA-001.md""";')  # 原始插值字符串的洞后段
        message = self.box.blocked()
        self.assertIn("Sample.cs:8 读取 docs/prd/15-delivery.md，", message)
        self.assertIn("Sample.cs:9 读取 pm/tasks/S1-QA-001.md，", message)
        self.assertIn("Sample.cs:10 读取 docs/prd/14-quality.md，", message)
        self.assertIn("Sample.cs:12 读取 pm/tasks/S1-QA-001.md，", message)
        self.assertEqual(4, message.count("读取"), message)


class MsbuildItemTests(unittest.TestCase):
    def setUp(self) -> None:
        self.box = Sandbox(self)

    def project(self, *items: str) -> None:
        self.box.write("tests/Sample.Tests/Sample.Tests.csproj",
                       '<Project Sdk="Microsoft.NET.Sdk">\n  <ItemGroup>\n    <PackageReference Include="xunit.v3" />\n'
                       + "".join(f"    {item}\n" for item in items) + "  </ItemGroup>\n</Project>\n")

    def test_content_and_none_items_pointing_into_docs_pm_or_root_markdown(self) -> None:
        self.project('<None Include="../../docs/prd/14-quality.md" CopyToOutputDirectory="PreserveNewest" />',
                     '<Content Include="$(MSBuildThisFileDirectory)../../pm/tasks/*.md" />',
                     r'<None Update="..\..\README.md" Pack="true" PackagePath="/" />')
        message = self.box.blocked()
        self.assertIn("tests/Sample.Tests/Sample.Tests.csproj:4 读取 docs/prd/14-quality.md，", message)
        self.assertIn("Sample.Tests.csproj:5 读取 pm/tasks/S1-QA-001.md，", message)
        self.assertIn("Sample.Tests.csproj:6 读取 README.md，", message)
        self.box.workflow("docs/prd/14-quality.md", "pm/tasks/*.md", "README.md")
        self.assertEqual(3, self.box.passes()["重新纳入"])

    def test_items_elsewhere_in_the_repository_and_unresolved_properties_pass(self) -> None:
        self.project('<EmbeddedResource Include="../../contracts/v0/*.json" />',
                     '<None Include="$(OutputPath)docs/prd/14-quality.md" />',
                     '<ProjectReference Include="../Sample.Host/Sample.Host.csproj" />')
        self.assertEqual(0, self.box.passes()["重新纳入"])


class PythonReferenceTests(unittest.TestCase):
    def setUp(self) -> None:
        self.box = Sandbox(self)

    def test_script_run_by_the_workflow(self) -> None:
        self.box.write("scripts/gate.py", textwrap.dedent("""\
            import os
            from pathlib import Path

            ROOT = Path(__file__).resolve().parents[1]


            def main(name):
                quality = (ROOT / "docs" / "prd" / "14-quality.md").read_text()
                with open(f"pm/tasks/{name}.md") as task:
                    delivery = ROOT.joinpath("docs", "prd", "15-delivery.md").read_text()
                agents = (Path.cwd() / "AGENTS.md").read_text()
                readme = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "README.md")
                return quality, task.read(), delivery, agents, readme
            """))
        message = self.box.blocked()
        self.assertIn("scripts/gate.py:8 读取 docs/prd/14-quality.md，", message)
        self.assertIn("scripts/gate.py:9 读取 pm/tasks/S1-QA-001.md，", message)  # open() 的相对路径相对本目录（作业在本目录运行）
        self.assertIn("scripts/gate.py:10 读取 docs/prd/15-delivery.md，", message)
        self.assertIn("scripts/gate.py:11 读取 AGENTS.md，", message)
        self.assertIn("scripts/gate.py:12 读取 README.md，", message)
        self.box.workflow("docs/prd/*.md", "pm/tasks/**", "*.md")
        self.assertEqual(5, self.box.passes()["重新纳入"])

    def test_unittest_module_and_the_helpers_it_imports(self) -> None:
        self.box.write("tests/gates/test_sample.py", textwrap.dedent("""\
            import sys
            import unittest
            from pathlib import Path

            ROOT = Path(__file__).resolve().parents[2]
            TASKS = ROOT / "pm" / "tasks"
            sys.path.insert(0, str(ROOT / "scripts"))

            import helper


            class SampleTests(unittest.TestCase):
                def test_tasks(self):
                    self.assertTrue(sorted(TASKS.glob("*.md")))
            """))
        self.box.write("scripts/helper.py", "from pathlib import Path\n\nAGENTS = Path(__file__).parent.parent / 'AGENTS.md'\n"
                                            "\n\ndef rules():\n    return open(AGENTS).read()\n")
        message = self.box.blocked()
        self.assertIn("tests/gates/test_sample.py:14 读取 pm/tasks/S1-QA-001.md，", message)  # 模块顶层的名字在用到它的地方算
        self.assertIn("scripts/helper.py:7 读取 AGENTS.md，", message)

    def test_paths_on_other_anchors_and_scripts_the_workflow_does_not_run_pass(self) -> None:
        self.box.write("tests/gates/test_sample.py", textwrap.dedent("""\
            import tempfile
            import unittest
            from pathlib import Path


            class SampleTests(unittest.TestCase):
                def setUp(self):
                    self.tmp = Path(tempfile.mkdtemp())
                    (self.tmp / "docs/prd").mkdir(parents=True)
                    (self.tmp / "docs/prd/14-quality.md").write_text("# 质量")
                    site = self.tmp / "pm"
                    (site / "README.md").write_text("# 任务")
            """))
        self.box.write("scripts/other.py", "from pathlib import Path\n\nprint((Path(__file__).parents[1] / 'docs/prd/14-quality.md').read_text())\n")
        self.assertEqual(0, self.box.passes()["重新纳入"])

    def test_scripts_run_from_the_repository_root_count_and_jobs_of_other_areas_do_not(self) -> None:
        reader = "from pathlib import Path\n\nprint((Path(__file__).resolve().parents[1] / 'docs/prd/14-quality.md').read_text())\n"
        self.box.write("scripts/images_gate.py", reader)  # 只由镜像自检作业运行，不归 dotnet 区域管
        self.assertEqual(0, self.box.passes()["重新纳入"])
        self.box.write("scripts/root_gate.py", reader)  # dotnet 区域的作业在仓库根运行，写成 _import/qian/scripts/root_gate.py
        self.assertIn("scripts/root_gate.py:3 读取 docs/prd/14-quality.md，", self.box.blocked())
        self.box.workflow("docs/prd/14-quality.md")
        self.assertEqual(1, self.box.passes()["重新纳入"])

    def test_module_whose_root_is_replaced_by_the_loading_test(self) -> None:
        # 照 test_check_docs.py：按路径加载被测脚本，再把它的 ROOT 换成临时目录；没换的名字照样算
        self.box.write("scripts/docs_gate.py", textwrap.dedent("""\
            from pathlib import Path

            ROOT = Path(__file__).resolve().parents[1]
            INDEX = ROOT / "docs" / "prd"


            def routes():
                return sorted((ROOT / "docs").rglob("*.md"))


            def index():
                return sorted(INDEX.glob("*.md"))
            """))
        loader = textwrap.dedent("""\
            import importlib.util
            import tempfile
            import unittest
            from pathlib import Path

            ROOT = Path(__file__).resolve().parents[2]
            _spec = importlib.util.spec_from_file_location("docs_gate", ROOT / "scripts" / "docs_gate.py")
            docs_gate = importlib.util.module_from_spec(_spec)
            _spec.loader.exec_module(docs_gate)


            class DocsGateTests(unittest.TestCase):
                def setUp(self):
                    self.saved = docs_gate.ROOT
                    docs_gate.ROOT{replaced} = Path(tempfile.mkdtemp()){extra}
            """)
        self.box.write("tests/gates/test_docs_gate.py", loader.format(replaced=", docs_gate.INDEX", extra=", Path(tempfile.mkdtemp())"))
        self.assertEqual(0, self.box.passes()["重新纳入"])
        self.box.write("tests/gates/test_docs_gate.py", loader.format(replaced="", extra=""))
        message = self.box.blocked()
        self.assertIn("scripts/docs_gate.py:12 读取 docs/prd，", message)  # INDEX 没被换掉
        self.assertNotIn("docs_gate.py:8", message)


class AreaTableTests(unittest.TestCase):
    """paths-check 的匹配式取自仓库根区域表里 dotnet 区域管 _import/qian/ 的那一组，去掉前缀。"""

    def setUp(self) -> None:
        self.box = Sandbox(self)

    def test_the_group_under_this_directory_without_the_prefix(self) -> None:
        self.box.workflow("docs/generated/**")
        with self.box.patched():
            self.assertEqual(["**", "!pm/**", "!docs/**", "!*.md", "docs/generated/**"], dotnet_ci.dotnet_paths())

    def test_exactly_one_group_under_this_directory_starting_with_double_star(self) -> None:
        for groups in ([("src/**",)], [qian("**"), qian("**", "!pm/**")], [qian("scripts/**", "!pm/**")],
                       [qian("**") + ("src/**",)]):
            self.box.areas(*groups)
            with self.subTest(groups=groups), self.box.patched(), self.assertRaises(dotnet_ci.ToolError):
                dotnet_ci.dotnet_paths()

    def test_paths_check_reads_the_table_again_after_every_change(self) -> None:
        self.box.write("scripts/gate.py", "from pathlib import Path\n\nprint((Path(__file__).parents[1] / 'AGENTS.md').read_text())\n")
        self.assertIn("scripts/gate.py:3 读取 AGENTS.md，", self.box.blocked())
        self.box.workflow("AGENTS.md")
        self.assertEqual(1, self.box.passes()["重新纳入"])
        self.box.workflow()
        self.assertIn("scripts/gate.py:3 读取 AGENTS.md，", self.box.blocked())


class VerifyTests(unittest.TestCase):
    """verify 核对各分片（含分片 k）的 TRX，并对 _import/qian/ 下不在 Qian.slnf 里的测试项目告警（门禁样例与专属作业运行的项目除外）。"""

    TRX = ('<?xml version="1.0" encoding="utf-8"?>\n'
           '<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">\n'
           '  <Times start="2026-10-25T00:00:00.0000000+00:00" finish="2026-10-25T00:00:05.0000000+00:00" />\n'
           '  <Results><UnitTestResult outcome="{outcome}" /></Results>\n'
           '  <TestDefinitions><UnitTest storage="/work/{module}.dll" /></TestDefinitions>\n'
           '</TestRun>\n')

    def setUp(self) -> None:
        self.box = Sandbox(self)
        self.results = self.box.root / "results"
        self.trx("a", "Sample.Tests")
        self.trx("k", "Kun.Sample.Tests", "unit")
        self.trx("k", "Kun.Sample.IntegrationTests", "integration")
        self.box.write("tests/gates/depgraph/Sample/Sample.csproj", XUNIT_PROJECT)
        self.box.write("tests/Sample.AotTests/Sample.AotTests.csproj", XUNIT_PROJECT)
        self.box.write("tests/Sample.TestHost/Sample.TestHost.csproj", '<Project Sdk="Microsoft.NET.Sdk" />\n')

    def trx(self, shard: str, module: str, directory: str = "", outcome: str = "Passed") -> Path:
        target = self.results / f"test-results-{shard}" / directory / f"{module}.trx"
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(self.TRX.format(module=module, outcome=outcome), encoding="utf-8")
        return target

    def verify(self):
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", {"a": None}):
            return dotnet_ci.verify(self.results)

    def test_gates_samples_dedicated_jobs_and_support_projects_are_not_reported(self) -> None:
        summary, problems, warnings = self.verify()
        self.assertEqual(([], []), (problems, warnings))
        self.assertNotIn("不在 Qian.slnf 里", "\n".join(summary))
        self.assertIn("合计 3 个模块、3 条用例；Qian.slnf 里识别出 1 个测试项目，分片 k 登记 2 个。", summary)

    def test_test_project_outside_the_solution_is_reported_by_name(self) -> None:
        self.box.write("tests/Extra.Tests/Extra.Tests.csproj", XUNIT_PROJECT)
        summary, problems, warnings = self.verify()
        self.assertEqual([], problems)
        self.assertEqual(1, len(warnings))
        self.assertIn("1 个测试项目不在 Qian.slnf 里", warnings[0])
        self.assertIn("：Extra.Tests。", warnings[0])
        self.assertIn("_import/qian/ 下不在 Qian.slnf 里、没有任何分片运行的测试项目：Extra.Tests。", summary)
        output = io.StringIO()
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", {"a": None}), contextlib.redirect_stdout(output):
            self.assertEqual(0, dotnet_ci.main(["verify", str(self.results), "--summary", str(self.box.root / "summary.md")]))
        self.assertIn("::warning::_import/qian/ 下有 1 个测试项目不在 Qian.slnf 里", output.getvalue())

    def test_shard_k_must_run_each_registered_project_once_without_failures(self) -> None:
        (self.results / "test-results-k" / "integration" / "Kun.Sample.IntegrationTests.trx").unlink()
        self.trx("a", "Kun.Sample.Tests", "extra")
        self.trx("k", "Kun.Sample.Tests", "unit", outcome="Failed")
        _, problems, _ = self.verify()
        self.assertIn("Kun.Sample.IntegrationTests（分片 k）没有测试结果。", problems)
        self.assertIn("Kun.Sample.Tests 应在分片 k，实际在分片 a。", problems)
        self.assertIn("Kun.Sample.Tests 跑了不止一次（分片 a 与 k）。", problems)
        self.assertIn("Kun.Sample.Tests：1 条失败。", problems)

    def test_shard_k_without_results_fails(self) -> None:
        shutil.rmtree(self.results / "test-results-k")
        _, problems, _ = self.verify()
        self.assertEqual(["分片 k 没有上传测试结果（test-results-k）。"], problems)

    def test_rerun_of_a_failed_shard_is_verified_with_its_latest_attempt(self) -> None:
        # 「重跑失败的作业」之后的下载目录：分片 a 第 1 次失败、第 2 次通过，分片 k 没重跑，只有第 1 次的。
        downloads = self.box.root / "downloads"
        downloads.mkdir()
        (self.results / "test-results-a").rename(downloads / "test-results-a-attempt-2")
        (self.results / "test-results-k").rename(downloads / "test-results-k-attempt-1")
        failed = downloads / "test-results-a-attempt-1" / "Sample.Tests.trx"
        failed.parent.mkdir()
        failed.write_text(self.TRX.format(module="Sample.Tests", outcome="Failed"), encoding="utf-8")
        self.results.rmdir()
        self.assertEqual(["a：第 2 次尝试（不用第 1 次）", "k：第 1 次尝试"], dotnet_ci.latest_attempts(downloads, self.results))
        self.assertEqual(([], []), self.verify()[1:])
        # 取错了尝试（上一次的那份）就是这次要修的毛病：核对报失败。
        shutil.rmtree(self.results / "test-results-a")
        failed.parent.rename(self.results / "test-results-a")
        self.assertEqual(["Sample.Tests：1 条失败。"], self.verify()[1])


class LatestAttemptsTests(unittest.TestCase):
    """latest-attempts：按构件名里的尝试号给每个分片（含门禁作业）留最新的一份，不看目录的先后与修改时间。"""

    def setUp(self) -> None:
        self.base = Path(tempfile.mkdtemp(prefix="qian-latest-attempts-"))
        self.addCleanup(shutil.rmtree, self.base, True)
        self.downloads = self.base / "downloads"
        self.results = self.base / "results"

    def attempt(self, key: str, attempt: int, text: str = "") -> None:
        target = self.downloads / f"test-results-{key}-attempt-{attempt}" / "marker.txt"
        target.parent.mkdir(parents=True)
        target.write_text(text or f"{key}{attempt}", encoding="utf-8")

    def marker(self, key: str) -> str:
        return (self.results / f"test-results-{key}" / "marker.txt").read_text(encoding="utf-8")

    def test_each_shard_keeps_its_highest_attempt(self) -> None:
        for key, attempt in (("a", 1), ("a", 10), ("a", 2), ("b", 1), ("gates", 1), ("k", 3), ("k", 1)):
            self.attempt(key, attempt)
        rows = dotnet_ci.latest_attempts(self.downloads, self.results)
        self.assertEqual(["a：第 10 次尝试（不用第 1、2 次）", "b：第 1 次尝试", "gates：第 1 次尝试", "k：第 3 次尝试（不用第 1 次）"], rows)
        self.assertEqual(["a10", "b1", "gates1", "k3"], [self.marker(key) for key in ("a", "b", "gates", "k")])
        self.assertEqual(["test-results-a", "test-results-b", "test-results-gates", "test-results-k"],
                         sorted(path.name for path in self.results.iterdir()))

    def test_unrecognized_or_missing_downloads_fail(self) -> None:
        with self.assertRaisesRegex(dotnet_ci.CheckFailed, "不存在"):
            dotnet_ci.latest_attempts(self.downloads, self.results)
        self.downloads.mkdir()
        with self.assertRaisesRegex(dotnet_ci.CheckFailed, "是空的"):
            dotnet_ci.latest_attempts(self.downloads, self.results)
        for name in ("test-results-a", "test-results-a-attempt-0", "test-results-a-attempt-x", "coverage"):
            with self.subTest(name=name):
                (self.downloads / name).mkdir()
                with self.assertRaisesRegex(dotnet_ci.CheckFailed, f"认不出的测试结果：{name}"):
                    dotnet_ci.latest_attempts(self.downloads, self.results)
                (self.downloads / name).rmdir()
        self.assertFalse(self.results.exists())

    def test_command_line_prints_the_choice_and_refuses_an_existing_results_directory(self) -> None:
        self.attempt("a", 1)
        self.attempt("a", 2)
        output = io.StringIO()
        with contextlib.redirect_stdout(output):
            self.assertEqual(0, dotnet_ci.main(["latest-attempts", str(self.downloads), str(self.results)]))
        self.assertIn("a：第 2 次尝试（不用第 1 次）", output.getvalue())
        self.assertEqual("a2", self.marker("a"))
        with contextlib.redirect_stderr(io.StringIO()) as error:
            self.assertEqual(2, dotnet_ci.main(["latest-attempts", str(self.downloads), str(self.results)]))
        self.assertIn("已存在", error.getvalue())


class KunShardTests(unittest.TestCase):
    """分片 k 的筛选文件：筛选仓库根的 Kun.slnx，列出 tests/kun-unit.slnf 的项目与 KUN_INTEGRATION。"""

    def setUp(self) -> None:
        self.box = Sandbox(self)

    def test_filter_lists_the_unit_and_integration_projects(self) -> None:
        out = self.box.repo / "out" / "tests.slnf"
        with self.box.patched(), contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(0, dotnet_ci.main(["shard", "k", "--out", str(out)]))
        document = json.loads(out.read_text(encoding="utf-8"))
        self.assertEqual(str((self.box.repo / "Kun.slnx").resolve()), document["solution"]["path"])
        self.assertEqual(["tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj", KUN_INTEGRATION], document["solution"]["projects"])

    def test_registered_projects_must_exist_and_be_test_projects(self) -> None:
        self.box.write_repo(KUN_INTEGRATION, '<Project Sdk="Microsoft.NET.Sdk" />\n')
        with self.box.patched(), self.assertRaises(dotnet_ci.ToolError) as caught:
            dotnet_ci.kun_projects()
        self.assertIn("不是测试项目", str(caught.exception))
        self.box.write_repo("tests/kun-unit.slnf", KUN_UNIT.replace("Kun.Sample.Tests/Kun.Sample.Tests", "Kun.Gone.Tests/Kun.Gone.Tests"))
        with self.box.patched(), self.assertRaises(dotnet_ci.ToolError) as caught:
            dotnet_ci.kun_projects()
        self.assertIn("不存在", str(caught.exception))

    def test_a_module_cannot_be_both_in_the_solution_filter_and_in_shard_k(self) -> None:
        self.box.write("Qian.slnf", '{"solution": {"path": "Kun.slnx", "projects": ["tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj"]}}\n')
        self.box.write("tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj", XUNIT_PROJECT)
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", {"a": None}), self.assertRaises(dotnet_ci.ToolError):
            dotnet_ci.expected_shards()


class CoverageTests(unittest.TestCase):
    """coverage：tests/kun-unit.slnf 里每个测试项目恰好一份 <项目名>.coverage.cobertura.<时间戳>.xml，集成测试不收覆盖率；
    命令行核对通过后把文件改名为 <项目名>.coverage.cobertura.xml。"""

    COBERTURA = '<?xml version="1.0" encoding="utf-8"?>\n<coverage line-rate="1" branch-rate="1" version="1.9"><packages /></coverage>\n'

    def setUp(self) -> None:
        self.box = Sandbox(self)
        self.directory = self.box.repo / "TestResults" / "unit"
        self.directory.mkdir(parents=True)

    def write(self, name: str, text: str = "") -> None:
        (self.directory / name).write_text(text or self.COBERTURA, encoding="utf-8")

    def check(self):
        with self.box.patched():
            return dotnet_ci.coverage(self.directory)

    def test_one_file_per_unit_test_project_passes(self) -> None:
        self.write("Kun.Sample.Tests.coverage.cobertura.181126163040422.xml")
        self.write("Kun.Sample.Tests.trx", "<TestRun />")  # 别的文件不管
        self.assertEqual((["Kun.Sample.Tests：1 份"], []), self.check())
        self.assertTrue((self.directory / "Kun.Sample.Tests.coverage.cobertura.181126163040422.xml").is_file())  # 只核对不改名
        for _ in range(2):  # 改名之后再跑一遍照样通过
            with self.box.patched(), contextlib.redirect_stdout(io.StringIO()):
                self.assertEqual(0, dotnet_ci.main(["coverage", str(self.directory)]))
            self.assertEqual(["Kun.Sample.Tests.coverage.cobertura.xml", "Kun.Sample.Tests.trx"],
                             sorted(path.name for path in self.directory.iterdir()))

    def test_missing_duplicate_unknown_and_malformed_files_fail(self) -> None:
        self.assertEqual(["Kun.Sample.Tests 没有覆盖率文件。"], self.check()[1])
        self.write("Kun.Sample.Tests.coverage.cobertura.181126163040422.xml")
        self.write("Kun.Sample.Tests.coverage.cobertura.181126163040999.xml")
        self.write("coverage.cobertura.181126163041000.xml")  # 没带项目名前缀
        self.write("Kun.Sample.IntegrationTests.coverage.cobertura.181126163041001.xml")  # 不在 kun-unit.slnf 里
        problems = "\n".join(self.check()[1])
        self.assertIn("Kun.Sample.Tests 有 2 份覆盖率文件", problems)
        self.assertIn("coverage.cobertura.181126163041000.xml：认不出", problems)
        self.assertIn("Kun.Sample.IntegrationTests.coverage.cobertura.181126163041001.xml：认不出", problems)
        for path in self.directory.iterdir():
            path.unlink()
        self.write("Kun.Sample.Tests.coverage.cobertura.181126163040422.xml", "<TestRun />")
        problems = self.check()[1]
        self.assertIn("根元素是 TestRun", "\n".join(problems))
        self.assertIn("Kun.Sample.Tests 没有覆盖率文件。", problems)
        with self.box.patched(), contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(1, dotnet_ci.main(["coverage", str(self.directory)]))
        self.assertTrue((self.directory / "Kun.Sample.Tests.coverage.cobertura.181126163040422.xml").is_file())  # 不通过时不改名


class ImagesTests(unittest.TestCase):
    """images 列出分片会拉的镜像，列不出即工具出错（退出码 2）：夹具目录不在、解析不出缺省镜像、分片里有项目引用了夹具却一个镜像
    也没解析出来。分片里没有容器测试时 0 个镜像不算错。prefetch 在后台跑，列不出与拉取失败都只记录，照样写完成标记。"""

    SHARDS = {"a": ["Sample.Tests"], "b": None}

    def setUp(self) -> None:
        self.box = Sandbox(self)
        self.box.write("Qian.slnf", json.dumps({"solution": {"path": "Kun.slnx", "projects": [
            "tests/Sample.Tests/Sample.Tests.csproj", "tests/Plain.Tests/Plain.Tests.csproj"]}}) + "\n")
        self.box.write("tests/Kun.Testing.Containers/Kun.Testing.Containers.csproj", CONTAINERS_PROJECT)
        self.box.write_repo("tests/Kun.Testing.PlatformFixtures/RealFixture.cs",
                            'public sealed class RealFixture { string Image() => Read("QIAN_TEST_IMAGE_PLATFORM", "platform:2"); }\n')
        self.box.write("tests/Kun.Testing.Containers/ContainerImages.cs", CONTAINER_IMAGES)
        self.box.write("tests/Kun.Testing.Containers/PostgresFixture.cs", POSTGRES_FIXTURE)
        self.box.write("tests/Sample.Tests/Sample.Tests.csproj", FIXTURE_USER)
        self.box.write("tests/Sample.Tests/Probe.cs", "namespace Sample;\n\npublic sealed class Probe(PostgresFixture fixture);\n")
        self.box.write("tests/Plain.Tests/Plain.Tests.csproj", XUNIT_PROJECT)
        self.box.write("tests/Plain.Tests/Plain.cs", "namespace Plain;\n\npublic sealed class Plain;\n")

    def environment(self) -> contextlib.AbstractContextManager:
        """去掉覆盖镜像的环境变量，结果只看缺省值。"""
        kept = {name: value for name, value in os.environ.items() if not name.startswith("QIAN_TEST_IMAGE_")}
        return mock.patch.dict(os.environ, kept, clear=True)

    def run_main(self, *argv: str) -> tuple:
        output, errors = io.StringIO(), io.StringIO()
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", self.SHARDS), self.environment(), \
                contextlib.redirect_stdout(output), contextlib.redirect_stderr(errors):
            code = dotnet_ci.main(list(argv))
        return code, output.getvalue(), errors.getvalue()

    def test_images_of_a_shard_and_of_all_tests(self) -> None:
        self.assertEqual((0, "postgres:16-alpine\n", ""), self.run_main("images", "a"))
        self.assertEqual((0, "", ""), self.run_main("images", "b"))  # 没有容器测试：0 个镜像不算错
        self.assertEqual((0, "postgres:16-alpine\nredis:7-alpine\nplatform:2\n", ""), self.run_main("images"))

    def test_missing_fixture_directory_fails_naming_the_path(self) -> None:
        shutil.rmtree(self.box.root / "tests" / "Kun.Testing.Containers")
        code, output, errors = self.run_main("images", "a")
        self.assertEqual((2, ""), (code, output))
        self.assertIn("容器夹具目录 _import/qian/tests/Kun.Testing.Containers 不在", errors)

    def test_platform_fixture_images_are_selected_by_the_actual_fixture_type(self) -> None:
        self.box.write("tests/Sample.Tests/Probe.cs", "public sealed class Probe(RealFixture fixture);")
        self.assertEqual((0, "platform:2\n", ""), self.run_main("images", "a"))

    def test_fixture_without_image_defaults_fails(self) -> None:
        self.box.write("tests/Kun.Testing.Containers/ContainerImages.cs", CONTAINER_IMAGES.replace("QIAN_TEST_IMAGE_", "TEST_IMAGE_"))
        code, _, errors = self.run_main("images", "a")
        self.assertEqual(2, code)
        self.assertIn("_import/qian/tests/Kun.Testing.Containers 里没有解析出 QIAN_TEST_IMAGE_* 缺省镜像", errors)

    def test_shard_whose_fixture_users_resolve_no_image_fails_naming_them(self) -> None:
        # 夹具类型改了名，测试源码里的旧名字对不上：分片 a 引用了夹具项目，却一个镜像也解析不出来
        self.box.write("tests/Kun.Testing.Containers/PostgresFixture.cs", POSTGRES_FIXTURE.replace("PostgresFixture", "DatabaseFixture"))
        code, output, errors = self.run_main("images", "a")
        self.assertEqual((2, ""), (code, output))
        self.assertIn("分片 a 里 Sample.Tests 引用了容器夹具项目（_import/qian/tests/Kun.Testing.Containers），却一个镜像也没解析出来", errors)
        # 经 tests/ 下的测试宿主项目间接引用夹具项目的也算
        self.box.write("tests/Plain.Host/Plain.Host.csproj", '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup>'
                       '<ProjectReference Include="../Kun.Testing.Containers/Kun.Testing.Containers.csproj" /></ItemGroup></Project>\n')
        self.box.write("tests/Plain.Tests/Plain.Tests.csproj",
                       XUNIT_PROJECT.replace("</ItemGroup>", '<ProjectReference Include="..\\Plain.Host\\Plain.Host.csproj" /></ItemGroup>'))
        code, _, errors = self.run_main("images", "b")
        self.assertEqual(2, code)
        self.assertIn("分片 b 里 Plain.Tests 引用了容器夹具项目", errors)

    def test_prefetch_only_records_failed_pulls_and_unlistable_images(self) -> None:
        state = self.box.repo / "prefetch"
        failed = subprocess.CompletedProcess(["docker"], 1, "", "Error response from daemon: toomanyrequests\n")
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", self.SHARDS), self.environment(), \
                mock.patch.object(dotnet_ci.subprocess, "run", return_value=failed) as pull:
            self.assertEqual(0, dotnet_ci.main(["prefetch", "a", "--state", str(state)]))
        pull.assert_called_once_with(["docker", "pull", "--quiet", "postgres:16-alpine"], capture_output=True, text=True)
        self.assertTrue((state / "done").exists())
        log = (state / "pulls.log").read_text(encoding="utf-8")
        self.assertIn("拉取失败 postgres:16-alpine", log)
        self.assertIn("toomanyrequests", log)
        # 后台预拉列不出镜像也只记录：拦下来的是工作流在它前面同步跑的 images
        shutil.rmtree(self.box.root / "tests" / "Kun.Testing.Containers")
        state = self.box.repo / "prefetch-again"
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", self.SHARDS):
            self.assertEqual(0, dotnet_ci.main(["prefetch", "a", "--state", str(state)]))
        self.assertTrue((state / "done").exists())
        self.assertIn("无法列出分片 a的镜像：容器夹具目录", (state / "pulls.log").read_text(encoding="utf-8"))


class RegistrationTests(unittest.TestCase):
    """registration：仓库根（_import/ 之外，tests/gates 除外）的测试项目在 kun-unit.slnf、KUN_INTEGRATION、dotnet 区域的作业
    dotnet test --project 或 NOT_IN_CI 里；_import/qian/ 下的在 Qian.slnf 里或由专属作业运行。NOT_IN_CI 过期即失败。"""

    def setUp(self) -> None:
        self.box = Sandbox(self)
        self.box.write("tests/Sample.AotTests/Sample.AotTests.csproj", XUNIT_PROJECT)  # 由沙箱 ci.yml 的 native-aot 作业运行

    def check(self, *integration: str) -> tuple:
        with self.box.patched(), mock.patch.object(dotnet_ci, "KUN_INTEGRATION", [KUN_INTEGRATION, *integration]):
            return dotnet_ci.registration()

    def problems(self) -> str:
        return "\n".join(self.check()[1])

    def jobs(self, text: str) -> None:
        """在沙箱 ci.yml 的 jobs 末尾加作业，text 是不带缩进的作业原文。"""
        self.box.write_repo(".github/workflows/ci.yml", WORKFLOW + textwrap.indent(textwrap.dedent(text), "  "))

    def test_unregistered_test_projects_anywhere_outside_import_fail_with_the_ways_to_register(self) -> None:
        self.assertEqual([], self.check()[1])
        self.box.write_repo("tests/Kun.Extra.Tests/Kun.Extra.Tests.csproj", XUNIT_PROJECT)
        self.box.write_repo("tools/Kun.Tool.Tests/Kun.Tool.Tests.csproj", XUNIT_PROJECT)
        problems = self.check()[1]
        self.assertEqual(2, len(problems))
        self.assertIn("tests/Kun.Extra.Tests/Kun.Extra.Tests.csproj 是测试项目，却没有着落，CI 不跑它。", problems[0])
        self.assertIn("tools/Kun.Tool.Tests/Kun.Tool.Tests.csproj 是测试项目，却没有着落", problems[1])
        for way in ("tests/kun-unit.slnf", "KUN_INTEGRATION", "dotnet test --project", "NOT_IN_CI"):
            self.assertIn(way, problems[0])

    def test_empty_transitional_filter_reads_tests_from_the_root_filter(self) -> None:
        self.box.write("Qian.slnf", json.dumps({"solution": {"path": "../../Kun.slnx", "projects": []}}))
        project = "tests/Moved.Tests/Moved.Tests.csproj"
        self.box.write_repo(project, XUNIT_PROJECT)
        with self.box.patched():
            with self.assertRaisesRegex(dotnet_ci.ToolError, "都没有列出测试项目"):
                dotnet_ci.test_projects()
            self.box.write_repo("tests/kun-platform.slnf", json.dumps({
                "solution": {"path": "../Kun.slnx", "projects": [project]},
            }))
            self.assertEqual({"Moved.Tests": "../../" + project}, dotnet_ci.test_projects())

    def test_platform_projects_keep_their_shard_across_the_repository_boundary(self) -> None:
        self.box.write("Qian.slnf", json.dumps({"solution": {"path": "../../Kun.slnx", "projects": [
            "_import/qian/tests/Sample.Tests/Sample.Tests.csproj"]}}))
        project = "tests/Kun.Moved.Tests/Kun.Moved.Tests.csproj"
        self.box.write_repo(project, XUNIT_PROJECT)
        self.box.write_repo("tests/kun-platform.slnf", json.dumps({"solution": {"path": "../Kun.slnx", "projects": [project]}}))
        with self.box.patched(), mock.patch.object(dotnet_ci, "SHARDS", {"a": ["Kun.Moved.Tests"], "b": None}):
            self.assertEqual("a", dotnet_ci.expected_shards()["Kun.Moved.Tests"])
            destination = self.box.repo / "build/test.slnf"
            with contextlib.redirect_stdout(io.StringIO()):
                dotnet_ci.write_filter("a", destination)
            self.assertEqual([project], json.loads(destination.read_text())["solution"]["projects"])
            rows, problems, _, _ = dotnet_ci.registration()
            self.assertFalse(problems)
            self.assertIn("kun-platform.slnf", dict(rows)[project])

    def test_unit_and_integration_registrations(self) -> None:
        rows = dict(self.check()[0])
        self.assertEqual("分片 k 的单元测试（kun-unit.slnf）", rows["tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj"])
        self.assertEqual("分片 k 的集成测试（KUN_INTEGRATION）", rows[KUN_INTEGRATION])
        extra = "tests/Kun.Extra.Tests/Kun.Extra.Tests.csproj"
        self.box.write_repo(extra, XUNIT_PROJECT)
        self.box.write_repo("tests/kun-unit.slnf", KUN_UNIT.replace('"]', f'", "{extra}"]'))
        self.assertEqual([], self.check()[1])
        self.box.write_repo("tests/kun-unit.slnf", KUN_UNIT)
        self.assertEqual(1, len(self.check()[1]))
        self.assertEqual([], self.check(extra)[1])

    def test_dedicated_jobs_count_however_the_project_and_working_directory_are_written(self) -> None:
        for name in "ABCDE":
            self.box.write_repo(f"tests/Kun.{name}.Tests/Kun.{name}.Tests.csproj", XUNIT_PROJECT)
        self.jobs("""\
            root-file:
              needs: changes
              if: needs.changes.outputs.dotnet == 'true'
              runs-on: ubuntu-24.04
              steps:
                - run: dotnet test --project tests/Kun.A.Tests/Kun.A.Tests.csproj -c Release
                - name: 折叠块，一个参数一行
                  run: >
                    dotnet test
                    --project tests/Kun.B.Tests
                    --configuration Release
            qian-directory:
              needs: changes
              if: ${{ !cancelled() && needs.changes.outputs.dotnet == 'true' }}
              runs-on: ubuntu-24.04
              defaults:
                run:
                  working-directory: _import/qian
              steps:
                - run: dotnet test --project "../../tests/Kun.C.Tests/Kun.C.Tests.csproj"
                - run: |
                    dotnet test \\
                      --project=../../tests/Kun.D.Tests/
                - run: dotnet test --project "$RUNNER_TEMP/Kun.Generated.Tests"
            other-area:
              needs: changes
              if: needs.changes.outputs.images == 'true'
              runs-on: ubuntu-24.04
              steps:
                - run: dotnet test --project tests/Kun.E.Tests
            """)
        rows, problems, counts, _ = self.check()
        rows = dict(rows)
        for name, job in (("A", "root-file"), ("B", "root-file"), ("C", "qian-directory"), ("D", "qian-directory")):
            self.assertEqual(f"作业 {job} 的 dotnet test --project", rows[f"tests/Kun.{name}.Tests/Kun.{name}.Tests.csproj"])
        self.assertEqual(4, counts["专属作业"])
        # 别的区域的作业不算：只改 .NET 代码的 PR 不跑它
        self.assertEqual(1, len(problems))
        self.assertIn("tests/Kun.E.Tests/Kun.E.Tests.csproj 是测试项目，却没有着落", problems[0])

    def test_gates_samples_executables_and_support_libraries_are_not_test_projects(self) -> None:
        self.box.write_repo("tests/gates/depgraph/Thing/Thing.csproj", XUNIT_PROJECT)
        self.box.write_repo("tests/Kun.Smoke/Kun.Smoke.csproj",
                            '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType></PropertyGroup></Project>\n')
        self.box.write_repo("tests/Kun.Testing.Fixtures/Kun.Testing.Fixtures.csproj", CONTAINERS_PROJECT)
        self.box.write_repo("tests/Kun.Helper/Kun.Helper.csproj",
                            XUNIT_PROJECT.replace("<ItemGroup>", "<PropertyGroup><IsTestProject>false</IsTestProject></PropertyGroup><ItemGroup>"))
        rows, problems, _, _ = self.check()
        self.assertEqual([], problems)
        self.assertEqual({"tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj", KUN_INTEGRATION}, {project for project, _ in rows})

    def test_other_runners_and_is_test_project_count_as_test_projects(self) -> None:
        projects = {
            "tests/Kun.Flagged.Tests/Kun.Flagged.Tests.csproj":
                '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup></Project>\n',
            "tests/Kun.Mstest.Tests/Kun.Mstest.Tests.csproj": '<Project Sdk="MSTest.Sdk/3.6.4" />\n',
            "tests/Kun.Nunit.Tests/Kun.Nunit.Tests.csproj": XUNIT_PROJECT.replace("xunit.v3", "NUnit"),
            "tests/Kun.XunitCore.Tests/Kun.XunitCore.Tests.csproj": XUNIT_PROJECT.replace("xunit.v3", "xunit.v3.core"),
        }
        for path, text in projects.items():
            self.box.write_repo(path, text)
        problems = self.problems()
        for path in projects:
            self.assertIn(f"{path} 是测试项目，却没有着落", problems)

    def test_projects_known_not_to_run_in_ci_are_listed_with_the_reason(self) -> None:
        live = "samples/Kun.Live.Tests/Kun.Live.Tests.csproj"
        self.box.write_repo(live, XUNIT_PROJECT)
        self.assertIn(f"{live} 是测试项目，却没有着落", self.problems())
        self.box.not_in_ci = {live: "要真实的对象存储"}
        rows, problems, counts, _ = self.check()
        self.assertEqual([], problems)
        self.assertEqual("不进 CI（NOT_IN_CI）：要真实的对象存储", dict(rows)[live])
        self.assertEqual(1, counts["不进 CI"])

    def test_a_table_entry_that_is_not_a_tracked_project_in_scope_fails(self) -> None:
        self.box.write_repo("tests/gates/depgraph/Thing/Thing.csproj", XUNIT_PROJECT)
        self.box.not_in_ci = {"samples/Kun.Gone.Tests/Kun.Gone.Tests.csproj": "搬走了", "tests/gates/depgraph/Thing/Thing.csproj": "门禁样例"}
        problems = self.check()[1]
        self.assertEqual(2, len(problems))
        self.assertIn("NOT_IN_CI 里的 samples/Kun.Gone.Tests/Kun.Gone.Tests.csproj 不是 git 跟踪的项目文件", problems[0])
        self.assertIn("NOT_IN_CI 里的 tests/gates/depgraph/Thing/Thing.csproj 不是 git 跟踪的项目文件", problems[1])

    def test_a_table_entry_registered_elsewhere_or_not_a_test_project_fails(self) -> None:
        self.box.write_repo("tests/Kun.Smoke/Kun.Smoke.csproj", '<Project Sdk="Microsoft.NET.Sdk" />\n')
        self.box.not_in_ci = {"tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj": "旧原因", "tests/Kun.Smoke/Kun.Smoke.csproj": "冒烟宿主"}
        problems = self.check()[1]
        self.assertEqual(2, len(problems))
        self.assertIn("tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj 写在 NOT_IN_CI 里，却已经登记在分片 k 的单元测试（kun-unit.slnf）："
                      "表过期了", problems[0])
        self.assertIn("NOT_IN_CI 里的 tests/Kun.Smoke/Kun.Smoke.csproj 不是测试项目", problems[1])

    def test_projects_under_import_must_be_in_the_solution_filter_or_run_by_a_dedicated_job(self) -> None:
        self.box.write("tests/Extra.Tests/Extra.Tests.csproj", XUNIT_PROJECT)
        self.box.write("tests/gates/sample/Sample/Sample.csproj", XUNIT_PROJECT)
        output, errors = io.StringIO(), io.StringIO()
        with self.box.patched(), contextlib.redirect_stdout(output), contextlib.redirect_stderr(errors):
            self.assertEqual(1, dotnet_ci.main(["registration"]))
        self.assertIn("_import/qian/tests/Extra.Tests/Extra.Tests.csproj 是测试项目，却不在 Qian.slnf 里，也没有 dotnet 区域的专属作业运行它",
                      errors.getvalue())
        self.assertNotIn("Sample.AotTests", errors.getvalue())  # 由 native-aot 作业运行
        self.assertNotIn("gates", errors.getvalue())
        self.box.write("Qian.slnf", json.dumps({"solution": {"path": "Kun.slnx", "projects": [
            "tests/Sample.Tests/Sample.Tests.csproj", "tests/Extra.Tests/Extra.Tests.csproj"]}}) + "\n")
        output = io.StringIO()
        with self.box.patched(), contextlib.redirect_stdout(output):
            self.assertEqual(0, dotnet_ci.main(["registration"]))
        self.assertIn("  tests/Kun.Sample.Tests/Kun.Sample.Tests.csproj：分片 k 的单元测试（kun-unit.slnf）", output.getvalue())
        self.assertIn("测试项目着落核对通过：仓库根 2 个（单元 1、集成 1），_import/qian/ 下 3 个（Qian.slnf 2、专属作业 1）。", output.getvalue())

    def test_every_test_project_in_this_repository_is_run_somewhere(self) -> None:
        rows, problems, counts, local = dotnet_ci.registration()
        self.assertEqual([], problems)
        places = dict(rows)
        for project, reason in dotnet_ci.NOT_IN_CI.items():
            self.assertEqual(f"不进 CI（NOT_IN_CI）：{reason}", places[project])
        self.assertTrue(all(counts[kind] for kind in ("单元", "集成", "专属作业")), counts)
        self.assertEqual(len(dotnet_ci.platform_projects()), counts["平台分片"])
        self.assertGreater(counts["平台分片"], 0)
        self.assertEqual(0, sum(local.values()), local)
        self.assertTrue(all(project.startswith("tests/") for project in dotnet_ci.dedicated_tests()))


if __name__ == "__main__":
    unittest.main()
