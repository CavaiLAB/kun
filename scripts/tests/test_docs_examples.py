# -*- coding: utf-8 -*-
"""使用文档示例脚本 scripts/docs_examples.py 的自测（S1-DOCS-012）。只依赖标准库与 bash，不要 .NET。

运行：python3 -m unittest discover -s scripts/tests -p 'test_docs_examples.py' -v
ci.yml 的「使用文档示例」作业先跑它，再跑 check、feed、run。核对的是：
  - 仓库里的三篇页面与 samples/docs/ 现在对得上，页面上装的包都能从 src/ 打出来；
  - check 能抓到页面与文件不一致、编号步骤里没人用的代码块、版本号不一致、节选不是子集、说明改了、多出来的文件；
  - bump 改完以后 check 照样通过；
  - JSON 比对、curl 输出、插入片段、命令替换与读包令牌的交接、终端、包来源的判定。
"""
from __future__ import annotations

import json
import os
import shutil
import sys
import tempfile
import unittest
import unittest.mock
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import docs_examples as docs  # noqa: E402

ROOT = docs.ROOT


def copy_repository_subset(target: Path) -> None:
    """check 读到的文件：三篇页面、samples/docs/、接权限第 6 节节选的两个文件。"""
    for example in docs.ALL:
        destination = target / example.page
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(ROOT / example.page, destination)
        for _, source in example.excerpts.values():
            (target / source).parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(ROOT / source, target / source)
    shutil.copytree(ROOT / docs.EXAMPLES, target / docs.EXAMPLES)


class RepositoryTests(unittest.TestCase):
    def test_pages_and_examples_agree(self) -> None:
        self.assertEqual([], docs.check())

    def test_documented_version_is_a_release_readers_can_install_from_nuget(self) -> None:
        version = docs.documented_version(list(docs.load_pages().values()))
        self.assertRegex(version, docs.RELEASE)
        self.assertNotIn("-beta.", version)

    def test_page_packages_come_from_src_and_the_closure_follows_project_references(self) -> None:
        packages = docs.page_packages(list(docs.load_pages().values()))
        self.assertTrue({"Kun.Mediator", "Kun.Errors.AspNetCore", "Kun.Security", "Kun.Runtime", "Kun.Plugin.Sdk",
                         "Kun.Plugin.Sdk.Generators", "Kun.Authorization.AspNetCore"} <= set(packages), packages)
        closure = docs.project_closure(packages)
        self.assertTrue({"Kun.Core", "Kun.Errors", "Kun.Authorization"} <= {project.stem for project in closure})
        for project in closure:
            self.assertTrue(project.as_posix().startswith("src/"), project)
            for referenced in docs.project_closure([project.stem]):
                self.assertIn(referenced, closure)

    def test_every_example_has_a_flow(self) -> None:
        self.assertEqual({example.slug for example in docs.ALL}, set(docs.FLOWS))


class PageTests(unittest.TestCase):
    def page(self, text: str) -> docs.Page:
        folder = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, folder)
        path = folder / "page.md"
        path.write_text(text, encoding="utf-8")
        return docs.Page(path, folder)

    def test_sections_blocks_and_tables(self) -> None:
        page = self.page("---\ntitle: 示例\n---\n\n# 标题\n\n```bash\nintro\n```\n\n## 1 第一步 {#first}\n\n```bash\na\nb\n```\n\n"
                         "| 列 | `x \\| y` |\n|---|---|\n| 1 | 2 |\n\n```json\n{}\n```\n\n## 2 第二步\n\n```bash\nc\n```\n")
        self.assertEqual(["1 第一步", "2 第二步"], page.sections)
        self.assertEqual([("", "bash", "intro"), ("1 第一步", "bash", "a\nb"), ("1 第一步", "json", "{}"), ("2 第二步", "bash", "c")],
                         [(block.section, block.lang, block.text) for block in page.blocks])
        self.assertEqual([["列", "`x | y`"], ["1", "2"]], page.table("1 第一步"))
        self.assertEqual("c", page.find(docs.Ref("2 第二步", "bash")).text)
        with self.assertRaisesRegex(docs.Failed, "没有第 2 个 bash"):
            page.find(docs.Ref("2 第二步", "bash", 1))

    def test_an_unclosed_fence_is_a_tool_error(self) -> None:
        with self.assertRaises(docs.ToolError):
            self.page("## 1 第一步\n\n```bash\nnever closed\n")

    def test_command_lines_skip_comments_and_join_continuations(self) -> None:
        self.assertEqual(["a b", "c"], docs.command_lines("# 注释\na \\\nb\n\nc\n"))


