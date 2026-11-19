#!/usr/bin/env python3
"""AOT 清单：Kun.slnx 里在项目文件中写了 IsAotCompatible=true 的项目（S1-KUN-020）。

清单只有这一份，没有别的副本：

- tests/Kun.AotSmoke 引用其中每一个，并对每个程序集整体扎根（TrimmerRootAssembly），CI 的「AOT 冒烟宿主」作业
  原生发布后运行它；
- scripts/release.sh 发版前照这个作业的做法核对、原生发布并运行冒烟宿主；
- 门禁自测（仓库根的 tests/gates）与本脚本的 --check 核对冒烟宿主的项目引用正好等于清单。

写了 IsAotCompatible=true 的项目，SDK 打开 IsTrimmable 与裁剪、AOT、单文件三个分析器，普通构建里就查。
守卫在仓库根的 Directory.Build.targets：不许再写 IsAotCandidate、可打包的库不许写 PublishAot、
写了 IsAotCompatible 不许关分析器。

用法（在仓库根运行，不需要先还原）：

  python3 scripts/aot_projects.py            列出清单，每行一个项目文件，相对仓库根
  python3 scripts/aot_projects.py --names    只列项目名
  python3 scripts/aot_projects.py --check    核对：清单里都是可打包的库；冒烟宿主的项目引用与扎根的程序集正好等于清单
"""
from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
from concurrent.futures import ThreadPoolExecutor
from dataclasses import dataclass
from pathlib import Path
from typing import List, Optional

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_deps import GateError, ROOT, _env, _rel, solution_projects  # noqa: E402

SOLUTION = ROOT / "Kun.slnx"
SMOKE_HOST = ROOT / "tests" / "Kun.AotSmoke" / "Kun.AotSmoke.csproj"
PROPERTIES = ("IsAotCompatible", "IsPackable", "AssemblyName")


@dataclass(frozen=True)
class Project:
    path: Path
    aot_compatible: bool
    packable: bool
    assembly_name: str

    @property
    def name(self) -> str:
        return self.path.stem


def _msbuild(project: Path, properties=(), items=()) -> dict:
    cmd = (["dotnet", "msbuild", str(project), "-nologo"] + [f"-getProperty:{p}" for p in properties]
           + [f"-getItem:{i}" for i in items])
    result = subprocess.run(cmd, capture_output=True, text=True, env=_env())
    if result.returncode != 0:
        raise GateError(f"读取项目失败：{_rel(project)}\n{result.stdout}{result.stderr}")
    return json.loads(result.stdout)


def read_project(path: Path) -> Project:
    props = _msbuild(path, PROPERTIES).get("Properties") or {}
    return Project(path, props.get("IsAotCompatible", "").lower() == "true", props.get("IsPackable", "").lower() == "true",
                   props.get("AssemblyName") or path.stem)


def solution_projects_evaluated(solution: Path = SOLUTION) -> List[Project]:
    paths = solution_projects(solution)
    with ThreadPoolExecutor(max_workers=min(8, os.cpu_count() or 4)) as pool:
        return list(pool.map(read_project, paths))


def aot_projects(projects: List[Project]) -> List[Project]:
    return sorted((p for p in projects if p.aot_compatible), key=lambda p: p.name.lower())


def smoke_host(path: Path = SMOKE_HOST) -> tuple:
    """冒烟宿主的项目引用（绝对路径）与扎根的程序集名。"""
    items = _msbuild(path, items=("ProjectReference", "TrimmerRootAssembly")).get("Items") or {}
    references = sorted({(path.parent / i["Identity"].replace("\\", "/")).resolve() for i in items.get("ProjectReference", [])})
    roots = sorted({i["Identity"] for i in items.get("TrimmerRootAssembly", [])})
    return references, roots


def check(projects: List[Project], references: List[Path], roots: List[str], smoke: Path = SMOKE_HOST) -> List[str]:
    listed = aot_projects(projects)
    problems = [f"{_rel(p.path)} 写了 IsAotCompatible=true，却不可打包：IsAotCompatible 只写在可打包的库上，"
                f"要原生发布的可执行项目写 PublishAot" for p in listed if not p.packable]
    if any(p.path == smoke.resolve() for p in listed):
        problems.append(f"{_rel(smoke)} 自己不能写 IsAotCompatible")
    expected = {p.path for p in listed}
    actual = set(references)
    problems += [f"冒烟宿主没有引用清单里的 {_rel(p)}：在 {_rel(smoke)} 里加上它的项目引用" for p in sorted(expected - actual)]
    problems += [f"冒烟宿主引用了不在清单里的 {_rel(p)}：它没写 IsAotCompatible=true，从 {_rel(smoke)} 里删掉"
                 for p in sorted(actual - expected)]
    expected_roots = {p.assembly_name for p in listed}
    problems += [f"冒烟宿主没有扎根程序集 {n}" for n in sorted(expected_roots - set(roots))]
    problems += [f"冒烟宿主扎根了不在清单里的程序集 {n}" for n in sorted(set(roots) - expected_roots)]
    return problems


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="列出 AOT 清单（项目文件里写了 IsAotCompatible=true 的项目），或核对冒烟宿主")
    group = parser.add_mutually_exclusive_group()
    group.add_argument("--names", action="store_true", help="只列项目名")
    group.add_argument("--check", action="store_true", help="核对清单与冒烟宿主 tests/Kun.AotSmoke")
    args = parser.parse_args(argv)
    try:
        projects = solution_projects_evaluated()
        if args.check:
            references, roots = smoke_host()
            problems = check(projects, references, roots)
            if problems:
                print(f"AOT 清单核对失败（{len(problems)} 处）：")
                for problem in problems:
                    print(f"  {problem}")
                return 1
            print(f"AOT 清单核对通过：{len(aot_projects(projects))} 个项目，冒烟宿主的项目引用与扎根的程序集都对得上")
            return 0
    except GateError as ex:
        print(f"AOT 清单出错：{ex}", file=sys.stderr)
        return 2
    for p in aot_projects(projects):
        print(p.name if args.names else _rel(p.path))
    return 0


if __name__ == "__main__":
    sys.exit(main())
