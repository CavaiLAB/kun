"""区域表 scripts/ci_changes.py 的自测（S1-KUN-020）。只依赖标准库与 git。

运行：python3 -m unittest discover -s scripts/tests -p 'test_ci_*.py' -v
ci.yml 的变更检测作业每次都先跑它。四项核对：
  1. 逐项核对任务、构建、镜像、组件与界面资产应触发和应跳过的区域；
  2. 要构建 .NET 的区域都含 .NET 基础（DOTNET_BASE），装 GitHub Packages 上 npm 包的区域都含 .npmrc；
  3. 每个被跟踪的文件至少落进一个区域，或者在明列的「无检查」名单里；
  4. 每个匹配式至少命中一个被跟踪的文件，没有死规则。
另有匹配语义、变更检测与「CI 汇总」判定的用例。
"""
from __future__ import annotations

import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path
from unittest import mock

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
sys.path.insert(0, str(HERE.parent))

import ci_changes  # noqa: E402

ROOT = ci_changes.ROOT


def tracked():
    output = subprocess.run(["git", "ls-files", "-z"], cwd=ROOT, capture_output=True, text=True, check=True).stdout
    return [name for name in output.split("\0") if name]



class MatchingTests(unittest.TestCase):
    def test_double_star_crosses_directories_and_may_be_empty(self) -> None:
        self.assertTrue(ci_changes.group_matches(["src/**"], "src/Kun.Core/Kun.Core.csproj"))
        self.assertTrue(ci_changes.group_matches(["**/*.md"], "README.md"))
        self.assertTrue(ci_changes.group_matches(["**/*.md"], "tests/README.md"))
        self.assertFalse(ci_changes.group_matches(["src/**"], "srcx/a.cs"))

    def test_single_star_stays_in_one_directory(self) -> None:
        self.assertTrue(ci_changes.group_matches(["fixtures/platform/*.props"], "fixtures/platform/Directory.Build.props"))
        self.assertFalse(ci_changes.group_matches(["fixtures/platform/*.props"], "fixtures/platform/tests/Directory.Build.props"))
        self.assertTrue(ci_changes.group_matches(["fixtures/platform/packages/Example.*/**"], "fixtures/platform/packages/Example.Data/a.cs"))

    def test_last_matching_pattern_wins_within_a_group(self) -> None:
        group = ["fixtures/platform/**", "!fixtures/platform/docs/**", "fixtures/platform/docs/generated/**"]
        self.assertTrue(ci_changes.group_matches(group, "fixtures/platform/scripts/pm.py"))
        self.assertFalse(ci_changes.group_matches(group, "fixtures/platform/docs/prd/01.md"))
        self.assertTrue(ci_changes.group_matches(group, "fixtures/platform/docs/generated/events.md"))

    def test_groups_are_evaluated_separately(self) -> None:
        area = ci_changes.Area("样例", (("src/**", "!**/*.md"), ("src/Kun.Core/README.md",)))
        self.assertTrue(ci_changes.area_matches(area, "src/Kun.Core/README.md"))
        self.assertFalse(ci_changes.area_matches(area, "src/Kun.Mediator/README.md"))

    def test_everything_files_mark_every_area(self) -> None:
        hits = ci_changes.changed_areas([".github/workflows/ci.yml"])
        self.assertEqual(set(ci_changes.AREAS), {name for name, files in hits.items() if files})
        hits = ci_changes.changed_areas(["README.md"])
        self.assertEqual({"dotnet", "docs-and-pm", "docs-site"}, {name for name, files in hits.items() if files})


class AreaTableTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.files = tracked()

    def test_import_directory_has_no_tracked_files(self) -> None:
        remaining = [path for path in self.files if path == "_import" or path.startswith("_import/")]
        self.assertEqual([], remaining)

    def test_workflow_areas_cover_their_inputs(self) -> None:
        cases = {
            "pm/tasks/S1-KUN-030.md": ({"docs-and-pm"}, {"dotnet", "docs-site"}),
            "docs/prd/specs/frontend.md": ({"docs-and-pm", "docs-site"}, {"dotnet", "images"}),
            "docs/generated/event-graph.md": ({"dotnet", "docs-and-pm", "docs-site", "documentation"}, {"images"}),
            "deploy/pm-board/builder.py": ({"dotnet", "docs-and-pm"}, {"images", "docs-site"}),
            "deploy/images/jit.Dockerfile": ({"dotnet", "images"}, {"shell-auth"}),
            "apps/qian-shell/package.json": ({"shell-auth", "shell-federation", "ui"}, {"images"}),
            "packages/qian-surfaces/package.json": ({"surfaces", "documentation"}, {"shell-federation"}),
            "tests/directory-e2e/package.json": ({"directory-e2e", "ui"}, {"tenant-ui"}),
            "tck/runtime/harness-ts.mjs": ({"documentation", "surfaces"}, {"shell-auth"}),
            "tools/openapi-codegen/package.json": ({"directory-e2e", "tenant-ui", "ui"}, {"images"}),
            "samples/qian-sample-crm-ui/package.json": ({"ui", "shell-auth", "dev-tenant-ops"}, {"images"}),
            "plugins/qian.shell/.qian-plugin/plugin.json": ({"shell-federation", "documentation", "surfaces"}, {"ui"}),
            ".npmrc": ({"docs-site", "documentation", "directory-e2e", "shell-auth", "shell-federation", "surfaces", "tenant-ui", "ui"}, {"images"}),
            # 文档的相对链接指向代码目录下的说明、源码、测试与脚本：这些文件重命名或删除，文档门禁要跑。
            "src/Kun.Capabilities/README.md": ({"dotnet", "docs-and-pm"}, set()),
            "hosts/Kun.Platform.Host/README.md": ({"docs-and-pm"}, {"images"}),
            "tests/Kun.Sample.Crm.Tests/CustomerCommandTests.cs": ({"dotnet", "docs-and-pm"}, {"docs-site", "images"}),
            "tools/Kun.Configuration.Secrets.Tool/README.md": ({"docs-and-pm"}, {"docs-site"}),
            "samples/Kun.Sample.Catalog/README.md": ({"docs-and-pm"}, {"images"}),
            "scripts/check_deps.py": ({"dotnet", "docs-and-pm"}, {"docs-site"}),
            # 使用文档的示例（S1-DOCS-012）：三篇页面、示例、脚本、示例装的包的源码与清单工具都跑它；别的页面、前端与任务文件不跑。
            "website/docs/start/getting-started.md": ({"docs-examples", "docs-site", "docs-and-pm"}, {"images"}),
            "website/docs/start/guides/authorization-dotnet.md": ({"docs-examples", "docs-site"}, {"images"}),
            "samples/docs/write-a-component/notes-demo/Acme.Notes/NotesPlugin.cs": ({"docs-examples", "dotnet", "docs-and-pm"}, {"docs-site"}),
            "samples/docs/README.md": ({"docs-examples", "docs-and-pm"}, {"docs-site"}),
            "scripts/docs_examples.py": ({"docs-examples", "dotnet", "docs-and-pm"}, {"docs-site"}),
            "src/Kun.Mediator/Kun.Mediator.csproj": ({"docs-examples", "dotnet"}, set()),
            "tools/Kun.Plugin.Tool/Kun.Plugin.Tool.csproj": ({"docs-examples", "dotnet"}, {"docs-site"}),
            "samples/Kun.Sample.Crm/metadata/roles.json": ({"docs-examples", "dotnet"}, {"docs-site"}),
            "scripts/release_finish.py": ({"docs-examples", "dotnet"}, {"docs-site"}),
            "scripts/release_feed.py": ({"docs-examples", "dotnet"}, {"docs-site"}),
            "website/docs/start/index.md": ({"docs-site"}, {"docs-examples"}),
            "packages/qian-cli/package.json": ({"surfaces"}, {"docs-examples"}),
            "pm/tasks/S1-DOCS-010.md": ({"docs-and-pm"}, {"docs-examples"}),
        }
        for path, (required, excluded) in cases.items():
            with self.subTest(path=path):
                self.assertIn(path, self.files)
                actual = {name for name, files in ci_changes.changed_areas([path]).items() if files}
                self.assertFalse(required - actual, required - actual)
                self.assertFalse(excluded & actual, excluded & actual)

    def test_2_dotnet_areas_include_the_dotnet_base_and_npm_areas_include_npmrc(self) -> None:
        problems = []
        for name, area in ci_changes.AREAS.items():
            if area.builds_dotnet and ci_changes.DOTNET_BASE not in area.groups:
                problems.append(f"区域 {name} 要构建 .NET，却没有 DOTNET_BASE 这一组")
            if area.installs_npm and not any(pattern in ci_changes.NPMRC for group in area.groups for pattern in group):
                problems.append(f"区域 {name} 装 GitHub Packages 上的包，却不含 {ci_changes.NPMRC[0]}")
        # 仓库根上的构建基线以后加文件（如 Directory.Build.targets），也要进 .NET 基础。
        for path in self.files:
            if "/" not in path and path.startswith("Directory.") and not ci_changes.group_matches(ci_changes.DOTNET_BASE, path):
                problems.append(f"仓库根的 {path} 不在 DOTNET_BASE 里")
        self.assertEqual([], problems)

    def test_3_every_tracked_file_has_an_area_or_is_listed_as_unchecked(self) -> None:
        orphans = [path for path in self.files
                   if not ci_changes.everything(path)
                   and not any(ci_changes.area_matches(area, path) for area in ci_changes.AREAS.values())
                   and not ci_changes.group_matches(ci_changes.NO_CHECK, path)]
        self.assertEqual([], orphans, "这些文件改了不触发任何检查：加进合适的区域，或者明列进 NO_CHECK。")

    def test_4_every_pattern_matches_some_tracked_file(self) -> None:
        patterns = {pattern for area in ci_changes.AREAS.values() for group in area.groups for pattern in group}
        patterns |= set(ci_changes.EVERYTHING) | set(ci_changes.NO_CHECK)
        dead = sorted(pattern for pattern in patterns
                      if not any(ci_changes.glob_regex(pattern.lstrip("!")).match(path) for path in self.files))
        self.assertEqual([], dead, "这些匹配式一个被跟踪的文件都对不上：文件挪走或删掉了，区域表跟着改。")


