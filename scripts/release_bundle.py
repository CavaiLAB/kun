"""核对演练和断点续发使用同一份 GitHub Actions 制品。"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess

from release_version import release_info


def command(*args):
    return subprocess.check_output(args, text=True).strip()


def resolve(version, resume):
    release_info(version)
    repo = os.environ["GITHUB_REPOSITORY"]
    sha = os.environ["GITHUB_SHA"]
    run_id = os.environ["GITHUB_RUN_ID"]
    if resume:
        if not re.fullmatch(r"[1-9][0-9]*", resume):
            raise ValueError("恢复运行号须为正整数")
        workflow = json.loads(command("gh", "api", f"repos/{repo}/actions/workflows/release.yml"))
        run = json.loads(command("gh", "api", f"repos/{repo}/actions/runs/{resume}"))
        if (run["workflow_id"] != workflow["id"] or run["head_repository"]["full_name"] != repo
                or run["event"] != "workflow_dispatch" or run["head_branch"] != "main"
                or run["status"] != "completed"):
            raise ValueError("制品须来自本仓库 main 上已结束的发布工作流")
        sha, run_id = run["head_sha"], resume
    if not re.fullmatch(r"[0-9a-f]{40}", sha):
        raise ValueError("制品来源提交号无效")
    subprocess.run(["git", "merge-base", "--is-ancestor", sha, "origin/main"], check=True)
    runs = json.loads(command("gh", "run", "list", "--repo", repo, "--workflow", "ci.yml", "--commit", sha,
                              "--branch", "main", "--status", "success", "--json", "event,headSha"))
    if not any(run["headSha"] == sha and run["event"] in ("push", "workflow_dispatch") for run in runs):
        raise ValueError(f"{sha} 缺少成功的 main CI")
    with Path(os.environ["GITHUB_OUTPUT"]).open("a") as output:
        print(f"sha={sha}\nrun_id={run_id}\nkind={release_info(version)['kind']}", file=output)


def sha256_stream(stream):
    digest = hashlib.sha256()
    for block in iter(lambda: stream.read(128 * 1024), b""):
        digest.update(block)
    return digest.hexdigest()


def bundle_record(root, version, sha, run_id):
    release_info(version)
    required = {"release-packages.json", "NuGet.config", "release-notes.md", "packages"}
    if {p.name for p in root.iterdir()} - {"manifest.json"} != required:
        raise ValueError("发布制品目录的文件不完整或含多余文件")
    if not (root / "packages").is_dir() or not any((root / "packages").glob("*.nupkg")):
        raise ValueError("制品缺少 NuGet 包")
    hashes = {}
    for p in sorted(root.rglob("*")):
        if p.is_symlink():
            raise ValueError(f"制品不允许符号链接 {p.name}")
        if p == root / "manifest.json" or p == root / "packages":
            continue
        if not p.is_file() or (p.parent != root and (p.parent != root / "packages" or p.suffix not in (".nupkg", ".snupkg"))):
            raise ValueError(f"制品中有意外的内容 {p.name}")
        with p.open("rb") as stream:
            hashes[p.relative_to(root).as_posix()] = sha256_stream(stream)
    return {"version": version, "commit": sha, "run": run_id, "sha256": hashes}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=("resolve", "seal", "verify"))
    parser.add_argument("--version", required=True)
    parser.add_argument("--resume", default="")
    parser.add_argument("--directory", type=Path)
    parser.add_argument("--commit")
    parser.add_argument("--run-id")
    args = parser.parse_args()
    if args.command == "resolve":
        resolve(args.version, args.resume)
        return
    if args.directory is None or not args.commit or not args.run_id:
        parser.error("seal/verify 须指定目录、提交和运行号")
    record = bundle_record(args.directory, args.version, args.commit, args.run_id)
    manifest = args.directory / "manifest.json"
    if args.command == "seal":
        manifest.write_text(json.dumps(record, ensure_ascii=False, indent=2) + "\n")
    elif json.loads(manifest.read_text()) != record:
        raise ValueError("运行号、提交、版本或制品 SHA-256 与发布清单不一致")


if __name__ == "__main__":
    main()
