"""看板 Pod 的构建容器（S1-PM-004）：按 git-sync 同步到的提交生成看板，交给 nginx。

git-sync 把仓库检出到 /git/.worktrees/<提交>，再把 /git/current 指向它。本循环每隔几秒看一次
/git/current：换了提交，就用该提交自己的 <前缀>/scripts/pm.py 把看板生成到 /site/<提交>，
再把 /site/current 原子地改指向它，最后删掉旧的生成目录。nginx 只读 /site/current。

前缀（环境变量 BOARD_PREFIX）是 scripts/pm.py 与 pm/tasks/ 在检出里所在的目录，空串表示检出根。
它是唯一一处写前缀的地方：启动时另建 /site/source → /git/current/<前缀>，nginx 的 /tasks/ 经它取任务源文件，
所以前缀变了只改 BOARD_PREFIX，nginx 配置不用动。

生成失败（任务文件校验不过等）时保留上一版看板，记一条日志，同一提交不再重试。
生成途中 git-sync 换了提交、删掉旧检出，这一版读不到文件而中断，不算失败（S1-PM-005）：
只记一条跳过说明，下一轮生成新提交。
收到 SIGTERM 立即退出：在容器里是 1 号进程，不处理的话内核会忽略它，Pod 要等宽限期满才被强杀。
只用 Python 标准库；加 --once 只检查一次就退出，供本机验证；自测见 test_builder.py。
"""
from __future__ import annotations

import os
import re
import shutil
import signal
import subprocess
import sys
import time

GIT_LINK = os.environ.get("BOARD_GIT_LINK", "/git/current")
SITE = os.environ.get("BOARD_SITE", "/site")
PERIOD = float(os.environ.get("BOARD_PERIOD", "5"))
# nginx 经这个链接取任务源文件（<前缀>/pm/tasks/），它总是跟着 /git/current 指向最新同步到的提交
SOURCE = "source"


def board_prefix(value: str) -> str:
    """规整 BOARD_PREFIX：检出里的相对目录，空串或 . 表示检出根；绝对路径与跳出检出的写法直接拒绝。"""
    value = value.strip()
    normalized = os.path.normpath(value) if value else "."
    if os.path.isabs(value) or normalized == ".." or normalized.startswith(".." + os.sep):
        raise SystemExit(f"BOARD_PREFIX 必须是检出里的相对目录，现在是 {value!r}")
    return "" if normalized == "." else normalized


PREFIX = board_prefix(os.environ.get("BOARD_PREFIX", ""))
# 已经记过「检出里没有 pm.py」的检出：同一检出只记一次，免得每轮都记
_missing: str | None = None


def log(message: str) -> None:
    print(time.strftime("%Y-%m-%dT%H:%M:%S%z"), message, flush=True)


def current() -> str | None:
    """git-sync 当前检出的目录；还没同步完第一次时为 None。"""
    return os.path.realpath(GIT_LINK) if os.path.lexists(GIT_LINK) else None


def pm_script(checkout: str) -> str:
    return os.path.join(checkout, PREFIX, "scripts", "pm.py")


def link_source() -> None:
    """建 /site/source → /git/current/<前缀>（原子替换），供 nginx 的 /tasks/ 取任务源文件。"""
    link = os.path.join(SITE, SOURCE)
    target = os.path.join(GIT_LINK, PREFIX) if PREFIX else GIT_LINK
    staging = link + ".tmp"
    if os.path.lexists(staging):
        os.remove(staging)
    os.symlink(target, staging)
    os.replace(staging, link)


def publish(checkout: str) -> bool:
    rev = os.path.basename(checkout)
    target = os.path.join(SITE, rev)
    staging = target + ".tmp"
    shutil.rmtree(staging, ignore_errors=True)
    result = subprocess.run(
        [sys.executable, pm_script(checkout), "build", "--out", staging],
        capture_output=True, text=True)
    if result.returncode != 0:
        shutil.rmtree(staging, ignore_errors=True)
        # git-sync 先改指 /git/current 再删旧检出，所以检出被删时 current() 已经换了。
        latest = current()
        if latest != checkout:
            switched = os.path.basename(latest)[:9] if latest else "（无）"
            log(f"生成 {rev[:9]} 途中 git-sync 已切到 {switched}，跳过这一版，下一轮生成新提交")
            return False
        log(f"生成失败 {rev[:9]}，继续提供上一版：{(result.stdout + result.stderr).strip()}")
        return False
    shutil.rmtree(target, ignore_errors=True)
    os.rename(staging, target)
    # 先建临时链接再 rename 覆盖，nginx 任何时刻看到的都是完整的一版。
    link = os.path.join(SITE, "current.tmp")
    if os.path.lexists(link):
        os.remove(link)
    os.symlink(rev, link)
    os.replace(link, os.path.join(SITE, "current"))
    for name in os.listdir(SITE):
        path = os.path.join(SITE, name)
        if name != rev and not os.path.islink(path) and os.path.isdir(path):
            shutil.rmtree(path, ignore_errors=True)
    count = re.search(r"（(\d+) 个任务）", result.stdout)
    log(f"已上线 {rev[:9]}" + (f"（{count.group(1)} 个任务）" if count else ""))
    return True


def check(seen: str | None) -> str | None:
    """看一次 /git/current：换了提交就生成。返回已处理过的检出目录，同一提交不再重试。

    检出里在前缀下没有 scripts/pm.py 时不生成、不算处理过，只记一次日志（多半是 BOARD_PREFIX 与仓库布局不符）。
    """
    global _missing
    checkout = current()
    if checkout and checkout != seen:
        if os.path.isfile(pm_script(checkout)):
            publish(checkout)
            return checkout
        if checkout != _missing:
            _missing = checkout
            script = os.path.join(PREFIX, "scripts", "pm.py")
            log(f"检出 {os.path.basename(checkout)[:9]} 里没有 {script}，不生成，继续提供上一版（核对 BOARD_PREFIX）")
    return seen


def stop(_signum: int, _frame: object) -> None:
    log("收到 SIGTERM，退出")
    sys.exit(0)


def main() -> None:
    once = "--once" in sys.argv[1:]
    seen = None
    signal.signal(signal.SIGTERM, stop)
    link_source()
    log(f"开始：{GIT_LINK} → {SITE}，前缀 {PREFIX or '（检出根）'}，每 {PERIOD:g} 秒检查一次")
    while True:
        seen = check(seen)
        if once:
            return
        time.sleep(PERIOD)


if __name__ == "__main__":
    main()
