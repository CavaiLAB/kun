#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""使用文档的示例（S1-DOCS-012）。只依赖标准库。

网站「开始」里有三篇使用文档：上手（website/docs/start/getting-started.md）、写一个组件
（website/docs/start/guides/write-a-component.md）、在 .NET 里接权限（website/docs/start/guides/authorization-dotnet.md）。
页面上让读者整段写进文件的代码收在 samples/docs/<页面>/ 下，路径与读者照页面做出来的目录一致，说明见 samples/docs/README.md。

  python3 scripts/docs_examples.py check
      不要 .NET。页面上整段的代码块与 samples/docs/ 里的文件逐字一致；页面上节选的 JSON 是仓库里对应文件的子集；
      页面上的每个代码块（顶格的、列表里缩进的，``` 与 ~~~ 围栏都算）要么有示例的步骤用到，要么在本文件里写明不跑的原因；
      三篇页面里的 Kun 版本号处处相同；只在发版后执行的两行（克隆发布标签、下载发布页的 NuGet.config）按版本号核对写法；
      run 要找的命令块、输出块、表格、插入片段的位置与说明里的字面都在。
  python3 scripts/docs_examples.py feed --version <版本> --out <目录>
      用当前源码按 <版本> 打出页面上 dotnet add package 装的包，以及它们依赖的 Kun 包，放进 <目录> 当本地包源。
  python3 scripts/docs_examples.py run --feed local --packages <目录> --version <版本> [--work <目录>] [--only <页面>]
  python3 scripts/docs_examples.py run --feed published [--version <版本>] [--work <目录>] [--only <页面>]
      像读者那样照页面做一遍：在仓库外的空目录里开一个 bash，按顺序执行页面上的命令（页面上的 Kun 版本号换成 <版本>），
      把 samples/docs/ 里的文件放到页面说的位置，页面上的 dotnet run 另起进程，核对页面贴出的状态码、响应头、响应体与错误码。
      local：Kun 的包都从 <目录> 装，清单工具用当前检出构建，页面上克隆发布标签、下载发布页包源映射的两行不执行；
      published：照页面写的，内核档从 nuget.org，平台档用发布页附带的 NuGet.config，读包令牌从环境变量
      KUN_PLATFORM_FEED_USER、KUN_PLATFORM_FEED_TOKEN 取，只在页面上 export 它们的那一步交给终端；清单工具从发布标签构建。
      HOME 与 NuGet 缓存都指向工作目录：用户机密、开发令牌与包缓存不落进本机的用户目录。
      照页面跑的进程一律用中文界面语言（LANG、LC_ALL 设为 zh_CN.UTF-8）：页面上贴的错误文案是中文界面语言下的，
      结果不随机器的语言设置变。
  python3 scripts/docs_examples.py bump --version <版本> --sdk <SDK 版本> [--date <YYYY-MM-DD>]
      把三篇页面里的 Kun 版本号，以及「照着跑过一遍」那句里的日期与 SDK 版本改成给定值。发版后示例跑通了再改。

退出码：0 通过；1 不通过；2 工具出错。
"""
from __future__ import annotations

import argparse
import contextlib
import datetime
import json
import os
import re
import select
import shlex
import shutil
import signal
import socket
import subprocess
import sys
import tempfile
import threading
import time
import urllib.error
import urllib.request
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path
from typing import Callable, Dict, Iterator, List, NamedTuple, Optional, Sequence, Tuple

import release_feed

ROOT = Path(__file__).resolve().parents[1]
EXAMPLES = Path("samples/docs")
SOLUTION = Path("Kun.slnx")
NUGET_ORG = "https://api.nuget.org/v3/index.json"
GITHUB_PACKAGES = "nuget.pkg.github.com"
SECRETS = ("KUN_PLATFORM_FEED_USER", "KUN_PLATFORM_FEED_TOKEN")
RELEASE = re.compile(r"^\d{4}\.[1-9]\d?\.[1-9]\d?(?:-(?:hotfix|beta)\.[1-9]\d*)?$")
# 页面里的 Kun 版本号：发布版本（可以带 -hotfix.N），或者程序集版本（版本的数字部分加 .0）。
VERSION_TOKEN = re.compile(r"(?<![\d.])(20\d{2}\.[1-9]\d?\.[1-9]\d?)(\.0|-hotfix\.[1-9]\d*)?(?!\d)(?!\.\d)")
# 每篇页面开头的那句「…在 <日期> 用 .NET SDK <版本>、Kun <版本> 照着跑过一遍」。
RAN = re.compile(r"在 (\d{4}-\d{2}-\d{2}) 用 \.NET SDK (\d+\.\d+\.\d+)、Kun (\S+?) 照着跑过一遍")
GUID_V7 = re.compile(r"[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}")
TRACE_ID = re.compile(r"[0-9a-f]{32}")
JWT = re.compile(r"[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]*")
# 发布页在这个仓库：发版后照页面克隆它的发布标签（release_finish.py 打的 v<版本>）、下载发布页附带的 NuGet.config。
REPOSITORY = "https://github.com/" + release_feed.REPOSITORY
# 照页面跑的进程一律用的界面语言。页面上贴的错误文案是中文界面语言下的；.NET 按 LC_ALL、LC_MESSAGES、LANG 定界面语言，
# 没设或是 C、C.UTF-8 时，Linux 上落到固定区域（Kun 的文案回退到 zh-CN），macOS 上跟着系统语言走，英文环境下文案就是英文。
READER_LOCALE = "zh_CN.UTF-8"
# 单条命令、等服务启动、打本地包源的上限，都远小于 CI 作业的超时：卡住时这一步先失败，摘要与日志还来得及留下。
COMMAND_TIMEOUT = 600
SERVER_TIMEOUT = 600
PACK_TIMEOUT = 600


class ToolError(Exception):
    """输入或环境不对，给不出结论（退出码 2）。"""


class Failed(Exception):
    """核对不通过（退出码 1）。"""


def say(text: str = "") -> None:
    print(text, flush=True)


def tail(text: str, lines: int = 60) -> str:
    rows = text.rstrip("\n").split("\n")
    return "\n".join(rows[-lines:]) if len(rows) > lines else "\n".join(rows)


# ---------- 页面 ----------

class Block(NamedTuple):
    section: str
    lang: str
    text: str
    line: int


class Ref(NamedTuple):
    """页面上的一个代码块：所在的二级标题（去掉 {#锚点}）、语言、同一小节同一语言里的第几个（从 0 数）。"""
    section: str
    lang: str
    index: int = 0


def split_row(line: str) -> List[str]:
    body = line.strip()
    body = body[1:] if body.startswith("|") else body
    body = body[:-1] if body.endswith("|") and not body.endswith("\\|") else body
    cells, current, index = [], "", 0
    while index < len(body):
        if body.startswith("\\|", index):
            current, index = current + "|", index + 2
            continue
        if body[index] == "|":
            cells.append(current.strip())
            current = ""
        else:
            current += body[index]
        index += 1
    cells.append(current.strip())
    return cells


FENCE = re.compile(r"(?P<indent>[ \t>]*)(?P<marker>`{3,}|~{3,})(?P<info>.*)")


def dedent(line: str, width: int) -> str:
    """列表里缩进的代码块：每行去掉围栏前那么多的空格，不够就去掉有的。"""
    spaces = len(line) - len(line.lstrip(" "))
    return line[min(width, spaces):]


class Page:
    """一篇页面：按二级标题分节的代码块与表格。

    代码块认顶格的与列表里用空格缩进的，``` 与 ~~~ 两种围栏都认（围栏长度不限，收尾的围栏不短于开头的）。
    引用块里的、用 Tab 缩进的认不出来，记进 problems，由 check 报出来，免得网站照常显示、示例却看不见它。
    """

    def __init__(self, path: Path, root: Path = ROOT) -> None:
        self.path = path
        self.name = path.relative_to(root).as_posix()
        self.text = path.read_text(encoding="utf-8")
        self.blocks: List[Block] = []
        self.tables: Dict[str, List[List[List[str]]]] = {}
        self.sections: List[str] = []
        self.problems: List[str] = []
        lines = self.text.split("\n")
        index, section, table = 0, "", []  # type: Tuple[int, str, List[List[str]]]
        if lines and lines[0] == "---":
            index = lines.index("---", 1) + 1
        while index < len(lines):
            line = lines[index]
            if table and not line.startswith("|"):
                self.tables.setdefault(section, []).append(table)
                table = []
            fence = FENCE.fullmatch(line)
            if fence and fence.group("marker")[0] == "`" and "`" in fence.group("info"):
                fence = None  # 一行里的行内代码，不是围栏
            if fence:
                indent, marker, start, body = fence.group("indent"), fence.group("marker"), index + 1, []
                quoted = bool(indent.strip(" "))
                if quoted:
                    self.problems.append(f"{self.name}:{start} 的代码块在引用块里或用了 Tab 缩进，示例认不出来："
                                         "改成顶格的围栏，在列表里就用空格缩进。")
                closing = re.compile((r"[ \t>]*" if quoted else r" *") + re.escape(marker[0]) + "{%d,}[ \t]*" % len(marker))
                index += 1
                while index < len(lines) and not closing.fullmatch(lines[index]):
                    body.append(dedent(lines[index], len(indent)))
                    index += 1
                if index == len(lines):
                    raise ToolError(f"{self.name}:{start}：代码块没有收尾")
                words = fence.group("info").split()
                self.blocks.append(Block(section, words[0] if words else "", "\n".join(body), start))
            elif line.startswith("|"):
                table.append(split_row(line))
            else:
                heading = re.match(r"^(#{1,2})\s+(.*?)\s*$", line)
                if heading:
                    section = re.sub(r"\s*\{#[^}]*\}$", "", heading.group(2)) if len(heading.group(1)) == 2 else ""
                    if section:
                        self.sections.append(section)
            index += 1
        if table:
            self.tables.setdefault(section, []).append(table)

    def find(self, ref: Ref) -> Block:
        found = [block for block in self.blocks if block.section == ref.section and block.lang == ref.lang]
        if ref.index >= len(found):
            raise Failed(f"{self.name}：小节「{ref.section}」里没有第 {ref.index + 1} 个 {ref.lang} 代码块（现有 {len(found)} 个）。")
        return found[ref.index]

    def table(self, section: str) -> List[List[str]]:
        """小节里唯一的表格，去掉分隔行。"""
        tables = self.tables.get(section, [])
        if len(tables) != 1:
            raise Failed(f"{self.name}：小节「{section}」里应有一张表，现有 {len(tables)} 张。")
        return [row for row in tables[0] if not all(re.fullmatch(r":?-{3,}:?", cell) for cell in row)]


