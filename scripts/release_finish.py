"""包源与消费者核对通过后，创建标签、Release 和版本日志分支。"""
from __future__ import annotations

import argparse
import datetime
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile

from release_version import release_info


def run(*args):
    return subprocess.check_output(args, text=True).strip()


def version_section(text, version):
    date = datetime.date(*map(int, version.split("-")[0].split(".")))
    section = f"## [Unreleased]\n\n（尚无新变更。）\n\n## [{version}] - {date.isoformat()}"
    result, count = re.subn(r"^## \[Unreleased\]$", lambda _: section, text, flags=re.M)
    if count != 1:
        raise ValueError("CHANGELOG.md 须有唯一的 Unreleased 段")
    return result


def finish(version, sha, directory):
    info = release_info(version)
    tag = "v" + version
    if run("git", "rev-parse", "HEAD") != sha:
        raise ValueError("当前检出与已通过核对的发布提交不同")
    run("git", "fetch", "origin", "main", "--tags")
    if run("git", "tag", "--list", tag):
        if run("git", "rev-parse", tag + "^{}") != sha:
            raise ValueError(f"{tag} 已指向其他提交")
    else:
        run("git", "tag", "-a", tag, sha, "-m", "Kun " + version)
        run("git", "push", "origin", "refs/tags/" + tag)

    assets = [directory / "release-packages.json", directory / "NuGet.config"]
    # gh release view 对身份/网络故障也会返回非零；创建同名 Release 会失败，绝不覆盖。
    view = subprocess.run(["gh", "release", "view", tag, "--json", "isDraft,isPrerelease,assets,body"],
                          text=True, capture_output=True)
    if view.returncode:
        create = ["gh", "release", "create", tag, "--draft", "--verify-tag", "--title", "Kun " + version,
                  "--notes-file", str(directory / "release-notes.md")]
        if info["prerelease"]:
            create.append("--prerelease")
        run(*create)
        view_data = {"isDraft": True, "isPrerelease": info["prerelease"], "assets": []}
    else:
        view_data = json.loads(view.stdout)
        expected_notes = (directory / "release-notes.md").read_text(encoding="utf-8").rstrip("\n")
        if view_data["body"].replace("\r\n", "\n").rstrip("\n") != expected_notes:
            raise ValueError("现有 Release 的说明与已核准的版本说明不符")
    if view_data["isPrerelease"] != info["prerelease"]:
        raise ValueError("现有 Release 的版本类型与发布清单不符")
    existing = {asset["name"] for asset in view_data["assets"]}
    for file in assets:
        if file.name in existing:
            with tempfile.TemporaryDirectory() as folder:
                run("gh", "release", "download", tag, "--pattern", file.name, "--dir", folder)
                if (Path(folder) / file.name).read_bytes() != file.read_bytes():
                    raise ValueError(f"Release 中已有不同的 {file.name}")
        else:
            run("gh", "release", "upload", tag, str(file))
    if view_data["isDraft"]:
        run("gh", "release", "edit", tag, "--draft=false")
    if info["prerelease"]:
        return

    main_log = run("git", "show", "origin/main:CHANGELOG.md")
    if re.search(r"^## \[" + re.escape(version) + r"\] - \d{4}-\d{2}-\d{2}$", main_log, re.M):
        return
    branch = f"cody/release-{version}-changelog"
    file = Path("CHANGELOG.md")
    desired = version_section(file.read_text(), version)
    if run("git", "ls-remote", "--heads", "origin", branch):
        run("git", "fetch", "origin", f"refs/heads/{branch}:refs/remotes/origin/{branch}")
        if (run("git", "show", f"origin/{branch}:CHANGELOG.md") != desired.strip()
                or run("git", "diff", "--name-only", sha, f"origin/{branch}") != "CHANGELOG.md"):
            raise ValueError(f"远端分支 {branch} 包含不同的更新")
    else:
        run("git", "switch", "-c", branch, sha)
        file.write_text(desired)
        run("git", "add", str(file))
        run("git", "commit", "-m", f"docs: 记录 Kun {version} 正式发布")
        run("git", "push", "origin", f"HEAD:refs/heads/{branch}")
    repo = os.environ["GITHUB_REPOSITORY"]
    with Path(os.environ["GITHUB_STEP_SUMMARY"]).open("a") as summary:
        print(f"[创建版本日志 PR](https://github.com/{repo}/compare/main...{branch}?expand=1)\n", file=summary)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version", required=True)
    parser.add_argument("--commit", required=True)
    parser.add_argument("--directory", type=Path, required=True)
    args = parser.parse_args()
    finish(args.version, args.commit, args.directory)
