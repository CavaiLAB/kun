"""真正的 Kun 注册调用与方法引用均必须经过安全入口，关闭警告即错误也不可绕过。"""
from pathlib import Path
from gate_sample import check_analyzer_scope
import os
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]

class AuthorizedHandlerTests(unittest.TestCase):
    def test_registration_boundary(self):
        for name, succeeds in [("Direct", False), ("Delegate", False),
                               ("GenericWrapper", False), ("FakeBridge", False), ("Safe", True),
                               ("Unprotected", True), ("Unrelated", True)]:
            with self.subTest(sample=name):
                project = ROOT / "tests/gates/authz" / name / (name + ".csproj")
                check_analyzer_scope(project)
                result = subprocess.run(["dotnet", "build", str(project), "-c", "Release"], cwd=ROOT,
                    env=dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en"), text=True, capture_output=True)
                output = result.stdout + result.stderr
                self.assertEqual(result.returncode == 0, succeeds, output[-5000:])
                if not succeeds:
                    self.assertIn("error KUNAUTHZ001", output)
