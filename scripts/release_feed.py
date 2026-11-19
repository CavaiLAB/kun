"""读取正式包源的版本索引，并核对 GitHub Packages 的仓库关联和可见性。"""
from __future__ import annotations

import json
import time
import urllib.error
import urllib.request
from urllib.parse import quote

from release_version import release_info


REPOSITORY = "CavaiLAB/kun"
GITHUB_API = "https://api.github.com/orgs/CavaiLAB/packages/nuget/"
NUGET_INDEX = "https://api.nuget.org/v3-flatcontainer/"


class FeedError(Exception):
    """包源不可访问、元数据不符或等待超时。"""


def _read(url, token, timeout):
    headers = {"User-Agent": "kun-release-verification"}
    if url.startswith(GITHUB_API):
        headers.update({"Accept": "application/vnd.github+json", "X-GitHub-Api-Version": "2022-11-28"})
    request = urllib.request.Request(url, headers=headers)
    if url.startswith(GITHUB_API) and token:
        request.add_unredirected_header("Authorization", "Bearer " + token)
    try:
        with urllib.request.urlopen(request, timeout=timeout) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        if error.code == 404:
            return None
        if error.code == 429 or 500 <= error.code < 600:
            raise ConnectionError(f"{url} 返回 HTTP {error.code}") from error
        raise FeedError(f"{url} 返回 HTTP {error.code}") from error
    except (urllib.error.URLError, TimeoutError) as error:
        raise ConnectionError(f"读取 {url} 暂时失败：{error.reason if isinstance(error, urllib.error.URLError) else error}") from error
    except (UnicodeError, ValueError) as error:
        raise FeedError(f"{url} 的响应缺少有效 JSON") from error


def _nuget_versions(name, read):
    index = read(NUGET_INDEX + quote(name.lower(), safe="") + "/index.json")
    if index is None:
        return []
    versions = index.get("versions") if isinstance(index, dict) else None
    if not isinstance(versions, list) or not all(isinstance(value, str) for value in versions):
        raise FeedError(f"{name} 的 nuget.org 版本索引形状有误")
    return versions


def _github_versions(name, read):
    url = GITHUB_API + quote(name, safe="")
    page = 1
    while True:
        versions = read(f"{url}/versions?per_page=100&page={page}")
        if versions is None:
            return
        if not isinstance(versions, list) or any(not isinstance(item, dict) or not isinstance(item.get("name"), str) for item in versions):
            raise FeedError(f"{name} 的 GitHub Packages 版本列表形状有误")
        for item in versions:
            yield item["name"]
        if len(versions) < 100:
            return
        page += 1


def _available(feed, name, version, read):
    if feed == "nuget":
        return version in _nuget_versions(name, read)
    url = GITHUB_API + quote(name, safe="")
    record = read(url)
    if record is None:
        return False
    if not isinstance(record, dict) or record.get("visibility") != "public":
        raise FeedError(f"{name} 在 GitHub Packages 上应为公开包")
    repository = record.get("repository") or {}
    if not isinstance(repository, dict) or str(repository.get("full_name", "")).casefold() != REPOSITORY.casefold():
        raise FeedError(f"{name} 在 GitHub Packages 上应关联 {REPOSITORY}")
    return version in _github_versions(name, read)


def release_order(version):
    """同日按 beta、正式版、hotfix 的发布顺序排列。"""
    kind = release_info(version)["kind"]
    date, _, suffix = version.partition("-")
    number = int(suffix.split(".")[1]) if suffix else 0
    return (*map(int, date.split(".")), {"beta": 0, "stable": 1, "hotfix": 2}[kind], number)


def check_unpublished(packages, version, token):
    if not token:
        raise FeedError("包源预检需要具有 packages:read 权限的 GITHUB_TOKEN")
    candidate = release_order(version)
    def read(url):
        try:
            return _read(url, token if url.startswith(GITHUB_API) else None, 30)
        except (ConnectionError, TimeoutError) as error:
            raise FeedError(f"无法完成版本预检：{error}") from error

    for package in packages:
        if version in _github_versions(package.id, read):
            raise FeedError(f"{package.id} 的 {version} 已在 GitHub Packages 发布")
        if package.tier != "kernel":
            continue
        for existing in _nuget_versions(package.id, read):
            if existing == version:
                raise FeedError(f"{package.id} 的 {version} 已在 nuget.org 发布")
            try:
                order = release_order(existing)
            except ValueError:
                continue  # 启用日历版本之前的历史版本。
            if order > candidate:
                raise FeedError(f"{version} 早于 {package.id} 已发布的 {existing}")


def verify_feed(feed, names, version, wait_seconds, token=None):
    """只重查尚未出现的包。wait_seconds 为零时执行一轮。"""
    if not names:
        raise FeedError("本版本在选定包源没有待核对的包")
    if wait_seconds < 0:
        raise FeedError("等待秒数必须非负")
    deadline = time.monotonic() + wait_seconds
    pending = set(names)
    last_error = None

    def read(url):
        remaining = deadline - time.monotonic()
        if wait_seconds and remaining <= 0:
            raise TimeoutError("包源索引等待超时")
        return _read(url, token, min(30, remaining) if wait_seconds else 30)

    while pending:
        for name in sorted(pending):
            try:
                if _available(feed, name, version, read):
                    pending.remove(name)
            except (ConnectionError, TimeoutError) as error:
                last_error = str(error)
        if pending:
            remaining = deadline - time.monotonic()
            if remaining <= 0:
                detail = f"。最近一次读取状态：{last_error}" if last_error else ""
                raise FeedError(f"{feed} 缺少版本 {version}：{', '.join(sorted(pending))}{detail}")
            time.sleep(min(30, remaining))