def command_lines(text: str) -> List[str]:
    """代码块里的命令：去掉空行与注释，行尾的反斜杠接下一行。"""
    lines, pending = [], ""
    for raw in text.split("\n"):
        line = pending + raw
        if line.rstrip().endswith("\\"):
            pending = line.rstrip()[:-1].rstrip() + " "
            continue
        pending = ""
        if line.strip() and not line.lstrip().startswith("#"):
            lines.append(line.strip())
    return lines


# ---------- 示例 ----------

class Example(NamedTuple):
    slug: str
    page: str
    # samples/docs/<slug>/ 下的文件 ⇔ 页面上整段写进这个文件的代码块，逐字一致。
    files: Dict[str, Ref]
    # 示例的步骤用到的命令块、输出块与插入的片段。
    blocks: Dict[str, Ref]
    # 页面上节选的 JSON ⇔ 仓库里的文件：节选里的每个字段文件里都有、值相同，数组一样长。
    excerpts: Dict[str, Tuple[Ref, str]]
    # 步骤里照说明断言的字面：页面上要有这些字。
    prose: Tuple[str, ...]
    # 插入片段、删行要找的行：(文件, 去掉缩进后的开头)，在文件里恰好出现一次。
    anchors: Tuple[Tuple[str, str], ...]
    # 不跑的小节与原因；这些小节以外的代码块都要有示例的步骤用到。
    skipped: Dict[str, str]


GS_BUILD = "var app = builder.Build();"
GS_ERRORS = "app.UseKunErrors();"
AZ_USING = "using Kun.Authorization;"
AZ_CHECKER = "builder.Services.AddSingleton<IPermissionChecker, DemoPermissionChecker>();"
AZ_EXPORT = 'app.MapPost("/notes/export",'
AZ_BEHAVIOR = "m.AddPermissionBehavior<ExportNotesCommand, Result<ExportResult>>(Result<ExportResult>.Fail);"
AZ_MISSING_BEHAVIOR = "受保护请求未装配权限行为：ExportNotesCommand"
AZ_LIBRARY_MODE = "授权已使用库模式，只检查 IPermissionChecker，不写决策审计、不比对身份修订。"
WAC_TIER_POINTER = ".qian-plugin/plugin.json#/runtime/form"
WAC_IN_SYNC = {"changed": [], "applied": False}
WAC_TOOL_FIELDS = ("inputSchema", "outputSchema", "schemaSha256")

GETTING_STARTED = Example(
    slug="getting-started",
    page="website/docs/start/getting-started.md",
    files={"Hello.Kun/Program.cs": Ref("2 写一个命令", "csharp")},
    blocks={
        "install": Ref("1 建项目，装包", "bash"),
        "run": Ref("3 跑起来", "bash", 0),
        "create": Ref("3 跑起来", "bash", 1),
        "created": Ref("3 跑起来", "json", 0),
        "taken": Ref("3 跑起来", "json", 1),
        "missing": Ref("3 跑起来", "bash", 2),
        "not-found": Ref("3 跑起来", "json", 2),
        "localization": Ref("文案的语言", "csharp"),
    },
    excerpts={},
    prose=("`200`", "`409`", "`Content-Type: application/problem+json`", "`Content-Language: en`", "`Title already exists`"),
    anchors=(("Hello.Kun/Program.cs", GS_BUILD), ("Hello.Kun/Program.cs", GS_ERRORS)),
    skipped={},
)

WRITE_A_COMPONENT = Example(
    slug="write-a-component",
    page="website/docs/start/guides/write-a-component.md",
    files={
        "notes-demo/Acme.Notes/NotesPlugin.cs": Ref("2 写入口与能力", "csharp"),
        "notes-demo/notes/.qian-plugin/plugin.json": Ref("3 手写清单", "json"),
        "notes-demo/NotesHost/Program.cs": Ref("5 装进宿主", "csharp"),
    },
    blocks={
        "install": Ref("1 建类库，装包", "bash"),
        "folders": Ref("3 手写清单", "bash"),
        "build": Ref("4 用清单工具派生", "bash", 0),
        "check": Ref("4 用清单工具派生", "bash", 1),
        "drift": Ref("4 用清单工具派生", "json", 0),
        "sync": Ref("4 用清单工具派生", "bash", 2),
        "synced": Ref("4 用清单工具派生", "json", 1),
        "tool": Ref("4 用清单工具派生", "json", 2),
        "host": Ref("5 装进宿主", "bash", 0),
        "run-host": Ref("5 装进宿主", "bash", 1),
        "host-output": Ref("5 装进宿主", "text"),
    },
    excerpts={},
    prose=("退出码是 1", "`sync` 退出码 0", '`{"changed":[],"applied":false}`', *(f"`{field}`" for field in WAC_TOOL_FIELDS),
           f"`{WAC_TIER_POINTER}`", "退出 2"),
    anchors=(),
    skipped={"无代码的组件": "命令行 qian 没有发布成包，要从仓库构建，装前端依赖要 GitHub Packages 的令牌；"
                            "skill-only 模板由 packages/qian-cli 的测试覆盖（CI 的「声明式表面」）"},
)

AUTHORIZATION = Example(
    slug="authorization-dotnet",
    page="website/docs/start/guides/authorization-dotnet.md",
    files={"Notes.Authz/Program.cs": Ref("2 写命令与判定器", "csharp")},
    blocks={
        "install": Ref("1 建项目，装包", "bash"),
        "tokens": Ref("3 签本机测试令牌", "bash"),
        "run": Ref("4 跑起来", "bash", 0),
        "variables": Ref("4 跑起来", "bash", 1),
        "requests": Ref("4 跑起来", "bash", 2),
        "unauthenticated": Ref("4 跑起来", "json", 0),
        "denied": Ref("4 跑起来", "json", 1),
        "exported": Ref("4 跑起来", "json", 2),
        "platform-package": Ref("5 再接 HTTP 端点", "bash"),
        "using": Ref("5 再接 HTTP 端点", "csharp", 0),
        "register": Ref("5 再接 HTTP 端点", "csharp", 1),
        "endpoint": Ref("5 再接 HTTP 端点", "csharp", 2),
    },
    excerpts={
        "permissions": (Ref("6 平台模式：权限引擎", "json", 0), "samples/Kun.Sample.Crm/metadata/permissions.json"),
        "roles": (Ref("6 平台模式：权限引擎", "json", 1), "samples/Kun.Sample.Crm/metadata/roles.json"),
    },
    prose=("不带令牌是 401", "alice 有导出权限是 200", AZ_MISSING_BEHAVIOR, "500 的 `INTERNAL`", AZ_LIBRARY_MODE),
    anchors=(("Notes.Authz/Program.cs", AZ_USING), ("Notes.Authz/Program.cs", AZ_CHECKER),
             ("Notes.Authz/Program.cs", AZ_EXPORT), ("Notes.Authz/Program.cs", AZ_BEHAVIOR)),
    skipped={"gRPC 方法": "页面上写明没有单独演示；客户参考应用的测试覆盖 gRPC 与 HTTP 对同一权限点判定一致"},
)

ALL: Tuple[Example, ...] = (GETTING_STARTED, WRITE_A_COMPONENT, AUTHORIZATION)


def load_pages(root: Path = ROOT) -> Dict[str, Page]:
    return {example.slug: Page(root / example.page, root) for example in ALL}


def documented_version(pages: Sequence[Page]) -> str:
    """三篇页面里的 Kun 版本号：发布版本处处相同，程序集版本是它的数字部分加 .0。"""
    releases: Dict[str, List[str]] = {}
    assemblies: Dict[str, List[str]] = {}
    for page in pages:
        for match in VERSION_TOKEN.finditer(page.text):
            place = f"{page.name}:{page.text.count(chr(10), 0, match.start()) + 1}"
            if match.group(2) == ".0":
                assemblies.setdefault(match.group(1), []).append(place)
            else:
                releases.setdefault(match.group(0), []).append(place)
    if len(releases) != 1:
        shown = "；".join(f"{version}（{'、'.join(places[:3])}{' 等' if len(places) > 3 else ''}）" for version, places in sorted(releases.items()))
        raise Failed(f"三篇页面里的 Kun 版本号应处处相同，现在是：{shown or '一个也没有'}。")
    documented = next(iter(releases))
    numeric = documented.split("-")[0]
    for version, places in assemblies.items():
        if version != numeric:
            raise Failed(f"程序集版本 {version}.0（{'、'.join(places)}）与页面上的 Kun 版本 {documented} 对不上，应为 {numeric}.0。")
    return documented


# ---------- 只在发版后执行的两行 ----------

# 平时用本地包源跑，这两行不执行（清单工具用当前检出构建，包源映射用工作目录里的）；check 按页面上的版本号核对写法，
# 免得地址写错、标签漏了 v、目录名对不上，要等包发布以后才发现。
RELEASE_LINES = {WRITE_A_COMPONENT.slug: "克隆发布标签", AUTHORIZATION.slug: "下载发布页的 NuGet.config"}


