"""用隔离的包缓存还原两个仓库外消费者，核对运行结果及包来源。"""
from __future__ import annotations

import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

from release_feed import FeedError

NUGET = "https://api.nuget.org/v3/index.json"
GITHUB = "https://nuget.pkg.github.com/CavaiLAB/index.json"
PROBES = Path(__file__).resolve().parent / "release-probes"
PLATFORM_PROBE = {"Kun.Data", "Kun.Plugin.Sdk", "Kun.Plugin.Sdk.Generators"}


def source_config(config, packages, version, stage, artifacts):
    document = ET.fromstring(config)
    github = {p.get("pattern") for p in document.findall("./packageSourceMapping/packageSource[@key='kun-platform']/package")}
    sources = {p.id: GITHUB if p.id in github else NUGET for p in packages}
    local_ids = {p.id for p in packages if stage == "local" or stage == "github" and p.id not in github}
    if local_ids:
        if artifacts is None or not artifacts.is_dir():
            raise FeedError("本地消费阶段需要已打包的制品目录")
        local = str(artifacts.resolve())
        ET.SubElement(document.find("packageSources"), "add", key="artifacts", value=local)
        mapping = document.find("packageSourceMapping")
        local_mapping = ET.SubElement(mapping, "packageSource", key="artifacts")
        for identifier in sorted(local_ids):
            if not (artifacts / f"{identifier}.{version}.nupkg").is_file():
                raise FeedError(f"缺少本地制品 {identifier} {version}")
            ET.SubElement(local_mapping, "package", pattern=identifier)
            sources[identifier] = local
        if github <= local_ids:
            mapping.remove(mapping.find("packageSource[@key='kun-platform']"))
            package_sources = document.find("packageSources")
            package_sources.remove(package_sources.find("add[@key='kun-platform']"))
            document.remove(document.find("packageSourceCredentials"))
    return ET.tostring(document, encoding="unicode"), sources


def check_restore(project, cache, expected, version, direct):
    assets = json.loads((project / "obj/project.assets.json").read_text())
    found = set()
    for identity, entry in assets["libraries"].items():
        name, actual = identity.split("/", 1)
        if entry.get("type") != "package" or not name.casefold().startswith("kun."):
            continue
        if name not in expected or actual != version:
            raise FeedError(f"消费者解析出名单外的包或版本 {identity}")
        meta = cache / name.lower() / actual.lower() / ".nupkg.metadata"
        origin = json.loads(meta.read_text()).get("source") if meta.is_file() else None
        if not origin or origin.rstrip("/") != expected[name].rstrip("/"):
            raise FeedError(f"{identity} 的还原来源与映射不符")
        found.add(name)
    if direct - found:
        raise FeedError(f"消费者没有还原全部直接依赖：{', '.join(sorted(direct - found))}")
    return found


def write_project(directory, kind, ids, version):
    kernel = kind == "Kernel"
    project = ET.Element("Project", Sdk="Microsoft.NET.Sdk.Web" if kernel else "Microsoft.NET.Sdk")
    props = ET.SubElement(project, "PropertyGroup")
    for key, value in {"OutputType": "Exe", "TargetFramework": "net10.0", "ImplicitUsings": "enable",
                       "Nullable": "enable", "TreatWarningsAsErrors": "true", "NuGetAudit": "false",
                       "PublishAot": str(kernel).lower(), "AssemblyName": "KernelProbe" if kernel else "ReleaseProbe"}.items():
        ET.SubElement(props, key).text = value
    group = ET.SubElement(project, "ItemGroup")
    for name in sorted(ids):
        ET.SubElement(group, "PackageReference", Include=name, Version=f"[{version}]")
    directory.mkdir()
    ET.ElementTree(project).write(directory / "Probe.csproj", encoding="utf-8", xml_declaration=True)
    shutil.copyfile(PROBES / (kind + ".cs"), directory / "Program.cs")


def run_probe(args, directory, env):
    result = subprocess.run(args, cwd=directory, env=env, text=True, capture_output=True)
    if result.returncode:
        raise FeedError(f"消费者命令 {Path(args[0]).name} 失败：\n{result.stdout}{result.stderr}")
    return result.stdout


