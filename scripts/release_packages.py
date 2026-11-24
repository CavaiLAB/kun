#!/usr/bin/env python3
"""发版的包名单与制品核对（S1-KUN-022；合并蓝图 D23.12）。只依赖标准库。包源核对为只读操作。

名单不另写，从项目文件读：Kun.slnx 里每个 IsPackable=true 的项目就是一个包，档位与角色取项目文件里的
KunTier、KunRole（与依赖门禁 scripts/check_deps.py 同一套属性）。包名与条数都不写死，换包名、加减包不用改这里。

用法（仓库根运行，不需要先还原）：

  python3 scripts/release_packages.py list [--json] [--expect kernel=K,platform=P]
      列出包名单并核对：档位与角色写全、包名不重复；--json 输出名单（verify-packages 可用 --list 读回）。
  python3 scripts/release_packages.py nuget-config --version V [--list release-packages.json]
      向标准输出写入消费者的 NuGet.config。凭据用环境变量占位，包源按档位与发布版本选择。
  python3 scripts/release_packages.py notes --version V [--list release-packages.json]
      从 CHANGELOG.md 的 [Unreleased] 生成版本说明，附本版本的包清单与包源映射。
  python3 scripts/release_packages.py verify-feed --feed github|nuget --version V [--list release-packages.json]
      等待索引中出现本版本。GitHub Packages 同时核对公开状态与 CavaiLAB/kun 仓库关联；可用 GITHUB_TOKEN 只读访问。
  python3 scripts/release_packages.py verify-packages --version V --packages <目录>
      核对 dotnet pack 的产物（版本用 -p:Version=V 统一传入）：
      - 恰好是名单上的包，各一个，nuspec 的包名、版本与文件名对得上，版本都是 V（V 要合日历版本的写法）；
      - 名单里的包之间的依赖都在本次产物里，版本写成 V、[V] 或 [V,)（去掉空白后；与 release_version.py 一样
        只认这三种）；与名单同前缀、却不在名单里的依赖也算缺；
      - 符号包（.snupkg）都有对应的包，项目写了 IncludeSymbols=true 的都出了符号包；只带分析器的包免符号包：
        项目求值 IncludeBuildOutput=false，并且打出来的包里 lib/、runtimes/ 下没有 PE 文件（.dll、.exe），两条都满足
        才免（不看包名）；免的包不能出符号包（空的或对不上的符号包推到 nuget.org 会被拒），项目也不能写
        IncludeSymbols=true。名单里「要出符号包的」不算不带编译输出的包，它们按制品判；
      - 许可是 MIT 表达式；仓库地址是本仓库，带的提交号是本次检出的提交；
      - 作者、版权、项目地址与 src/Directory.Build.props 写的一致（仓库地址也照它）；描述不空，也不写过渡目录的名字
        （取 _import/ 下各子目录的名字，首字母大写后区分大小写地查，不写死；小写的运行期字面
        如组件 id、Cookie 名不算）；
      - 内核档的包不依赖平台档的包；
      - 稳定版不带预发布依赖，除非项目在 NoWarn 里写了 NU5104（有意为之，比如上游只发预发布版）。
        打包时要传 -p:WarningsNotAsErrors=NU5104，让 NU5104 先不挡打包，由这里按项目判。

CI 和发布都按上面各条核对，有一处不符就不通过。下面几类问题在报告里括注原因（键见 REASONS）：

  prefix       包名不以 Kun. 开头
  exception    项目文件里记着档位例外 KunTierException，包括例外带来的内核档依赖平台档
  repository   包里的仓库地址不是本仓库，或没带提交号
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
from dataclasses import asdict, dataclass, field, replace
from pathlib import Path
from typing import Dict, List, Optional, Tuple

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_deps import ROLES, TIERS, GateError, ROOT, _env, _metadata, _rel, solution_projects  # noqa: E402
from release_consumer import verify_consumer  # noqa: E402
from release_feed import FeedError, check_unpublished, release_order, verify_feed  # noqa: E402
from release_version import PACKAGE_ID, release_info  # noqa: E402

SOLUTION = ROOT / "Kun.slnx"
METADATA_PROPS = ROOT / "src" / "Directory.Build.props"
METADATA = ("Authors", "Copyright", "PackageProjectUrl", "RepositoryUrl")
LEGACY_ROOT = ROOT / "_import"
PREFIX = "Kun."
# 停发的包名仍可能存在于包源历史版本中，合并版清单不能再次使用。
RETIRED_IDS = frozenset(name.casefold() for name in (
    "Kun.Audit", "Kun.Auditing", "Kun.Dapper", "Kun.Infrastructure",
    "Kun.MultiTenancy.EntityFrameworkCore", "Kun.Saga", "Kun.Sandbox", "Kun.Storage",
))
LICENSE = "MIT"
DEFAULT_DESCRIPTION = "Package Description"  # SDK 在项目没写 Description 时填的
PROPERTIES = ("IsPackable", "PackageId", "AssemblyName", "KunTier", "KunRole", "IsAotCompatible", "IncludeSymbols", "IncludeBuildOutput", "NoWarn")
# 判「只带分析器的包」时看包里这两处有没有 PE 文件：有就照旧要符号包。
BINARY_ROOTS = ("lib/", "runtimes/")
PE_SUFFIXES = (".dll", ".exe")
EXCEPTION_ITEM = "KunTierException"
# 报告里括注原因的几类问题。
REASONS = {
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

    friend_dependencies: Tuple[str, ...] = ()  # 对本程序集开放内部成员的发布包名
    assembly: str = ""
    declared_friends: Tuple[str, ...] = ()  # 项目声明的全部朋友（含测试），与制品逐项比对


@dataclass
class Report:
    """核对结果：problems 里的每一项都判失败。"""
    problems: List[str] = field(default_factory=list)

    def fail(self, key: str, message: str) -> None:
        """记一处问题，后面括注这一类的原因（键见 REASONS）。"""
        self.problems.append(f"{message}（{REASONS[key]}）")


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
           + [f"-getItem:{EXCEPTION_ITEM},InternalsVisibleTo,AssemblyAttribute"])
    result = subprocess.run(cmd, capture_output=True, text=True, env=_env())
    if result.returncode != 0:
        raise GateError(f"读取项目失败：{_rel(path)}\n{result.stdout}{result.stderr}")
    return json.loads(result.stdout)


def discover(solution: Path = SOLUTION) -> List[Package]:
    """解决方案里每个可打包项目，按包名排序。"""
    paths = solution_projects(solution)
    with ThreadPoolExecutor(max_workers=min(8, os.cpu_count() or 4)) as pool:
        data = list(pool.map(_evaluate, paths))
    entries = [(p, d) for path, d in zip(paths, data) if (p := package_from(path, d))]
    return with_friend_dependencies(entries)


def with_friend_dependencies(entries: List[Tuple[Package, dict]]) -> List[Package]:
    assemblies = {}
    for package, data in entries:
        assembly = data["Properties"].get("AssemblyName") or Path(package.project).stem
        if assembly.casefold() in assemblies:
            raise GateError(f"多个可打包项目使用同一个程序集名 {assembly}")
        assemblies[assembly.casefold()] = package
    exact: Dict[str, set] = {}
    for owner, data in entries:
        items = data.get("Items") or {}
        for attribute in items.get("AssemblyAttribute", []):
            if _metadata(attribute, "Identity").rsplit(".", 1)[-1] in ("InternalsVisibleTo", "InternalsVisibleToAttribute"):
                raise GateError(f"{owner.project} 的朋友程序集须用 InternalsVisibleTo 项声明")
        for declaration in items.get("InternalsVisibleTo", []):
            friend = assemblies.get(_metadata(declaration, "Identity").casefold())
            if friend is None:
                continue
            if not _metadata(declaration, "KunPackageFriendReason").strip():
                raise GateError(f"{owner.id} 向包 {friend.id} 开放内部成员，需要记录 KunPackageFriendReason")
            exact.setdefault(friend.id, set()).add(owner.id)
    packages = []
    for package, data in entries:
        declarations = sorted(_metadata(item, "Identity") for item in
                              (data.get("Items") or {}).get("InternalsVisibleTo", []))
        packages.append(replace(package, friend_dependencies=tuple(sorted(exact.get(package.id, ()))),
                                assembly=data["Properties"].get("AssemblyName") or Path(package.project).stem,
                                declared_friends=tuple(declarations)))
    return sorted(packages, key=lambda p: p.id.casefold())


def tier_counts(text: str) -> Dict[str, int]:
    counts: Dict[str, int] = {}
    for field in text.split(","):
        tier, sep, number = field.partition("=")
        if not sep or tier not in TIERS or tier in counts or not re.fullmatch(r"[0-9]+", number):
            raise argparse.ArgumentTypeError("请给出各档包数，如 kernel=41,platform=44")
        counts[tier] = int(number)
    if counts.keys() != set(TIERS):
        raise argparse.ArgumentTypeError("期望包数须包含 kernel 与 platform 两档")
    return counts


def check_list(packages: List[Package], expected: Optional[Dict[str, int]] = None) -> Report:
    report = Report()
    if not packages:
        report.problems.append("解决方案里没有可打包的项目")
    for tier, wanted in (expected or {}).items():
        actual = sum(p.tier == tier for p in packages)
        if actual != wanted:
            report.problems.append(f"{tier} 档有 {actual} 个包，预期 {wanted} 个")
    owners: Dict[str, str] = {}
    for p in packages:
        if p.id.lower() in owners:
            report.problems.append(f"两个项目的包名都是 {p.id}：{owners[p.id.lower()]}、{p.project}")
        owners[p.id.lower()] = p.project
        if p.id.casefold() in RETIRED_IDS:
            report.problems.append(f"{p.id} 已停发，不能出现在合并版中")
        if p.tier not in TIERS or p.role not in ROLES:
            report.problems.append(f"{p.id}（{p.project}）的档位或角色没写对：KunTier=「{p.tier}」、KunRole=「{p.role}」")
        if not p.id.startswith(PREFIX):
            report.fail("prefix", p.id)
        for target, rule in p.exceptions:
            report.fail("exception", f"{p.id} → {target}（规则 {rule}）")
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
            files = [entry.filename for entry in archive.infolist() if not entry.is_dir()]
            binaries = pe_files(files)
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
        "files": files,
        "developmentDependency": text("developmentDependency").casefold() == "true",
    }


def check_assembly_metadata(packages: List[Package], directory: Path, version: str) -> List[str]:
    files = {f"{p.id}.{version}.nupkg": p for p in packages}
    paths = [str(directory / name) for name in files if (directory / name).is_file()]
    run = subprocess.run(["dotnet", "run", "--file", str(ROOT / "scripts" / "inspect-package.cs")],
                         cwd=ROOT, input=json.dumps(paths), text=True, capture_output=True, env=_env())
    if run.returncode:
        raise Failure(f"读取制品的程序集元数据失败：{run.stdout}{run.stderr}")
    return compare_assembly_metadata(files, json.loads(run.stdout), version)


def compare_assembly_metadata(files: Dict[str, Package], assemblies: List[dict], version: str) -> List[str]:
    expected_version = version.split("-", 1)[0] + ".0"
    problems = []
    found = set()
    for item in assemblies:
        package = files[item["package"]]
        if item["assembly"].casefold() != (package.assembly or Path(package.project).stem).casefold():
            continue
        found.add(package.id)
        location = f"{package.id}/{item['entry']}"
        if item["version"] != expected_version:
            problems.append(f"{location} 的 AssemblyVersion 为 {item['version']}，发布版本要求 {expected_version}")
        if package.aot and not item["trimmable"]:
            problems.append(f"{location} 缺少 IsTrimmable=True 程序集元数据")
        # CLR 的朋友名可在逗号后附公钥；精确依赖按包所属的程序集名匹配。
        names = sorted(name.split(",", 1)[0].strip().casefold() for name in item["friends"])
        declared = sorted(name.split(",", 1)[0].strip().casefold() for name in package.declared_friends)
        if names != declared:
            problems.append(f"{location} 的 InternalsVisibleTo 与项目声明不同，朋友程序集须用项目项声明")
    for package in files.values():
        if package.id not in found:
            problems.append(f"{package.id} 缺少项目的程序集 {package.assembly or Path(package.project).stem}")
    return problems


def lower_bound(range_text: str) -> str:
    """NuGet 版本范围的下限（查别家依赖是不是预发布版用）：1.0.0、[1.0.0, )、[1.0.0] 都给出 1.0.0；(,1.0.0] 给出空串。"""
    text = range_text.strip()
    if text[:1] in "[(":
        return text[1:].split(",", 1)[0].rstrip("])").strip()
    return text


def verify(packages: List[Package], directory: Path, version: str, commit: str,
           expected: Optional[Dict[str, str]] = None, legacy: Optional[List[str]] = None) -> Report:
    expected = expected if expected is not None else expected_metadata()
    legacy = legacy if legacy is not None else legacy_names()
    report = check_list(packages)
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
        if package.id == "Kun.Plugin.Sdk.Generators":
            analyzer = f"analyzers/dotnet/cs/{package.assembly or package.id}.dll"
            metadata_files = {f"{package.id}.nuspec", "_rels/.rels", "[Content_Types].xml", "README.md"}
            payload = [name for name in spec["files"] if name not in metadata_files
                       and not re.fullmatch(r"package/services/metadata/core-properties/[A-Za-z0-9_-]+\.psmdcp", name)]
            if payload != [analyzer] or package.build_output or spec["dependencies"]:
                report.problems.append(f"{package.id} 只能分发 {analyzer}，项目须关闭编译输出且制品不能声明依赖")
            if not spec["developmentDependency"]:
                report.problems.append(f"{package.id} 的 nuspec 须声明 developmentDependency=true")
        if not package.build_output and not spec["binaries"]:
            # 只带分析器的包（项目不带编译输出，包里 lib/、runtimes/ 下也没有 PE 文件）免符号包，也不能出符号包；
            # 项目写了 IncludeSymbols=true 也报，名单就不会说要、核对却说免。
            if key in symbols:
                report.problems.append(f"{package.id} 只带分析器、免符号包，却出了符号包 {symbols[key].name}："
                                       "空的或对不上的符号包推到 nuget.org 会被拒")
            if package.symbols:
                report.problems.append(f"{package.id} 只带分析器、免符号包，项目却写了 IncludeSymbols=true：改写 IncludeSymbols=false")
        elif key not in symbols:
            if package.symbols:
                report.problems.append(f"{package.id} 的项目写了 IncludeSymbols=true，却没出符号包")
            elif not package.build_output:
                report.fail("symbols", f"{package.id}（项目写了 IncludeBuildOutput=false，可包里有 "
                                                     f"{spec['binaries'][0]}，不算只带分析器的包）")
            else:
                report.fail("symbols", package.id)
        if spec["license"] != ("expression", LICENSE):
            report.problems.append(f"{package.id} 的许可不是 {LICENSE} 表达式：{spec['license']}")
        repository = spec["repository"] or {}
        if repository.get("commit") and repository["commit"] != commit:
            report.problems.append(f"{package.id} 带的提交号是 {repository['commit']}，不是本次检出的 {commit}：包不是从这次检出打的")
        if repository.get("url") != expected["RepositoryUrl"] or not repository.get("commit"):
            gaps = ([] if repository.get("url") == expected["RepositoryUrl"] else ["仓库地址不是本仓库" if repository.get("url") else "没有仓库地址"]) \
                + ([] if repository.get("commit") else ["没带提交号"])
            report.fail("repository", f"{package.id}（{'、'.join(gaps)}）")
        wrong = [label for label, actual, wanted in (("作者", spec["authors"], expected["Authors"]),
                                                      ("版权", spec["copyright"], expected["Copyright"]),
                                                      ("项目地址", spec["projectUrl"], expected["PackageProjectUrl"])) if actual != wanted]
        if wrong:
            report.fail("metadata", f"{package.id}（{'、'.join(wrong)}不对）")
        if spec["description"] in ("", DEFAULT_DESCRIPTION):
            report.fail("description", f"{package.id}（描述是空的）")
        elif any(name[:1].upper() + name[1:] in spec["description"] for name in legacy):
            report.fail("description", f"{package.id}（描述里写着过渡目录的名字）")
        exact = {name.casefold() for name in package.friend_dependencies}
        declared = {name.casefold() for name, _ in spec["dependencies"]}
        for missing in sorted(exact - declared):
            report.problems.append(f"{package.id} 缺少对朋友程序集所有者 {missing} 的精确依赖")
        for dependency, range_text in spec["dependencies"]:
            target = dependency.lower()
            if target.startswith("qian."):
                report.problems.append(f"{package.id} 依赖已停止使用的包 {dependency}，请引用现行包")
            if target in listed:
                if target in exact and range_text != f"[{version}]":
                    report.problems.append(f"{package.id} 对 {dependency} 的朋友程序集依赖应为 [{version}]，实际为 {range_text}")
                elif range_text not in pinned:
                    report.problems.append(f"{package.id} 对 {dependency} 的依赖写成 {range_text}：只认 {'、'.join(sorted(pinned))}")
                if target not in found:
                    report.problems.append(f"{package.id} 依赖的 {dependency} 不在本次产物里")
                if package.tier == "kernel" and listed[target].tier == "platform":
                    if (listed[target].id.lower(), "K") in {(t.lower(), r) for t, r in package.exceptions}:
                        report.fail("exception", f"内核档的 {package.id} 依赖平台档的 {dependency}")
                    else:
                        report.problems.append(f"内核档的 {package.id} 依赖平台档的 {dependency}，项目文件里没有记档位例外")
            elif any(target.startswith(prefix) for prefix in prefixes):
                report.problems.append(f"{package.id} 依赖的 {dependency} {range_text} 不在名单里：使用方从包源还原不到同一版本")
            elif stable and "-" in lower_bound(range_text) and not package.prerelease_ok:
                report.fail("prerelease", f"{package.id} → {dependency} {range_text}")
    return report


def git_head() -> str:
    result = subprocess.run(["git", "-C", str(ROOT), "rev-parse", "HEAD"], capture_output=True, text=True)
    if result.returncode != 0:
        raise GateError(f"读不到本次检出的提交：{result.stderr.strip()}")
    return result.stdout.strip()


def github_packages(packages: List[Package], version: str) -> List[str]:
    beta = release_info(version)["prerelease"]
    for package in packages:
        if not PACKAGE_ID.fullmatch(package.id):
            raise Failure(f"包源映射需要精确的 Kun 包名：{package.id}")
    return sorted((p.id for p in packages if beta or p.tier == "platform"), key=str.casefold)


def nuget_config(packages: List[Package], version: str) -> str:
    github_ids = github_packages(packages, version)
    root = ET.Element("configuration")
    sources = ET.SubElement(root, "packageSources")
    ET.SubElement(sources, "clear")
    ET.SubElement(sources, "add", key="nuget.org", value="https://api.nuget.org/v3/index.json")
    ET.SubElement(sources, "add", key="kun-platform", value="https://nuget.pkg.github.com/CavaiLAB/index.json")
    mapping = ET.SubElement(root, "packageSourceMapping")
    platform = ET.SubElement(mapping, "packageSource", key="kun-platform")
    for identifier in github_ids:
        ET.SubElement(platform, "package", pattern=identifier)
    public = ET.SubElement(mapping, "packageSource", key="nuget.org")
    ET.SubElement(public, "package", pattern="*")
    credentials = ET.SubElement(ET.SubElement(root, "packageSourceCredentials"), "kun-platform")
    ET.SubElement(credentials, "add", key="Username", value="%KUN_PLATFORM_FEED_USER%")
    ET.SubElement(credentials, "add", key="ClearTextPassword", value="%KUN_PLATFORM_FEED_TOKEN%")
    ET.indent(root, space="  ")
    return ET.tostring(root, encoding="unicode") + "\n"


def check_release_tags(version: str, tags: List[str], changelog: str) -> None:
    if "v" + version in tags:
        raise Failure(f"发布标签 v{version} 已存在")
    official = []
    for tag in tags:
        try:
            info = release_info(tag, tag=True)
        except ValueError:
            continue
        if not info["prerelease"]:
            official.append(info["version"])
    latest = max(official, key=release_order, default=None)
    entry = re.search(r"^## \[([^\]]+)\] - \d{4}-\d{2}-\d{2}\s*$", changelog, re.MULTILINE)
    documented = entry.group(1) if entry else None
    if latest != documented:
        raise Failure(f"CHANGELOG.md 最新发布段 {documented} 与最新正式标签 {latest} 不一致")


def release_notes(packages: List[Package], version: str, changelog: str) -> str:
    section = re.search(r"^## \[Unreleased\][ \t]*\n(.*?)(?=^## \[|\Z)", changelog, re.MULTILINE | re.DOTALL)
    body = section.group(1).strip() if section else ""
    if not any(line.strip() and line.strip() != "（尚无新变更。）" for line in body.splitlines()):
        raise Failure("CHANGELOG.md 的 [Unreleased] 需要至少一条变更")
    marker = "<!-- package-migration-table -->"
    if marker in body:
        guide = (ROOT / "docs/guides/package-migration.md").read_text(encoding="utf-8")
        section = re.search(r"^## 平台 0\.x 的包\n.*?(?=^## |\Z)", guide, re.MULTILINE | re.DOTALL)
        if not section or "| `Qian." not in section.group():
            raise Failure("迁移指南缺少旧平台包对照表")
        body = body.replace(marker, section.group().strip(), 1)
    github = set(github_packages(packages, version))
    rows = ["| 包 | 档位 | 包源 |", "|---|---|---|"]
    for package in sorted(packages, key=lambda p: p.id.casefold()):
        tier = "平台" if package.tier == "platform" else "内核"
        feed = "GitHub Packages" if package.id in github else "nuget.org"
        rows.append(f"| `{package.id}` | {tier} | {feed} |")
    return (f"# Kun {version}\n\n{body}\n\n"
            + f"升级已有项目请参阅[该版本的 NuGet 包迁移指南](https://github.com/CavaiLAB/kun/blob/v{version}/docs/guides/package-migration.md)。\n\n"
            + "## 本版本的包\n\n" + "\n".join(rows)
            + "\n\n## NuGet 包源\n\n还原前请设置 `KUN_PLATFORM_FEED_USER` 与 `KUN_PLATFORM_FEED_TOKEN`。\n\n"
            + "```xml\n" + nuget_config(packages, version) + "```\n")


# ---------- 命令行 ----------

def _load_list(path: Path) -> List[Package]:
    return [Package(**{**item, "exceptions": tuple(tuple(e) for e in item.get("exceptions", [])),
                       "declared_friends": tuple(item.get("declared_friends", [])),
                       "friend_dependencies": tuple(item.get("friend_dependencies", []))})
            for item in json.loads(path.read_text(encoding="utf-8"))]


def _summary(packages: List[Package]) -> str:
    count = lambda f: sum(1 for p in packages if f(p))  # noqa: E731
    return (f"{len(packages)} 个包（内核档 {count(lambda p: p.tier == 'kernel')}、平台档 {count(lambda p: p.tier == 'platform')}；"
            f"契约 {count(lambda p: p.role == 'contract')}；AOT 兼容 {count(lambda p: p.aot)}；"
            f"要出符号包的 {count(lambda p: p.symbols and p.build_output)}、不带编译输出的 {count(lambda p: not p.build_output)}）")


def _print(report: Report, title: str) -> int:
    if report.problems:
        print(f"{title}不通过：{len(report.problems)} 处")
        for problem in report.problems:
            print(f"  {problem}")
    else:
        print(f"{title}通过")
    return 1 if report.problems else 0


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="发版的包名单与制品核对；不发布、不推送")
    sub = parser.add_subparsers(dest="command", required=True)
    listing = sub.add_parser("list", help="列出包名单并核对")
    listing.add_argument("--json", action="store_true", help="以 JSON 输出名单")
    listing.add_argument("--expect", type=tier_counts, metavar="kernel=K,platform=P", help="核对两个档位的发布包数")
    verifying = sub.add_parser("verify-packages", help="核对 dotnet pack 的产物")
    verifying.add_argument("--version", required=True, help="打包时 -p:Version= 传入的版本（日历版本）")
    verifying.add_argument("--packages", required=True, type=Path, help="dotnet pack -o 的目录")
    verifying.add_argument("--list", type=Path, help="list --json 的输出；不给就现读项目文件")
    verifying.add_argument("--commit", help="包里应带的提交号；不给就取仓库的 HEAD")
    config = sub.add_parser("nuget-config", help="生成该版本的消费者包源映射")
    consumer = sub.add_parser("verify-consumer", help="用仓库外的原生与平台消费者核验实际制品")
    consumer.add_argument("--stage", choices=("local", "github", "published"), required=True)
    consumer.add_argument("--packages", type=Path, help="本地制品目录，供发布前及推送中间阶段使用")
    preflight = sub.add_parser("check-version", help="核对版本、发布标签、CHANGELOG 与两个正式包源")
    notes = sub.add_parser("notes", help="从 CHANGELOG.md 的 [Unreleased] 生成该版本的发布说明")
    feed = sub.add_parser("verify-feed", help="在指定包源核对本次发布的包")
    feed.add_argument("--feed", choices=("github", "nuget"), required=True)
    feed.add_argument("--wait-seconds", type=int, default=2700, help="最多等待索引的秒数，0 表示只核对一轮")
    preflight.add_argument("--resume-commit", help="复用当前已验证运行的提交；核对已存在的同名标签")
    for p in (config, notes, feed, preflight, consumer):
        p.add_argument("--version", required=True, help="日历版本")
        p.add_argument("--list", type=Path, help="list --json 的输出；不给就现读项目文件")
    args = parser.parse_args(argv)
    try:
        if args.command == "list":
            packages = discover()
            report = check_list(packages, args.expect)
            if args.json:
                print(json.dumps([asdict(p) for p in packages], ensure_ascii=False, indent=2))
                for problem in report.problems:
                    print(f"包名单核对不通过：{problem}", file=sys.stderr)
                return 1 if report.problems else 0
            print(f"包名单：{_summary(packages)}")
            return _print(report, "包名单核对")
        release_info(args.version)
        packages = _load_list(args.list) if args.list else discover()
        if args.command in ("nuget-config", "notes", "verify-feed", "check-version", "verify-consumer"):
            report = check_list(packages)
            if report.problems:
                print("\n".join(report.problems), file=sys.stderr)
                return 1
            if args.command == "verify-consumer":
                verify_consumer(nuget_config(packages, args.version), packages, args.version, args.stage, args.packages, ROOT / "global.json")
            elif args.command == "check-version":
                result = subprocess.run(["git", "tag", "--list", "v*"], cwd=ROOT, capture_output=True, text=True)
                if result.returncode:
                    raise Failure("无法读取仓库的发布标签")
                tags = result.stdout.splitlines()
                changelog = (ROOT / "CHANGELOG.md").read_text(encoding="utf-8")
                if args.resume_commit:
                    if git_head() != args.resume_commit:
                        raise Failure("恢复核对所用提交与检出不一致")
                    changelog = subprocess.check_output(["git", "show", "origin/main:CHANGELOG.md"], cwd=ROOT, text=True)
                    target = "v" + args.version
                    if target in tags:
                        commit = subprocess.check_output(["git", "rev-list", "-n1", target], cwd=ROOT, text=True).strip()
                        if commit != args.resume_commit:
                            raise Failure("此版本已关联其他提交")
                        tags.remove(target)
                        # 该版本的日志 PR 已合入时，完整复核其余标签与上一个发布段。
                        changelog = re.sub(r"^## \[" + re.escape(args.version) + r"\] - \d{4}-\d{2}-\d{2}[^\n]*$",
                                           "", changelog, count=1, flags=re.M)
                    for tag in tags:
                        try:
                            tagged = release_info(tag, tag=True)
                        except ValueError:
                            continue
                        if release_order(tagged["version"]) > release_order(args.version):
                            raise Failure("已有更新的发布标签，不能恢复旧版本")
                check_release_tags(args.version, tags, changelog)
                if args.resume_commit:
                    check_unpublished(packages, args.version, os.environ.get("GITHUB_TOKEN"), resume=True)
                else:
                    check_unpublished(packages, args.version, os.environ.get("GITHUB_TOKEN"))
                print(f"发布版本预检通过：{args.version}，{len(packages)} 个包")
            elif args.command == "verify-feed":
                github = set(github_packages(packages, args.version))
                names = [p.id for p in packages if (p.id in github) == (args.feed == "github")]
                token = os.environ.get("GITHUB_TOKEN") if args.feed == "github" else None
                verify_feed(args.feed, names, args.version, args.wait_seconds, token)
                print(f"{args.feed} 包源核对通过：{len(names)} 个包，版本 {args.version}")
            elif args.command == "nuget-config":
                print(nuget_config(packages, args.version), end="")
            else:
                print(release_notes(packages, args.version, (ROOT / "CHANGELOG.md").read_text(encoding="utf-8")), end="")
            return 0
        report = verify(packages, args.packages, args.version, args.commit or git_head())
        report.problems.extend(check_assembly_metadata(packages, args.packages, args.version))
        print(f"包名单：{_summary(packages)}；版本 {args.version}")
        return _print(report, "制品核对")
    except ValueError as error:
        print(f"版本不合要求：{error}", file=sys.stderr)
        return 2
    except (GateError, Failure, FeedError) as error:
        print(f"发版核对出错：{error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
