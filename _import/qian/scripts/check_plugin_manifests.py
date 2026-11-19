#!/usr/bin/env python3
"""从实际编译程序集发现组件入口，校验唯一清单与非组件实现的显式归属。"""
from __future__ import annotations

import argparse
import json
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET


def run(arguments: list[str]) -> str:
    result = subprocess.run(arguments, text=True, capture_output=True, check=False)
    if result.returncode:
        raise ValueError(" ".join(arguments) + "\n" + result.stdout + result.stderr)
    return result.stdout


def solution_projects(root: Path) -> list[str]:
    """Qian.slnf 列出的项目，路径换算成相对 root；筛选文件里的路径相对它筛选的解决方案（仓库根的 Kun.slnx）所在目录。"""
    document = json.loads((root / "Qian.slnf").read_text(encoding="utf-8-sig"))["solution"]
    base = (root / document["path"].replace("\\", "/")).resolve().parent
    resolved = root.resolve()
    return [(base / project.replace("\\", "/")).resolve().relative_to(resolved).as_posix() for project in document["projects"]]


def check(root: Path, sync: bool = False) -> tuple[int, int]:
    tool = root / "tools/Qian.Plugin.Tool/bin/Release/net10.0/Qian.Plugin.Tool.dll"
    if not tool.is_file():
        raise ValueError("先以 Release 构建 Qian.slnf，再校验清单。")
    classifications = json.loads((root / "scripts/plugin-implementations.json").read_text())
    used: set[str] = set()
    components: dict[str, tuple[Path, str]] = {}
    for relative in solution_projects(root):
        if not relative.startswith(("packages/", "samples/")) or not relative.endswith(".csproj"):
            continue
        path = root / relative
        configuration = ET.parse(path).getroot()
        if configuration.findtext(".//TargetFramework", "").startswith("netstandard"):
            continue  # 分析器程序集不是可执行组件，不能实现 net10 的 IKunPlugin。
        name = configuration.findtext(".//AssemblyName") or path.stem
        assembly = path.parent / "bin/Release/net10.0" / (name + ".dll")
        if not assembly.is_file():
            raise ValueError("缺少实际构建产物：" + str(assembly.relative_to(root)))
        for implementation in json.loads(run(["dotnet", str(tool), "inventory", str(assembly)])):
            key = name + ":" + implementation["type"]
            component = implementation["component"]
            if component:
                if component in components:
                    raise ValueError("重复组件 ID：" + component)
                if key in classifications:
                    raise ValueError("真实入口不能同时分类为实现构件：" + key)
                components[component] = (assembly, implementation["type"])
            else:
                declaration = classifications.get(key)
                if not declaration or not declaration.get("owner") or not declaration.get("reason"):
                    raise ValueError("IKunPlugin 实现没有入口注解或显式归属：" + key)
                used.add(key)
    stale = set(classifications) - used
    if stale:
        raise ValueError("实现构件分类已失效：" + ", ".join(sorted(stale)))
    manifests: dict[str, list[Path]] = {}
    for base in (root / "plugins", root / "samples"):
        for manifest in base.rglob(".qian-plugin/plugin.json"):
            if any(part in {"bin", "obj", "node_modules", ".git"} for part in manifest.relative_to(root).parts):
                continue
            identifier = json.loads(manifest.read_text())["id"]
            manifests.setdefault(identifier, []).append(manifest.parent.parent)
    for identifier, (assembly, _) in sorted(components.items()):
        targets = manifests.get(identifier, [])
        if len(targets) != 1:
            raise ValueError(f"代码组件 {identifier} 必须有唯一清单，实际 {len(targets)} 份。")
        run(["dotnet", str(tool), "sync" if sync else "check", str(assembly), str(targets[0])])
        print(identifier + "：代码与唯一清单一致")
    if not components:
        raise ValueError("没有发现任何组件入口，拒绝空检查。")
    return len(components), len(used)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sync", action="store_true", help="显式同步所有已发现组件；CI 只使用默认只读检查")
    arguments = parser.parse_args()
    try:
        components, implementations = check(Path(__file__).resolve().parents[1], arguments.sync)
        print(f"清单门禁通过：{components} 个组件，{implementations} 个显式分类实现构件。")
        return 0
    except (ValueError, OSError, KeyError, ET.ParseError) as error:
        print(str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