def clones_release(words: Sequence[str]) -> bool:
    return list(words[:2]) == ["git", "clone"]


def downloads_release_config(words: Sequence[str]) -> bool:
    return words[:1] == ["curl"] and any(word.startswith("https://") and word.endswith("/NuGet.config") for word in words)


def clone_arguments(words: Sequence[str]) -> Tuple[Optional[str], List[str]]:
    """git clone 后面的参数：--branch 的值，与位置参数（仓库地址、目录）。"""
    branch, positional, index = None, [], 0
    while index < len(words):
        word = words[index]
        if word in ("--branch", "-b", "--depth", "--origin", "-o", "--filter", "--reference", "--config", "-c", "--jobs", "-j"):
            if word in ("--branch", "-b") and index + 1 < len(words):
                branch = words[index + 1]
            index += 2
            continue
        if word.startswith("--branch="):
            branch = word.split("=", 1)[1]
        elif not word.startswith("-"):
            positional.append(word)
        index += 1
    return branch, positional


def release_line_problems(example: Example, page: Page, documented: str) -> List[str]:
    """克隆发布标签、下载发布页 NuGet.config 的写法，与后面的命令找清单工具用的目录。"""
    problems: List[str] = []
    tag = "v" + documented
    folder = "kun-" + tag
    found = set()
    for block in page.blocks:
        if block.lang != "bash":
            continue
        for line in command_lines(block.text):
            try:
                words = shlex.split(line)
            except ValueError:
                continue
            where = f"{page.name}:{block.line} 的 `{line}`"
            if clones_release(words):
                found.add("克隆发布标签")
                branch, positional = clone_arguments(words[2:])
                if branch != tag:
                    now = f"现在是 {branch}" if branch else "现在没写，克隆的是缺省分支"
                    problems.append(f"{where}：要克隆发布标签 {tag}（--branch {tag}），{now}。")
                if positional[:1] != [REPOSITORY + ".git"]:
                    problems.append(f"{where}：仓库地址应是 {REPOSITORY}.git。")
                if positional[1:] != [folder]:
                    problems.append(f"{where}：要写明克隆到 {folder}，后面的命令按这个目录找清单工具。")
            elif downloads_release_config(words):
                found.add("下载发布页的 NuGet.config")
                url = next(word for word in words if word.startswith("https://"))
                expected = f"{REPOSITORY}/releases/download/{tag}/NuGet.config"
                if url != expected:
                    problems.append(f"{where}：下载地址应是 {expected}。")
                flags = "".join(word[1:] for word in words if re.fullmatch(r"-[A-Za-z]+", word))
                if "f" not in flags and "--fail" not in words:
                    problems.append(f"{where}：要带 -f，下载失败时报错，不然 404 的页面会被存成 NuGet.config。")
                option = next((word for word in ("-o", "--output") if word in words[:-1]), None)
                saved = words[words.index(option) + 1] if option else ("NuGet.config" if "O" in flags or "--remote-name" in words else None)
                if saved != "NuGet.config":
                    problems.append(f"{where}：要存成项目目录里的 NuGet.config（-o NuGet.config）。")
        for match in re.finditer(r"(?:^|[\s=\"'])([^\s=\"']+)/tools/Kun\.Plugin\.Tool\b", block.text, re.MULTILINE):
            if match.group(1) != folder:
                problems.append(f"{page.name}:{block.line} 按 {match.group(1)}/ 找清单工具，克隆发布标签的目录是 {folder}。")
            elif "克隆发布标签" not in found:
                problems.append(f"{page.name}:{block.line} 按 {folder}/ 找清单工具，在这之前却没有克隆发布标签的那一行。")
    wanted = RELEASE_LINES.get(example.slug)
    if wanted and wanted not in found:
        problems.append(f"{page.name} 里应有一行{wanted}：发版后照页面跑要用它。")
    return problems


# ---------- JSON 比对 ----------

def compact(value: object) -> str:
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"))


def compare_json(expected: object, actual: object, volatile: Dict[str, "re.Pattern[str]"], path: str = "$") -> List[str]:
    """逐字段相同，对象的键与顺序也相同；volatile 里的键只要求是匹配该式样的字符串（每次不同的编号）。"""
    if isinstance(expected, dict):
        if not isinstance(actual, dict):
            return [f"{path} 是 {compact(actual)}，页面上是对象"]
        if list(expected) != list(actual):
            return [f"{path} 的键是 {list(actual)}，页面上是 {list(expected)}"]
        problems: List[str] = []
        for key, value in expected.items():
            pattern = volatile.get(key)
            if pattern is not None:
                if not isinstance(value, str) or not pattern.fullmatch(value):
                    problems.append(f"页面上 {path}.{key} 的样例值 {compact(value)} 不是这种编号")
                if not isinstance(actual[key], str) or not pattern.fullmatch(actual[key]):
                    problems.append(f"{path}.{key} 是 {compact(actual[key])}，不是页面上那种每次不同的编号")
            else:
                problems += compare_json(value, actual[key], volatile, f"{path}.{key}")
        return problems
    if isinstance(expected, list):
        if not isinstance(actual, list) or len(actual) != len(expected):
            return [f"{path} 是 {compact(actual)}，页面上是 {compact(expected)}"]
        return [problem for index, (item, other) in enumerate(zip(expected, actual))
                for problem in compare_json(item, other, volatile, f"{path}[{index}]")]
    if type(expected) is not type(actual) or expected != actual:
        return [f"{path} 是 {compact(actual)}，页面上是 {compact(expected)}"]
    return []


def subset_problems(part: object, whole: object, path: str = "$") -> List[str]:
    """part 是 whole 的节选：对象只看 part 里有的键，数组一样长、逐项是节选，其余的值相同。"""
    if isinstance(part, dict):
        if not isinstance(whole, dict):
            return [f"{path} 在文件里是 {compact(whole)}，页面上是对象"]
        problems: List[str] = []
        for key, value in part.items():
            if key not in whole:
                problems.append(f"{path}.{key} 在文件里没有")
            else:
                problems += subset_problems(value, whole[key], f"{path}.{key}")
        return problems
    if isinstance(part, list):
        if not isinstance(whole, list) or len(part) != len(whole):
            return [f"{path} 在文件里是 {compact(whole)}，页面上是 {compact(part)}"]
        return [problem for index, (item, other) in enumerate(zip(part, whole))
                for problem in subset_problems(item, other, f"{path}[{index}]")]
    if type(part) is not type(whole) or part != whole:
        return [f"{path} 在文件里是 {compact(whole)}，页面上是 {compact(part)}"]
    return []


def last_json(output: str) -> object:
    """命令输出里最后一行 JSON。"""
    for line in reversed(output.strip().split("\n")):
        line = line.strip()
        if line.startswith(("{", "[")):
            try:
                return json.loads(line)
            except ValueError:
                continue
    raise Failed(f"命令输出里没有 JSON：\n{tail(output)}")


# ---------- 插入片段 ----------

def anchored(text: str, anchor: str) -> int:
    lines = text.split("\n")
    hits = [index for index, line in enumerate(lines) if line.strip().startswith(anchor)]
    if len(hits) != 1:
        raise Failed(f"以「{anchor}」开头的行出现 {len(hits)} 次，应恰好一次。")
    return hits[0]


def insert(text: str, anchor: str, snippet: str, after: bool, blank: bool = False) -> str:
    """在以 anchor 开头的那一行之前或之后插入 snippet，按那一行的缩进；blank 时与原文隔一个空行。"""
    lines = text.split("\n")
    index = anchored(text, anchor)
    indent = re.match(r"\s*", lines[index]).group(0)  # type: ignore[union-attr]
    added = [indent + line if line else line for line in snippet.split("\n")]
    if blank:
        added = [""] + added if after else added + [""]
    position = index + 1 if after else index
    return "\n".join(lines[:position] + added + lines[position:])


def remove(text: str, anchor: str) -> str:
    lines = text.split("\n")
    index = anchored(text, anchor)
    return "\n".join(lines[:index] + lines[index + 1:])


def split_snippet(text: str) -> Tuple[str, str]:
    """「文案的语言」那段：空行前是服务登记，空行后是中间件。"""
    parts = text.split("\n\n")
    if len(parts) != 2:
        raise Failed("「文案的语言」的代码块应是两段（服务登记、中间件），中间隔一个空行。")
    return parts[0], parts[1]


def localized(program: str, snippet: str) -> str:
    services, middleware = split_snippet(snippet)
    return insert(insert(program, GS_BUILD, services, after=False, blank=True), GS_ERRORS, middleware, after=True)


def with_http_endpoint(program: str, using: str, register: str, endpoint: str) -> str:
    program = insert(program, AZ_USING, using, after=True)
    program = insert(program, AZ_CHECKER, register, after=True)
    return insert(program, AZ_EXPORT, endpoint, after=False, blank=True)


# ---------- check ----------

