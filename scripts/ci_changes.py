#!/usr/bin/env python3
"""ci.yml 的区域表、变更检测与「CI 汇总」判定（S1-KUN-020）。只依赖标准库。

.github/workflows/ci.yml 调用：

  python3 scripts/ci_changes.py detect --base <提交> --head <提交>
  python3 scripts/ci_changes.py detect --all
      列出 base 到 head 改动的文件（git diff --name-only --no-renames），按区域表算出每个区域是否改动，
      写进 $GITHUB_OUTPUT（每个区域一行 <区域>=true|false）与作业摘要。推 main 与手动触发时传 --all，全部为真。
  python3 scripts/ci_changes.py summary
      「CI 汇总」作业用：读环境变量 NEEDS（ci.yml 里的 toJSON(needs)）。变更检测没有成功、或任一作业失败或取消，
      即不通过；跳过算通过。各区域与各作业的结果写进作业摘要。
  python3 scripts/ci_changes.py files <区域>
      列出命中该区域的被跟踪文件，排查用。

匹配式照 GitHub Actions 路径过滤的写法：** 跨目录（可以是零层），* 不跨目录，! 开头的是排除，
同一组里最后一个命中的匹配式说了算。一个区域由若干组组成，任一组命中即算改动；组与组分开算，
一组里的排除不影响别的组。EVERYTHING 里的文件一改，全部区域都算改动。

区域表的自测在 scripts/tests/test_ci_changes.py，ci.yml 的结构自测在 scripts/tests/test_ci_workflow.py，
ci.yml 的变更检测作业每次都先跑这两组自测。

退出码：0 通过；1 不通过；2 工具出错。
"""
from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Dict, Iterable, List, NamedTuple, Optional, Sequence, Tuple

ROOT = Path(__file__).resolve().parents[1]
Q = "_import/qian/"


def q(*patterns: str) -> Tuple[str, ...]:
    """_import/qian/ 下的匹配式：逐条加前缀，! 留在最前。"""
    return tuple("!" + Q + pattern[1:] if pattern.startswith("!") else Q + pattern for pattern in patterns)


# 凡要构建 .NET 的区域都含这一组：_import/qian 下的项目从源码编译 src/ 下的 Kun 项目，
# 仓库根的构建基线、中央包版本、包源、格式规则、禁用符号清单与 SDK 版本一改，构建就跟着变。
DOTNET_BASE: Tuple[str, ...] = ("src/**", "Kun.slnx", "Directory.Build.props", "Directory.Build.targets",
                                "Directory.Packages.props", "global.json", "NuGet.config", ".editorconfig",
                                "BannedSymbols.txt")
# 装 GitHub Packages 上的 @cavailab/qian-* 的区域都含这一组：包源写在这个文件里。
NPMRC: Tuple[str, ...] = (Q + ".npmrc",)
# 这些文件一改，全部区域都算改动：作业的定义、区域表本身或共用的复合动作变了。
EVERYTHING: Tuple[str, ...] = (".github/workflows/ci.yml", ".github/actions/**", "scripts/ci_changes.py")
# 不落进任何区域、改了不跑检查的文件。
NO_CHECK: Tuple[str, ...] = ("*.md", "LICENSE", ".gitignore", "docs/**", "**/*.md", ".github/originals/**")


class Area(NamedTuple):
    title: str
    groups: Tuple[Tuple[str, ...], ...]
    # 过滤搬自哪个工作流（_import/qian/.github/workflows/ 下的文件名）：自测核对本区域覆盖它的 pull_request 过滤。
    original: Optional[str] = None
    # 要构建 .NET：必须含 DOTNET_BASE。
    builds_dotnet: bool = False
    # 装 GitHub Packages 上的 @cavailab/qian-*：必须含 _import/qian/.npmrc。
    installs_npm: bool = False


