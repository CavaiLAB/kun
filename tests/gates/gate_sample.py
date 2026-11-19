"""核验门禁样例的分析器作用域，再运行样例构建。"""
from pathlib import Path
import subprocess


def check_analyzer_scope(project: Path, *, expected: bool = True) -> None:
    actual = subprocess.check_output(
        ["dotnet", "msbuild", str(project), "-nologo", "-getProperty:_KunPlatformAnalyzers"], text=True,
    ).strip()
    if actual != str(expected).lower():
        raise AssertionError(f"{project.name} 的分析器作用域为 {actual}，预期 {expected}")
