"""核对两个部署入口的清单、看板产物与交给 nginx 的任务文件。"""
import os
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[2]
OLD = "_import/qian"


def render(path):
    return subprocess.check_output(["kubectl", "kustomize", str(ROOT / path)], text=True)


def board(site, prefix):
    site.mkdir()
    subprocess.run(
        [sys.executable, str(ROOT / "deploy/pm-board/builder.py"), "--once"],
        env={**os.environ, "BOARD_GIT_LINK": str(ROOT), "BOARD_SITE": str(site), "BOARD_PREFIX": prefix},
        check=True,
    )
    return site / "current"


def main():
    canonical = render("deploy/pm-board")
    original = render(OLD + "/deploy/pm-board")
    empty_prefix = '- name: BOARD_PREFIX\n          value: ""'
    if canonical.count(empty_prefix) != 1:
        raise SystemExit("看板根部署应声明一个空 BOARD_PREFIX")
    if original != canonical.replace(empty_prefix, "- name: BOARD_PREFIX\n          value: " + OLD):
        raise SystemExit("看板两个部署入口的渲染仅应在 BOARD_PREFIX 取值有差异")

    with tempfile.TemporaryDirectory() as scratch:
        base = Path(scratch)
        old = board(base / "old", OLD)
        new = board(base / "new", "")
        for name in ("index.html", "tasks.json", "STATUS.md"):
            if (old / name).read_bytes() != (new / name).read_bytes():
                raise SystemExit("看板入口产生不同内容 " + name)
        tasks = sorted((ROOT / "pm/tasks").glob("*.md"))
        for site in (old.parent, new.parent):
            published = sorted((site / "source/pm/tasks").glob("*.md"))
            if [p.name for p in published] != [p.name for p in tasks]:
                raise SystemExit("nginx 的任务原文清单与仓库不同")
            for local, served in zip(tasks, published):
                if not local.samefile(served):
                    raise SystemExit("nginx 的文件与看板源不同 " + local.name)
        print(f"看板三个产物与 nginx 的 {len(tasks)} 份任务原文均已核对。")


if __name__ == "__main__":
    main()
