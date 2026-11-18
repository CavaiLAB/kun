#!/usr/bin/env python3
"""依赖门禁：按项目文件里写的档位与角色检查依赖（S1-KUN-020；合并蓝图 D23.7、D23.14 第 1 点）。只依赖标准库。

被检查方是解决方案里每个可打包的项目（IsPackable=true）。每个都要在项目文件里写下面几个属性，仓库根的
Directory.Build.targets 在构建时核对写没写、取值对不对（KUNTIER001、KUNTIER002）：

  KunTier            kernel（内核档）或 platform（平台档）
  KunRole            contract（契约）或 provider（提供者）
  KunZeroDependency  写 true 的包另受规则 C 约束

门禁只看这几个属性，不看包名。规则：

  R  声明    可打包的项目没写档位或角色，或者取值不对；依赖图里有不是所查解决方案里可打包项目的项目引用，档位不明
  K  内核档  内核档包的传递依赖里不得有平台档的包
  P  平台档  平台档包的传递依赖里，平台档的包只能是契约；内核档的包不限
  D  契约    契约包对内核档包的直接依赖只能是契约
  C  零依赖  KunZeroDependency=true 的包不得有项目引用，包引用只许 Microsoft.Extensions.*

D、C 不计只在构建时用的包引用：PrivateAssets=all（或 SDK 自动加的）且不带编译、运行时程序集的，比如分析器。
PrivateAssets=all 引用运行时库照样算：它不进 nuspec，使用方拿不到它，包会坏。

例外写在依赖边起点的那个项目的项目文件里，一条边一项：

  <ItemGroup Label="档位例外">
    <KunTierException Include="被依赖的包" Rule="K" Until="B5" Reason="为什么现在还要这条边、到哪一批去掉" />
  </ItemGroup>

Rule 是 K、P、D、C 之一，Until 写最早能去掉的批次，Reason 写理由，都必填。K、P 查传递依赖时跳过记了例外的边，
经过这条边的路径都不再报，所以只在出问题的那条边的起点记一次。每条例外都要对得上一条现存、而且确实违反该规则的
直接依赖；边没了或者已经不违规，门禁就报「例外已失效」，要把它删掉，例外只减不增。

依赖读各项目的 obj/project.assets.json：包与项目两类依赖都在里面（dotnet list package 看不到项目引用）。
失败时报出从被检查方到违规依赖的完整路径。自测在 tests/gates/test_check_deps.py。

用法（仓库根目录）：
  python3 scripts/check_deps.py                         还原仓库根的 Kun.slnx 后检查（在别的目录跑也是它）
  python3 scripts/check_deps.py --no-restore            已还原时直接检查（CI 在构建之后跑）
  python3 scripts/check_deps.py --solution x.slnf --json
退出码：0 通过；1 有违规或失效的例外；2 工具出错（还原失败、缺 assets 文件等）。
"""
from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from collections import deque
from concurrent.futures import ThreadPoolExecutor
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Callable, Dict, List, Optional, Set, Tuple

ROOT = Path(__file__).resolve().parents[1]
TIERS = ("kernel", "platform")
ROLES = ("contract", "provider")
RULES = ("K", "P", "D", "C")
ZERO_DEPENDENCY_PACKAGES = "Microsoft.Extensions."
PROPERTIES = ("IsPackable", "PackageId", "ProjectAssetsFile", "KunTier", "KunRole", "KunZeroDependency")
EXCEPTION_ITEM = "KunTierException"
REASONS = {
    "R": "可打包的项目要写档位 KunTier（kernel 或 platform）与角色 KunRole（contract 或 provider）",
    "K": "内核档包的传递依赖里不得有平台档的包（D23.7）",
    "P": "平台档包只依赖内核档的包与平台档的契约包，不依赖平台档的提供者包（D23.7）",
    "D": "契约包对内核档包的直接依赖只能是契约",
    "C": "零依赖的包不得有项目引用，包引用只许 Microsoft.Extensions.*",
    "X": "档位例外",
}


class GateError(Exception):
    """工具层面的失败（不是规则违规）。"""


@dataclass(frozen=True)
class TierException:
    target: str
    rule: str
    until: str
    reason: str


