"""动态清单门必须发现新增实现，不能靠旧组件白名单漏检。"""
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch


SOURCE = Path(__file__).resolve().parents[2] / "scripts/check_plugin_manifests.py"
SPEC = importlib.util.spec_from_file_location("plugin_manifest_gate", SOURCE)
GATE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(GATE)


class PluginManifestGateTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        (self.root / "scripts").mkdir()
        (self.root / "scripts/plugin-implementations.json").write_text("{}")
        tool = self.root / "tools/Kun.Plugin.Tool/bin/Release/net10.0/Kun.Plugin.Tool.dll"
        tool.parent.mkdir(parents=True)
        tool.touch()
        project = self.root / "packages/Qian.New/Qian.New.csproj"
        project.parent.mkdir(parents=True)
        project.write_text('<Project Sdk="Microsoft.NET.Sdk" />')
        assembly = project.parent / "bin/Release/net10.0/Qian.New.dll"
        assembly.parent.mkdir(parents=True)
        assembly.touch()
        (self.root / "Qian.slnf").write_text('{"solution": {"path": "Kun.slnx", "projects": ["packages/Qian.New/Qian.New.csproj"]}}')
        manifest = self.root / "plugins/qian.new/.qian-plugin/plugin.json"
        manifest.parent.mkdir(parents=True)
        manifest.write_text('{"id":"qian.new"}')

    def run_gate(self, inventory):
        commands = []

        def run(arguments):
            commands.append(arguments)
            return json.dumps(inventory) if arguments[2] == "inventory" else "{}"

        with patch.object(GATE, "run", side_effect=run), contextlib.redirect_stdout(io.StringIO()):
            result = GATE.check(self.root)
        return result, commands

    def test_new_component_is_discovered_without_updating_a_list(self):
        result, commands = self.run_gate([{"type": "Qian.New.Entry", "component": "qian.new"}])
        self.assertEqual((1, 0), result)
        self.assertEqual(["inventory", "check"], [command[2] for command in commands])
        self.assertEqual(str(self.root / "plugins/qian.new"), commands[1][-1])

    def test_new_unclassified_implementation_fails(self):
        with self.assertRaisesRegex(ValueError, "没有入口注解或显式归属"):
            self.run_gate([{"type": "Qian.New.Entry", "component": "qian.new"}, {"type": "Qian.New.Hidden", "component": None}])

    def test_stale_classification_fails(self):
        (self.root / "scripts/plugin-implementations.json").write_text(json.dumps({"Qian.Old:Deleted": {"owner": "qian.old", "reason": "旧实现"}}))
        with self.assertRaisesRegex(ValueError, "分类已失效"):
            self.run_gate([{"type": "Qian.New.Entry", "component": "qian.new"}])

    def test_a_second_component_manifest_fails(self):
        extra = self.root / "samples/Copy/.qian-plugin/plugin.json"
        extra.parent.mkdir(parents=True)
        extra.write_text('{"id":"qian.new"}')
        with self.assertRaisesRegex(ValueError, "必须有唯一清单，实际 2 份"):
            self.run_gate([{"type": "Qian.New.Entry", "component": "qian.new"}])

    def test_tool_is_located_next_to_the_filtered_solution(self):
        # 工具按 Qian.slnf 筛的解决方案（仓库根的 Kun.slnx）定位：过渡目录下同样的相对路径不算。
        nested = self.root / "_import/qian"
        (nested / "scripts").mkdir(parents=True)
        (nested / "scripts/plugin-implementations.json").write_text("{}")
        (nested / "Qian.slnf").write_text('{"solution": {"path": "../../Kun.slnx", "projects": []}}')
        with patch.object(GATE, "run", return_value="[]"), contextlib.redirect_stdout(io.StringIO()):
            with self.assertRaisesRegex(ValueError, "没有发现任何组件入口"):
                GATE.check(nested)
            (self.root / "tools/Kun.Plugin.Tool/bin/Release/net10.0/Kun.Plugin.Tool.dll").unlink()
            moved = nested / "tools/Kun.Plugin.Tool/bin/Release/net10.0/Kun.Plugin.Tool.dll"
            moved.parent.mkdir(parents=True)
            moved.touch()
            with self.assertRaisesRegex(ValueError, "先以 Release 构建 Kun.slnx"):
                GATE.check(nested)

    def test_missing_build_never_counts_as_empty_success(self):
        (self.root / "packages/Qian.New/bin/Release/net10.0/Qian.New.dll").unlink()
        with self.assertRaisesRegex(ValueError, "缺少实际构建产物"):
            self.run_gate([])


if __name__ == "__main__":
    unittest.main()
