"""本地、混合包源和发布后的消费者须遵守同一份精确包映射。"""
import json
import sys
import tempfile
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import release_consumer as consumer
import release_packages as release


class ConsumerTests(unittest.TestCase):
    packages = [release.Package("Kun.Core", "kernel", "contract", "src/Kun.Core/Kun.Core.csproj"),
                release.Package("Kun.Data", "platform", "provider", "src/Kun.Data/Kun.Data.csproj")]

    def test_source_mapping_by_publication_stage(self):
        for version in ("2026.11.20", "2026.11.20-hotfix.1", "2026.11.20-beta.1"):
            with self.subTest(version=version), tempfile.TemporaryDirectory() as tmp:
                artifacts = Path(tmp).resolve()
                for package in self.packages:
                    (artifacts / f"{package.id}.{version}.nupkg").touch()
                cfg = release.nuget_config(self.packages, version)
                beta = "-beta." in version
                for phase, expected in [
                    ("local", [str(artifacts)] * 2),
                    ("github", [consumer.GITHUB if beta else str(artifacts), consumer.GITHUB]),
                    ("published", [consumer.GITHUB if beta else consumer.NUGET, consumer.GITHUB]),
                ]:
                    with self.subTest(stage=phase):
                        xml, sources = consumer.source_config(cfg, self.packages, version, phase, artifacts)
                        self.assertEqual(expected, list(sources.values()))
                        configured = {e.get("value") for e in ET.fromstring(xml).findall("./packageSources/add")}
                        self.assertTrue(set(expected) <= configured)
                        self.assertEqual(consumer.GITHUB in expected, "packageSourceCredentials" in xml)
                (artifacts / f"Kun.Core.{version}.nupkg").unlink()
                with self.assertRaisesRegex(consumer.FeedError, "Kun.Core"):
                    consumer.source_config(cfg, self.packages, version, "local", artifacts)

    def test_restore_requires_exact_source_version_and_direct_dependencies(self):
        with tempfile.TemporaryDirectory() as tmp:
            work = Path(tmp)
            libs = work / "obj/project.assets.json"
            libs.parent.mkdir()
            cache = work / "cache"
            meta = cache / "kun.core/2026.11.20/.nupkg.metadata"
            meta.parent.mkdir(parents=True)
            expected = {"Kun.Core": consumer.NUGET}
            libs.write_text(json.dumps({"libraries": {"Kun.Core/2026.11.20": {"type": "package"}, "Other/1.0": {"type":"package"}}}))
            meta.write_text(json.dumps({"source": consumer.NUGET}))
            self.assertEqual({"Kun.Core"}, consumer.check_restore(work, cache, expected, "2026.11.20", {"Kun.Core"}))
            for version, direct, source in [("2026.11.19", {"Kun.Core"}, consumer.NUGET),
                                            ("2026.11.20", {"Kun.Missing"}, consumer.NUGET),
                                            ("2026.11.20", {"Kun.Core"}, consumer.GITHUB)]:
                meta.write_text(json.dumps({"source": source}))
                with self.assertRaises(consumer.FeedError):
                    consumer.check_restore(work, cache, expected, version, direct)