@dataclass(frozen=True)
class Package:
    package_id: str
    project: Path
    assets_file: Path
    tier: str = ""
    role: str = ""
    zero_dependency: str = ""
    exceptions: Tuple[TierException, ...] = ()


@dataclass
class Violation:
    rule: str
    package: str
    project: str
    framework: str
    dependency: str
    path: List[str]
    detail: str = ""

    @property
    def reason(self) -> str:
        return REASONS[self.rule] + (f"：{self.detail}" if self.detail else "")


@dataclass
class ExceptionStatus:
    package: str
    target: str
    rule: str
    until: str
    reason: str
    problem: str = ""


@dataclass(frozen=True)
class Direct:
    project: bool  # 项目引用
    private: bool  # 不传给使用方：PrivateAssets=all 的包引用、SDK 自动加的引用
    code: bool = True  # 带编译或运行时程序集（项目引用一律算带）

    @property
    def build_only(self) -> bool:
        """只在构建时用：不传给使用方，也不带程序集（分析器之类）。规则 D、C 不计它。"""
        return self.private and not self.code


@dataclass
class Graph:
    framework: str
    names: Dict[str, str]  # 小写名 → 原样的名字
    edges: Dict[str, List[str]]  # 小写名 → 依赖的小写名
    projects: Set[str]  # 其中项目类的依赖（项目引用带进来的）
    code: Set[str]  # 其中带编译或运行时程序集的（项目一律算带）
    direct: Dict[str, Direct]  # 被检查方的直接依赖


def _matching(frameworks: dict, framework: str) -> List[dict]:
    return [f for k, f in frameworks.items() if k == framework or f.get("targetAlias") == framework] or list(frameworks.values())


def graphs(assets: dict, project_dir: Path) -> List[Graph]:
    """一份 project.assets.json 里每个目标框架的依赖图（带 RID 的那份与不带的重复，跳过）。"""
    project = assets.get("project") or {}
    libraries = assets.get("libraries") or {}
    by_path: Dict[str, str] = {}
    for key, library in libraries.items():
        if library.get("type") == "project" and library.get("path"):
            by_path[os.path.normcase(str((project_dir / library["path"]).resolve()))] = key.split("/", 1)[0].lower()

    out: List[Graph] = []
    for framework, entries in (assets.get("targets") or {}).items():
        if "/" in framework:
            continue
        names: Dict[str, str] = {}
        edges: Dict[str, List[str]] = {}
        projects: Set[str] = set()
        code: Set[str] = set()
        for key, entry in entries.items():
            name = key.split("/", 1)[0]
            names[name.lower()] = name
            edges[name.lower()] = [d.lower() for d in (entry.get("dependencies") or {})]
            if entry.get("type") == "project":
                projects.add(name.lower())
            if entry.get("type") == "project" or any(not path.endswith("/_._") and path != "_._"
                                                     for kind in ("compile", "runtime") for path in (entry.get(kind) or {})):
                code.add(name.lower())

        direct: Dict[str, Direct] = {}
        for f in _matching(project.get("frameworks") or {}, framework):
            for name, spec in (f.get("dependencies") or {}).items():
                private = str(spec.get("suppressParent", "")).lower() == "all" or bool(spec.get("autoReferenced"))
                # 查不到的按带程序集算，宁可多报
                direct[name.lower()] = Direct(project=False, private=private, code=name.lower() in code or name.lower() not in names)
        for f in _matching((project.get("restore") or {}).get("frameworks") or {}, framework):
            for reference in (f.get("projectReferences") or {}):
                name = by_path.get(os.path.normcase(str(Path(reference).resolve())))
                if name:
                    direct[name] = Direct(project=True, private=False)
        out.append(Graph(framework, names, edges, projects, code, direct))
    return out


def declaration_problems(package: Package) -> List[str]:
    problems = []
    if not package.tier or not package.role:
        problems.append("没写 " + "、".join(n for n, v in (("KunTier", package.tier), ("KunRole", package.role)) if not v))
    if package.tier and package.tier not in TIERS:
        problems.append(f"KunTier 是「{package.tier}」")
    if package.role and package.role not in ROLES:
        problems.append(f"KunRole 是「{package.role}」")
    if package.zero_dependency not in ("", "true", "false"):
        problems.append(f"KunZeroDependency 是「{package.zero_dependency}」")
    return problems


