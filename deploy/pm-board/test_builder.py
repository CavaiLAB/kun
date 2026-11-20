"""builder.py 的自测（S1-PM-005），只用标准库，docs-and-pm 工作流也会跑：

    python3 -m unittest discover -s deploy/pm-board -p 'test_*.py' -v

每个用例在临时目录里搭出 git-sync 的布局（/git/.worktrees/<提交> + /git/current 链接），
检出里放一个假的 <前缀>/scripts/pm.py，按检出里的 mode 文件模拟三种情形：正常生成、任务文件校验不过、
生成途中 git-sync 同步到新提交（先改指 current，再删旧检出）。另有一个用例确认收到 SIGTERM 立即退出。
全部用例覆盖两个入口：默认从检出根读取，旧版看板可以通过 BOARD_PREFIX 指向子目录；
另有用例核对 /site/source 链接与 BOARD_PREFIX 的规整。
"""
from __future__ import annotations

import contextlib
import io
import os
import shutil
import signal
import subprocess
import sys
import tempfile
import unittest
from unittest import mock

import builder

FAKE_PM = '''\
import os, pathlib, shutil, sys

# 假 pm.py 在 <检出>/<前缀>/scripts/ 下；检出目录是 .worktrees 下的那一层
here = pathlib.Path(__file__).resolve()
root = here.parents[1]
checkout = next(p for p in here.parents if p.parent.name == ".worktrees")
out = pathlib.Path(sys.argv[sys.argv.index("--out") + 1])
mode = (checkout / "mode").read_text(encoding="utf-8").strip()
if mode == "invalid":
    sys.exit("校验失败：S1-XX-001 缺少「验收」小节")
if mode == "switch":
    # 与 git-sync 相同的顺序：先把 current 改指新提交，再删掉旧检出；本次生成随后读不到任务文件
    link = os.environ["BOARD_TEST_LINK"]
    os.symlink(os.environ["BOARD_TEST_NEXT"], link + ".tmp")
    os.replace(link + ".tmp", link)
    shutil.rmtree(checkout)
    list((root / "pm" / "tasks").iterdir())
out.mkdir(parents=True)
for name in ("index.html", "tasks.json", "STATUS.md"):
    (out / name).write_text(checkout.name, encoding="utf-8")
print(f"已生成 tasks.json, index.html, STATUS.md 到 {out}（3 个任务）")
'''

A, B, C = "a" * 40, "b" * 40, "c" * 40