def check(root: Path = ROOT) -> List[str]:
    """不要 .NET 的那部分核对，返回发现的问题。"""
    problems: List[str] = []
    try:
        pages = load_pages(root)
        documented = documented_version(list(pages.values()))
    except Failed as error:
        return [str(error)]
    mapped = set()
    for example in ALL:
        page = pages[example.slug]
        problems.extend(page.problems)
        problems.extend(release_line_problems(example, page, documented))
        used = set()

        def find(ref: Ref) -> Optional[Block]:
            try:
                block = page.find(ref)
            except Failed as error:
                problems.append(str(error))
                return None
            used.add(block)
            return block

        for relative, ref in example.files.items():
            path = root / EXAMPLES / example.slug / relative
            mapped.add(path)
            block = find(ref)
            if block is None:
                continue
            if not path.is_file():
                problems.append(f"{page.name}:{block.line} 的代码块应有对应的文件 {path.relative_to(root).as_posix()}，现在没有。")
                continue
            text = path.read_text(encoding="utf-8")
            if text != block.text + "\n":
                expected, actual = block.text.split("\n"), text.rstrip("\n").split("\n")
                differ = next((index for index, (left, right) in enumerate(zip(expected, actual)) if left != right),
                              min(len(expected), len(actual)))
                left = expected[differ] if differ < len(expected) else "（页面上到此为止）"
                right = actual[differ] if differ < len(actual) else "（文件到此为止）"
                problems.append(f"{page.name}:{block.line} 的代码块与 {path.relative_to(root).as_posix()} 不一致，"
                                f"从第 {differ + 1} 行起：页面「{left}」，文件「{right}」。页面与文件要一起改。")
        for name, ref in example.blocks.items():
            block = find(ref)
            if block is not None and ref.lang == "json":
                try:
                    json.loads(block.text)
                except ValueError as error:
                    problems.append(f"{page.name}:{block.line} 的 JSON（{name}）读不了：{error}")
        for name, (ref, target) in example.excerpts.items():
            block = find(ref)
            if block is None:
                continue
            try:
                part = json.loads(block.text)
                whole = json.loads((root / target).read_text(encoding="utf-8"))
            except (OSError, ValueError) as error:
                problems.append(f"{page.name}:{block.line} 的节选（{name}）或 {target} 读不了：{error}")
                continue
            for problem in subset_problems(part, whole):
                problems.append(f"{page.name}:{block.line} 节选自 {target}，{problem}。")
        for literal in example.prose:
            if literal not in page.text:
                problems.append(f"{page.name} 里找不到「{literal}」：示例照这句断言，说明改了，示例的断言也要跟着改。")
        for relative, anchor in example.anchors:
            path = root / EXAMPLES / example.slug / relative
            if path.is_file():
                try:
                    anchored(path.read_text(encoding="utf-8"), anchor)
                except Failed as error:
                    problems.append(f"{path.relative_to(root).as_posix()}：{error}")
        for section in example.skipped:
            if section not in page.sections:
                problems.append(f"{page.name} 里没有小节「{section}」：不跑的小节改名或删了，scripts/docs_examples.py 跟着改。")
        for block in page.blocks:
            if block not in used and block.section not in example.skipped:
                problems.append(f"{page.name}:{block.line} 的 {block.lang} 代码块（小节「{block.section}」）没有示例的步骤用到："
                                "在 scripts/docs_examples.py 里给它加步骤，或者写明不跑的原因。")
        ran = RAN.findall(page.text)
        if len(ran) != 1:
            problems.append(f"{page.name} 里应有一句「在 <日期> 用 .NET SDK <版本>、Kun <版本> 照着跑过一遍」，现有 {len(ran)} 句。")
        elif ran[0][2] != documented:
            problems.append(f"{page.name} 里「照着跑过一遍」那句的 Kun 版本是 {ran[0][2]}，页面上装的是 {documented}。")
    folder = root / EXAMPLES
    for path in example_files(root):
        if path not in mapped and path != folder / "README.md":
            problems.append(f"{path.relative_to(root).as_posix()} 不对应页面上的代码块：删掉，或者在 scripts/docs_examples.py 里登记。")
    return problems


def example_files(root: Path) -> List[Path]:
    """samples/docs/ 下的文件：在 git 检出里取跟踪的与没被忽略的（编辑器、系统留下的忽略文件不算），不在检出里就逐个列出。"""
    folder = root / EXAMPLES
    result = subprocess.run(["git", "-C", str(root), "ls-files", "-co", "--exclude-standard", "-z", "--", EXAMPLES.as_posix()],
                            capture_output=True, text=True)
    if result.returncode == 0 and (root / ".git").exists():
        return sorted(root / name for name in result.stdout.split("\0") if name and (root / name).is_file())
    return sorted(path for path in folder.rglob("*") if path.is_file()) if folder.is_dir() else []


# ---------- feed ----------

def page_packages(pages: Sequence[Page]) -> List[str]:
    """页面上 dotnet add package 装的包。"""
    found = set()
    for page in pages:
        for block in page.blocks:
            for match in re.finditer(r"dotnet add package (\S+) --version (\S+)", block.text):
                found.add(match.group(1))
    return sorted(found)


def project_closure(packages: Sequence[str], root: Path = ROOT) -> List[Path]:
    """这些包的项目与它们的项目引用（递归），都是相对仓库根的路径。"""
    pending: List[Path] = []
    for package in packages:
        project = Path("src") / package / f"{package}.csproj"
        if not (root / project).is_file():
            raise ToolError(f"页面上装的包 {package} 在仓库里找不到项目 {project.as_posix()}。")
        pending.append(project)
    seen: Dict[Path, None] = {}
    while pending:
        project = pending.pop()
        if project in seen:
            continue
        seen[project] = None
        for item in ET.parse(root / project).getroot().iter("ProjectReference"):
            include = (item.get("Include") or "").replace("\\", "/")
            target = Path(os.path.normpath((project.parent / include).as_posix()))
            if not (root / target).is_file():
                raise ToolError(f"{project.as_posix()} 引用的 {include} 不存在。")
            pending.append(target)
    return sorted(seen)


def nuspec(package: Path) -> Tuple[str, str, List[str]]:
    """包的 id、版本与依赖的包 id。"""
    with zipfile.ZipFile(package) as archive:
        name = next(entry for entry in archive.namelist() if entry.endswith(".nuspec") and "/" not in entry)
        document = ET.fromstring(archive.read(name))
    space = document.tag[:document.tag.index("}") + 1] if document.tag.startswith("{") else ""
    metadata = document.find(f"{space}metadata")
    if metadata is None:
        raise ToolError(f"{package.name} 的 nuspec 没有 metadata。")
    dependencies = [item.get("id") or "" for item in metadata.iter(f"{space}dependency")]
    return metadata.findtext(f"{space}id") or "", metadata.findtext(f"{space}version") or "", dependencies


def feed(version: str, out: Path, root: Path = ROOT) -> List[str]:
    if not RELEASE.fullmatch(version):
        raise ToolError(f"版本 {version} 不是发布版本的写法（YYYY.M.D，可带 -hotfix.N 或 -beta.N）。")
    pages = load_pages(root)
    packages = page_packages(list(pages.values()))
    projects = project_closure(packages, root)
    out = out.resolve()
    if out.exists() and any(out.iterdir()):
        raise ToolError(f"{out} 不是空目录。")
    out.mkdir(parents=True, exist_ok=True)
    say(f"页面上装的包：{'、'.join(packages)}；连同依赖的 Kun 项目共 {len(projects)} 个，按版本 {version} 打包到 {out}。")
    with tempfile.TemporaryDirectory(prefix="kun-docs-feed-") as folder:
        solution_filter = Path(folder) / "docs-examples.slnf"
        solution_filter.write_text(json.dumps({"solution": {"path": str(root / SOLUTION),
                                                            "projects": [project.as_posix() for project in projects]}},
                                              ensure_ascii=False, indent=2), encoding="utf-8")
        command = ["dotnet", "pack", str(solution_filter), "-c", "Release", f"-p:Version={version}",
                   "-p:WarningsNotAsErrors=NU5104", "--disable-build-servers", "-o", str(out)]
        say("$ " + " ".join(shlex.quote(word) for word in command))
        try:
            result = subprocess.run(command, cwd=root, timeout=PACK_TIMEOUT)
        except subprocess.TimeoutExpired:
            raise ToolError(f"dotnet pack 超过 {PACK_TIMEOUT} 秒没有结束。") from None
        if result.returncode:
            raise ToolError(f"dotnet pack 失败（退出码 {result.returncode}）。")
    produced: Dict[str, List[str]] = {}
    for package in sorted(out.glob("*.nupkg")):
        identity, actual, dependencies = nuspec(package)
        if actual != version:
            raise ToolError(f"{package.name} 的版本是 {actual}，应为 {version}。")
        produced[identity] = dependencies
    missing = sorted({dependency for dependencies in produced.values() for dependency in dependencies
                      if dependency.startswith("Kun.") and dependency not in produced} | (set(packages) - set(produced)))
    if missing:
        raise ToolError(f"本地包源缺 {'、'.join(missing)}：项目引用的闭包没算全，改 scripts/docs_examples.py 的 project_closure。")
    say(f"本地包源就绪：{len(produced)} 个包。")
    return sorted(produced)


# ---------- 终端、服务、请求 ----------

def without_locale(env: Dict[str, str]) -> Dict[str, str]:
    """去掉机器的语言设置（LANG、LANGUAGE、LC_*）；照页面跑的进程用的语言由 locale_setup 在 bash 里设。"""
    return {key: value for key, value in env.items() if key not in ("LANG", "LANGUAGE") and not key.startswith("LC_")}


def locale_setup() -> str:
    """在读者的 bash 里设界面语言，标准错误丢掉。

    机器上没有这个区域时（比如 CI 的 runner 没生成 zh_CN），bash 从环境变量继承它会往标准错误打 cannot change locale 的警告，
    混进要比对的输出；在 bash 里设、把这一句的标准错误丢掉就不会。.NET 不靠系统的区域数据，照样按这个名字定界面语言。
    """
    return f"{{ export LANG={READER_LOCALE} LC_ALL={READER_LOCALE}; }} 2>/dev/null"


