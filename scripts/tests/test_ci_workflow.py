"""ci.yml 的结构自测（S1-KUN-020）。只依赖标准库。

运行：python3 -m unittest discover -s scripts/tests -p 'test_ci_*.py' -v
ci.yml 的变更检测作业每次都先跑它。核对的是「CI 汇总」能当必需检查的几个前提：
  - 拉取请求触发没有 paths、paths-ignore、branches 过滤；
  - 除变更检测与汇总外，每个作业都依赖变更检测、带区域条件，区域都在区域表里，区域表的每个区域都有作业用；
  - 汇总作业的 id 是 summary、名字字面是「CI 汇总」、带 if: always()、needs 正好是全部其余作业；
  - 作业名不重复，「CI 汇总」不与被调用工作流里的作业重名；
  - .github/workflows/ 下别的工作流只能由 ci.yml 调用（workflow_call，可留 workflow_dispatch），且都被调用、没有自己的并发组；
  - 调用前端与文档工作流的作业给 packages: read；被调用方有要写权限的作业时，调用方给 contents: write；
  - 区域表里「要构建 .NET」「装 GitHub Packages 上的 npm 包」两个标记与作业实际做的事一致。
"""
from __future__ import annotations

import re
import sys
import unittest
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
sys.path.insert(0, str(HERE.parent))

import ci_changes  # noqa: E402
import ci_yaml  # noqa: E402

WORKFLOWS = ci_changes.ROOT / ".github" / "workflows"
CONDITION = re.compile(r"needs\.changes\.outputs\.([A-Za-z0-9_-]+)\s*==\s*'true'")
SUMMARY_NAME = "CI 汇总"


def load(path: Path):
    return ci_yaml.load(path.read_text(encoding="utf-8"))


class CiWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.text = (WORKFLOWS / "ci.yml").read_text(encoding="utf-8")
        cls.workflow = ci_yaml.load(cls.text)
        cls.jobs = cls.workflow["jobs"]
        cls.called = {name: load(WORKFLOWS / Path(job["uses"]).name)
                      for name, job in cls.jobs.items() if isinstance(job.get("uses"), str)}

    def area_of(self, name: str) -> str:
        match = CONDITION.search(str(self.jobs[name].get("if", "")))
        self.assertIsNotNone(match, f"作业 {name} 没有 needs.changes.outputs.<区域> == 'true' 的条件")
        return match.group(1)  # type: ignore[union-attr]

    def test_triggers(self) -> None:
        triggers = self.workflow["on"]
        self.assertEqual({"push", "pull_request", "workflow_dispatch"}, set(triggers))
        self.assertEqual({"branches": ["main"]}, triggers["push"])
        pull_request = triggers["pull_request"] or {}
        for key in ("paths", "paths-ignore", "branches", "branches-ignore"):
            self.assertNotIn(key, pull_request, f"拉取请求触发不能有 {key}：只改文档的 PR 也要跑出「CI 汇总」")

    def test_every_job_depends_on_change_detection_and_has_an_area(self) -> None:
        used = set()
        for name, job in self.jobs.items():
            if name in ("changes", "summary"):
                continue
            needs = job.get("needs")
            needs = [needs] if isinstance(needs, str) else needs or []
            self.assertIn("changes", needs, f"作业 {name} 要依赖变更检测")
            area = self.area_of(name)
            self.assertIn(area, ci_changes.AREAS, f"作业 {name} 的区域 {area} 不在区域表里")
            used.add(area)
        self.assertEqual(set(ci_changes.AREAS), used, "区域表里每个区域都要有作业用")
        self.assertNotIn("needs", self.jobs["changes"])
        self.assertNotIn("if", self.jobs["changes"])

    def test_change_detection_outputs_every_area(self) -> None:
        outputs = self.jobs["changes"]["outputs"]
        self.assertEqual(list(ci_changes.AREAS), list(outputs))
        for area, value in outputs.items():
            self.assertEqual(f"${{{{ steps.detect.outputs.{area} }}}}", value)

    def test_summary_job(self) -> None:
        summary = self.jobs["summary"]
        self.assertEqual(SUMMARY_NAME, summary["name"])
        self.assertIn(summary["if"], ("always()", "${{ always() }}"))
        self.assertEqual(sorted(name for name in self.jobs if name != "summary"), sorted(summary["needs"]))
        self.assertNotIn("strategy", summary)
        self.assertIn("python3 scripts/ci_changes.py summary", str(summary["steps"]))
        self.assertIn(f"name: {SUMMARY_NAME}", self.text, "名字要写成字面值，不带表达式")

    def test_job_names_are_unique_and_the_summary_name_is_not_reused(self) -> None:
        names = [str(job.get("name", name)) for name, job in self.jobs.items()]
        self.assertEqual(len(names), len(set(names)))
        for name, document in self.called.items():
            for job_id, job in document["jobs"].items():
                self.assertNotEqual(SUMMARY_NAME, job.get("name"), f"{name} 调用的工作流里作业 {job_id} 与汇总重名")

    def test_other_workflows_are_only_called_by_ci(self) -> None:
        called_files = {Path(job["uses"]).name for job in self.jobs.values() if isinstance(job.get("uses"), str)}
        for path in sorted(WORKFLOWS.glob("*.y*ml")):
            if path.name == "ci.yml":
                continue
            document = load(path)
            self.assertIn(path.name, called_files, f"{path.name} 没有被 ci.yml 调用，它的检查进不了「CI 汇总」")
            self.assertLessEqual(set(document["on"]), {"workflow_call", "workflow_dispatch"},
                                 f"{path.name} 只能由 ci.yml 调用（或手动触发），否则会和 ci.yml 重复跑")
            self.assertIn("workflow_call", document["on"])
            self.assertNotIn("concurrency", document, f"{path.name} 的并发组由 ci.yml 统一管")

    def test_permissions_of_calling_jobs(self) -> None:
        self.assertEqual({"contents": "read"}, self.workflow["permissions"])
        for name, document in self.called.items():
            permissions = self.jobs[name].get("permissions") or {}
            self.assertEqual("read", permissions.get("packages"), f"作业 {name} 要给 packages: read")
            writes = any((job.get("permissions") or {}).get("contents") == "write" for job in document["jobs"].values())
            self.assertEqual("write" if writes else "read", permissions.get("contents"),
                             f"作业 {name} 的 contents 权限要与被调用方的需要一致")

    def test_area_flags_match_what_the_jobs_do(self) -> None:
        dotnet, npm = set(), set()
        for name, job in self.jobs.items():
            if name in ("changes", "summary"):
                continue
            area = self.area_of(name)
            text = str(self.called[name]) if name in self.called else str(job)
            if "setup-dotnet" in text:
                dotnet.add(area)
            if "npm.pkg.github.com" in text:
                npm.add(area)
        self.assertEqual({name for name, area in ci_changes.AREAS.items() if area.builds_dotnet}, dotnet)
        # 分片 b 装 doctor 客户端的 npm 包，但 dotnet 区域已含整个 _import/qian（.npmrc 在内），不另列标记。
        self.assertEqual({name for name, area in ci_changes.AREAS.items() if area.installs_npm}, npm - {"dotnet"})

    def test_dotnet_jobs_use_the_shared_sdk_action_and_ubuntu_24_04(self) -> None:
        for name, job in self.jobs.items():
            if name in self.called:
                continue
            self.assertEqual("ubuntu-24.04", job["runs-on"], f"作业 {name} 要钉 ubuntu-24.04")
            steps = job.get("steps") or []
            for step in steps:
                uses = str(step.get("uses", ""))
                self.assertFalse(uses.startswith("actions/setup-dotnet"), f"作业 {name} 要用 ./.github/actions/setup-dotnet")


if __name__ == "__main__":
    unittest.main()
