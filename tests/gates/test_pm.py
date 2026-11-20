"""任务看板 scripts/pm.py 生成的链接的自测（S1-KUN-019，迁移执行计划 B2）。

运行：python3 -m unittest discover -s tests/gates -v
看板上任务文件与 refs 的链接指向 CavaiLAB/kun 的 main，按 pm.py 所在工作区在检出中的相对位置生成，仓库根使用空前缀。
前缀由 pm.py 从自己的位置往上找第一个有 .git 的目录得出。这里在临时目录里搭出几种检出逐个核对：普通克隆（.git 是目录）、
看板 Pod 里 git-sync 的检出（/git/.worktrees/<提交>，.git 是文件，上面还有 /git/.git 目录）、pm.py 就在仓库根、不在 git 检出里；
再在 git-sync 的布局与仓库根的布局里各生成一次看板，核对 tasks.json 与看板页里的地址。只依赖标准库。

不读仓库里 pm/ 下的文件：tests/gates 由仓库根 ci.yml 的 dotnet 区域跑，只改 pm/ 的 PR 不跑这个区域，读了会被它的
触发过滤自检（scripts/dotnet_ci.py paths-check）拦下。所以 pm.py 的路径常量在用例里都换成临时目录。
"""
from __future__ import annotations

import contextlib
import importlib.util
import io
import json
import shutil
import tempfile
import unittest
from pathlib import Path
from unittest import mock

ROOT = Path(__file__).resolve().parents[2]
_spec = importlib.util.spec_from_file_location("pm", ROOT / "scripts" / "pm.py")
assert _spec and _spec.loader
pm = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(pm)

BLOB = "https://github.com/CavaiLAB/kun/blob/main/"
REV = "a" * 40

TASK = """\
---
id: S1-KUN-001
title: 示例任务
status: todo
stage: S1
area: KUN
wave: 1
priority: P1
type: doc
owner: ""
estimate: 1d
depends: []
refs:
  - docs/a.md#3.1
updated: 2026-11-18
---

## 背景

示例。

## 验收

- [ ] 示例

## 进度日志

- 2026-11-18 claude：创建任务
"""


def temporary_directory(case: unittest.TestCase) -> Path:
    # macOS 的临时目录经过 /var → /private/var 链接，pm.py 的 ROOT 是 resolve() 过的
    path = Path(tempfile.mkdtemp(prefix="qian-pm-links-")).resolve()
    case.addCleanup(shutil.rmtree, path, True)
    return path


def git_sync_checkout(git: Path) -> Path:
    """搭出 git-sync 的布局：/git/.git 目录，检出在 /git/.worktrees/<提交>，其中的 .git 是指回 /git/.git 的文件。"""
    (git / ".git").mkdir(parents=True)
    checkout = git / ".worktrees" / REV
    checkout.mkdir(parents=True)
    (checkout / ".git").write_text(f"gitdir: ../../.git/worktrees/{REV}\n", encoding="utf-8")
    return checkout


class RepoPrefixTests(unittest.TestCase):
    def setUp(self) -> None:
        self.tmp = temporary_directory(self)

    def test_plain_clone(self) -> None:
        """普通克隆：.git 是目录，前缀是 pm.py 的上一级相对仓库根的目录。"""
        (self.tmp / ".git").mkdir()
        root = self.tmp / "_import" / "qian"
        root.mkdir(parents=True)
        self.assertEqual(pm.repo_prefix(root), "_import/qian")

    def test_git_sync_worktree_stops_at_checkout(self) -> None:
        """看板 Pod 的检出：停在 worktree 里的 .git 文件这一层，不算到上面的 /git/.git 去。"""
        root = git_sync_checkout(self.tmp / "git") / "_import" / "qian"
        root.mkdir(parents=True)
        self.assertEqual(pm.repo_prefix(root), "_import/qian")

    def test_repository_root(self) -> None:
        """pm/ 与 scripts/pm.py 提到仓库根之后前缀为空。"""
        (self.tmp / ".git").mkdir()
        self.assertEqual(pm.repo_prefix(self.tmp), "")

    def test_outside_git_checkout(self) -> None:
        """不在 git 检出里时把 pm.py 的上一级当作仓库根。"""
        if any((directory / ".git").exists() for directory in self.tmp.parents):
            self.skipTest("临时目录在某个 git 检出里，搭不出「不在检出里」的情形")
        root = self.tmp / "_import" / "qian"
        root.mkdir(parents=True)
        self.assertEqual(pm.repo_prefix(root), "")

    def test_blob_url(self) -> None:
        """地址是 main 上的文件：拼上前缀，去掉 # 之后的小节，./ 这类写法规整掉。"""
        self.assertEqual(pm.blob_url("_import/qian", "docs/a.md#3.1"), BLOB + "_import/qian/docs/a.md")
        self.assertEqual(pm.blob_url("_import/qian", "./docs/a.md"), BLOB + "_import/qian/docs/a.md")
        self.assertEqual(pm.blob_url("", "pm/tasks/S1-KUN-001.md"), BLOB + "pm/tasks/S1-KUN-001.md")