class Terminal:
    """读者的终端：一直开着的 bash，cd 与变量在命令之间保留；标准错误并进标准输出；每条命令的标准输入是 /dev/null。
    界面语言固定为 READER_LOCALE，不随机器的语言设置变。"""

    MARK = "__KUN_DOCS_EXAMPLES_EXIT__"

    def __init__(self, cwd: Path, env: Dict[str, str], log: Path) -> None:
        log.parent.mkdir(parents=True, exist_ok=True)
        self.log = log.open("w", encoding="utf-8")
        self.process = subprocess.Popen(["bash", "--noprofile", "--norc"], cwd=str(cwd), env=without_locale(env),
                                        stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                        start_new_session=True)
        self.pending = b""
        code, output = self.run(locale_setup(), 30)
        if code:
            self.close()
            raise ToolError(f"读者的终端设不了界面语言（退出码 {code}）：{output}")

    def run(self, command: str, timeout: float) -> Tuple[int, str]:
        self.log.write(f"$ {command}\n")
        self.log.flush()
        script = "{ " + command + "\n} < /dev/null\nprintf '\\n%s %d\\n' '" + self.MARK + "' \"$?\"\n"
        assert self.process.stdin is not None and self.process.stdout is not None
        self.process.stdin.write(script.encode("utf-8"))
        self.process.stdin.flush()
        marker = ("\n" + self.MARK + " ").encode("utf-8")
        deadline = time.monotonic() + timeout
        descriptor = self.process.stdout.fileno()
        while True:
            at = self.pending.find(marker)
            end = self.pending.find(b"\n", at + len(marker)) if at >= 0 else -1
            if end >= 0:
                output = self.pending[:at].decode("utf-8", "replace")
                code = int(self.pending[at + len(marker):end])
                self.pending = self.pending[end + 1:]
                self.log.write(output + ("" if output.endswith("\n") or not output else "\n") + f"[退出码 {code}]\n")
                self.log.flush()
                return code, output
            remaining = deadline - time.monotonic()
            if remaining <= 0:
                self.close()
                raise Failed(f"`{command}` 超过 {int(timeout)} 秒没有结束。")
            ready, _, _ = select.select([descriptor], [], [], min(remaining, 5.0))
            if ready:
                chunk = os.read(descriptor, 65536)
                if not chunk:
                    raise ToolError("终端里的 bash 意外退出。")
                self.pending += chunk

    def close(self) -> None:
        if self.process.poll() is None:
            with contextlib.suppress(OSError, ValueError):
                assert self.process.stdin is not None
                self.process.stdin.write(b"exit\n")
                self.process.stdin.flush()
            with contextlib.suppress(subprocess.TimeoutExpired):
                self.process.wait(10)
        # 终端里起的进程（构建节点等）与它同一个进程组，一并结束。
        with contextlib.suppress(ProcessLookupError, PermissionError):
            os.killpg(self.process.pid, signal.SIGKILL)
        with contextlib.suppress(subprocess.TimeoutExpired):
            self.process.wait(10)
        for stream in (self.process.stdin, self.process.stdout):
            with contextlib.suppress(OSError):
                if stream is not None:
                    stream.close()
        self.log.close()


class Server:
    """页面上的 dotnet run：另起的进程（界面语言与读者的终端相同），等它打印监听地址。"""

    READY = re.compile(r"Now listening on: (https?://\S+)")

    def __init__(self, command: str, cwd: Path, env: Dict[str, str], log: Path) -> None:
        self.command = command
        self.lines: List[str] = []
        self.lock = threading.Lock()
        self.log_path = log
        self.process = subprocess.Popen(["bash", "--noprofile", "--norc", "-c", locale_setup() + "\nexec " + command],
                                        cwd=str(cwd), env=without_locale(env),
                                        stdin=subprocess.DEVNULL, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                        start_new_session=True)
        self.thread = threading.Thread(target=self._pump, daemon=True)
        self.thread.start()
        self.url = ""

    def _pump(self) -> None:
        assert self.process.stdout is not None
        with self.log_path.open("w", encoding="utf-8") as log:
            for raw in iter(self.process.stdout.readline, b""):
                text = raw.decode("utf-8", "replace").rstrip("\n")
                with self.lock:
                    self.lines.append(text)
                log.write(text + "\n")
                log.flush()

    def output(self) -> str:
        with self.lock:
            return "\n".join(self.lines)

    def wait_for(self, pattern: "re.Pattern[str]", timeout: float) -> "re.Match[str]":
        deadline = time.monotonic() + timeout
        while True:
            match = pattern.search(self.output())
            if match:
                return match
            if self.process.poll() is not None:
                self.thread.join(5)
                raise Failed(f"`{self.command}` 提前退出（退出码 {self.process.returncode}）：\n{tail(self.output())}")
            if time.monotonic() > deadline:
                raise Failed(f"`{self.command}` 等了 {int(timeout)} 秒没有输出「{pattern.pattern}」：\n{tail(self.output())}")
            time.sleep(0.2)

    def start(self, timeout: float = SERVER_TIMEOUT) -> None:
        self.url = self.wait_for(self.READY, timeout).group(1)

    def stop(self) -> None:
        if self.process.poll() is None:
            with contextlib.suppress(ProcessLookupError, PermissionError):
                os.killpg(self.process.pid, signal.SIGTERM)
            try:
                self.process.wait(20)
            except subprocess.TimeoutExpired:
                with contextlib.suppress(ProcessLookupError, PermissionError):
                    os.killpg(self.process.pid, signal.SIGKILL)
                self.process.wait(10)
        with contextlib.suppress(ProcessLookupError, PermissionError):
            os.killpg(self.process.pid, signal.SIGKILL)
        self.thread.join(10)
        if self.process.stdout is not None:
            with contextlib.suppress(OSError):
                self.process.stdout.close()


class Response(NamedTuple):
    status: Optional[int]
    headers: Dict[str, str]
    body: str


def request(method: str, url: str, headers: Optional[Dict[str, str]] = None, body: Optional[bytes] = None) -> Response:
    """不走代理的 HTTP 请求；4xx、5xx 也照常返回。"""
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    message = urllib.request.Request(url, data=body, method=method, headers=headers or {})
    try:
        with opener.open(message, timeout=60) as response:
            return Response(response.status, {key.lower(): value for key, value in response.headers.items()},
                            response.read().decode("utf-8", "replace"))
    except urllib.error.HTTPError as error:
        return Response(error.code, {key.lower(): value for key, value in error.headers.items()},
                        error.read().decode("utf-8", "replace"))
    except (urllib.error.URLError, OSError) as error:
        raise Failed(f"{method} {url} 没有连上：{error}") from error


def curl_response(command: str, output: str) -> Response:
    """页面上的 curl 命令的输出：-i 时带状态行与响应头，-w 写了 %{http_code} 时末尾有一行 HTTP <状态码>。"""
    words = shlex.split(command)
    status: Optional[int] = None
    headers: Dict[str, str] = {}
    text = output
    if any("%{http_code}" in word for word in words):
        match = re.search(r"\nHTTP (\d{3})\s*$", text)
        if not match:
            raise Failed(f"`{command}` 的输出末尾没有状态码：\n{tail(text)}")
        status, text = int(match.group(1)), text[:match.start()]
    if "-i" in words or "--include" in words:
        text = text.replace("\r\n", "\n")
        start = re.search(r"(?m)^HTTP/\S+ \d{3}", text)
        head, separator, body = text[start.start() if start else 0:].partition("\n\n")
        lines = head.split("\n")
        match = re.match(r"HTTP/\S+ (\d{3})", lines[0])
        if not separator or not match:
            raise Failed(f"`{command}` 的输出不是「状态行、响应头、空行、响应体」：\n{tail(text)}")
        status = int(match.group(1))
        for line in lines[1:]:
            key, _, value = line.partition(":")
            headers[key.strip().lower()] = value.strip()
        text = body
    return Response(status, headers, text.strip())


def expect(response: Response, expected: object, where: str, status: Optional[int] = None,
           volatile: Optional[Dict[str, "re.Pattern[str]"]] = None, content_type: Optional[str] = None) -> None:
    problems: List[str] = []
    if status is not None and response.status != status:
        problems.append(f"状态码 {response.status}，应为 {status}")
    if isinstance(expected, dict) and isinstance(expected.get("status"), int) and response.status is not None \
            and response.status != expected["status"]:
        problems.append(f"状态码 {response.status}，页面上的响应体写的是 {expected['status']}")
    if content_type and not response.headers.get("content-type", "").startswith(content_type):
        problems.append(f"Content-Type 是 {response.headers.get('content-type')!r}，页面上是 {content_type}")
    try:
        actual = json.loads(response.body)
    except ValueError:
        problems.append("响应体不是 JSON")
    else:
        problems += compare_json(expected, actual, volatile or {})
    if problems:
        raise Failed(f"{where}：{'；'.join(problems)}。\n响应体：{response.body}")


def free_port(preferred: int) -> int:
    for port in (preferred, 0):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as probe:
            try:
                probe.bind(("127.0.0.1", port))
            except OSError:
                continue
            return probe.getsockname()[1]
    raise ToolError("找不到空闲端口。")


# ---------- 读者 ----------

class Settings(NamedTuple):
    feed: str                  # local 或 published
    version: str               # 这次装的版本
    documented: str            # 页面上写的版本
    packages: Optional[Path]   # local 的本地包源
    work: Path
    env: Dict[str, str]
    secrets: Dict[str, str]


class Result(NamedTuple):
    line: str
    command: Optional[str]  # 实际执行的命令；没执行的是 None
    code: int
    output: str