def evaluate(packages: List[Package], load_assets: Callable[[Path], dict]) -> Tuple[List[Violation], List[ExceptionStatus]]:
    """按档位检查每个包，返回违规与每条例外的状态（problem 为空即有效）。"""
    by_name: Dict[str, Package] = {p.package_id.lower(): p for p in packages}
    declared = {name: p for name, p in by_name.items() if not declaration_problems(p)}
    excepted: Set[Tuple[str, str, str]] = {(p.package_id.lower(), e.rule, e.target.lower())
                                           for p in packages for e in p.exceptions if e.rule in RULES}

    def tier(name: str) -> str:
        return declared[name].tier if name in declared else ""

    def role(name: str) -> str:
        return declared[name].role if name in declared else ""

    def violates(package: Package, rule: str, name: str, edge: Direct) -> bool:
        """起点 package 到 name 的这条直接依赖本身是否违反 rule。"""
        if rule == "K":
            return package.tier == "kernel" and tier(name) == "platform"
        if rule == "P":
            return package.tier == "platform" and tier(name) == "platform" and role(name) == "provider"
        if rule == "D":
            return package.role == "contract" and not edge.build_only and tier(name) == "kernel" and role(name) == "provider"
        return package.zero_dependency == "true" and not edge.build_only and (edge.project or not name.startswith(ZERO_DEPENDENCY_PACKAGES.lower()))

    violations: List[Violation] = []
    statuses: List[ExceptionStatus] = []
    for package in sorted(packages, key=lambda p: p.package_id.lower()):
        source = package.package_id.lower()
        rel = _rel(package.project)
        problems = declaration_problems(package)
        if problems:
            violations.append(Violation("R", package.package_id, rel, "", "", [package.package_id], "；".join(problems)))
        found = graphs(load_assets(package.assets_file), package.project.parent)

        for graph in found:
            # 项目引用带进来的依赖都要是所查解决方案里的可打包项目，否则档位不明，规则无从查起。
            unknown = sorted(name for name in graph.projects if name not in by_name)
            if unknown:
                parents = _reachable(graph, source, "", set())
                first = min(unknown, key=lambda n: (len(_chain(graph, parents, n)) if n in parents else 0, n))
                path = [package.package_id] + (_chain(graph, parents, first) if first in parents else [graph.names.get(first, first)])
                shown = "、".join(graph.names.get(n, n) for n in unknown[:5]) + ("等" if len(unknown) > 5 else "")
                violations.append(Violation("R", package.package_id, rel, graph.framework, graph.names.get(first, first), path,
                                            f"经项目引用依赖的 {len(unknown)} 个项目不是所查解决方案里的可打包项目，档位不明：{shown}"
                                            "（在仓库根用 Kun.slnx 检查）"))

        if not problems:
            for graph in found:
                display = graph.names.get
                rule = "K" if package.tier == "kernel" else "P"
                parents = _reachable(graph, source, rule, excepted)
                for name in sorted(parents):
                    if (rule == "K" and tier(name) == "platform") or (rule == "P" and tier(name) == "platform" and role(name) == "provider"):
                        violations.append(Violation(rule, package.package_id, rel, graph.framework, display(name, name),
                                                    [package.package_id] + _chain(graph, parents, name)))
                for name, edge in sorted(graph.direct.items()):
                    for rule in ("D", "C"):
                        if violates(package, rule, name, edge) and (source, rule, name) not in excepted:
                            detail = ("项目引用" if edge.project else "PrivateAssets=all 引用运行时库：它不进 nuspec，使用方拿不到"
                                      if edge.private else "") if rule == "C" else ""
                            violations.append(Violation(rule, package.package_id, rel, graph.framework, display(name, name),
                                                        [package.package_id, display(name, name)], detail))

        seen: Set[Tuple[str, str]] = set()
        for e in package.exceptions:
            status = ExceptionStatus(package.package_id, e.target, e.rule, e.until, e.reason)
            missing = [n for n, v in (("Rule", e.rule), ("Until", e.until.strip()), ("Reason", e.reason.strip())) if not v]
            if missing or e.rule not in RULES:
                status.problem = ("缺 " + "、".join(missing)) if missing else f"Rule 是「{e.rule}」，只能是 K、P、D、C"
            elif (e.rule, e.target.lower()) in seen:
                status.problem = "与前面一条重复"
            elif problems:
                status.problem = "起点没有写对档位与角色，无法核对"
            else:
                edges = [g.direct[e.target.lower()] for g in found if e.target.lower() in g.direct]
                if not edges:
                    status.problem = f"{package.package_id} 已不直接依赖 {e.target}"
                elif not any(violates(package, e.rule, e.target.lower(), edge) for edge in edges):
                    status.problem = f"{package.package_id} → {e.target} 已不违反规则 {e.rule}"
            seen.add((e.rule, e.target.lower()))
            statuses.append(status)
            if status.problem:
                violations.append(Violation("X", package.package_id, rel, "", e.target, [package.package_id, e.target],
                                            f"Rule={e.rule} Until={e.until}：{status.problem}，请删除或改正这一项"))
    return violations, statuses


