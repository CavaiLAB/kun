#!/usr/bin/env python3
"""发版的包名单与制品核对（S1-KUN-022；合并蓝图 D23.12）。只依赖标准库，不发布、不推送、不读凭据。

名单不另写，从项目文件读：Kun.slnx 里每个 IsPackable=true 的项目就是一个包，档位与角色取项目文件里的
KunTier、KunRole（与依赖门禁 scripts/check_deps.py 同一套属性）。包名与条数都不写死，换包名、加减包不用改这里。

用法（仓库根运行，不需要先还原）：

  python3 scripts/release_packages.py list [--transitional] [--json]
      列出包名单并核对：档位与角色写全、包名不重复；--json 输出名单（verify-packages 可用 --list 读回）。
  python3 scripts/release_packages.py verify-packages --version V --packages <目录> [--transitional]
      核对 dotnet pack 的产物（版本用 -p:Version=V 统一传入）：
      - 恰好是名单上的包，各一个，nuspec 的包名、版本与文件名对得上，版本都是 V（V 要合日历版本的写法）；
      - 名单里的包之间的依赖都在本次产物里，版本写成 V、[V] 或 [V,)（去掉空白后；与 release_version.py 一样
        只认这三种）；与名单同前缀、却不在名单里的依赖也算缺；
      - 符号包（.snupkg）都有对应的包，项目写了 IncludeSymbols=true 的都出了符号包；只带分析器的包免符号包：
        项目求值 IncludeBuildOutput=false，并且打出来的包里 lib/、runtimes/ 下没有 PE 文件（.dll、.exe），两条都满足
        才免（不看包名）；免的包不能出符号包（空的或对不上的符号包推到 nuget.org 会被拒），项目也不能写
        IncludeSymbols=true。这条两种模式、不分位置都一样；名单里「要出符号包的」不算不带编译输出的包，它们按制品判；
      - 许可是 MIT 表达式；仓库地址是本仓库，带的提交号是本次检出的提交；
      - 作者、版权、项目地址与 src/Directory.Build.props 写的一致（仓库地址也照它）；描述不空，也不写过渡目录的名字
        （取 _import/ 下各子目录的名字，首字母大写后区分大小写地查，不写死；小写的运行期字面
        如组件 id、Cookie 名不算）；
      - 内核档的包不依赖平台档的包；
      - 稳定版不带预发布依赖，除非项目在 NoWarn 里写了 NU5104（有意为之，比如上游只发预发布版）。
        打包时要传 -p:WarningsNotAsErrors=NU5104，让 NU5104 先不挡打包，由这里按项目判。

过渡模式（--transitional）：B4–B10 归并期间用，CI「构建与门禁」的打包核对带着它跑。TRANSITIONAL 里的几项
在过渡模式下只报数、不判失败，其余各项两种模式一样；B10 之后去掉 --transitional，这几项全部判失败。
放行只给项目在过渡目录（LEGACY_ROOT，即 _import/）下的包：src/ 等别处的包两种模式都按严格判，包一搬进
src/，当次 CI 就查。只有 exception 不分位置、只报数：每条例外写着到期的批次，由依赖门禁那边管。

  prefix       包名不以 Kun. 开头（还没归并进来的包）
  exception    项目文件里记着档位例外 KunTierException，包括例外带来的内核档依赖平台档（不分位置）
  repository   包里的仓库地址不是本仓库，或没带提交号（过渡子目录的打包设置还没跟上）
  symbols      没出符号包（只带分析器的包免，见上）
  metadata     作者、版权、项目地址与 src/Directory.Build.props 不一致
  description  描述是空的，或写着过渡目录的名字
  prerelease   稳定版带预发布依赖，项目却没写 NoWarn NU5104

退出码：0 通过；1 核对不通过；2 工具出错。自测在 scripts/tests/test_release_packages.py。
"""
from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile
from concurrent.futures import ThreadPoolExecutor
from dataclasses import asdict, dataclass, field
from pathlib import Path
from typing import Dict, List, Optional, Tuple

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_deps import ROLES, TIERS, GateError, ROOT, _env, _metadata, _rel, solution_projects  # noqa: E402
from release_version import release_info  # noqa: E402