class Reader:
    """照一篇页面做一遍：终端执行页面上的命令，文件取自 samples/docs/，dotnet run 另起进程。"""

    def __init__(self, example: Example, page: Page, settings: Settings) -> None:
        self.example, self.page, self.settings = example, page, settings
        self.root = settings.work / example.slug
        self.root.mkdir(parents=True)
        self.logs = settings.work / "logs"
        self.exports: Dict[str, str] = {}
        self.replacements: List[Tuple[str, str]] = []
        if settings.documented != settings.version:
            self.replacements.append((settings.documented, settings.version))
        self.servers = 0
        self.holders: List[Path] = []
        self.terminal = Terminal(self.root, settings.env, self.logs / f"{example.slug}.terminal.log")

    def close(self) -> None:
        self.terminal.close()
        for holder in self.holders:
            holder.unlink(missing_ok=True)

    def block(self, name: str) -> Block:
        return self.page.find(self.example.blocks[name])

    def json(self, name: str) -> object:
        return json.loads(self.block(name).text)

    def path(self, relative: str) -> Path:
        return self.root / relative

    def place(self, relative: str) -> None:
        """把 samples/docs/<页面>/<relative> 放到读者目录的同一位置。"""
        target = self.path(relative)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(ROOT / EXAMPLES / self.example.slug / relative, target)
        say(f"  写入 {relative}（samples/docs/{self.example.slug}/{relative}）")

    def write(self, relative: str, text: str) -> None:
        self.path(relative).write_text(text, encoding="utf-8")

    def read(self, relative: str) -> str:
        return self.path(relative).read_text(encoding="utf-8")

    def use_port(self, address: str) -> None:
        """页面上写死的本机端口被占用时换一个空闲端口，页面上的命令跟着换。"""
        host, _, port = address.rpartition(":")
        actual = free_port(int(port))
        if actual != int(port):
            self.replacements.append((address, f"{host}:{actual}"))
            say(f"  端口 {port} 被占用，页面上的 {address} 换成 {host}:{actual}")

    def secret(self, name: str) -> str:
        """页面上 export 读包令牌的那一行：值经临时文件交给终端，不进命令行与日志。"""
        value = self.settings.secrets.get(name)
        if not value:
            raise ToolError(f"缺环境变量 {name}：published 模式装平台档的包要它。")
        holder = self.settings.work / f".{name.lower()}"
        holder.touch(mode=0o600)
        holder.write_text(value, encoding="utf-8")
        self.holders.append(holder)
        self.exports[name] = value
        quoted = shlex.quote(str(holder))
        return f'export {name}="$(cat {quoted})"; rm -f {quoted}'

    def prepare(self, line: str) -> Optional[str]:
        """页面上的一行命令换成这次执行的；None 是这一行在本模式下不执行。"""
        for old, new in self.replacements:
            line = line.replace(old, new)
        words = shlex.split(line)
        local = self.settings.feed == "local"
        if local and clones_release(words):
            say(f"  （本地包源：不克隆发布标签，清单工具用当前检出构建）{line}")
            return None
        if local and downloads_release_config(words):
            say(f"  （本地包源：不下载发布页的包源映射，用工作目录的 NuGet.config）{line}")
            return None
        if local:
            line = line.replace(f"kun-v{self.settings.version}/", shlex.quote(str(ROOT)) + "/")
        export = re.fullmatch(r"export (KUN_PLATFORM_FEED_USER|KUN_PLATFORM_FEED_TOKEN)=<[^>]+>", line)
        if export:
            if local:
                say(f"  （本地包源：不要读包令牌）{line}")
                return None
            say(f"  {line}（值取自同名环境变量）")
            return self.secret(export.group(1))
        if re.search(r"=<[^>]*>|(?:^|\s)<[^>\s/][^>]*>", line):
            raise Failed(f"`{line}` 里有占位符，示例给不出它的值。")
        return line

    def execute(self, command: str, timeout: float) -> Tuple[int, str]:
        attempts = 4 if self.settings.feed == "published" and command.startswith("dotnet add package") else 1
        for attempt in range(1, attempts + 1):
            code, output = self.terminal.run(command, timeout)
            if code == 0 or attempt == attempts or not re.search(r"NU110[123]|NU1301", output):
                return code, output
            say(f"  包源还没有这个包或连不上（第 {attempt} 次），30 秒后重试")
            time.sleep(30)
        raise AssertionError("unreachable")

    def commands(self, name: str, expect_codes: Optional[Dict[int, int]] = None, timeout: float = COMMAND_TIMEOUT) -> List[Result]:
        """按顺序执行页面上一块命令；expect_codes 按命令的序号给出不为 0 的退出码。"""
        block = self.block(name)
        results: List[Result] = []
        for number, line in enumerate(command_lines(block.text)):
            command = self.prepare(line)
            if command is None:
                results.append(Result(line, None, 0, ""))
                continue
            if not command.startswith("export "):
                say(f"  $ {command}")
            code, output = self.execute(command, timeout)
            wanted = (expect_codes or {}).get(number, 0)
            if code != wanted:
                raise Failed(f"{self.page.name}:{block.line} 的 `{line}` 退出码 {code}，应为 {wanted}：\n{tail(output)}")
            results.append(Result(line, command, code, output))
        return results

    def assign(self, name: str, values: Dict[str, str]) -> None:
        """页面上「把地址和令牌放进变量」的那块：换成实际的值。"""
        block = self.block(name)
        names = []
        for line in command_lines(block.text):
            match = re.fullmatch(r"([A-Z_][A-Z0-9_]*)=(.*)", line)
            if not match:
                raise Failed(f"{self.page.name}:{block.line} 这块应只有变量赋值，却有 `{line}`。")
            if match.group(1) not in values:
                raise Failed(f"{self.page.name}:{block.line} 的变量 {match.group(1)} 示例给不出值。")
            names.append(match.group(1))
            say(f"  {match.group(1)}=…（{line}）")
            self.terminal.run(f"{match.group(1)}={shlex.quote(values[match.group(1)])}", 30)
        if set(values) - set(names):
            raise Failed(f"{self.page.name}:{block.line} 少了变量 {'、'.join(sorted(set(values) - set(names)))}。")

    def http(self, name: str) -> Tuple[str, Response]:
        """执行页面上的一条 curl，读出状态码、响应头与响应体。"""
        result, = self.commands(name)
        assert result.command is not None
        return result.command, curl_response(result.command, result.output)

    @contextlib.contextmanager
    def serve(self, name: str) -> Iterator[Server]:
        block = self.block(name)
        lines = command_lines(block.text)
        if len(lines) != 1:
            raise Failed(f"{self.page.name}:{block.line} 应只有一条启动命令。")
        command = self.prepare(lines[0])
        assert command is not None
        code, cwd = self.terminal.run("pwd", 30)
        self.servers += 1
        log = self.logs / f"{self.example.slug}.server-{self.servers}.log"
        say(f"  $ {command}（另一个终端，日志 logs/{log.name}）")
        server = Server(command, Path(cwd.strip()), {**self.settings.env, **self.exports}, log)
        try:
            server.start()
            say(f"    监听 {server.url}")
            yield server
        finally:
            server.stop()


# ---------- 三篇页面的步骤 ----------

def run_getting_started(reader: Reader) -> None:
    say("1 建项目，装包")
    reader.commands("install")
    say("2 写一个命令")
    reader.place("Hello.Kun/Program.cs")
    say("3 跑起来")
    reader.use_port("127.0.0.1:5199")
    with reader.serve("run"):
        command, response = reader.http("create")
        expect(response, reader.json("created"), f"`{command}`", status=200, volatile={"id": GUID_V7})
        command, response = reader.http("create")
        expect(response, reader.json("taken"), f"同一个标题再发一次 `{command}`", volatile={"trace_id": TRACE_ID},
               content_type="application/problem+json")
        command, response = reader.http("missing")
        expect(response, reader.json("not-found"), f"`{command}`", volatile={"trace_id": TRACE_ID})
    say("文案的语言：装上请求本地化，带 Accept-Language: en 再发一次重复的标题")
    program = reader.read("Hello.Kun/Program.cs")
    reader.write("Hello.Kun/Program.cs", localized(program, reader.block("localization").text))
    with reader.serve("run") as server:
        headers = {"Content-Type": "application/json", "Accept": "application/json"}
        payload = json.dumps({"title": "周报"}, ensure_ascii=False).encode("utf-8")
        first = request("POST", server.url + "/notes", headers, payload)
        if first.status != 200:
            raise Failed(f"装上请求本地化后新建笔记：状态码 {first.status}，应为 200。\n{first.body}")
        second = request("POST", server.url + "/notes", {**headers, "Accept-Language": "en"}, payload)
        title = json.loads(second.body).get("title") if second.body.startswith("{") else None
        if (second.status, second.headers.get("content-language"), title) != (409, "en", "Title already exists"):
            raise Failed("带 Accept-Language: en 发重复的标题：应是 409、Content-Language: en、title 为 Title already exists，"
                         f"实际是 {second.status}、{second.headers.get('content-language')!r}、{title!r}。\n{second.body}")


def run_write_a_component(reader: Reader) -> None:
    manifest = "notes-demo/notes/.qian-plugin/plugin.json"
    say("1 建类库，装包")
    reader.commands("install")
    say("2 写入口与能力")
    reader.place("notes-demo/Acme.Notes/NotesPlugin.cs")
    say("3 手写清单")
    reader.commands("folders")
    reader.place(manifest)
    say("4 用清单工具派生")
    reader.commands("build")
    first = reader.commands("check", expect_codes={2: 1})
    problems = compare_json(reader.json("drift"), last_json(first[2].output), {})
    sync = reader.commands("sync")
    problems += [f"第二次 check：{problem}" for problem in compare_json(WAC_IN_SYNC, last_json(sync[1].output), {})]
    synced = json.loads(reader.read(manifest))
    for key, value in dict(reader.json("synced")).items():  # type: ignore[call-overload]
        problems += [f"同步后的清单：{problem}" for problem in compare_json(value, synced.get(key), {}, f"$.{key}")]
    tool = json.loads(reader.read("notes-demo/notes/tools/note.create.json"))
    problems += [f"tools/note.create.json：{problem}" for problem in subset_problems(reader.json("tool"), tool)]
    problems += [f"tools/note.create.json 没有 {field}" for field in WAC_TOOL_FIELDS if field not in tool]
    if problems:
        raise Failed("清单工具派生的结果与页面不符：" + "；".join(problems) + "。")
    say("  信任等级写成别的（private）时 check 退出 2，报 " + WAC_TIER_POINTER)
    original = reader.read(manifest)
    changed = json.loads(original)
    changed["policy"]["tier"] = "private"
    reader.write(manifest, json.dumps(changed, ensure_ascii=False, indent=2) + "\n")
    command = first[2].command
    assert command is not None
    code, output = reader.terminal.run(command, COMMAND_TIMEOUT)
    if code != 2 or WAC_TIER_POINTER not in output:
        raise Failed(f"信任等级写成 private 时 `{command}` 应退出 2 并报 {WAC_TIER_POINTER}，实际退出码 {code}：\n{tail(output)}")
    reader.write(manifest, original)
    say("5 装进宿主")
    reader.commands("host")
    reader.place("notes-demo/NotesHost/Program.cs")
    result, = reader.commands("run-host")
    expected = reader.block("host-output").text.strip()
    if result.output.strip() != expected:
        raise Failed(f"宿主的输出与页面不符。\n页面：\n{expected}\n实际：\n{tail(result.output)}")