class CheckTests(unittest.TestCase):
    def setUp(self) -> None:
        self.root = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, self.root)
        copy_repository_subset(self.root)

    def edit(self, relative: str, old: str, new: str) -> None:
        path = self.root / relative
        text = path.read_text(encoding="utf-8")
        self.assertIn(old, text)
        path.write_text(text.replace(old, new, 1), encoding="utf-8")

    def problems(self) -> str:
        return "\n".join(docs.check(self.root))

    def test_a_clean_copy_passes(self) -> None:
        self.assertEqual("", self.problems())

    def test_an_edited_example_file_is_reported_with_the_first_differing_line(self) -> None:
        self.edit("samples/docs/getting-started/Hello.Kun/Program.cs", "app.UseKunErrors();", "app.UseKunErrors(); // 改过")
        problems = self.problems()
        self.assertIn("samples/docs/getting-started/Hello.Kun/Program.cs 不一致", problems)
        self.assertIn("// 改过", problems)

    def test_a_new_code_block_in_a_numbered_step_needs_a_step_or_a_reason(self) -> None:
        self.edit(docs.GETTING_STARTED.page, "## 3 跑起来\n", "## 3 跑起来\n\n```bash\ndotnet --info\n```\n")
        problems = self.problems()
        self.assertIn("没有示例的步骤用到", problems)

    def test_versions_must_agree_across_the_three_pages(self) -> None:
        self.edit(docs.AUTHORIZATION.page, "Kun.Security --version 2026.11.24", "Kun.Security --version 2026.11.21")
        self.assertIn("版本号应处处相同", self.problems())

    def test_the_assembly_version_follows_the_documented_version(self) -> None:
        self.edit(docs.WRITE_A_COMPONENT.page, "`2026.11.24.0`", "`2026.11.21.0`")
        self.assertIn("程序集版本 2026.11.21.0", self.problems())

    def test_an_excerpt_must_be_a_subset_of_the_sample_file(self) -> None:
        self.edit("samples/Kun.Sample.Crm/metadata/roles.json", '"qian.sample.crm:customer:read"', '"qian.sample.crm:customer:write"')
        self.assertIn("节选自 samples/Kun.Sample.Crm/metadata/roles.json", self.problems())

    def test_prose_the_steps_assert_on_must_stay(self) -> None:
        self.edit(docs.GETTING_STARTED.page, "`Title already exists`", "英文标题")
        self.assertIn("找不到「`Title already exists`」", self.problems())

    def test_a_stray_file_in_the_examples_is_reported(self) -> None:
        (self.root / "samples/docs/getting-started/Hello.Kun/Extra.cs").write_text("// 多出来的\n", encoding="utf-8")
        self.assertIn("Extra.cs 不对应页面上的代码块", self.problems())

    def test_bump_rewrites_versions_dates_and_sdk_and_check_still_passes(self) -> None:
        changed = docs.bump("2026.12.3-hotfix.1", "10.0.500", "2026-12-04", self.root)
        self.assertEqual(3, len(changed))
        self.assertEqual("", self.problems())
        pages = docs.load_pages(self.root)
        self.assertEqual("2026.12.3-hotfix.1", docs.documented_version(list(pages.values())))
        component = pages[docs.WRITE_A_COMPONENT.slug].text
        self.assertIn("`2026.12.3.0`", component)
        self.assertIn("kun-v2026.12.3-hotfix.1/tools/Kun.Plugin.Tool", component)
        self.assertIn("在 2026-12-04 用 .NET SDK 10.0.500、Kun 2026.12.3-hotfix.1 照着跑过一遍", component)
        self.assertIn("\nupdated: 2026-12-04\n", component)
        self.assertNotIn("2026.11.24", "".join(page.text for page in pages.values()))

    def test_bump_refuses_beta_versions(self) -> None:
        with self.assertRaises(docs.ToolError):
            docs.bump("2026.12.3-beta.1", "10.0.500", "2026-12-04", self.root)