SOLUTION = ROOT / "Kun.slnx"
METADATA_PROPS = ROOT / "src" / "Directory.Build.props"
METADATA = ("Authors", "Copyright", "PackageProjectUrl", "RepositoryUrl")
LEGACY_ROOT = ROOT / "_import"
PREFIX = "Kun."
LICENSE = "MIT"
DEFAULT_DESCRIPTION = "Package Description"  # SDK 在项目没写 Description 时填的
PROPERTIES = ("IsPackable", "PackageId", "KunTier", "KunRole", "IsAotCompatible", "IncludeSymbols", "IncludeBuildOutput", "NoWarn")
# 判「只带分析器的包」时看包里这两处有没有 PE 文件：有就照旧要符号包。
BINARY_ROOTS = ("lib/", "runtimes/")
PE_SUFFIXES = (".dll", ".exe")
EXCEPTION_ITEM = "KunTierException"
TRANSITIONAL = {
    "prefix": "包名不以 Kun. 开头",
    "exception": "项目文件里记着档位例外 KunTierException（含例外带来的内核档依赖平台档）",
    "repository": "包里的仓库地址不是本仓库，或没带提交号",
    "symbols": "没出符号包",
    "metadata": "作者、版权、项目地址与 src/Directory.Build.props 不一致",
    "description": "描述是空的，或写着过渡目录的名字",
    "prerelease": "稳定版带预发布依赖，项目没写 NoWarn NU5104",
}
NUSPEC_LIMIT = 1_048_576


class Failure(Exception):
    """读包出错：单个包读不出来记为核对不通过；包目录本身不对算工具出错。"""


@dataclass(frozen=True)
class Package:
    """名单上的一个包（一个可打包项目）。"""
    id: str
    tier: str
    role: str
    project: str
    aot: bool = False
    symbols: bool = False
    prerelease_ok: bool = False  # 项目的 NoWarn 里有 NU5104
    exceptions: Tuple[Tuple[str, str], ...] = ()  # (被依赖的包, 规则)
    build_output: bool = True  # 项目求值的 IncludeBuildOutput 不是 false；是 false、包里也没有 PE 文件的免符号包


@dataclass
class Report:
    """核对结果：problems 判失败；allowed 是过渡模式放行的项（键见 TRANSITIONAL）。"""
    problems: List[str] = field(default_factory=list)
    allowed: Dict[str, List[str]] = field(default_factory=dict)

    def strict_only(self, allow: bool, key: str, message: str) -> None:
        """B10 之后才判失败的项：allow（过渡模式且这个包可以放行）时记进 allowed，否则记进 problems。"""
        if allow:
            self.allowed.setdefault(key, []).append(message)
        else:
            self.problems.append(f"{message}（{TRANSITIONAL[key]}）")


# ---------- 名单 ----------

def _split(value: str) -> List[str]:
    return [item.strip() for item in re.split(r"[;,]", value) if item.strip()]


def package_from(path: Path, data: dict) -> Optional[Package]:
    props = data.get("Properties") or {}
    if props.get("IsPackable", "").lower() != "true":
        return None
    exceptions = tuple(sorted((_metadata(item, "Identity"), _metadata(item, "Rule"))
                              for item in (data.get("Items") or {}).get(EXCEPTION_ITEM, [])))
    return Package(id=props.get("PackageId") or path.stem, tier=props.get("KunTier", ""), role=props.get("KunRole", ""),
                   project=_rel(path), aot=props.get("IsAotCompatible", "").lower() == "true",
                   symbols=props.get("IncludeSymbols", "").lower() == "true",
                   prerelease_ok="NU5104" in (n.upper() for n in _split(props.get("NoWarn", ""))), exceptions=exceptions,
                   build_output=props.get("IncludeBuildOutput", "").lower() != "false")


