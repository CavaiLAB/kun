"""区域表 scripts/ci_changes.py 的自测（S1-KUN-020）。只依赖标准库与 git。

运行：python3 -m unittest discover -s scripts/tests -p 'test_ci_*.py' -v
ci.yml 的变更检测作业每次都先跑它。四项核对：
  1. 搬来的过滤与来源等价：每个区域在被跟踪文件上覆盖它来源工作流（_import/qian/.github/workflows/ 下）的 pull_request 过滤，
     逐条加 _import/qian/ 前缀后算；
  2. 要构建 .NET 的区域都含 .NET 基础（DOTNET_BASE），装 GitHub Packages 上 npm 包的区域都含 _import/qian/.npmrc；
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
import ci_yaml  # noqa: E402

ROOT = ci_changes.ROOT
ORIGINALS = ROOT / "_import" / "qian" / ".github" / "workflows"


def tracked():
    output = subprocess.run(["git", "ls-files", "-z"], cwd=ROOT, capture_output=True, text=True, check=True).stdout
    return [name for name in output.split("\0") if name]


def prefixed(patterns):
    return [("!" + ci_changes.Q + pattern[1:]) if pattern.startswith("!") else ci_changes.Q + pattern for pattern in patterns]


class MatchingTests(unittest.TestCase):
    def test_double_star_crosses_directories_and_may_be_empty(self) -> None:
        self.assertTrue(ci_changes.group_matches(["src/**"], "src/Kun.Core/Kun.Core.csproj"))
        self.assertTrue(ci_changes.group_matches(["**/*.md"], "README.md"))
        self.assertTrue(ci_changes.group_matches(["**/*.md"], "tests/README.md"))
        self.assertFalse(ci_changes.group_matches(["src/**"], "srcx/a.cs"))

    def test_single_star_stays_in_one_directory(self) -> None:
        self.assertTrue(ci_changes.group_matches(["_import/qian/*.props"], "_import/qian/Directory.Build.props"))
        self.assertFalse(ci_changes.group_matches(["_import/qian/*.props"], "_import/qian/tests/Directory.Build.props"))
        self.assertTrue(ci_changes.group_matches(["_import/qian/packages/Qian.*/**"], "_import/qian/packages/Qian.Data/a.cs"))

    def test_last_matching_pattern_wins_within_a_group(self) -> None:
        group = ["_import/qian/**", "!_import/qian/docs/**", "_import/qian/docs/generated/**"]
        self.assertTrue(ci_changes.group_matches(group, "_import/qian/scripts/pm.py"))
        self.assertFalse(ci_changes.group_matches(group, "_import/qian/docs/prd/01.md"))
        self.assertTrue(ci_changes.group_matches(group, "_import/qian/docs/generated/events.md"))

    def test_groups_are_evaluated_separately(self) -> None:
        area = ci_changes.Area("样例", (("src/**", "!**/*.md"), ("src/Kun.Core/README.md",)))
        self.assertTrue(ci_changes.area_matches(area, "src/Kun.Core/README.md"))
        self.assertFalse(ci_changes.area_matches(area, "src/Kun.Mediator/README.md"))

    def test_docs_deployment_source_runs_rendering_checks(self) -> None:
        changed = ci_changes.changed_areas(["_import/qian/deploy/docs-site/kustomization.yaml"])
        selected = {area for area, paths in changed.items() if paths}
        self.assertTrue({"docs-and-pm", "docs-site", "dotnet"} <= selected)

    def test_everything_files_mark_every_area(self) -> None:
        hits = ci_changes.changed_areas([".github/workflows/ci.yml"])
        self.assertEqual(set(ci_changes.AREAS), {name for name, files in hits.items() if files})
        hits = ci_changes.changed_areas(["README.md"])
        self.assertEqual({"dotnet", "docs-site"}, {name for name, files in hits.items() if files})


class AreaTableTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.files = tracked()

    def test_1_each_area_covers_the_filter_it_was_moved_from(self) -> None:
        def original_path(path):
            if any(ci_changes.group_matches(group, path) for group in (ci_changes.PLATFORM_ASSETS, ci_changes.PLATFORM_DOCUMENTATION)):
                return ci_changes.Q + path
            return path

        problems = []
        for name, area in ci_changes.AREAS.items():
            if area.original is None:
                continue
            document = ci_yaml.load((ORIGINALS / area.original).read_text(encoding="utf-8"))
            original = ci_yaml.pull_request_paths(document)
            patterns = prefixed(original if original is not None else ["**"])
            missed = [path for path in self.files if ci_changes.group_matches(patterns, original_path(path)) and not ci_changes.area_matches(area, path)]
            if missed:
                problems.append(f"区域 {name} 漏了 {area.original} 的过滤覆盖的 {len(missed)} 个文件，如 {'、'.join(missed[:3])}")
        self.assertEqual([], problems)

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

    def test_all_marks_every_area(self) -> None:
        code, values, summary = self.run_detect("--all")
        self.assertEqual(0, code)
        self.assertEqual({name: "true" for name in ci_changes.AREAS}, values)
        self.assertIn("全部区域都跑", summary)

    def test_diff_selects_areas_by_changed_files(self) -> None:
        with mock.patch.object(ci_changes, "diff", return_value=["_import/qian/pm/tasks/S1-KUN-020.md", "CONTRIBUTING.md"]):
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