AREAS: Dict[str, Area] = {
    "dotnet": Area(
        "构建、门禁、测试与原生发布",
        (
            q("**", "!pm/**", "!docs/**", "!*.md", "docs/generated/**", "docs/ops/configuration-runbook.md"),
            # 仓库根的 .NET 代码、宿主、测试、样例、基准与脚本；Markdown 不算，打进网关包的 GATEWAY.md 除外。
            ("src/**", "hosts/**", "tests/**", "samples/**", "benchmarks/**", "tools/**", "scripts/**", "!**/*.md",
             "src/Kun.Gateway/GATEWAY.md", ".gitattributes"),
            DOTNET_BASE,
        ),
        original="dotnet.yml", builds_dotnet=True),
    "images": Area(
        "镜像自检",
        (
            q("deploy/images/**", "tools/Qian.Image.SelfCheck*/**", "tools/Qian.Build.Analyzers/**", "scripts/images.py",
              "scripts/prepare_image_tools.py", "scripts/image-tools.lock.json", "tests/gates/test_images.py",
              "profiles/**", "plugins/**", "packages/Qian.Profiles*/**", "packages/Qian.Surfaces*/**",
              "packages/Common/**", "schemas/**",
              "contracts/v0/**", "*.props", "*.targets", ".editorconfig", ".github/workflows/images.yml"),
            DOTNET_BASE,
        ),
        original="images.yml", builds_dotnet=True),
    "docs-and-pm": Area(
        "文档门禁、任务文件与看板",
        (q("**"), (".github/workflows/qian-docs-and-pm.yml",)),
        original="docs-and-pm.yml"),
    "docs-site": Area(
        "文档站",
        (
            q("website/**", "apps/qian-shell/e2e/settle.ts", "docs/**", "package.json", "pnpm-lock.yaml",
              "pnpm-workspace.yaml", "deploy/docs-site/**", "Directory.Packages.props", ".github/workflows/docs-site.yml"),
            # 仓库根的 CHANGELOG.md：Kun 最新的发布版从这里取（_import/qian/website/scripts/kun.ts）。
            (".github/workflows/qian-docs-site.yml", "CHANGELOG.md"),
            DOTNET_BASE, NPMRC,
        ),
        original="docs-site.yml", builds_dotnet=True, installs_npm=True),
    "documentation": Area(
        "派生文档与事件模式",
        (
            q("packages/**", "plugins/**", "profiles/**", "tools/Qian.Docs.Tool/**", "tests/Qian.Docs.Tests/**",
              "schemas/**", "tck/runtime/**", "docs/generated/**", "*.props", "*.targets", "Qian.slnf",
              "package.json", "pnpm*", ".github/workflows/documentation.yml"),
            (".github/workflows/qian-documentation.yml",),
            DOTNET_BASE, NPMRC,
        ),
        original="documentation.yml", builds_dotnet=True, installs_npm=True),
    "dev-tenant-ops": Area(
        "租户运营手测环境",
        (
            q("packages/**", "samples/**", "plugins/**", "schemas/**", "contracts/**", "tools/Qian.Dev.TenantOps/**",
              "tools/Qian.Build.Analyzers/**", "scripts/dev-tenant-ops.sh", "*.props", "*.targets",
              ".editorconfig", ".github/workflows/dev-tenant-ops.yml"),
            (".github/workflows/qian-dev-tenant-ops.yml",),
            DOTNET_BASE,
        ),
        original="dev-tenant-ops.yml", builds_dotnet=True),
    "directory-e2e": Area(
        "目录选择器验收",
        (
            q("tests/directory-e2e/**", "tests/gates/ui/pins.test.ts",
              "tests/Kun.Identity.Server.Tests/Contracts/biz.openapi.json", "packages/Qian.*/**", "packages/Common/**",
              "samples/Qian.Sample.Crm/**", "plugins/**", "schemas/**", "contracts/**", "tools/Qian.Dev.TenantOps/**",
              "tools/Qian.Build.Analyzers/**", "tools/openapi-codegen/**", "scripts/dev-tenant-ops.sh",
              "Directory.*.props", "Directory.Build.targets", ".editorconfig",
              "package.json", "pnpm-lock.yaml", "pnpm-workspace.yaml", "turbo.json", ".github/workflows/directory-e2e.yml"),
            (".github/workflows/qian-directory-e2e.yml",),
            DOTNET_BASE, NPMRC,
        ),
        original="directory-e2e.yml", builds_dotnet=True, installs_npm=True),
    "shell-auth": Area(
        "外壳认证验收",
        (
            q("apps/qian-shell/**", "packages/qian-shell-kit/**", "packages/qian-identity-ui/**",
              "samples/qian-sample-crm-ui/**", "packages/qian-tenancy-ui/**", "packages/qian-tenancy-signup-ui/**",
              "packages/Qian.*/**", "packages/Common/**", "samples/Qian.Sample.Crm/**", "plugins/**", "schemas/**",
              "contracts/**", "tools/Qian.Dev.Shell/**", "tools/Qian.Dev.TenantOps/**", "tools/Qian.Build.Analyzers/**",
              "scripts/dev-shell.sh", "Directory.*.props", "Directory.Build.targets",
              ".editorconfig", "package.json", "pnpm-lock.yaml", "pnpm-workspace.yaml", ".github/workflows/shell-auth.yml"),
            (".github/workflows/qian-shell-auth.yml",),
            DOTNET_BASE, NPMRC,
        ),
        original="shell-auth.yml", builds_dotnet=True, installs_npm=True),
    "shell-federation": Area(
        "外壳联邦验收",
        (
            q("apps/qian-shell/**", "packages/qian-shell-kit/**", "packages/qian-tenancy-ui/**", "plugins/qian.shell/**",
              "plugins/qian.tenancy/ui/**", "tests/Qian.Shell.TestHost/**", "tests/Qian.Testing.Containers/**",
              "tests/Shared/NativeAuditDatabase.cs", "packages/Qian.Audit*/**",
              "Directory.*", "pnpm-lock.yaml", "pnpm-workspace.yaml",
              "package.json", ".github/workflows/shell-federation.yml"),
            (".github/workflows/qian-shell-federation.yml",),
            DOTNET_BASE, NPMRC,
        ),
        original="shell-federation.yml", builds_dotnet=True, installs_npm=True),
    "surfaces": Area(
        "声明式表面",
        (
            q("packages/Qian.Surfaces*/**", "packages/qian-surfaces/**",
              "packages/Qian.Profiles*/**", "packages/qian-cli/**", "profiles/**", "plugins/**", "tck/profiles/**",
              "tck/registry-artifacts/**", "tests/Qian.Profiles.Tests/**", "package.json", "schemas/v2/**",
              "tck/surfaces/**", "tck/runtime/**", "tck/marketplaces/**", "contracts/v0/**", "tests/Qian.Surfaces.Tests/**",
              "scripts/generate_surface_fixtures.py", "packages/Common/**", "pnpm*", "*.props", "*.targets", "Qian.slnf",
              ".github/workflows/surfaces.yml"),
            (".github/workflows/qian-surfaces.yml",),
            DOTNET_BASE, NPMRC,
        ),
        original="surfaces.yml", builds_dotnet=True, installs_npm=True),
    "tenant-ui": Area(
        "租户运营页面验收",
        (
            q("packages/qian-tenancy-ui/**", "turbo.json", "packages/Qian.*/**", "packages/Common/**",
              "samples/Qian.Sample.Crm/**", "plugins/**", "schemas/**", "contracts/**", "tools/Qian.Dev.TenantOps/**",
              "tools/Qian.Build.Analyzers/**", "tools/openapi-codegen/**", "scripts/dev-tenant-ops.sh",
              "Directory.*.props", "Directory.Build.targets", ".editorconfig",
              "package.json", "pnpm-lock.yaml", "pnpm-workspace.yaml", ".github/workflows/tenant-ui.yml"),
            (".github/workflows/qian-tenant-ui.yml",),
            DOTNET_BASE, NPMRC,
        ),
        original="tenant-ui.yml", builds_dotnet=True, installs_npm=True),
    "ui": Area(
        "外壳与界面包门禁",
        (
            q("apps/**", "packages/qian-tenancy-ui/**", "packages/qian-tenancy-signup-ui/**",
              "packages/qian-identity-ui/**", "packages/qian-shell-kit/**", "samples/qian-sample-crm-ui/**",
              "tests/directory-e2e/**", "tests/gates/ui/**", "tests/Kun.Identity.Server.Tests/Contracts/biz.openapi.json",
              "plugins/qian.tenancy/contracts/**", "plugins/qian.tenancy.signup/contracts/**", "tools/openapi-codegen/**",
              "website/demo/**", "scripts/ui-tokens.mjs", "eslint.config.mjs", "eslint.boundaries.config.mjs",
              "eslint.i18n.mjs", ".dependency-cruiser.cjs", "vitest.config.ts", "tsconfig.base.json", "turbo.json",
              ".npmrc", "package.json", "pnpm-lock.yaml", "pnpm-workspace.yaml"),
            (".github/workflows/qian-ui.yml",),
        ),
        installs_npm=True),
}