def _evaluate(path: Path) -> dict:
    cmd = (["dotnet", "msbuild", str(path), "-nologo"] + [f"-getProperty:{p}" for p in PROPERTIES]
           + [f"-getItem:{EXCEPTION_ITEM}"])
    result = subprocess.run(cmd, capture_output=True, text=True, env=_env())
    if result.returncode != 0:
        raise GateError(f"读取项目失败：{_rel(path)}\n{result.stdout}{result.stderr}")
    return json.loads(result.stdout)


def discover(solution: Path = SOLUTION) -> List[Package]:
    """解决方案里每个可打包项目，按包名排序。"""
    paths = solution_projects(solution)
    with ThreadPoolExecutor(max_workers=min(8, os.cpu_count() or 4)) as pool:
        data = list(pool.map(_evaluate, paths))
    return sorted((p for p in (package_from(path, d) for path, d in zip(paths, data)) if p), key=lambda p: p.id.lower())


def in_legacy(package: Package, root: Path = LEGACY_ROOT) -> bool:
    """项目在过渡目录下：过渡模式只给这些包放行。"""
    try:
        (ROOT / package.project).resolve().relative_to(root.resolve())
    except ValueError:
        return False
    return True


def check_list(packages: List[Package], transitional: bool) -> Report:
    report = Report()
    if not packages:
        report.problems.append("解决方案里没有可打包的项目")
    owners: Dict[str, str] = {}
    for p in packages:
        if p.id.lower() in owners:
            report.problems.append(f"两个项目的包名都是 {p.id}：{owners[p.id.lower()]}、{p.project}")
        owners[p.id.lower()] = p.project
        if p.tier not in TIERS or p.role not in ROLES:
            report.problems.append(f"{p.id}（{p.project}）的档位或角色没写对：KunTier=「{p.tier}」、KunRole=「{p.role}」")
        if not p.id.startswith(PREFIX):
            report.strict_only(transitional and in_legacy(p), "prefix", p.id)
        for target, rule in p.exceptions:
            report.strict_only(transitional, "exception", f"{p.id} → {target}（规则 {rule}）")
    return report


# ---------- 期望的元数据 ----------

def expected_metadata(props: Path = METADATA_PROPS) -> Dict[str, str]:
    """src/ 下的包统一用的元数据：作者、版权、项目地址、仓库地址，取 props 里第一处写法。"""
    try:
        root = ET.fromstring(props.read_bytes())
    except (OSError, ET.ParseError) as error:
        raise GateError(f"读不出 {_rel(props)}：{error}") from error
    found: Dict[str, str] = {}
    for element in root.iter():
        name = _local(element.tag)
        if name in METADATA and name not in found and (element.text or "").strip():
            found[name] = element.text.strip()
    missing = [name for name in METADATA if name not in found]
    if missing:
        raise GateError(f"{_rel(props)} 里没写 {'、'.join(missing)}")
    return found


def legacy_names(root: Path = LEGACY_ROOT) -> List[str]:
    """过渡目录下各子目录的名字（本仓库现在的名字除外），描述里不该再写它们。目录不在了就没有。"""
    current = PREFIX.rstrip(".").lower()
    return sorted(d.name for d in root.iterdir() if d.is_dir() and d.name.lower() != current) if root.is_dir() else []


# ---------- 制品 ----------