class JsonTests(unittest.TestCase):
    def test_keys_order_values_and_volatile_fields(self) -> None:
        volatile = {"trace_id": docs.TRACE_ID}
        page = {"code": "X", "trace_id": "ab3bbb4b5c1ae95af67a42a7f6a9cdfc"}
        self.assertEqual([], docs.compare_json(page, {"code": "X", "trace_id": "0" * 32}, volatile))
        self.assertTrue(docs.compare_json(page, {"trace_id": "0" * 32, "code": "X"}, volatile))
        self.assertTrue(docs.compare_json(page, {"code": "Y", "trace_id": "0" * 32}, volatile))
        self.assertTrue(docs.compare_json(page, {"code": "X", "trace_id": 7}, volatile))
        self.assertTrue(docs.compare_json({"status": 409}, {"status": "409"}, {}))
        self.assertTrue(docs.compare_json({"id": "not-a-guid"}, {"id": "0190a6e4-0000-7000-8000-000000000000"}, {"id": docs.GUID_V7}))

    def test_annotations_escape_what_github_actions_would_cut(self) -> None:
        with unittest.mock.patch.object(docs, "say") as say:
            docs.annotate("error", "使用文档示例", "100% 不对\n第二行")
        say.assert_called_once_with("::error title=使用文档示例::100%25 不对%0A第二行")

    def test_subsets_and_the_last_json_line(self) -> None:
        self.assertEqual([], docs.subset_problems({"a": [{"b": 1}]}, {"a": [{"b": 1, "c": 2}], "d": 3}))
        self.assertTrue(docs.subset_problems({"a": [1]}, {"a": [1, 2]}))
        self.assertTrue(docs.subset_problems({"e": 1}, {"a": 1}))
        self.assertEqual({"changed": []}, docs.last_json('构建输出\n{"changed": []}\n'))
        with self.assertRaises(docs.Failed):
            docs.last_json("没有 JSON")


class CurlTests(unittest.TestCase):
    def test_include_parses_status_and_headers_even_after_progress_noise(self) -> None:
        output = ("  % Total    % Received\n  0     0    0\r\nHTTP/1.1 409 Conflict\r\nContent-Type: application/problem+json\r\n"
                  "Content-Language: en\r\n\r\n{\"status\":409}")
        response = docs.curl_response("curl -i http://127.0.0.1:5199/notes --json '{}'", output)
        self.assertEqual(409, response.status)
        self.assertEqual("application/problem+json", response.headers["content-type"])
        self.assertEqual('{"status":409}', response.body)

    def test_write_out_status_trailer(self) -> None:
        command = "curl -s -X POST \"${BASE}/notes/export\" -w '\\nHTTP %{http_code}\\n'"
        response = docs.curl_response(command, '{"count":3}\nHTTP 200\n')
        self.assertEqual((200, '{"count":3}'), (response.status, response.body))
        with self.assertRaises(docs.Failed):
            docs.curl_response(command, '{"count":3}')

    def test_plain_body(self) -> None:
        self.assertEqual(docs.Response(None, {}, "{}"), docs.curl_response("curl -s http://127.0.0.1:5199/nope", "{}\n"))