class ToolError(Exception):
    """输入或环境不对，给不出结论（退出码 2）。"""


# ---------- 匹配 ----------

_REGEX: Dict[str, "re.Pattern[str]"] = {}


def glob_regex(pattern: str) -> "re.Pattern[str]":
    """GitHub Actions 路径过滤的通配：** 跨目录（**/ 可以是零层），* 不跨目录，其余字符照字面。"""
    cached = _REGEX.get(pattern)
    if cached is not None:
        return cached
    parts = []
    index = 0
    while index < len(pattern):
        if pattern.startswith("**/", index):
            parts.append("(?:.*/)?")
            index += 3
        elif pattern.startswith("**", index):
            parts.append(".*")
            index += 2
        elif pattern[index] == "*":
            parts.append("[^/]*")
            index += 1
        else:
            parts.append(re.escape(pattern[index]))
            index += 1
    regex = re.compile("".join(parts) + r"\Z")
    _REGEX[pattern] = regex
    return regex


def group_matches(patterns: Sequence[str], path: str) -> bool:
    """组内最后一个命中的匹配式说了算：! 开头排除，否则纳入。"""
    result = False
    for pattern in patterns:
        negative = pattern.startswith("!")
        if glob_regex(pattern[1:] if negative else pattern).match(path):
            result = not negative
    return result