def _local(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def pe_files(names: List[str]) -> List[str]:
    """lib/、runtimes/ 下的 PE 文件（.dll、.exe），不分大小写。只带分析器的包这里是空的。"""
    return sorted(n for n in names if n.replace("\\", "/").lower().startswith(BINARY_ROOTS) and n.lower().endswith(PE_SUFFIXES))


def read_nuspec(path: Path) -> dict:
    """包里唯一的根 nuspec：包名、版本、许可、仓库信息、依赖；另带上 lib/、runtimes/ 下的 PE 文件。拒绝 DTD 与实体声明。"""
    try:
        with zipfile.ZipFile(path) as archive:
            entries = [e for e in archive.infolist() if "/" not in e.filename and e.filename.endswith(".nuspec")]
            if len(entries) != 1 or entries[0].file_size > NUSPEC_LIMIT:
                raise Failure(f"{path.name} 里应恰有一个大小受限的根 nuspec")
            raw = archive.read(entries[0])
            binaries = pe_files(archive.namelist())
        if b"<!DOCTYPE" in raw or b"<!ENTITY" in raw:
            raise Failure(f"{path.name} 的 nuspec 不许有 DTD 或实体声明")
        root = ET.fromstring(raw)
    except (zipfile.BadZipFile, ET.ParseError, OSError) as error:
        raise Failure(f"{path.name} 读不出合法的 nuspec：{error}") from error
    metadata = next((e for e in root if _local(e.tag) == "metadata"), None)
    if _local(root.tag) != "package" or metadata is None:
        raise Failure(f"{path.name} 的 nuspec 缺少 package/metadata")

    def child(name: str) -> Optional[ET.Element]:
        return next((e for e in metadata if _local(e.tag) == name), None)

    def text(name: str) -> str:
        node = child(name)
        return (node.text or "").strip() if node is not None else ""

    license_node, repository = child("license"), child("repository")
    return {
        "id": text("id"),
        "version": text("version"),
        "authors": text("authors"),
        "copyright": text("copyright"),
        "projectUrl": text("projectUrl"),
        "description": text("description"),
        "license": (license_node.get("type", ""), (license_node.text or "").strip()) if license_node is not None else None,
        "repository": {"url": repository.get("url", ""), "commit": repository.get("commit", "")} if repository is not None else None,
        "dependencies": sorted({(node.get("id", ""), re.sub(r"\s+", "", node.get("version", "")))
                                for node in metadata.iter() if _local(node.tag) == "dependency"}),
        "binaries": binaries,
    }


def lower_bound(range_text: str) -> str:
    """NuGet 版本范围的下限（查别家依赖是不是预发布版用）：1.0.0、[1.0.0, )、[1.0.0] 都给出 1.0.0；(,1.0.0] 给出空串。"""
    text = range_text.strip()
    if text[:1] in "[(":
        return text[1:].split(",", 1)[0].rstrip("])").strip()
    return text


def verify(packages: List[Package], directory: Path, version: str, commit: str, transitional: bool,
           expected: Optional[Dict[str, str]] = None, legacy: Optional[List[str]] = None) -> Report:
    expected = expected if expected is not None else expected_metadata()
    legacy = legacy if legacy is not None else legacy_names()
    report = check_list(packages, transitional)
    listed = {p.id.lower(): p for p in packages}
    prefixes = {p.id.split(".", 1)[0].lower() + "." for p in packages}
    stable = "-" not in version
    pinned = {version, f"[{version}]", f"[{version},)"}
    if not directory.is_dir():
        raise Failure(f"包目录不存在：{directory}")

    found: Dict[str, Tuple[Path, dict]] = {}
    symbols: Dict[str, Path] = {}
    for path in sorted(directory.iterdir()):
        name = path.name
        if not path.is_file() or path.is_symlink() or not name.endswith((".nupkg", ".snupkg")):
            continue
        if name.endswith(".symbols.nupkg"):
            report.problems.append(f"{name} 是旧格式的符号包：符号包用 snupkg（SymbolPackageFormat）")
            continue
        try:
            spec = read_nuspec(path)
        except Failure as error:
            report.problems.append(str(error))
            continue
        key = spec["id"].lower()
        if not name.lower() == f"{key}.{spec['version'].lower()}{path.suffix}":
            report.problems.append(f"{name} 与 nuspec 里的包名 {spec['id']}、版本 {spec['version']} 对不上")
        if spec["version"] != version:
            report.problems.append(f"{name} 的版本是 {spec['version']}，不是 {version}")
        if path.suffix == ".snupkg":
            symbols[key] = path
            continue
        if key in found:
            report.problems.append(f"{spec['id']} 出现了不止一个包：{found[key][0].name}、{name}")
            continue
        found[key] = (path, spec)

    report.problems += [f"缺少名单上的包：{listed[k].id}（{listed[k].project}）" for k in sorted(set(listed) - set(found))]
    report.problems += [f"多出名单以外的包：{found[k][1]['id']}" for k in sorted(set(found) - set(listed))]
    report.problems += [f"符号包 {symbols[k].name} 没有对应的包" for k in sorted(set(symbols) - set(found))]

    for key in sorted(set(found) & set(listed)):
        package, spec = listed[key], found[key][1]
        relax = transitional and in_legacy(package)
        if not package.build_output and not spec["binaries"]:
            # 只带分析器的包（项目不带编译输出，包里 lib/、runtimes/ 下也没有 PE 文件）免符号包，也不能出符号包；
            # 项目写了 IncludeSymbols=true 也报，名单就不会说要、核对却说免。两种模式、不分位置都这么判。
            if key in symbols:
                report.problems.append(f"{package.id} 只带分析器、免符号包，却出了符号包 {symbols[key].name}："
                                       "空的或对不上的符号包推到 nuget.org 会被拒")
            if package.symbols:
                report.problems.append(f"{package.id} 只带分析器、免符号包，项目却写了 IncludeSymbols=true：改写 IncludeSymbols=false")
        elif key not in symbols:
            if package.symbols:
                report.problems.append(f"{package.id} 的项目写了 IncludeSymbols=true，却没出符号包")
            elif not package.build_output:
                report.strict_only(relax, "symbols", f"{package.id}（项目写了 IncludeBuildOutput=false，可包里有 "
                                                     f"{spec['binaries'][0]}，不算只带分析器的包）")
            else:
                report.strict_only(relax, "symbols", package.id)
        if spec["license"] != ("expression", LICENSE):
            report.problems.append(f"{package.id} 的许可不是 {LICENSE} 表达式：{spec['license']}")
        repository = spec["repository"] or {}
        if repository.get("commit") and repository["commit"] != commit:
            report.problems.append(f"{package.id} 带的提交号是 {repository['commit']}，不是本次检出的 {commit}：包不是从这次检出打的")
        if repository.get("url") != expected["RepositoryUrl"] or not repository.get("commit"):
            gaps = ([] if repository.get("url") == expected["RepositoryUrl"] else ["仓库地址不是本仓库" if repository.get("url") else "没有仓库地址"]) \
                + ([] if repository.get("commit") else ["没带提交号"])
            report.strict_only(relax, "repository", f"{package.id}（{'、'.join(gaps)}）")
        wrong = [label for label, actual, wanted in (("作者", spec["authors"], expected["Authors"]),
                                                      ("版权", spec["copyright"], expected["Copyright"]),
                                                      ("项目地址", spec["projectUrl"], expected["PackageProjectUrl"])) if actual != wanted]
        if wrong:
            report.strict_only(relax, "metadata", f"{package.id}（{'、'.join(wrong)}不对）")
        if spec["description"] in ("", DEFAULT_DESCRIPTION):
            report.strict_only(relax, "description", f"{package.id}（描述是空的）")
        elif any(name[:1].upper() + name[1:] in spec["description"] for name in legacy):
            report.strict_only(relax, "description", f"{package.id}（描述里写着过渡目录的名字）")
        for dependency, range_text in spec["dependencies"]:
            target = dependency.lower()
            if target in listed:
                if range_text not in pinned:
                    report.problems.append(f"{package.id} 对 {dependency} 的依赖写成 {range_text}：只认 {'、'.join(sorted(pinned))}")
                if target not in found:
                    report.problems.append(f"{package.id} 依赖的 {dependency} 不在本次产物里")
                if package.tier == "kernel" and listed[target].tier == "platform":
                    if (listed[target].id.lower(), "K") in {(t.lower(), r) for t, r in package.exceptions}:
                        report.strict_only(transitional, "exception", f"内核档的 {package.id} 依赖平台档的 {dependency}")
                    else:
                        report.problems.append(f"内核档的 {package.id} 依赖平台档的 {dependency}，项目文件里没有记档位例外")
            elif any(target.startswith(prefix) for prefix in prefixes):
                report.problems.append(f"{package.id} 依赖的 {dependency} {range_text} 不在名单里：使用方从包源还原不到同一版本")
            elif stable and "-" in lower_bound(range_text) and not package.prerelease_ok:
                report.strict_only(relax, "prerelease", f"{package.id} → {dependency} {range_text}")
    return report


def git_head() -> str:
    result = subprocess.run(["git", "-C", str(ROOT), "rev-parse", "HEAD"], capture_output=True, text=True)
    if result.returncode != 0:
        raise GateError(f"读不到本次检出的提交：{result.stderr.strip()}")
    return result.stdout.strip()


# ---------- 命令行 ----------

def _load_list(path: Path) -> List[Package]:
    return [Package(**{**item, "exceptions": tuple(tuple(e) for e in item.get("exceptions", []))})
            for item in json.loads(path.read_text(encoding="utf-8"))]


def _summary(packages: List[Package]) -> str:
    count = lambda f: sum(1 for p in packages if f(p))  # noqa: E731
    return (f"{len(packages)} 个包（内核档 {count(lambda p: p.tier == 'kernel')}、平台档 {count(lambda p: p.tier == 'platform')}；"
            f"契约 {count(lambda p: p.role == 'contract')}；AOT 兼容 {count(lambda p: p.aot)}；"
            f"要出符号包的 {count(lambda p: p.symbols and p.build_output)}、不带编译输出的 {count(lambda p: not p.build_output)}）")


def _print(report: Report, title: str, transitional: bool) -> int:
    mode = "（过渡模式）" if transitional else ""
    if report.problems:
        print(f"{title}不通过{mode}：{len(report.problems)} 处")
        for problem in report.problems:
            print(f"  {problem}")
    else:
        print(f"{title}通过{mode}")
    if report.allowed:
        print("过渡期放行（只给过渡目录下的包，档位例外不分位置；B10 之后去掉 --transitional，下面这些都判失败）：")
        for key, items in report.allowed.items():
            shown = "、".join(items[:5]) + (f" 等 {len(items)} 项" if len(items) > 5 else "")
            print(f"  [{key}] {TRANSITIONAL[key]}：{len(items)} 项（{shown}）")
    return 1 if report.problems else 0


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="发版的包名单与制品核对；不发布、不推送")
    sub = parser.add_subparsers(dest="command", required=True)
    listing = sub.add_parser("list", help="列出包名单并核对")
    listing.add_argument("--json", action="store_true", help="以 JSON 输出名单")
    verifying = sub.add_parser("verify-packages", help="核对 dotnet pack 的产物")
    verifying.add_argument("--version", required=True, help="打包时 -p:Version= 传入的版本（日历版本）")
    verifying.add_argument("--packages", required=True, type=Path, help="dotnet pack -o 的目录")
    verifying.add_argument("--list", type=Path, help="list --json 的输出；不给就现读项目文件")
    verifying.add_argument("--commit", help="包里应带的提交号；不给就取仓库的 HEAD")
    for p in (listing, verifying):
        p.add_argument("--transitional", action="store_true",
                       help="过渡模式：过渡目录下的包放行 TRANSITIONAL 里的几项，档位例外只报数（B10 之前用）")
    args = parser.parse_args(argv)
    try:
        if args.command == "list":
            packages = discover()
            if args.json:
                report = check_list(packages, args.transitional)
                print(json.dumps([asdict(p) for p in packages], ensure_ascii=False, indent=2))
                for problem in report.problems:
                    print(f"包名单核对不通过：{problem}", file=sys.stderr)
                return 1 if report.problems else 0
            print(f"包名单：{_summary(packages)}")
            return _print(check_list(packages, args.transitional), "包名单核对", args.transitional)
        release_info(args.version)
        packages = _load_list(args.list) if args.list else discover()
        report = verify(packages, args.packages, args.version, args.commit or git_head(), args.transitional)
        print(f"包名单：{_summary(packages)}；版本 {args.version}")
        return _print(report, "制品核对", args.transitional)
    except ValueError as error:
        print(f"版本不合要求：{error}", file=sys.stderr)
        return 2
    except (GateError, Failure) as error:
        print(f"发版核对出错：{error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