class SnippetTests(unittest.TestCase):
    def test_localization_goes_before_build_and_after_the_error_middleware(self) -> None:
        pages = docs.load_pages()
        program = (ROOT / docs.EXAMPLES / "getting-started/Hello.Kun/Program.cs").read_text(encoding="utf-8")
        snippet = pages[docs.GETTING_STARTED.slug].find(docs.GETTING_STARTED.blocks["localization"]).text
        lines = docs.localized(program, snippet).split("\n")
        self.assertLess(lines.index("builder.Services.AddRequestLocalization(o => o"), lines.index(docs.GS_BUILD))
        self.assertEqual("app.UseRequestLocalization();", lines[lines.index(docs.GS_ERRORS) + 2])

    def test_http_endpoint_snippets_land_where_the_page_says(self) -> None:
        example = docs.AUTHORIZATION
        page = docs.load_pages()[example.slug]
        program = (ROOT / docs.EXAMPLES / example.slug / "Notes.Authz/Program.cs").read_text(encoding="utf-8")
        composed = docs.with_http_endpoint(program, *(page.find(example.blocks[name]).text for name in ("using", "register", "endpoint")))
        lines = composed.split("\n")
        self.assertEqual("using Kun.Authorization.AspNetCore;", lines[lines.index(docs.AZ_USING) + 1])
        self.assertEqual("builder.Services.AddKunAuthorization();", lines[lines.index(docs.AZ_CHECKER) + 3])
        export = next(index for index, line in enumerate(lines) if line.startswith(docs.AZ_EXPORT))
        self.assertTrue(lines[export - 2].strip().startswith('.RequireKunPermission("com.example.notes:note:read")'))
        self.assertNotIn(docs.AZ_BEHAVIOR, docs.remove(program, docs.AZ_BEHAVIOR))

    def test_an_anchor_must_appear_exactly_once(self) -> None:
        with self.assertRaisesRegex(docs.Failed, "出现 0 次"):
            docs.insert("a\nb\n", "c", "x", after=True)
        with self.assertRaisesRegex(docs.Failed, "出现 2 次"):
            docs.insert("a\na\n", "a", "x", after=True)


class ReaderTests(unittest.TestCase):
    def reader(self, feed: str, secrets=None) -> docs.Reader:
        work = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, work)
        env = docs.reader_environment(work)
        settings = docs.Settings(feed, "9999.12.31", "2026.11.24", work if feed == "local" else None, work, env, secrets or {})
        reader = docs.Reader(docs.AUTHORIZATION, docs.load_pages()[docs.AUTHORIZATION.slug], settings)
        self.addCleanup(reader.close)
        return reader

    def test_local_feed_replaces_the_version_and_skips_what_needs_the_release(self) -> None:
        reader = self.reader("local")
        self.assertEqual("dotnet add package Kun.Security --version 9999.12.31",
                         reader.prepare("dotnet add package Kun.Security --version 2026.11.24"))
        self.assertIsNone(reader.prepare("git clone --depth 1 --branch v2026.11.24 https://github.com/CavaiLAB/kun.git kun-v2026.11.24"))
        self.assertIsNone(reader.prepare("curl -fsSL -o NuGet.config https://github.com/CavaiLAB/kun/releases/download/v2026.11.24/NuGet.config"))
        self.assertIsNone(reader.prepare("export KUN_PLATFORM_FEED_TOKEN=<带 read:packages 权限的令牌>"))
        self.assertEqual(f"dotnet build {ROOT}/tools/Kun.Plugin.Tool -c Release -p:Version=9999.12.31",
                         reader.prepare("dotnet build kun-v2026.11.24/tools/Kun.Plugin.Tool -c Release -p:Version=2026.11.24"))
        with self.assertRaisesRegex(docs.Failed, "占位符"):
            reader.prepare("ALICE=<alice 的令牌>")

    def test_published_feed_hands_the_token_over_without_putting_it_on_a_command_line(self) -> None:
        reader = self.reader("published", {"KUN_PLATFORM_FEED_TOKEN": "secret-value"})
        self.assertIsNotNone(reader.prepare("git clone --depth 1 --branch v2026.11.24 https://github.com/CavaiLAB/kun.git kun-v2026.11.24"))
        command = reader.prepare("export KUN_PLATFORM_FEED_TOKEN=<带 read:packages 权限的令牌>")
        assert command is not None
        self.assertNotIn("secret-value", command)
        code, output = reader.terminal.run(command, 30)
        self.assertEqual(0, code, output)
        code, output = reader.terminal.run('test "$KUN_PLATFORM_FEED_TOKEN" = secret-value && echo same', 30)
        self.assertEqual((0, "same"), (code, output.strip()))
        self.assertEqual([], list(reader.settings.work.glob(".kun_platform_feed_*")), "交接用的临时文件读完即删")
        self.assertEqual("secret-value", reader.exports["KUN_PLATFORM_FEED_TOKEN"])
        self.assertNotIn("KUN_PLATFORM_FEED_TOKEN", reader.settings.env, "读者的环境里没有令牌，只在 export 那一步交给终端")
        with self.assertRaises(docs.ToolError):
            reader.prepare("export KUN_PLATFORM_FEED_USER=<你的 GitHub 用户名>")


