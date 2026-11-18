"""构建并轨（S1-KUN-020）的门禁自测：原 Qian 改为项目引用 Kun 源码之后，构建里不再有 Kun 的包版本单点。

运行：python3 -m unittest discover -s tests/gates -v
- 仓库里没有 csproj、props、targets 引用 $(KunVersion)。_import/qian/Directory.Packages.props 里留的 <KunVersion>
  只给文档站、scripts/check_docs.py 与镜像构建证据读取，任何构建都不读它；
- Qian.slnf 与仓库根 Kun.slnx 里 _import/qian/ 下的项目一一对应：scripts/dotnet_ci.py 按 Qian.slnf 分片，
  只加进 Kun.slnx、没加进 Qian.slnf 的项目哪个分片都不跑；
- 子目录的 global.json 暂留（文档站与原 Qian 的工作流还读它），内容必须与仓库根的相同；
- 只有仓库根一份 NuGet.config，包源只有 nuget.org，CI 还原不需要私有源凭据。
只依赖标准库与 git。
"""
from __future__ import annotations

import json
import re
import subprocess
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
REPOSITORY = ROOT.parents[1]
PREFIX = ROOT.relative_to(REPOSITORY).as_posix() + "/"
KUN_VERSION = re.compile(r"\$\(\s*KunVersion\s*\)", re.IGNORECASE)
COMMENT = re.compile(r"<!--.*?-->", re.DOTALL)


def tracked(*patterns: str) -> list[str]:
    output = subprocess.run(["git", "ls-files", "-z", "--", *patterns], cwd=REPOSITORY, capture_output=True, text=True, check=True)
    return [name for name in output.stdout.split("\0") if name]


class BuildMergeTests(unittest.TestCase):
    def test_no_msbuild_file_reads_kun_version(self):
        offenders = []
        files = tracked("*.csproj", "*.props", "*.targets")
        self.assertIn(PREFIX + "Directory.Packages.props", files)
        for name in files:
            text = (REPOSITORY / name).read_text(encoding="utf-8", errors="replace")
            # 注释里提到它不算读取；换成同样多的换行，行号不变。
            text = COMMENT.sub(lambda match: "\n" * match.group(0).count("\n"), text)
            offenders += [f"{name}:{number}" for number, line in enumerate(text.splitlines(), 1) if KUN_VERSION.search(line)]
        self.assertEqual([], offenders, "构建不再读 KunVersion：原 Qian 已改为项目引用 Kun 源码（S1-KUN-020）")

    def test_qian_filter_matches_the_solution(self):
        solution = REPOSITORY / "Kun.slnx"
        document = json.loads((ROOT / "Qian.slnf").read_text(encoding="utf-8-sig"))["solution"]
        self.assertEqual(solution.resolve(), (ROOT / document["path"]).resolve(), "Qian.slnf 必须筛选仓库根的 Kun.slnx")
        listed = [project.replace("\\", "/") for project in document["projects"]]
        self.assertEqual(len(listed), len(set(listed)), "Qian.slnf 里有重复的项目")
        in_solution = [node.attrib["Path"].replace("\\", "/") for node in ET.parse(solution).getroot().iter("Project")]
        self.assertEqual(sorted(path for path in in_solution if path.startswith(PREFIX)), sorted(listed),
                         f"{PREFIX} 下的项目要同时加进 Kun.slnx 与 Qian.slnf")

    def test_transitional_global_json_matches_root(self):
        root = json.loads((REPOSITORY / "global.json").read_text(encoding="utf-8"))
        self.assertEqual(root, json.loads((ROOT / "global.json").read_text(encoding="utf-8")),
                         f"{PREFIX}global.json 必须与仓库根的 global.json 相同，改一处要两处一起改")

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