def run_authorization(reader: Reader) -> None:
    program = "Notes.Authz/Program.cs"
    say("1 建项目，装包")
    reader.commands("install")
    say("2 写命令与判定器")
    reader.place(program)
    base = reader.read(program)
    say("3 签本机测试令牌")
    tokens: Dict[str, str] = {}
    for result in reader.commands("tokens"):
        match = re.search(r"user-jwts create --name (\S+)", result.line)
        if match:
            token = result.output.strip().split("\n")[-1].strip()
            if not JWT.fullmatch(token):
                raise Failed(f"`{result.line}` 没有打印出令牌：\n{tail(result.output)}")
            tokens[match.group(1)] = token
    say("4 跑起来")
    with reader.serve("run") as server:
        values = {"BASE": server.url, **{name.upper(): token for name, token in tokens.items()}}
        reader.assign("variables", values)
        results = reader.commands("requests")
        expected = [("unauthenticated", None, {"trace_id": TRACE_ID}), ("denied", None, {"trace_id": TRACE_ID}),
                    ("exported", 200, {})]
        if len(results) != len(expected):
            raise Failed(f"「4 跑起来」应有 {len(expected)} 条请求，现有 {len(results)} 条。")
        for result, (name, status, volatile) in zip(results, expected):
            assert result.command is not None
            expect(curl_response(result.command, result.output), reader.json(name), f"`{result.line}`", status=status,
                   volatile=volatile)
    say("忘了接执行点：删掉 AddPermissionBehavior 那一行再请求")
    reader.write(program, remove(base, AZ_BEHAVIOR))
    with reader.serve("run") as server:
        response = request("POST", server.url + "/notes/export", {"Authorization": "Bearer " + tokens["alice"]})
        code = json.loads(response.body).get("code") if response.body.startswith("{") else None
        if (response.status, code) != (500, "INTERNAL"):
            raise Failed(f"漏装执行点时应是 500 的 INTERNAL，实际是 {response.status}、{code!r}。\n{response.body}")
        server.wait_for(re.compile(re.escape(AZ_MISSING_BEHAVIOR)), 30)
    reader.write(program, base)
    say("5 再接 HTTP 端点")
    reader.commands("platform-package")
    reader.write(program, with_http_endpoint(base, reader.block("using").text, reader.block("register").text,
                                             reader.block("endpoint").text))
    with reader.serve("run") as server:
        server.wait_for(re.compile(re.escape(AZ_LIBRARY_MODE)), 30)
        table = reader.page.table("5 再接 HTTP 端点")
        endpoints = [re.search(r"`(GET|POST|PUT|DELETE) (/\S*)`", cell) for cell in table[0][1:]]
        if not table[1:] or not all(endpoints):
            raise Failed(f"「5 再接 HTTP 端点」的表头应是「调用方 | `方法 路径`…」：{table[0]}")
        problems = []
        for row in table[1:]:
            caller = re.split(r"[（(\s]", row[0])[0]
            if caller != "不带令牌" and caller not in tokens:
                raise Failed(f"「5 再接 HTTP 端点」的表里有调用方 {row[0]}，第 3 步没有给它签令牌。")
            headers = {} if caller == "不带令牌" else {"Authorization": "Bearer " + tokens[caller]}
            for match, cell in zip(endpoints, row[1:]):
                assert match is not None
                method, route = match.group(1), match.group(2)
                response = request(method, server.url + route, headers)
                if str(response.status) != cell:
                    problems.append(f"{row[0]} {method} {route} 是 {response.status}，表里是 {cell}")
        if problems:
            raise Failed("接上 HTTP 端点后的四种身份：" + "；".join(problems) + "。")


FLOWS: Dict[str, Callable[[Reader], None]] = {
    GETTING_STARTED.slug: run_getting_started,
    WRITE_A_COMPONENT.slug: run_write_a_component,
    AUTHORIZATION.slug: run_authorization,
}


# ---------- run ----------

def reader_environment(work: Path) -> Dict[str, str]:
    """读者的环境：HOME 与 NuGet 的目录都在工作目录里；不带读包令牌；构建完不留常驻进程；
    不带机器的语言设置与 .NET 的全球化开关（界面语言由终端与服务进程固定，页面假定 ICU 正常可用）。"""
    dropped = ("MSBUILD", "MSBuild", "NUGET_", "ASPNETCORE_", "DOTNET_ENVIRONMENT", "DOTNET_USER_SECRETS", "APPDATA",
               "DOTNET_SYSTEM_GLOBALIZATION_")
    env = without_locale({key: value for key, value in os.environ.items() if key not in SECRETS and not key.startswith(dropped)})
    nuget = work / ".nuget"
    env.update({
        "HOME": str(work / "home"),
        "NUGET_PACKAGES": str(nuget / "packages"),
        "NUGET_HTTP_CACHE_PATH": str(nuget / "http-cache"),
        "NUGET_SCRATCH": str(nuget / "scratch"),
        "NUGET_PLUGINS_CACHE_PATH": str(nuget / "plugins-cache"),
        "DOTNET_NOLOGO": "1",
        "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
        "DOTNET_SKIP_FIRST_TIME_EXPERIENCE": "1",
        "DOTNET_GENERATE_ASPNET_CERTIFICATE": "false",
        "MSBUILDDISABLENODEREUSE": "1",
        "DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER": "1",
        "UseSharedCompilation": "false",
        "TERM": "dumb",
    })
    (work / "home").mkdir(parents=True, exist_ok=True)
    # 读者在终端里跑 curl 看不到进度条；这里的输出接的是管道，curl 会把进度条写进标准错误，与响应混在一起。
    (work / "home" / ".curlrc").write_text("--no-progress-meter\n", encoding="utf-8")
    return env


def local_config(packages: Path) -> str:
    root = ET.Element("configuration")
    sources = ET.SubElement(root, "packageSources")
    ET.SubElement(sources, "clear")
    ET.SubElement(sources, "add", key="kun-local", value=str(packages))
    ET.SubElement(sources, "add", key="nuget.org", value=NUGET_ORG)
    mapping = ET.SubElement(root, "packageSourceMapping")
    ET.SubElement(ET.SubElement(mapping, "packageSource", key="kun-local"), "package", pattern="Kun.*")
    ET.SubElement(ET.SubElement(mapping, "packageSource", key="nuget.org"), "package", pattern="*")
    ET.indent(root) if hasattr(ET, "indent") else None
    return ET.tostring(root, encoding="unicode") + "\n"


def platform_ids(config: Path) -> List[str]:
    """发布页 NuGet.config 里映射到 GitHub Packages 的包。"""
    if not config.is_file():
        return []
    document = ET.parse(config).getroot()
    keys = {item.get("key") for item in document.iter("add") if GITHUB_PACKAGES in (item.get("value") or "")}
    return sorted((item.get("pattern") or "").lower() for source in document.iter("packageSource") if source.get("key") in keys
                  for item in source.iter("package"))


def source_problems(settings: Settings, platform: Sequence[str]) -> Tuple[List[str], int]:
    """装进缓存的 Kun 包：版本是这次的版本，来源与页面写的一致。"""
    problems: List[str] = []
    cache = Path(settings.env["NUGET_PACKAGES"])
    metadata = sorted(cache.glob("kun.*/*/.nupkg.metadata"))
    for path in metadata:
        identity, version = path.parent.parent.name, path.parent.name
        source = (json.loads(path.read_text(encoding="utf-8")).get("source") or "").rstrip("/")
        if version != settings.version.lower():
            problems.append(f"{identity} 装的是 {version}，应为 {settings.version}")
        if settings.feed == "local":
            wanted = str(settings.packages).rstrip("/")
        elif identity in platform:
            wanted = "GitHub Packages"
            if GITHUB_PACKAGES not in source:
                problems.append(f"平台档的 {identity} 来自 {source}，页面上写的是 GitHub Packages")
            continue
        else:
            wanted = NUGET_ORG
        if source != wanted.rstrip("/"):
            problems.append(f"{identity} 来自 {source}，应来自 {wanted}")
    return problems, len(metadata)


def annotate(kind: str, title: str, message: str) -> None:
    """GitHub Actions 的运行注记（error、notice），显示在运行的摘要页上。"""
    escaped = message.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")
    say(f"::{kind} title={title}::{escaped}")


def summary(lines: Sequence[str]) -> None:
    target = os.environ.get("GITHUB_STEP_SUMMARY")
    if target:
        with open(target, "a", encoding="utf-8") as handle:
            handle.write("\n".join(lines) + "\n")


def version_key(version: str) -> Tuple[int, int, int, int, int]:
    """发布版本的先后：按日期；同一天 beta 在前、正式版居中、hotfix 在后，编号按数字比。"""
    match = re.fullmatch(r"(\d{4})\.(\d{1,2})\.(\d{1,2})(?:-(beta|hotfix)\.(\d+))?", version)
    if not match:
        raise ToolError(f"版本 {version} 不是发布版本的写法。")
    stage = {"beta": 0, None: 1, "hotfix": 2}[match.group(4)]
    return int(match.group(1)), int(match.group(2)), int(match.group(3)), stage, int(match.group(5) or 0)