def verify_consumer(config, packages, version, stage, artifacts, sdk):
    config, sources = source_config(config, packages, version, stage, artifacts)
    if not PLATFORM_PROBE <= set(sources):
        raise FeedError("平台探针所需的三个组件包必须出现在发布名单中")
    if GITHUB in sources.values() and not all(os.environ.get(key) for key in ("KUN_PLATFORM_FEED_USER", "KUN_PLATFORM_FEED_TOKEN")):
        raise FeedError("GitHub Packages 消费阶段缺少包源用户名或令牌")
    system = {"linux": "linux", "darwin": "osx", "win32": "win"}.get(sys.platform)
    cpu = {"aarch64": "arm64", "arm64": "arm64", "amd64": "x64", "x86_64": "x64"}.get(platform.machine().lower())
    if not system or not cpu:
        raise FeedError("当前宿主架构不能执行原生消费者")
    rid = f"{system}-{cpu}"
    with tempfile.TemporaryDirectory(prefix="kun-release-consumers-") as tmp:
        work = Path(tmp)
        (work / "NuGet.config").write_text(config, encoding="utf-8")
        shutil.copyfile(sdk, work / "global.json")
        for kind in ("Kernel", "Platform"):
            direct = {p.id for p in packages if p.tier == kind.lower()}
            if kind == "Platform":
                direct |= PLATFORM_PROBE
            if not direct:
                raise FeedError(f"{kind} 消费者没有直接依赖")
            directory = work / kind
            write_project(directory, kind, direct, version)
            cache = directory / ".packages"
            env = dict(os.environ, DOTNET_NOLOGO="1", NUGET_PACKAGES=str(cache),
                       NUGET_HTTP_CACHE_PATH=str(directory / ".http-cache"), NUGET_SCRATCH=str(directory / ".scratch"),
                       NUGET_PLUGINS_CACHE_PATH=str(directory / ".plugins-cache"))
            restore = ["dotnet", "restore", "Probe.csproj", "--configfile", str(work / "NuGet.config"),
                       "-p:Configuration=Release"]
            if kind == "Kernel":
                restore += ["-r", rid]
            run_probe(restore, directory, env)
            restored = check_restore(directory, cache, sources, version, direct)
            if kind == "Kernel":
                output = directory / "native"
                run_probe(["dotnet", "publish", "Probe.csproj", "-c", "Release", "-r", rid, "--no-restore",
                           "-o", str(output)], directory, env)
                binary = output / ("KernelProbe.exe" if system == "win" else "KernelProbe")
                if (output / "KernelProbe.dll").exists():
                    raise FeedError("内核消费者未生成原生可执行文件")
                if "Kun kernel consumer ready" not in run_probe([str(binary)], directory, env).splitlines():
                    raise FeedError("内核消费者未完成 HTTP 请求")
            else:
                result = run_probe(["dotnet", "run", "--project", "Probe.csproj", "-c", "Release", "--no-restore"], directory, env)
                report = json.loads(result.strip().splitlines()[-1])
                plugin, data = report["probe"], report["data"]
                if (plugin["ComponentId"], plugin["RuntimeEntry"], plugin["Provides"]) != ("qian.release-probe", "ReleaseProbe.ProbePlugin, ReleaseProbe", ["ReleaseProbe"]):
                    raise FeedError("平台消费者的组件元数据与提供的服务不符")
                caps = plugin["Capabilities"]
                if len(caps) != 1 or caps[0]["Id"] != "qian.release-probe/probe.read" or caps[0]["Metadata"].get("requiredScopes") != ["qian.release-probe:read"]:
                    raise FeedError("平台消费者的能力契约与声明不符")
                if (data["ComponentId"], data["RuntimeEntry"], data["Provides"]) != ("qian.data", "Kun.Data.DataPlugin, Kun.Data", ["Data"]):
                    raise FeedError("平台消费者读取的数据组件元数据与声明不符")
            print(f"{kind} 消费者运行成功，核对 {len(restored)} 个包的来源与版本")
