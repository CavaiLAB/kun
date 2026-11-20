"""按包名单推送指定档位，并复核包源返回的内容。"""
from __future__ import annotations

import argparse
import base64
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
from urllib.error import HTTPError
from urllib.parse import quote, urlsplit
from urllib.request import Request, urlopen
import zipfile

from release_bundle import sha256_stream
from release_consumer import GITHUB
from release_feed import NUGET_INDEX
from release_packages import _load_list, github_packages
from release_version import release_info


def read_url(url):
    request = Request(url, headers={"User-Agent": "kun-release"})
    if urlsplit(url).hostname == urlsplit(GITHUB).hostname:
        user = os.environ["KUN_PLATFORM_FEED_USER"]
        token = os.environ["KUN_PLATFORM_FEED_TOKEN"]
        credential = base64.b64encode(f"{user}:{token}".encode()).decode()
        request.add_unredirected_header("Authorization", "Basic " + credential)
    return urlopen(request, timeout=120)


def content_base(feed):
    if feed == "nuget":
        return NUGET_INDEX
    with read_url(GITHUB) as response:
        resources = json.load(response)["resources"]
    for item in resources:
        if item["@type"] == "PackageBaseAddress/3.0.0":
            base = item["@id"]
            if urlsplit(base).scheme == "https" and urlsplit(base).hostname == urlsplit(GITHUB).hostname:
                return base.rstrip("/") + "/"
    raise ValueError("GitHub Packages 的服务索引缺少包下载地址")


def payload(path):
    # nuget.org 会为制品添加仓库签名，比较其余每个条目的内容。
    with zipfile.ZipFile(path) as archive:
        hashes = {}
        for entry in archive.infolist():
            if entry.is_dir() or entry.filename == ".signature.p7s":
                continue
            if entry.filename in hashes:
                raise ValueError(f"{path.name} 含重名条目 {entry.filename}")
            with archive.open(entry) as contents:
                hashes[entry.filename] = sha256_stream(contents)
        return hashes


def push_package(file, config, feed, has_symbols):
    source = "kun-platform" if feed == "github" else "nuget.org"
    key = os.environ["GITHUB_TOKEN" if feed == "github" else "NUGET_API_KEY"]
    env = {**os.environ, "NUGET_API_KEY": key}
    options = ["--configfile", str(config), "--source", source, "--skip-duplicate"]
    subprocess.run(["dotnet", "nuget", "push", str(file), *options, "--no-symbols"], env=env, check=True)
    # A duplicate main package makes NuGet skip its implicit symbol upload.
    if feed == "nuget" and has_symbols:
        env["NUGET_SYMBOL_API_KEY"] = key
        subprocess.run(["dotnet", "nuget", "push", str(file.with_suffix(".snupkg")), *options], env=env, check=True)


def compare_package(base, identifier, version, file, *, allow_missing):
    name = quote(identifier.lower(), safe="")
    version = quote(version, safe="")
    try:
        response = read_url(f"{base}{name}/{version}/{name}.{version}.nupkg")
    except HTTPError as error:
        if error.code == 404 and allow_missing:
            error.close()
            return False
        raise
    with response, tempfile.TemporaryDirectory() as folder:
        remote = Path(folder) / file.name
        with remote.open("wb") as output:
            shutil.copyfileobj(response, output)
        if payload(file) != payload(remote):
            raise ValueError(f"{identifier} {version} 包源内容与已核准制品不同")
    return True


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=("push", "compare", "compare-existing"))
    parser.add_argument("--feed", choices=("github", "nuget"), required=True)
    parser.add_argument("--version", required=True)
    parser.add_argument("--directory", type=Path, required=True)
    args = parser.parse_args()
    release_info(args.version)
    packages = _load_list(args.directory / "release-packages.json")
    github_ids = set(github_packages(packages, args.version))
    selected = [p for p in packages if (p.id in github_ids) == (args.feed == "github")]
    if not selected:
        raise ValueError("指定包源没有属于本次发版的包")
    base = content_base(args.feed) if args.command != "push" else None
    checked = 0
    for package in selected:
        file = args.directory / "packages" / f"{package.id}.{args.version}.nupkg"
        if args.command == "push":
            push_package(file, args.directory / "NuGet.config", args.feed, package.symbols and package.build_output)
            checked += 1
        elif compare_package(base, package.id, args.version, file, allow_missing=args.command == "compare-existing"):
            checked += 1
    print(f"{args.feed} {args.command} 通过，处理 {checked}/{len(selected)} 个包")


if __name__ == "__main__":
    main()