class TerminalTests(unittest.TestCase):
    def test_directory_and_variables_persist_and_exit_codes_come_back(self) -> None:
        folder = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, folder)
        (folder / "sub").mkdir()
        terminal = docs.Terminal(folder, dict(os.environ), folder / "logs" / "terminal.log")
        try:
            self.assertEqual(0, terminal.run("cd sub && NAME=值", 30)[0])
            code, output = terminal.run('echo "$NAME"; pwd', 30)
            self.assertEqual(0, code)
            self.assertEqual(["值", str((folder / "sub").resolve())], [line for line in output.strip().split("\n")])
            self.assertEqual(3, terminal.run("(exit 3)", 30)[0])
            self.assertEqual((0, ""), terminal.run("cat", 30), "每条命令的标准输入是 /dev/null，读不到后面的命令")
            self.assertEqual(1, terminal.run("echo 到标准错误 >&2; false", 30)[0])
        finally:
            terminal.close()
        self.assertIn("$ cd sub && NAME=值", (folder / "logs" / "terminal.log").read_text(encoding="utf-8"))


class SourceTests(unittest.TestCase):
    def settings(self, feed: str, cache: Path) -> docs.Settings:
        return docs.Settings(feed, "2026.11.24", "2026.11.24", cache.parent / "feed" if feed == "local" else None, cache.parent,
                             {"NUGET_PACKAGES": str(cache)}, {})

    def restored(self, cache: Path, identity: str, version: str, source: str) -> None:
        folder = cache / identity / version
        folder.mkdir(parents=True)
        (folder / ".nupkg.metadata").write_text(json.dumps({"version": 2, "source": source}), encoding="utf-8")

    def test_kernel_from_nuget_platform_from_github_and_local_from_the_feed(self) -> None:
        cache = Path(tempfile.mkdtemp()) / "packages"
        self.addCleanup(shutil.rmtree, cache.parent)
        self.restored(cache, "kun.mediator", "2026.11.24", docs.NUGET_ORG)
        self.restored(cache, "kun.authorization.aspnetcore", "2026.11.24", "https://nuget.pkg.github.com/CavaiLAB/index.json")
        problems, count = docs.source_problems(self.settings("published", cache), ["kun.authorization.aspnetcore"])
        self.assertEqual(([], 2), (problems, count))
        problems, _ = docs.source_problems(self.settings("published", cache), [])
        self.assertEqual(1, len(problems))
        problems, _ = docs.source_problems(self.settings("local", cache), [])
        self.assertEqual(2, len(problems))

    def test_platform_ids_come_from_the_release_package_source_mapping(self) -> None:
        folder = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, folder)
        config = folder / "NuGet.config"
        config.write_text('<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" />'
                          '<add key="kun-platform" value="https://nuget.pkg.github.com/CavaiLAB/index.json" /></packageSources>'
                          '<packageSourceMapping><packageSource key="nuget.org"><package pattern="Kun.Core" /></packageSource>'
                          '<packageSource key="kun-platform"><package pattern="Kun.Authorization.AspNetCore" /></packageSource>'
                          '</packageSourceMapping></configuration>', encoding="utf-8")
        self.assertEqual(["kun.authorization.aspnetcore"], docs.platform_ids(config))
        self.assertEqual([], docs.platform_ids(folder / "missing.config"))

    def test_nuspec_reads_identity_version_and_dependencies(self) -> None:
        folder = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, folder)
        package = folder / "Kun.Mediator.9999.12.31.nupkg"
        with zipfile.ZipFile(package, "w") as archive:
            archive.writestr("Kun.Mediator.nuspec", '<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">'
                             '<metadata><id>Kun.Mediator</id><version>9999.12.31</version><dependencies><group targetFramework="net10.0">'
                             '<dependency id="Kun.Core" version="[9999.12.31]" /></group></dependencies></metadata></package>')
        self.assertEqual(("Kun.Mediator", "9999.12.31", ["Kun.Core"]), docs.nuspec(package))


if __name__ == "__main__":
    unittest.main()