def area_matches(area: Area, path: str) -> bool:
    return any(group_matches(group, path) for group in area.groups)


def everything(path: str) -> bool:
    return group_matches(EVERYTHING, path)


def changed_areas(paths: Iterable[str]) -> Dict[str, List[str]]:
    """每个区域命中的改动文件。EVERYTHING 里的文件算进全部区域。"""
    hits: Dict[str, List[str]] = {name: [] for name in AREAS}
    for path in paths:
        for name, area in AREAS.items():
            if everything(path) or area_matches(area, path):
                hits[name].append(path)
    return hits


# ---------- git ----------

def git(*args: str) -> str:
    result = subprocess.run(["git", *args], cwd=ROOT, capture_output=True, text=True)
    if result.returncode:
        raise ToolError(f"git {' '.join(args)} 失败：{result.stderr.strip()}")
    return result.stdout


def tracked() -> List[str]:
    return [name for name in git("ls-files", "-z").split("\0") if name]


def diff(base: str, head: str) -> List[str]:
    return [name for name in git("diff", "--name-only", "--no-renames", "-z", base, head).split("\0") if name]


# ---------- 输出 ----------

def append(variable: str, lines: Iterable[str]) -> None:
    target = os.environ.get(variable)
    if not target:
        return
    with open(target, "a", encoding="utf-8") as handle:
        for line in lines:
            handle.write(line + "\n")