def bump_hint(version: str, documented: str, sdk: str, today: str) -> List[str]:
    """发版后照页面跑通了：这次的版本比页面上写的新，给出把页面改过去的命令；更旧的不建议往回改；相同的什么也不说。"""
    if version_key(version) > version_key(documented):
        return ["", f"示例用 {version} 照页面跑通了，页面上写的还是 {documented}。把页面改到 {version}：", "",
                f"`python3 scripts/docs_examples.py bump --version {version} --sdk {sdk} --date {today}`", "",
                "再跑一遍 check，提 PR。"]
    if version_key(version) < version_key(documented):
        return ["", f"这次的版本 {version} 比页面上写的 {documented} 旧，页面不用改。"]
    return []


def run(selected: Sequence[Example], feed_kind: str, version: Optional[str], packages: Optional[Path],
        work: Optional[Path], keep: bool) -> int:
    problems = check()
    if problems:
        for problem in problems:
            say(problem)
        say("页面与示例对不上，先改好再跑（python3 scripts/docs_examples.py check）。")
        return 1
    pages = load_pages()
    documented = documented_version(list(pages.values()))
    version = version or documented
    if not RELEASE.fullmatch(version):
        raise ToolError(f"版本 {version} 不是发布版本的写法。")
    if feed_kind == "local":
        if packages is None or not packages.is_dir():
            raise ToolError("local 模式要 --packages <本地包源目录>（先跑 feed）。")
        packages = packages.resolve()
        if not list(packages.glob(f"Kun.*.{version}.nupkg")):
            raise ToolError(f"{packages} 里没有版本 {version} 的 Kun 包。")
    secrets = {key: os.environ[key] for key in SECRETS if os.environ.get(key)}
    if feed_kind == "published" and AUTHORIZATION in selected and len(secrets) != len(SECRETS):
        raise ToolError("published 模式要环境变量 KUN_PLATFORM_FEED_USER 与 KUN_PLATFORM_FEED_TOKEN（在 .NET 里接权限第 5 步装平台档的包）。")
    created = work is None
    work = Path(tempfile.mkdtemp(prefix="kun-docs-examples-")) if work is None else work.resolve()
    if work == ROOT or ROOT in work.parents:
        raise ToolError("工作目录要在仓库外面：仓库里的 NuGet.config、构建基线会影响读者的项目。")
    if work.exists() and any(work.iterdir()) and not created:
        raise ToolError(f"{work} 不是空目录。")
    work.mkdir(parents=True, exist_ok=True)
    env = reader_environment(work)
    if feed_kind == "local":
        assert packages is not None
        (work / "NuGet.config").write_text(local_config(packages), encoding="utf-8")
    shutil.copyfile(ROOT / "global.json", work / "global.json")
    sdk = subprocess.run(["dotnet", "--version"], cwd=work, env=env, capture_output=True, text=True).stdout.strip()
    settings = Settings(feed_kind, version, documented, packages, work, env, secrets)
    origin = (f"本地包源 {packages}（当前源码，版本 {version}）" if feed_kind == "local"
              else f"已发布的包（版本 {version}：内核档 nuget.org，平台档 GitHub Packages）")
    say(f"照页面跑示例：{origin}；页面上写的版本 {documented}；.NET SDK {sdk or '?'}；工作目录 {work}")
    outcomes: List[Tuple[Example, Optional[str], float]] = []
    for example in selected:
        say()
        say(f"== {example.slug}（{example.page}）")
        started = time.monotonic()
        reader = Reader(example, pages[example.slug], settings)
        try:
            FLOWS[example.slug](reader)
            outcomes.append((example, None, time.monotonic() - started))
            say(f"== {example.slug}：通过")
        except Failed as error:
            outcomes.append((example, str(error), time.monotonic() - started))
            say(f"== {example.slug}：不通过\n{error}")
            if os.environ.get("GITHUB_ACTIONS"):
                annotate("error", f"使用文档示例 {example.slug}", str(error).splitlines()[0])
        finally:
            reader.close()
    platform = platform_ids(work / AUTHORIZATION.slug / "Notes.Authz" / "NuGet.config")
    sources, count = source_problems(settings, platform)
    if sources:
        outcomes.append((Example("包来源", "", {}, {}, {}, (), (), {}), "；".join(sources), 0.0))
    failed = [outcome for outcome in outcomes if outcome[1]]
    lines = ["## 使用文档的示例", "", f"包：{origin}；页面上写的版本：{documented}；.NET SDK：{sdk or '?'}。", "",
             "| 示例 | 页面 | 结果 | 用时 |", "|---|---|---|---|"]
    for example, error, elapsed in outcomes:
        result = ("不通过：" + error.splitlines()[0]).replace("|", "\\|") if error else "通过"
        lines.append(f"| {example.slug} | `{example.page}` | {result} | {elapsed:.0f} 秒 |")
    lines += ["", f"缓存里的 Kun 包 {count} 个，版本与来源{'有问题' if sources else '都对'}。"]
    if failed and feed_kind == "published":
        lines += ["", "发布已经完成，这一步不撤回它，也不要因此重跑整个发布。按上面失败的那一步改页面与示例（samples/docs/），"
                      "或者出补丁版本。"]
    elif not failed and feed_kind == "published":
        hint = bump_hint(version, documented, sdk, datetime.date.today().isoformat())
        lines += hint
        if version_key(version) > version_key(documented) and os.environ.get("GITHUB_ACTIONS"):
            annotate("notice", "使用文档示例", f"示例用 {version} 照页面跑通了，页面上写的还是 {documented}；运行摘要里有把页面改过去的命令。")
    say()
    for line in lines:
        say(line)
    summary(lines)
    if not failed and created and not keep:
        shutil.rmtree(work, ignore_errors=True)
    elif created:
        say(f"工作目录留着：{work}")
    return 1 if failed else 0


# ---------- bump ----------

def bump(version: str, sdk: str, date: str, root: Path = ROOT) -> List[str]:
    """把三篇页面改到 version；返回改过的页面。"""
    if not RELEASE.fullmatch(version) or "-beta." in version:
        raise ToolError(f"{version} 不是能写进页面的版本（YYYY.M.D 或 YYYY.M.D-hotfix.N；beta 不发 nuget.org）。")
    if not re.fullmatch(r"\d+\.\d+\.\d+", sdk):
        raise ToolError(f"SDK 版本 {sdk} 的写法不对，应像 10.0.404。")
    datetime.date.fromisoformat(date)
    pages = load_pages(root)
    documented = documented_version(list(pages.values()))
    old, new = documented.split("-")[0], version.split("-")[0]

    def replace(match: "re.Match[str]") -> str:
        return new + ".0" if match.group(2) == ".0" else version

    changed = []
    for page in pages.values():
        text = VERSION_TOKEN.sub(replace, page.text)
        text, count = RAN.subn(lambda match: f"在 {date} 用 .NET SDK {sdk}、Kun {version} 照着跑过一遍", text)
        if count != 1:
            raise Failed(f"{page.name} 里应有一句「照着跑过一遍」，现有 {count} 句。")
        text = re.sub(r"\A(---\n(?:.*\n)*?)updated: .*\n", lambda match: f"{match.group(1)}updated: {date}\n", text, count=1)
        if text != page.text:
            page.path.write_text(text, encoding="utf-8")
            changed.append(page.name)
    say(f"页面上的 Kun 版本 {documented} → {version}（程序集版本 {old}.0 → {new}.0），日期 {date}，SDK {sdk}。")
    return changed


# ---------- 入口 ----------

def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("check", help="页面与示例文件逐字一致等不要 .NET 的核对")
    feeding = commands.add_parser("feed", help="用当前源码打出页面上要装的包，当本地包源")
    feeding.add_argument("--version", required=True)
    feeding.add_argument("--out", type=Path, required=True)
    running = commands.add_parser("run", help="照页面建项目、装包、编译、运行并核对输出")
    running.add_argument("--feed", choices=("local", "published"), required=True)
    running.add_argument("--version", help="要装的 Kun 版本；缺省用页面上写的版本")
    running.add_argument("--packages", type=Path, help="local 模式的本地包源目录")
    running.add_argument("--work", type=Path, help="工作目录（仓库外、空的）；缺省用临时目录，通过后删掉")
    running.add_argument("--keep", action="store_true", help="通过后也留着临时工作目录")
    running.add_argument("--only", action="append", choices=[example.slug for example in ALL], help="只跑这篇页面，可以写多次")
    bumping = commands.add_parser("bump", help="把三篇页面里的 Kun 版本、日期与 SDK 版本改成给定值")
    bumping.add_argument("--version", required=True)
    bumping.add_argument("--sdk", required=True)
    bumping.add_argument("--date", default=datetime.date.today().isoformat())
    args = parser.parse_args(argv)
    try:
        if args.command == "check":
            problems = check()
            for problem in problems:
                say(problem)
            if problems:
                return 1
            pages = load_pages()
            blocks = sum(len(page.blocks) for page in pages.values())
            say(f"使用文档的示例核对通过：三篇页面 {blocks} 个代码块，Kun 版本 {documented_version(list(pages.values()))}。")
            return 0
        if args.command == "feed":
            feed(args.version, args.out)
            return 0
        if args.command == "run":
            selected = [example for example in ALL if not args.only or example.slug in args.only]
            return run(selected, args.feed, args.version, args.packages, args.work, args.keep)
        changed = bump(args.version, args.sdk, args.date)
        problems = check()
        for problem in problems:
            say(problem)
        say(f"改了 {len(changed)} 篇页面：{'、'.join(changed) or '无'}。")
        return 1 if problems else 0
    except Failed as error:
        say(str(error))
        return 1
    except ToolError as error:
        print(f"docs_examples：{error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