class BuilderTest(unittest.TestCase):
    PREFIX = ""  # 默认从检出根生成看板

    def setUp(self) -> None:
        # macOS 的临时目录经过 /var → /private/var 链接，builder 比较的是 realpath
        root = os.path.realpath(tempfile.mkdtemp(prefix="pm-board-builder-"))
        self.addCleanup(shutil.rmtree, root, True)
        self.git = os.path.join(root, "git")
        self.site = os.path.join(root, "site")
        self.link = os.path.join(self.git, "current")
        os.makedirs(os.path.join(self.git, ".worktrees"))
        os.makedirs(self.site)
        for patcher in (
            mock.patch.object(builder, "GIT_LINK", self.link),
            mock.patch.object(builder, "SITE", self.site),
            mock.patch.object(builder, "PREFIX", self.PREFIX),
            mock.patch.object(builder, "_missing", None),
            mock.patch.dict(os.environ, {"BOARD_TEST_LINK": self.link}),
        ):
            patcher.start()
            self.addCleanup(patcher.stop)

    def checkout(self, rev: str, mode: str = "ok") -> str:
        path = os.path.join(self.git, ".worktrees", rev)
        root = os.path.join(path, self.PREFIX)
        os.makedirs(os.path.join(root, "scripts"))
        os.makedirs(os.path.join(root, "pm", "tasks"))
        with open(os.path.join(root, "pm", "tasks", f"{rev[:5]}.md"), "w", encoding="utf-8") as f:
            f.write(rev)
        with open(os.path.join(root, "scripts", "pm.py"), "w", encoding="utf-8") as f:
            f.write(FAKE_PM)
        with open(os.path.join(path, "mode"), "w", encoding="utf-8") as f:
            f.write(mode)
        return path

    def point(self, checkout: str) -> None:
        """像 git-sync 一样原子地把 /git/current 改指某个检出。"""
        os.symlink(checkout, self.link + ".tmp")
        os.replace(self.link + ".tmp", self.link)

    def check(self, seen: str | None) -> tuple[str | None, str]:
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            seen = builder.check(seen)
        return seen, out.getvalue()

    def served(self) -> str:
        return os.readlink(os.path.join(self.site, "current"))

    def published(self) -> str | None:
        a = self.checkout(A)
        self.point(a)
        seen, log = self.check(None)
        self.assertIn(f"已上线 {A[:9]}（3 个任务）", log)
        return seen

    def test_does_nothing_before_first_sync(self) -> None:
        """git-sync 还没同步完第一次时不生成。"""
        self.assertEqual(self.check(None), (None, ""))
        self.assertEqual(os.listdir(self.site), [])

    def test_new_commit_is_published_and_old_build_removed(self) -> None:
        """新提交生成后原子切换 /site/current，删掉旧版；同一提交不再生成。"""
        seen = self.published()
        self.assertEqual(self.served(), A)
        with open(os.path.join(self.site, "current", "tasks.json"), encoding="utf-8") as f:
            self.assertEqual(f.read(), A)

        self.point(self.checkout(B))
        seen, log = self.check(seen)
        self.assertIn(f"已上线 {B[:9]}", log)
        self.assertEqual(self.served(), B)
        self.assertEqual(sorted(os.listdir(self.site)), sorted([B, "current"]))

        # 同一提交不再生成
        self.assertEqual(self.check(seen), (seen, ""))

    def test_invalid_tasks_log_failure_and_keep_previous_board(self) -> None:
        """任务文件校验不过：记「生成失败」，继续提供上一版，同一提交不再重试。"""
        seen = self.published()
        b = self.checkout(B, "invalid")
        self.point(b)
        seen, log = self.check(seen)
        self.assertIn(f"生成失败 {B[:9]}，继续提供上一版：校验失败：S1-XX-001 缺少「验收」小节", log)
        self.assertEqual(self.served(), A)
        self.assertEqual(sorted(os.listdir(self.site)), sorted([A, "current"]))
        # 同一提交不再重试
        self.assertEqual(self.check(seen), (b, ""))

    def test_commit_switch_during_build_is_skipped_not_failed(self) -> None:
        """生成途中 git-sync 切到新提交并删掉旧检出：只记跳过，不记失败，下一轮生成新提交。"""
        seen = self.published()
        c = self.checkout(C)
        b = self.checkout(B, "switch")
        self.point(b)
        with mock.patch.dict(os.environ, {"BOARD_TEST_NEXT": c}):
            seen, log = self.check(seen)
        self.assertFalse(os.path.exists(b), "假 pm.py 应像 git-sync 一样删掉旧检出")
        self.assertIn(f"生成 {B[:9]} 途中 git-sync 已切到 {C[:9]}，跳过这一版", log)
        self.assertNotIn("生成失败", log)
        self.assertEqual(self.served(), A)
        self.assertEqual(sorted(os.listdir(self.site)), sorted([A, "current"]))

        seen, log = self.check(seen)
        self.assertEqual(seen, c)
        self.assertIn(f"已上线 {C[:9]}", log)
        self.assertEqual(self.served(), C)

    def test_source_link_follows_current_into_prefix(self) -> None:
        """/site/source 指向 /git/current/<前缀>：nginx 的 /tasks/ 总是取最新同步到的提交的任务源文件。"""
        builder.link_source()
        source = os.path.join(self.site, "source")
        self.assertEqual(os.readlink(source), os.path.join(self.link, self.PREFIX) if self.PREFIX else self.link)
        seen = self.published()
        tasks = os.path.join(source, "pm", "tasks")
        self.assertEqual(os.listdir(tasks), [f"{A[:5]}.md"])

        self.point(self.checkout(B))
        self.assertEqual(os.listdir(tasks), [f"{B[:5]}.md"])
        # 再建一次（容器重启）是原子替换，不报错；生成新版、清理旧版时也不动它
        builder.link_source()
        seen, _ = self.check(seen)
        self.assertEqual(sorted(os.listdir(self.site)), sorted([B, "current", "source"]))
        self.assertTrue(os.path.islink(source))

    def test_checkout_without_pm_script_under_prefix_is_not_built(self) -> None:
        """检出里在前缀下找不到 scripts/pm.py（例如前缀配错）时不生成、不记成处理过，只记一次日志。"""
        wrong = os.path.join(self.git, ".worktrees", A)
        os.makedirs(os.path.join(wrong, "elsewhere", "scripts"))
        self.point(wrong)
        seen, log = self.check(None)
        self.assertIsNone(seen)
        script = os.path.join(self.PREFIX, "scripts", "pm.py")
        self.assertIn(f"检出 {A[:9]} 里没有 {script}，不生成，继续提供上一版", log)
        self.assertEqual(os.listdir(self.site), [])
        # 同一检出不再记；换成布局正确的提交后照常生成
        self.assertEqual(self.check(None), (None, ""))
        self.point(self.checkout(B))
        seen, log = self.check(None)
        self.assertIn(f"已上线 {B[:9]}", log)

    def test_exits_promptly_on_sigterm(self) -> None:
        """收到 SIGTERM 立即退出：容器里的 1 号进程不处理 SIGTERM 就会忽略它，Pod 要等宽限期满才被强杀。

        这里 builder 不是 1 号进程，没有处理函数时会被 SIGTERM 直接杀掉（返回码 -15），所以以返回码 0 与退出日志为准。
        """
        env = dict(os.environ, BOARD_GIT_LINK=self.link, BOARD_SITE=self.site, BOARD_PERIOD="60",
                   BOARD_PREFIX=self.PREFIX, PYTHONDONTWRITEBYTECODE="1")
        proc = subprocess.Popen([sys.executable, "-u", builder.__file__], env=env, text=True,
                                stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        self.addCleanup(proc.kill)
        self.assertIn("开始", proc.stdout.readline())  # 处理函数在这条日志之前装好
        proc.send_signal(signal.SIGTERM)
        out, _ = proc.communicate(timeout=5)
        self.assertEqual(proc.returncode, 0, out)
        self.assertIn("收到 SIGTERM，退出", out)
        self.assertTrue(os.path.islink(os.path.join(self.site, "source")), "启动时应先建好 /site/source")


class PrefixedBuilderTest(BuilderTest):
    """带前缀的历史版本仍可从检出中的子目录生成看板。"""
    PREFIX = os.path.join("fixtures", "workspace")


class BoardPrefixTest(unittest.TestCase):
    def test_normalizes_relative_prefix(self) -> None:
        self.assertEqual(builder.board_prefix(""), "")
        self.assertEqual(builder.board_prefix("."), "")
        self.assertEqual(builder.board_prefix(" fixtures/workspace/ "), os.path.join("fixtures", "workspace"))
        self.assertEqual(builder.board_prefix("fixtures/./workspace"), os.path.join("fixtures", "workspace"))

    def test_rejects_prefix_outside_checkout(self) -> None:
        for value in ("/abs", "..", "../workspace", "fixtures/../../workspace"):
            with self.subTest(value=value), self.assertRaises(SystemExit):
                builder.board_prefix(value)


if __name__ == "__main__":
    unittest.main()