def detect(base: Optional[str], head: str, everything_changed: bool) -> Dict[str, bool]:
    if everything_changed:
        changed: List[str] = []
        hits = {name: [] for name in AREAS}
        result = {name: True for name in AREAS}
        reason = "推 main 或手动触发：全部区域都跑。"
    else:
        if not base:
            raise ToolError("没给 --base，也没给 --all。")
        changed = diff(base, head)
        hits = changed_areas(changed)
        result = {name: bool(files) for name, files in hits.items()}
        reason = f"`{base}` 到 `{head}` 改动 {len(changed)} 个文件。"
    unclaimed = [path for path in changed
                 if not everything(path) and not any(area_matches(area, path) for area in AREAS.values())]
    lines = ["## 变更检测", "", reason, "", "| 区域 | 名称 | 跑不跑 | 命中的改动文件 |", "|---|---|---|---|"]
    for name, area in AREAS.items():
        files = hits[name]
        shown = "、".join(f"`{path}`" for path in files[:3]) + (f" 等 {len(files)} 个" if len(files) > 3 else "")
        lines.append(f"| `{name}` | {area.title} | {'跑' if result[name] else '跳过'} | {shown or '—'} |")
    if unclaimed:
        lines += ["", f"不落进任何区域的改动文件 {len(unclaimed)} 个（区域表的「无检查」名单）："
                  + "、".join(f"`{path}`" for path in unclaimed[:10])]
    for line in lines:
        print(line)
    append("GITHUB_STEP_SUMMARY", lines)
    append("GITHUB_OUTPUT", [f"{name}={'true' if value else 'false'}" for name, value in result.items()])
    return result


def summarize(needs: Dict[str, dict]) -> Tuple[List[str], List[str]]:
    """返回作业摘要的 Markdown 行与不通过的原因；原因为空才算通过。"""
    problems: List[str] = []
    changes = needs.get("changes")
    if changes is None:
        problems.append("needs 里没有变更检测作业 changes。")
    elif changes.get("result") != "success":
        problems.append(f"变更检测没有成功（{changes.get('result')}），各区域跑没跑无从判断。")
    outputs = (changes or {}).get("outputs") or {}
    lines = ["## CI 汇总", ""]
    if outputs:
        lines += ["| 区域 | 名称 | 跑不跑 |", "|---|---|---|"]
        for name, area in AREAS.items():
            value = outputs.get(name)
            lines.append(f"| `{name}` | {area.title} | {'跑' if value == 'true' else '跳过' if value == 'false' else '—'} |")
        lines.append("")
    lines += ["| 作业 | 结果 |", "|---|---|"]
    for job, data in needs.items():
        result = (data or {}).get("result") or "?"
        lines.append(f"| `{job}` | {result} |")
        if job != "changes" and result in ("failure", "cancelled"):
            problems.append(f"作业 {job}：{result}。")
        elif job != "changes" and result not in ("success", "skipped"):
            problems.append(f"作业 {job} 的结果是 {result}，不认识。")
    lines += ["", "不通过：" + "；".join(problems) if problems else "通过：没有失败或取消的作业（跳过算通过）。"]
    return lines, problems


# ---------- 入口 ----------

def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)
    detecting = commands.add_parser("detect", help="算出各区域是否改动")
    detecting.add_argument("--base")
    detecting.add_argument("--head", default="HEAD")
    detecting.add_argument("--all", action="store_true", help="全部区域都算改动（推 main、手动触发）")
    commands.add_parser("summary", help="「CI 汇总」：读环境变量 NEEDS 判定")
    listing = commands.add_parser("files", help="列出命中某个区域的被跟踪文件")
    listing.add_argument("area", choices=sorted(AREAS))
    args = parser.parse_args(argv)
    try:
        if args.command == "detect":
            detect(args.base, args.head, args.all)
            return 0
        if args.command == "summary":
            try:
                needs = json.loads(os.environ.get("NEEDS", ""))
            except ValueError as error:
                raise ToolError(f"环境变量 NEEDS 不是 JSON：{error}") from error
            if not isinstance(needs, dict):
                raise ToolError("环境变量 NEEDS 应是对象。")
            lines, problems = summarize(needs)
            for line in lines:
                print(line)
            append("GITHUB_STEP_SUMMARY", lines)
            return 1 if problems else 0
        for path in tracked():
            if area_matches(AREAS[args.area], path):
                print(path)
        return 0
    except ToolError as error:
        print(f"ci_changes：{error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