def _reachable(graph: Graph, source: str, rule: str, excepted: Set[Tuple[str, str, str]]) -> Dict[str, Optional[str]]:
    """从被检查方出发、不经过记了 rule 例外的边能到的节点，值是最短路径上的前一个节点（None 即直接依赖）。"""
    parents: Dict[str, Optional[str]] = {}
    queue: deque = deque()
    for name in sorted(graph.direct):
        if name in graph.edges and name not in parents and (source, rule, name) not in excepted:
            parents[name] = None
            queue.append(name)
    while queue:
        node = queue.popleft()
        for child in graph.edges[node]:
            if child in graph.edges and child not in parents and (node, rule, child) not in excepted:
                parents[child] = node
                queue.append(child)
    return parents


def _chain(graph: Graph, parents: Dict[str, Optional[str]], name: str) -> List[str]:
    chain: List[str] = []
    node: Optional[str] = name
    while node is not None:
        chain.append(graph.names.get(node, node))
        node = parents[node]
    return list(reversed(chain))


# ---------- 与 dotnet 交互 ----------

def _env() -> Dict[str, str]:
    env = dict(os.environ)
    env.update({"DOTNET_CLI_UI_LANGUAGE": "en", "DOTNET_NOLOGO": "1", "DOTNET_CLI_TELEMETRY_OPTOUT": "1"})
    return env


def _rel(path: Path) -> str:
    try:
        return path.resolve().relative_to(ROOT).as_posix()
    except ValueError:
        return path.as_posix()


def solution_projects(solution: Path) -> List[Path]:
    text = solution.read_text(encoding="utf-8-sig")
    if solution.suffix.lower() == ".slnf":
        # 解决方案筛选文件：项目路径相对它筛选的那个解决方案所在的目录。
        document = json.loads(text)["solution"]
        base = (solution.parent / document["path"].replace("\\", "/")).parent
        return [(base / p.replace("\\", "/")).resolve() for p in document["projects"]]
    if solution.suffix.lower() == ".slnx":
        paths = [e.get("Path") for e in ET.fromstring(text).iter("Project") if e.get("Path")]
    else:
        paths = re.findall(r'^Project\("\{[^}]+\}"\)\s*=\s*"[^"]*",\s*"([^"]+\.(?:cs|fs|vb)proj)"', text, re.M)
    return [(solution.parent / p.replace("\\", "/")).resolve() for p in paths]


def _metadata(item: dict, name: str) -> str:
    for key, value in item.items():
        if key.lower() == name.lower():
            return str(value)
    return ""


def read_project(project: Path) -> dict:
    """项目的打包属性、档位属性与例外项（dotnet msbuild -getProperty / -getItem，不需要已还原）。"""
    cmd = (["dotnet", "msbuild", str(project), "-nologo"] + [f"-getProperty:{p}" for p in PROPERTIES]
           + [f"-getItem:{EXCEPTION_ITEM}"])
    result = subprocess.run(cmd, capture_output=True, text=True, env=_env())
    if result.returncode != 0:
        raise GateError(f"读取项目属性失败：{_rel(project)}\n{result.stdout}{result.stderr}")
    return json.loads(result.stdout)