class DetectTests(unittest.TestCase):
    def run_detect(self, *args: str):
        with tempfile.TemporaryDirectory() as directory:
            output, summary = Path(directory, "output"), Path(directory, "summary")
            with mock.patch.dict(os.environ, {"GITHUB_OUTPUT": str(output), "GITHUB_STEP_SUMMARY": str(summary)}), \
                    mock.patch("sys.stdout"):
                code = ci_changes.main(["detect", *args])
            values = dict(line.split("=", 1) for line in output.read_text(encoding="utf-8").splitlines())
            return code, values, summary.read_text(encoding="utf-8")

    def test_moved_gateway_host_triggers_image_checks(self) -> None:
        with mock.patch.object(ci_changes, "diff", return_value=["hosts/Kun.Gateway.Host/Dockerfile.acceptance"]):
            code, values, _ = self.run_detect("--base", "a", "--head", "b")
        self.assertEqual(0, code)
        self.assertEqual("true", values["images"])
        self.assertEqual("true", values["dotnet"])

    def test_renamed_or_deleted_code_files_run_the_documentation_gate(self) -> None:
        # diff 用 --no-renames：重命名是旧路径加新路径，删除只有旧路径。文档里的相对链接可能正指向它们。
        cases = (
            ["src/Kun.Capabilities/CapabilityService.cs", "src/Kun.Capabilities/Catalog/CapabilityService.cs"],
            ["hosts/Kun.Platform.Host/README.md"],
            ["tools/Kun.Configuration.Secrets.Tool/README.md", "tools/Kun.Configuration.Secrets.Tool/docs/README.md"],
        )
        for paths in cases:
            with self.subTest(paths=paths):
                with mock.patch.object(ci_changes, "diff", return_value=paths):
                    code, values, _ = self.run_detect("--base", "a", "--head", "b")
                self.assertEqual(0, code)
                self.assertEqual("true", values["docs-and-pm"])

    def test_all_marks_every_area(self) -> None:
        code, values, summary = self.run_detect("--all")
        self.assertEqual(0, code)
        self.assertEqual({name: "true" for name in ci_changes.AREAS}, values)
        self.assertIn("全部区域都跑", summary)

    def test_diff_selects_areas_by_changed_files(self) -> None:
        with mock.patch.object(ci_changes, "diff", return_value=["pm/tasks/S1-KUN-020.md", "CONTRIBUTING.md"]):
            code, values, summary = self.run_detect("--base", "a", "--head", "b")
        self.assertEqual(0, code)
        self.assertEqual({"docs-and-pm"}, {name for name, value in values.items() if value == "true"})
        self.assertIn("CONTRIBUTING.md", summary)  # 不落进任何区域的改动列在摘要里


class SummaryTests(unittest.TestCase):
    def needs(self, **results: str):
        data = {"changes": {"result": "success", "outputs": {name: "false" for name in ci_changes.AREAS}}}
        data.update({job: {"result": result, "outputs": {}} for job, result in results.items()})
        return data

    def test_skipped_jobs_pass(self) -> None:
        _, problems = ci_changes.summarize(self.needs(**{"dotnet-build": "skipped", "ui": "skipped"}))
        self.assertEqual([], problems)

    def test_failed_or_cancelled_jobs_fail(self) -> None:
        _, problems = ci_changes.summarize(self.needs(**{"dotnet-build": "failure", "ui": "cancelled", "docs-site": "success"}))
        self.assertEqual(2, len(problems))

    def test_failed_change_detection_fails_even_if_everything_else_was_skipped(self) -> None:
        needs = self.needs(**{"dotnet-build": "skipped"})
        needs["changes"]["result"] = "failure"
        _, problems = ci_changes.summarize(needs)
        self.assertEqual(1, len(problems))
        self.assertIn("变更检测没有成功", problems[0])

    def test_summary_command_reads_needs_from_the_environment(self) -> None:
        payload = json.dumps(self.needs(**{"dotnet-build": "failure"}))
        with mock.patch.dict(os.environ, {"NEEDS": payload}, clear=False), mock.patch("sys.stdout"):
            os.environ.pop("GITHUB_STEP_SUMMARY", None)
            self.assertEqual(1, ci_changes.main(["summary"]))


if __name__ == "__main__":
    unittest.main()
