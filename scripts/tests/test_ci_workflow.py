"""ci.yml 的结构自测（S1-KUN-020）。只依赖标准库。

运行：python3 -m unittest discover -s scripts/tests -p 'test_ci_*.py' -v
ci.yml 的变更检测作业每次都先跑它。核对的是「CI 汇总」能当必需检查的几个前提：
  - 拉取请求触发没有 paths、paths-ignore、branches 过滤；
  - 除变更检测与汇总外，每个作业都依赖变更检测、带区域条件，区域都在区域表里，区域表的每个区域都有作业用；
  - 汇总作业的 id 是 summary、名字字面是「CI 汇总」、带 if: always()、needs 正好是全部其余作业；
  - 作业名不重复，「CI 汇总」不与被调用工作流里的作业重名；
  - release.yml 只手动运行。其余检查工作流只能由 ci.yml 调用（workflow_call，可留 workflow_dispatch），且都被调用、没有自己的并发组；
  - 调用前端与文档工作流的作业给 packages: read；被调用方有要写权限的作业时，调用方给 contents: write；
  - 区域表里「要构建 .NET」「装 GitHub Packages 上的 npm 包」两个标记与作业实际做的事一致；
  - 测试结果构件名带尝试号，测试分片汇总核对按尝试号取每个分片最新的一份：「重跑失败的作业」之后汇总能按重跑的结果变绿；
  - 分片作业在后台预拉镜像之前，先在前台列出本分片的镜像，列不出这一步就失败；门禁作业核对每个测试项目都有着落；
  - 使用文档的示例：ci.yml 里先核对页面再打本地包、照页面跑，版本与包源前后一致；release.yml 在发布之后用刚发布的版本
    照页面跑，排在最后、不挡发布，只读权限，读包令牌照 publish 的做法取（S1-DOCS-012）。
"""
from __future__ import annotations