class BoardLinkTests(unittest.TestCase):
    """在临时检出里生成一次看板（validate → export → 看板页、tasks.json、STATUS.md），核对链接。"""

    def setUp(self) -> None:
        self.tmp = temporary_directory(self)

    def build(self, root: Path) -> tuple[list[dict], str]:
        (root / "pm" / "tasks").mkdir(parents=True)
        (root / "docs").mkdir()
        (root / "docs" / "a.md").write_text("# a\n", encoding="utf-8")
        (root / "pm" / "tasks" / "S1-KUN-001.md").write_text(TASK, encoding="utf-8")
        (root / "pm" / "_template.html").write_text("<script>const TASKS = /*__TASKS_JSON__*/[];</script>\n", encoding="utf-8")
        out = self.tmp / "site"
        # 名字照字面写：触发过滤自检按字面认出被换掉的模块级名字，写成循环就认不出了
        patches = (
            mock.patch.object(pm, "ROOT", root),
            mock.patch.object(pm, "TASKS_DIR", root / "pm" / "tasks"),
            mock.patch.object(pm, "PM_DIR", root / "pm"),
            mock.patch.object(pm, "TEMPLATE", root / "pm" / "_template.html"),
        )
        with contextlib.ExitStack() as stack:
            for patch in patches:
                stack.enter_context(patch)
            stack.enter_context(contextlib.redirect_stdout(io.StringIO()))
            pm.build(out=out)
        rows = json.loads((out / "tasks.json").read_text(encoding="utf-8"))
        return rows, (out / "index.html").read_text(encoding="utf-8")

    def test_git_sync_checkout_at_root_links_to_tasks_and_refs(self) -> None:
        rows, page = self.build(git_sync_checkout(self.tmp / "git"))
        self.assertEqual(rows[0]["fileUrl"], BLOB + "pm/tasks/S1-KUN-001.md")
        self.assertEqual(rows[0]["refs"][0]["url"], BLOB + "docs/a.md")
        self.assertIn(f'"fileUrl":"{BLOB}pm/tasks/S1-KUN-001.md"', page)

    def test_git_sync_checkout_links_carry_prefix(self) -> None:
        """从旧版子目录生成的看板链接继续保留该目录前缀。"""
        rows, page = self.build(git_sync_checkout(self.tmp / "git") / "_import" / "qian")
        row = rows[0]
        self.assertEqual(row["file"], "pm/tasks/S1-KUN-001.md")
        self.assertEqual(row["fileUrl"], BLOB + "_import/qian/pm/tasks/S1-KUN-001.md")
        self.assertEqual(row["refs"], [{"path": "docs/a.md#3.1", "url": BLOB + "_import/qian/docs/a.md"}])
        self.assertIn(f'"fileUrl":"{BLOB}_import/qian/pm/tasks/S1-KUN-001.md"', page)

    def test_repository_root_links_have_no_prefix(self) -> None:
        """pm/ 与 scripts/pm.py 在仓库根时地址不带前缀。"""
        (self.tmp / "repo" / ".git").mkdir(parents=True)
        rows, _ = self.build(self.tmp / "repo")
        self.assertEqual(rows[0]["fileUrl"], BLOB + "pm/tasks/S1-KUN-001.md")
        self.assertEqual(rows[0]["refs"][0]["url"], BLOB + "docs/a.md")


if __name__ == "__main__":
    unittest.main()
