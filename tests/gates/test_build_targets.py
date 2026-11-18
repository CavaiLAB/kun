"""仓库根 Directory.Build.targets 里的公共 API 门禁的自测（S1-KUN-020）。

运行（仓库根目录）：python3 -m unittest discover -s tests/gates -v
读 Kun.slnx 里每个项目求值后的属性与项（dotnet msbuild -getProperty / -getItem，不需要还原）。
要 dotnet；CI 上缺 dotnet 直接失败而不是跳过。
"""
from __future__ import annotations

import json
import os
import shutil
import subprocess
import sys
import unittest
from concurrent.futures import ThreadPoolExecutor
from functools import lru_cache
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

import check_deps  # noqa: E402

NEEDS_DOTNET = unittest.skipUnless(shutil.which("dotnet") or os.environ.get("CI") == "true", "本机没有 dotnet")
PROPERTIES = ("IsPackable", "HasPublicApi")
ITEMS = ("PackageReference",)


def msbuild_evaluate(project: Path, properties=(), items=()) -> dict:
    cmd = (["dotnet", "msbuild", str(project), "-nologo"] + [f"-getProperty:{p}" for p in properties]
           + [f"-getItem:{i}" for i in items])
    result = subprocess.run(cmd, capture_output=True, text=True, env=check_deps._env())
    if result.returncode != 0:
        raise AssertionError(f"读取项目失败：{project}\n{result.stdout}{result.stderr}")
    return json.loads(result.stdout)


@lru_cache(maxsize=None)
def evaluated() -> dict:
    """Kun.slnx 里每个项目求值后的属性与项：{项目文件: (属性, 项)}。"""
    def read(path: Path):
        data = msbuild_evaluate(path, PROPERTIES, ITEMS)
        return path, (data.get("Properties") or {}, data.get("Items") or {})
    paths = check_deps.solution_projects(ROOT / "Kun.slnx")
    with ThreadPoolExecutor(max_workers=min(8, os.cpu_count() or 4)) as pool:
        return dict(pool.map(read, paths))


def is_true(props: dict, name: str) -> bool:
    return props.get(name, "").lower() == "true"


def package_ids(items: dict) -> set:
    return {i["Identity"] for i in items.get("PackageReference", [])}


@NEEDS_DOTNET
class PublicApiTests(unittest.TestCase):
    """可打包的项目都接公共 API 门禁，不再按 HasPublicApi 或包名判定。"""

    def test_packable_projects_have_both_files_and_the_analyzer(self):
        packable = {path: v for path, v in evaluated().items() if is_true(v[0], "IsPackable")}
        self.assertTrue(any(p.stem.startswith("Kun.") for p in packable) and any(p.stem.startswith("Qian.") for p in packable))
        for path, (props, items) in packable.items():
            with self.subTest(project=path.stem):
                self.assertTrue((path.parent / "PublicAPI.Shipped.txt").is_file())
                self.assertTrue((path.parent / "PublicAPI.Unshipped.txt").is_file())
                self.assertIn("Microsoft.CodeAnalysis.PublicApiAnalyzers", package_ids(items))

    def test_other_projects_do_not_get_it_and_nobody_writes_has_public_api(self):
        for path, (props, items) in evaluated().items():
            with self.subTest(project=path.stem):
                self.assertEqual("", props.get("HasPublicApi", ""))
                if not is_true(props, "IsPackable"):
                    self.assertNotIn("Microsoft.CodeAnalysis.PublicApiAnalyzers", package_ids(items))


if __name__ == "__main__":
    unittest.main()
