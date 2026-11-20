"""根解决方案、SDK、NuGet 配置及 .NET 项目布局的守护。"""
from __future__ import annotations

import re
import subprocess
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

REPOSITORY = Path(__file__).resolve().parents[2]
# 读取写成 $(KunVersion)，定义写成 <KunVersion>。
KUN_VERSION = re.compile(r"\$\(\s*KunVersion\s*\)|<\s*KunVersion\b", re.IGNORECASE)
COMMENT = re.compile(r"<!--.*?-->", re.DOTALL)


def tracked(*patterns: str) -> list[str]:
    output = subprocess.run(["git", "ls-files", "-z", "--", *patterns], cwd=REPOSITORY, capture_output=True, text=True, check=True)
    return [name for name in output.stdout.split("\0") if name]


class BuildMergeTests(unittest.TestCase):
    def test_no_msbuild_file_defines_or_reads_kun_version(self):
        offenders = []
        files = tracked("*.csproj", "*.props", "*.targets")
        self.assertIn("Directory.Packages.props", files)
        for name in files:
            text = (REPOSITORY / name).read_text(encoding="utf-8", errors="replace")
            # 注释里提到它不算读取；换成同样多的换行，行号不变。
            text = COMMENT.sub(lambda match: "\n" * match.group(0).count("\n"), text)
            offenders += [f"{name}:{number}" for number, line in enumerate(text.splitlines(), 1) if KUN_VERSION.search(line)]
        self.assertEqual([], offenders, "请使用 Kun 源码项目引用；文档站从仓库根 CHANGELOG.md 读取发布版本（S1-KUN-020）")

    def test_no_dotnet_build_inputs_under_import(self):
        remaining = tracked("_import/**/*.csproj", "_import/**/*.props", "_import/**/*.targets", "_import/**/*.slnf")
        self.assertEqual([], remaining, "请将 .NET 项目和构建输入放在仓库根的对应目录。")
        projects = [item.attrib["Path"].replace("\\", "/") for item in ET.parse(REPOSITORY / "Kun.slnx").iter("Project")]
        self.assertTrue(projects)
        self.assertEqual([], [path for path in projects if path.startswith("_import/")])

    def test_only_the_root_global_json(self):
        self.assertEqual(["global.json"], [name for name in tracked() if Path(name).name.lower() == "global.json"],
                         "SDK 版本只钉在仓库根的 global.json：子目录里不要再放一份，dotnet 会向上找到根上这份")

    def test_single_nuget_config_with_nuget_org_only(self):
        self.assertEqual(["NuGet.config"], [name for name in tracked() if Path(name).name.lower() == "nuget.config"])
        configuration = ET.parse(REPOSITORY / "NuGet.config").getroot()
        sources = configuration.find("packageSources")
        self.assertIsNotNone(sources)
        self.assertEqual(["clear", "add"], [element.tag for element in sources])
        self.assertEqual("https://api.nuget.org/v3/index.json", sources.find("add").get("value"))
        patterns = [(source.get("key"), package.get("pattern")) for source in configuration.iter("packageSource") for package in source.iter("package")]
        self.assertEqual([("nuget.org", "*")], patterns)


if __name__ == "__main__":
    unittest.main()