import fnmatch
import re
import shlex
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

    def test_release_is_manual_and_independent_of_ci(self) -> None:
        release = load(WORKFLOWS / "release.yml")
        self.assertEqual({"workflow_dispatch"}, set(release["on"]))
        self.assertEqual("github.ref == 'refs/heads/main'", release["jobs"]["prepare"]["if"])
        self.assertEqual("release", release["jobs"]["publish"]["environment"])
        self.assertFalse(any(job.get("uses") == "./.github/workflows/release.yml" for job in self.jobs.values()))

    def test_check_workflows_are_only_called_by_ci(self) -> None:
        called_files = {Path(job["uses"]).name for job in self.jobs.values() if isinstance(job.get("uses"), str)}
        for path in sorted(WORKFLOWS.glob("*.y*ml")):
            if path.name in {"ci.yml", "release.yml"}:
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
        # 分片 b 为 doctor 测试安装 npm 客户端，其余安装依赖的作业由 installs_npm 登记。
        self.assertEqual({name for name, area in ci_changes.AREAS.items() if area.installs_npm}, npm - {"dotnet"})

    def test_test_results_are_named_per_attempt_and_the_summary_takes_the_latest(self) -> None:
        # 「重跑失败的作业」之后同名构件每次尝试各有一份。download-artifact（按模式、按名字、按 ID 都一样）对同名构件
        # 只留 ID 最大的一份，而重跑后的 ID 不一定更大：汇总会取到上一次尝试的结果，重跑过了还是红（CavaiLAB/kun#38 实测）。
        suffix = "-attempt-${{ github.run_attempt }}"
        uploads = [step for job in self.jobs.values() for step in job.get("steps") or []
                   if str(step.get("uses", "")).startswith("actions/upload-artifact")]
        self.assertEqual([], [step["uses"] for step in uploads if not str(step["uses"]).startswith("actions/upload-artifact@")],
                         "合并构件的子动作按名字合并几次尝试的构件，同名文件互相覆盖，不用它")
        names = [str(step["with"]["name"]) for step in uploads]
        results = [name for name in names if name.startswith("test-results-")]
        self.assertGreaterEqual(len(results), 2, "分片与门禁作业都上传测试结果")
        for name in results:
            self.assertTrue(name.endswith(suffix), f"测试结果构件 {name} 的名字要以 {suffix} 结尾")

        steps = self.jobs["dotnet-test-summary"]["steps"]
        downloads = [step["with"] for step in steps if str(step.get("uses", "")).startswith("actions/download-artifact")]
        self.assertEqual([{"pattern": "test-results-*-attempt-*", "path": "${{ runner.temp }}/test-results-attempts"}], downloads,
                         "按模式下载全部尝试，每个构件一个子目录（不按名字、不合并）")
        for name in results:
            sample = name.replace("${{ matrix.shard }}", "a").replace("${{ github.run_attempt }}", "2")
            self.assertTrue(fnmatch.fnmatchcase(sample, downloads[0]["pattern"]), f"{name} 没被汇总下载")
        runs = [str(step.get("run", "")) for step in steps]
        latest = [index for index, run in enumerate(runs)
                  if 'dotnet_ci.py latest-attempts "$RUNNER_TEMP/test-results-attempts" "$RUNNER_TEMP/test-results"' in run]
        verify = [index for index, run in enumerate(runs) if 'dotnet_ci.py verify "$RUNNER_TEMP/test-results"' in run]
        self.assertEqual(1, len(latest), "汇总要先按尝试号取每个分片最新的一份")
        self.assertEqual(1, len(verify))
        self.assertLess(latest[0], verify[0], "先取最新一次尝试，再核对")

        merged = [step["with"] for step in steps if str(step.get("uses", "")).startswith("actions/upload-artifact")]
        self.assertEqual([("test-results", "${{ runner.temp }}/test-results/")], [(item["name"], item["path"]) for item in merged],
                         "汇总上传的是取好的那份")
        self.assertFalse(fnmatch.fnmatchcase("test-results", downloads[0]["pattern"]), "汇总自己上传的构件不能被下一次尝试当成分片结果下载")

    def test_package_check_build_and_pack_use_one_version(self) -> None:
        job = self.jobs["dotnet-build"]
        self.assertEqual("9999.12.31", job["env"]["KUN_PACK_CHECK_VERSION"])
        commands = [step.get("run", "") for step in job["steps"]]
        for command in ("dotnet build Kun.slnx", "dotnet pack src/Kun.Gateway", "dotnet pack Kun.slnx"):
            line, = (line for script in commands for line in script.splitlines() if line.startswith(command))
            self.assertIn('-p:Version="$KUN_PACK_CHECK_VERSION"', line)

    def test_image_evidence_upload_uses_the_selfcheck_output(self) -> None:
        steps = self.jobs["images-selfcheck"]["steps"]
        command = next(step["run"] for step in steps if "images.py selfcheck " in step.get("run", ""))
        arguments = shlex.split(command)
        output = arguments[arguments.index("--out") + 1]
        upload = next(step["with"] for step in steps if step.get("with", {}).get("name") == "image-selfcheck")
        self.assertEqual("$GITHUB_WORKSPACE/" + upload["path"].rstrip("/"), output)

    def test_shards_list_their_images_in_the_foreground_before_the_background_prefetch(self) -> None:
        # 夹具搬走、镜像变量改名、扫描器漏看了引用夹具的项目时，dotnet_ci.py images 退出码 2。它要在前台跑、退出码不被吞掉，
        # 分片作业才会失败；后台的 prefetch 只记录拉取结果，网络失败不拦，由 Testcontainers 补拉。
        steps = [step for step in self.jobs["dotnet-test"]["steps"] if "dotnet_ci.py prefetch" in str(step.get("run", ""))]
        self.assertEqual(1, len(steps), "分片作业要有一步后台预拉镜像")
        step = steps[0]
        self.assertNotIn("continue-on-error", step)
        lines = [line.strip() for line in str(step["run"]).splitlines()]
        listing = [index for index, line in enumerate(lines) if "dotnet_ci.py images ${{ matrix.shard }}" in line]
        prefetch = [index for index, line in enumerate(lines) if "dotnet_ci.py prefetch ${{ matrix.shard }}" in line]
        self.assertEqual(1, len(listing), "后台预拉之前先在前台列出本分片的镜像")
        self.assertLess(listing[0], prefetch[0])
        self.assertTrue(lines[listing[0]].startswith("python3 "), lines[listing[0]])
        # 不放后台（&）、不接管道（缺省的 bash -e 不带 pipefail）、不接 && 或 ||（-e 不管它们左边的命令）。
        self.assertNotRegex(lines[listing[0]], r"[|&]|\b(?:nohup|setsid)\b|\\$")

    def test_the_gates_job_checks_that_every_test_project_is_run_somewhere(self) -> None:
        steps = [step for step in self.jobs["dotnet-build"]["steps"] if "dotnet_ci.py registration" in str(step.get("run", ""))]
        self.assertEqual([(None, "python3 scripts/dotnet_ci.py registration")],
                         [(step.get("working-directory"), str(step["run"]).strip()) for step in steps],
                         "门禁作业要核对每个测试项目都有着落")
        self.assertFalse({"if", "continue-on-error"} & set(steps[0]), "这一步不能有条件，失败不能放过")

    def test_platform_shards_restore_build_and_test_the_same_filter(self) -> None:
        steps = self.jobs["dotnet-test"]["steps"]
        filtered = [step for step in steps if step.get("if") == "matrix.shard != 'k'"]
        commands = [str(step.get("run", "")) for step in filtered]
        generate = next(i for i, script in enumerate(commands) if "dotnet_ci.py shard" in script)
        build = next(i for i, script in enumerate(commands) if "dotnet build" in script)
        test = next(i for i, script in enumerate(commands) if "dotnet test" in script)
        self.assertLess(generate, build)
        self.assertLess(build, test)
        for i in (generate, build, test):
            self.assertNotIn("working-directory", filtered[i])
            self.assertIn('"$RUNNER_TEMP/tests.slnf"', commands[i])
        self.assertIn('dotnet restore "$RUNNER_TEMP/tests.slnf"', commands[generate])
        self.assertIn("-p:KunTrxPerModule=true", commands[test])
        self.assertEqual({"a", "b", "c", "k"}, {entry["shard"] for entry in self.jobs["dotnet-test"]["strategy"]["matrix"]["include"]})

    def test_dotnet_jobs_use_the_shared_sdk_action_and_ubuntu_24_04(self) -> None:
        for name, job in self.jobs.items():
            if name in self.called:
                continue
            self.assertEqual("ubuntu-24.04", job["runs-on"], f"作业 {name} 要钉 ubuntu-24.04")
            steps = job.get("steps") or []
            for step in steps:
                uses = str(step.get("uses", ""))
                self.assertFalse(uses.startswith("actions/setup-dotnet"), f"作业 {name} 要用 ./.github/actions/setup-dotnet")

    def test_docs_examples_check_the_pages_before_packing_and_running_them(self) -> None:
        job = self.jobs["docs-examples"]
        version = job["env"]["KUN_DOCS_EXAMPLES_VERSION"]
        self.assertEqual(self.jobs["dotnet-build"]["env"]["KUN_PACK_CHECK_VERSION"], version, "与打包核对用同一个占位版本")
        commands = [str(step.get("run", "")) for step in job["steps"]]
        check = next(index for index, run in enumerate(commands) if "docs_examples.py check" in run)
        feed = next(index for index, run in enumerate(commands) if "docs_examples.py feed" in run)
        run = next(index for index, script in enumerate(commands) if "docs_examples.py run" in script)
        self.assertIn("test_docs_examples.py", commands[check], "先跑示例脚本的自测")
        self.assertLess(check, feed, "页面与示例对不上时不用打包")
        self.assertLess(feed, run)
        feeding, running = shlex.split(commands[feed]), shlex.split(commands[run])
        self.assertEqual(feeding[feeding.index("--out") + 1], running[running.index("--packages") + 1], "照页面跑用的就是刚打的本地包源")
        self.assertEqual("local", running[running.index("--feed") + 1])
        for words in (feeding, running):
            self.assertEqual("$KUN_DOCS_EXAMPLES_VERSION", words[words.index("--version") + 1])
        self.assertTrue(running[running.index("--work") + 1].startswith("$RUNNER_TEMP/"), "工作目录在仓库外")
        for step in job["steps"][:run + 1]:
            self.assertFalse({"if", "continue-on-error"} & set(step), "核对与运行的步骤不能有条件，失败不能放过")

    def test_release_runs_the_docs_examples_after_publishing_without_holding_it_up(self) -> None:
        release = load(WORKFLOWS / "release.yml")
        jobs = release["jobs"]
        job = jobs["docs-examples"]
        self.assertEqual(["prepare", "publish"], sorted(job["needs"]))
        for condition in ("github.ref == 'refs/heads/main'", "!inputs.dry_run", "needs.prepare.outputs.kind != 'beta'"):
            self.assertIn(condition, job["if"])
        self.assertNotRegex(job["if"], r"always\(\)|failure\(\)|cancelled\(\)", "发布成功了才跑")
        others = [name for name, other in jobs.items() if name != "docs-examples"]
        self.assertFalse([name for name in others if "docs-examples" in str(jobs[name].get("needs", ""))],
                         "没有作业依赖它：它失败不挡发布")
        self.assertEqual({"contents": "read", "packages": "read"}, job["permissions"])
        self.assertNotIn("environment", job, "不再走一次发布审批")
        self.assertNotIn("continue-on-error", job, "失败要在运行里看得见")
        checkout = job["steps"][0]
        self.assertTrue(str(checkout["uses"]).startswith("actions/checkout@"))
        self.assertEqual("${{ needs.prepare.outputs.sha }}", checkout["with"]["ref"], "核对的是发布的那个提交")
        step = next(step for step in job["steps"] if "docs_examples.py run" in str(step.get("run", "")))
        words = shlex.split(step["run"])
        self.assertEqual(("published", "$VERSION"), (words[words.index("--feed") + 1], words[words.index("--version") + 1]))
        publish = jobs["publish"]["env"]
        for name in ("KUN_PLATFORM_FEED_USER", "KUN_PLATFORM_FEED_TOKEN"):
            self.assertEqual(publish[name], step["env"][name], "读包令牌照 publish 的做法取")
        self.assertNotIn("secrets.", str(job), "不用另外的密钥")
        self.assertLess(list(jobs).index("publish"), list(jobs).index("docs-examples"))


if __name__ == "__main__":
    unittest.main()
