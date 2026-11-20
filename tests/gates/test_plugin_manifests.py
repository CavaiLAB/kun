"""动态清单门必须发现新增实现，不能靠旧组件白名单漏检。"""
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET


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
        project = self.root / "src/Kun.New/Kun.New.csproj"
        project.parent.mkdir(parents=True)
        project.write_text('<Project Sdk="Microsoft.NET.Sdk" />')
        assembly = project.parent / "bin/Release/net10.0/Kun.New.dll"
        assembly.parent.mkdir(parents=True)
        assembly.touch()
        (self.root / "Kun.slnx").write_text('<Solution><Project Path="src/Kun.New/Kun.New.csproj" /></Solution>')
        manifest = self.root / "plugins/qian.new/.qian-plugin/plugin.json"
        manifest.parent.mkdir(parents=True)
        manifest.write_text('{"id":"qian.new"}')

    def add_src_component(self):
        """仓库根 src/ 下的组件包（搬出过渡目录的那种）：Kun.slnx 登记、有构建产物、清单仍在 plugins/ 下。"""
        project = self.root / "src/Kun.Moved/Kun.Moved.csproj"
        project.parent.mkdir(parents=True)
        project.write_text('<Project Sdk="Microsoft.NET.Sdk" />')
        assembly = project.parent / "bin/Release/net10.0/Kun.Moved.dll"
        assembly.parent.mkdir(parents=True)
        assembly.touch()
        solution = self.root / "Kun.slnx"
        document = ET.parse(solution)
        ET.SubElement(document.getroot(), "Project", Path="src/Kun.Moved/Kun.Moved.csproj")
        document.write(solution)
        manifest = self.root / "plugins/qian.moved/.qian-plugin/plugin.json"
        manifest.parent.mkdir(parents=True)
        manifest.write_text('{"id":"qian.moved"}')

    def run_gate(self, inventory, src_inventory=()):
        commands = []

        def run(arguments):
            commands.append(arguments)
            if arguments[2] != "inventory":
                return "{}"
            return json.dumps(list(src_inventory) if Path(arguments[3]).stem == "Kun.Moved" else inventory)

        with patch.object(GATE, "run", side_effect=run), contextlib.redirect_stdout(io.StringIO()):
            result = GATE.check(self.root)
        return result, commands

    def test_new_component_is_discovered_without_updating_a_list(self):
        result, commands = self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}])
        self.assertEqual((1, 0), result)
        self.assertEqual(["inventory", "check"], [command[2] for command in commands])
        self.assertEqual(str(self.root / "plugins/qian.new"), commands[1][-1])

    def test_new_unclassified_implementation_fails(self):
        with self.assertRaisesRegex(ValueError, "没有入口注解或显式归属"):
            self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}, {"type": "Kun.New.Hidden", "component": None}])

    def test_stale_classification_fails(self):
        (self.root / "scripts/plugin-implementations.json").write_text(json.dumps({"Qian.Old:Deleted": {"owner": "qian.old", "reason": "旧实现"}}))
        with self.assertRaisesRegex(ValueError, "分类已失效"):
            self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}])

    def test_a_second_component_manifest_fails(self):
        extra = self.root / "samples/Copy/.qian-plugin/plugin.json"
        extra.parent.mkdir(parents=True)
        extra.write_text('{"id":"qian.new"}')
        with self.assertRaisesRegex(ValueError, "必须有唯一清单，实际 2 份"):
            self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}])

    def test_repository_sample_is_discovered_with_its_own_manifest(self):
        sample = self.root / "samples/Kun.Sample.Example"
        sample.mkdir(parents=True)
        (sample / "Kun.Sample.Example.csproj").write_text('<Project Sdk="Microsoft.NET.Sdk.Web" />')
        assembly = sample / "bin/Release/net10.0/Kun.Sample.Example.dll"
        assembly.parent.mkdir(parents=True)
        assembly.touch()
        manifest = sample / ".qian-plugin/plugin.json"
        manifest.parent.mkdir()
        manifest.write_text('{"id":"qian.example"}')
        (self.root / "Kun.slnx").write_text('<Solution><Project Path="samples/Kun.Sample.Example/Kun.Sample.Example.csproj" /></Solution>')
        with patch.object(GATE, "run", side_effect=['[{"type":"Example.Plugin","component":"qian.example"}]', '{}']) as run:
            with contextlib.redirect_stdout(io.StringIO()):
                self.assertEqual((1, 0), GATE.check(self.root))
        self.assertEqual(assembly.resolve(), Path(run.call_args_list[0].args[0][-1]).resolve())
        self.assertEqual(sample.resolve(), Path(run.call_args_list[1].args[0][-1]).resolve())

    def test_tool_is_located_next_to_the_solution(self):
        nested = self.root / "alternate"
        with patch.object(GATE, "run", return_value="[]"), contextlib.redirect_stdout(io.StringIO()):
            with self.assertRaisesRegex(ValueError, "没有发现任何组件入口"):
                GATE.check(self.root)
            (self.root / "tools/Kun.Plugin.Tool/bin/Release/net10.0/Kun.Plugin.Tool.dll").unlink()
            moved = nested / "tools/Kun.Plugin.Tool/bin/Release/net10.0/Kun.Plugin.Tool.dll"
            moved.parent.mkdir(parents=True)
            moved.touch()
            with self.assertRaisesRegex(ValueError, "先以 Release 构建 Kun.slnx"):
                GATE.check(self.root)

    def test_tool_declares_the_aspnetcore_shared_framework_itself(self):
        # 清单工具装载组件程序集盘点类型，*.AspNetCore 组件要 ASP.NET Core 共享框架。工具在自己的项目文件里声明，
        # 不靠被引用的包顺带传递：插件开发包拆包后传递就断过，盘点 Kun.Authorization.AspNetCore 时加载不到程序集。
        project = SOURCE.parents[1] / "tools/Kun.Plugin.Tool/Kun.Plugin.Tool.csproj"
        references = {item.get("Include") for item in ET.parse(project).getroot().iter("FrameworkReference")}
        self.assertIn("Microsoft.AspNetCore.App", references, f"{project.name} 没有自己声明 ASP.NET Core 共享框架")

    def test_component_in_repository_src_is_discovered_and_its_classification_counts(self):
        self.add_src_component()
        (self.root / "scripts/plugin-implementations.json").write_text(
            json.dumps({"Kun.Moved:Kun.Moved.Worker": {"owner": "qian.moved", "reason": "入口的程序化实现"}}))
        result, commands = self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}],
                                         [{"type": "Kun.Moved.Entry", "component": "qian.moved"},
                                          {"type": "Kun.Moved.Worker", "component": None}])
        self.assertEqual((2, 1), result)
        checked = [command[-1] for command in commands if command[2] == "check"]
        self.assertIn(str(self.root / "plugins/qian.moved"), checked)

    def test_component_in_repository_src_with_a_wrong_classification_fails(self):
        self.add_src_component()
        # 分类还写着搬家之前的程序集名：src/ 下的实现没有归属。
        (self.root / "scripts/plugin-implementations.json").write_text(
            json.dumps({"Qian.Moved:Qian.Moved.Worker": {"owner": "qian.moved", "reason": "入口的程序化实现"}}))
        with self.assertRaisesRegex(ValueError, "没有入口注解或显式归属：Kun.Moved:Kun.Moved.Worker"):
            self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}],
                          [{"type": "Kun.Moved.Entry", "component": "qian.moved"},
                           {"type": "Kun.Moved.Worker", "component": None}])

    def test_component_in_repository_src_without_a_build_fails(self):
        self.add_src_component()
        (self.root / "src/Kun.Moved/bin/Release/net10.0/Kun.Moved.dll").unlink()
        with self.assertRaisesRegex(ValueError, "缺少实际构建产物"):
            self.run_gate([{"type": "Kun.New.Entry", "component": "qian.new"}])

    def test_missing_build_never_counts_as_empty_success(self):
        (self.root / "src/Kun.New/bin/Release/net10.0/Kun.New.dll").unlink()
        with self.assertRaisesRegex(ValueError, "缺少实际构建产物"):
            self.run_gate([])


if __name__ == "__main__":
    unittest.main()