def package_of(project: Path, data: dict) -> Optional[Package]:
    props = data.get("Properties") or {}
    if props.get("IsPackable", "").lower() != "true":
        return None
    exceptions = tuple(TierException(_metadata(i, "Identity"), _metadata(i, "Rule"), _metadata(i, "Until"), _metadata(i, "Reason"))
                       for i in (data.get("Items") or {}).get(EXCEPTION_ITEM, []))
    return Package(props.get("PackageId") or project.stem, project, Path(props.get("ProjectAssetsFile", "")),
                   props.get("KunTier", ""), props.get("KunRole", ""), props.get("KunZeroDependency", ""), exceptions)


def discover(solution: Path, restore: bool) -> List[Package]:
    if restore:
        result = subprocess.run(["dotnet", "restore", str(solution), "-nologo", "-v:q"], capture_output=True, text=True, env=_env())
        if result.returncode != 0:
            raise GateError(f"dotnet restore 失败：\n{result.stdout}{result.stderr}")
    projects = solution_projects(solution)
    with ThreadPoolExecutor(max_workers=min(8, os.cpu_count() or 4)) as pool:
        data = list(pool.map(read_project, projects))
    packages = [p for p in (package_of(project, d) for project, d in zip(projects, data)) if p]
    owners: Dict[str, Path] = {}
    for p in packages:
        if p.package_id.lower() in owners:
            raise GateError(f"两个项目的包名都是 {p.package_id}：{_rel(owners[p.package_id.lower()])}、{_rel(p.project)}")
        owners[p.package_id.lower()] = p.project
        if not p.assets_file.is_file():
            raise GateError(f"缺少 {_rel(p.assets_file)}：先 dotnet restore {_rel(solution)}")
    return sorted(packages, key=lambda x: x.package_id.lower())


def _load(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8-sig"))


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="按档位与角色检查可打包项目的依赖")
    parser.add_argument("--solution", default=str(ROOT / "Kun.slnx"), help="解决方案或筛选文件（.slnx / .slnf / .sln），默认仓库根的 Kun.slnx")
    parser.add_argument("--no-restore", action="store_true", help="跳过 dotnet restore（已还原时使用）")
    parser.add_argument("--json", action="store_true", help="以 JSON 输出检查结果")
    args = parser.parse_args(argv)

    solution = Path(args.solution)
    if not solution.is_absolute():
        solution = (Path.cwd() / solution).resolve()
    try:
        packages = discover(solution, restore=not args.no_restore)
        violations, statuses = evaluate(packages, _load)
    except GateError as ex:
        print(f"依赖门禁出错：{ex}", file=sys.stderr)
        return 2

    if args.json:
        print(json.dumps({
            "solution": _rel(solution),
            "packages": [{"packageId": p.package_id, "project": _rel(p.project), "tier": p.tier, "role": p.role,
                          "zeroDependency": p.zero_dependency == "true"} for p in packages],
            "exceptions": [asdict(s) for s in statuses],
            "violations": [dict(asdict(v), reason=v.reason) for v in violations],
        }, ensure_ascii=False, indent=2))
    elif violations:
        print(f"依赖门禁失败：{len({v.package for v in violations})} 个包共 {len(violations)} 处")
        for v in violations:
            where = f"（{v.framework}）" if v.framework else ""
            print(f"  [{v.rule}] {' → '.join(v.path)}{where}")
            print(f"      {v.reason}")
        print("修法：改依赖；确实要暂时保留的边，在起点的项目文件里记一条 KunTierException，写法见 scripts/check_deps.py 开头。")
    else:
        count = lambda f: sum(1 for p in packages if f(p))  # noqa: E731
        print(f"依赖门禁通过：检查了 {len(packages)} 个可打包项目（内核档 {count(lambda p: p.tier == 'kernel')}、"
              f"平台档 {count(lambda p: p.tier == 'platform')}；契约 {count(lambda p: p.role == 'contract')}），"
              f"例外 {len(statuses)} 条都对得上现存的违规边")
        for s in statuses:
            print(f"  例外 [{s.rule}] {s.package} → {s.target}（{s.until} 去掉）")
    return 1 if violations else 0


if __name__ == "__main__":
    sys.exit(main())
