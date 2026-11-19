"""依赖门禁 scripts/check_deps.py 与档位声明检查（Directory.Build.targets）的自测（S1-KUN-020）。

运行（仓库根目录）：python3 -m unittest discover -s tests/gates -v
- 规则、路径与例外用合成的 project.assets.json 验证，不需要 dotnet；
- 端到端用 tests/gates/depgraph 下的样例项目真跑一遍，另读仓库里真实的依赖图核对两条验收（S1-RT-014、S1-IDP-007）。
  这几组要 dotnet，真实依赖图那两组还要先还原 Kun.slnx；CI 上缺 dotnet 直接失败而不是跳过。
样例项目用与规则无关的名字，档位全部显式写在项目文件里，门禁只认这些声明。
"""
from __future__ import annotations

import json
import os
import shutil
import subprocess
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "scripts" / "check_deps.py"
SAMPLES = ROOT / "tests" / "gates" / "depgraph"
sys.path.insert(0, str(SCRIPT.parent))

import check_deps  # noqa: E402

PROJECTS = ROOT / "packages"
NEEDS_DOTNET = unittest.skipUnless(shutil.which("dotnet") or os.environ.get("CI") == "true", "本机没有 dotnet")


def make_assets(subject, direct, edges, projects=(), private=(), build_only=(), framework="net10.0", rid_copy=False):
    """合成一份 project.assets.json。edges: 名称 -> 依赖名称列表；projects 里的按项目引用处理，private 里的按 PrivateAssets=all
    处理；包默认带编译与运行时程序集，build_only 里的只带构建资产（分析器之类）。"""
    projects, private, build_only = set(projects), set(private), set(build_only)
    subject_dir = PROJECTS / subject
    targets, libraries = {}, {}
    for name, deps in edges.items():
        kind = "project" if name in projects else "package"
        targets[f"{name}/1.0.0"] = {"type": kind, "dependencies": {d: "1.0.0" for d in deps}}
        if name in build_only:
            targets[f"{name}/1.0.0"]["build"] = {f"build/{name}.props": {}}
        else:
            assembly = f"bin/placeholder/{name}.dll" if kind == "project" else f"lib/{framework}/{name}.dll"
            targets[f"{name}/1.0.0"].update(compile={assembly: {}}, runtime={assembly: {}})
        libraries[f"{name}/1.0.0"] = ({"type": "project", "path": f"../{name}/{name}.csproj", "msbuildProject": f"../{name}/{name}.csproj"}
                                      if kind == "project" else {"type": "package", "path": f"{name.lower()}/1.0.0"})
    project_refs = {str((subject_dir / f"../{n}/{n}.csproj").resolve()): {"projectPath": str((subject_dir / f"../{n}/{n}.csproj").resolve())}
                    for n in direct if n in projects}
    package_refs = {n: dict({"target": "Package", "version": "[1.0.0, )"}, **({"suppressParent": "All"} if n in private else {}))
                    for n in direct if n not in projects}
    all_targets = {framework: targets}
    if rid_copy:
        all_targets[f"{framework}/linux-x64"] = targets
    return {
        "version": 3,
        "targets": all_targets,
        "libraries": libraries,
        "project": {
            "version": "1.0.0",
            "restore": {"frameworks": {framework: {"targetAlias": framework, "projectReferences": project_refs}}},
            "frameworks": {framework: {"targetAlias": framework, "dependencies": package_refs}},
        },
    }


def package(name, tier="", role="", zero="", exceptions=()):
    return check_deps.Package(name, PROJECTS / name / f"{name}.csproj", Path(f"{name}.assets.json"), tier, role, zero,
                              tuple(check_deps.TierException(*e) for e in exceptions))


def run(packages, assets):
    """assets: 包名 -> 合成的 assets；没给的包按没有依赖处理。"""
    def load(path):
        name = path.name[:-len(".assets.json")]
        return assets.get(name) or make_assets(name, [], {})
    return check_deps.evaluate(packages, load)


def found(violations):
    return {(v.rule, " → ".join(v.path)) for v in violations}


class DeclarationTests(unittest.TestCase):
    def test_missing_or_invalid_declarations_are_reported_case_sensitively(self):
        packages = [package("Alpha"), package("Bravo", "Kernel", "provider"), package("Charlie", "kernel", "port"),
                    package("Delta", "kernel", "contract", zero="yes"), package("Echo", "kernel", "contract")]
        violations, _ = run(packages, {})
        self.assertEqual({("R", "Alpha"), ("R", "Bravo"), ("R", "Charlie"), ("R", "Delta")}, found(violations))
        details = {v.package: v.detail for v in violations}
        self.assertIn("KunTier、KunRole", details["Alpha"])
        self.assertIn("Kernel", details["Bravo"])

    def test_project_dependency_outside_the_checked_set_is_reported(self):
        # 只查子集（如 Qian.slnf）时，子集外的项目没有档位可查；可打包项目引用不可打包的项目同样档位不明。
        packages = [package("Alpha", "kernel", "provider")]
        assets = {"Alpha": make_assets("Alpha", ["Helper"], {"Helper": ["Kun.Core"], "Kun.Core": []}, projects={"Helper", "Kun.Core"})}
        [violation] = run(packages, assets)[0]
        self.assertEqual(("R", ["Alpha", "Helper"]), (violation.rule, violation.path))
        self.assertIn("2 个项目", violation.detail)
        self.assertIn("Helper、Kun.Core", violation.detail)

    def test_undeclared_dependency_is_not_classified(self):
        # 没写档位的包自己报 R；别的包依赖它时不按任何档位算，不会连带误报。
        packages = [package("Alpha", "kernel", "provider"), package("Bravo")]
        violations, _ = run(packages, {"Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": []}, projects={"Bravo"})})
        self.assertEqual({("R", "Bravo")}, found(violations))


class RuleTests(unittest.TestCase):
    def test_kernel_must_not_reach_platform_directly_or_transitively(self):
        packages = [package("Alpha", "kernel", "provider"), package("Bravo", "kernel", "provider"), package("Charlie", "platform", "contract")]
        assets = {
            "Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": ["Charlie"], "Charlie": []}, projects={"Bravo", "Charlie"}),
            "Bravo": make_assets("Bravo", ["Charlie"], {"Charlie": []}, projects={"Charlie"}),
        }
        violations, _ = run(packages, assets)
        self.assertEqual({("K", "Alpha → Bravo → Charlie"), ("K", "Bravo → Charlie")}, found(violations))

    def test_kernel_reaching_platform_through_third_party_package_is_reported(self):
        packages = [package("Alpha", "kernel", "provider"), package("Charlie", "platform", "contract")]
        assets = {"Alpha": make_assets("Alpha", ["Some.Vendor"], {"Some.Vendor": ["Charlie"], "Charlie": []})}
        self.assertEqual({("K", "Alpha → Some.Vendor → Charlie")}, found(run(packages, assets)[0]))

    def test_platform_may_use_kernel_providers_and_platform_contracts_only(self):
        packages = [package("Alpha", "platform", "provider"), package("Bravo", "kernel", "provider"),
                    package("Charlie", "platform", "contract"), package("Delta", "platform", "provider")]
        allowed = make_assets("Alpha", ["Bravo", "Charlie"], {"Bravo": [], "Charlie": []}, projects={"Bravo", "Charlie"})
        self.assertEqual(set(), found(run(packages, {"Alpha": allowed})[0]))
        via_contract = make_assets("Alpha", ["Charlie"], {"Charlie": ["Delta"], "Delta": []}, projects={"Charlie", "Delta"})
        self.assertIn(("P", "Alpha → Charlie → Delta"), found(run(packages, {"Alpha": via_contract})[0]))

    def test_contract_may_depend_directly_on_kernel_contracts_only(self):
        packages = [package("Alpha", "platform", "contract"), package("Bravo", "kernel", "contract"),
                    package("Charlie", "kernel", "provider"), package("Delta", "kernel", "contract")]
        assets = {
            "Alpha": make_assets("Alpha", ["Bravo", "Charlie"], {"Bravo": [], "Charlie": []}, projects={"Bravo", "Charlie"}),
            # 规则 D 只看直接依赖：契约经别的契约传递带进提供者不报（那条边由起点自己的规则管）。
            "Delta": make_assets("Delta", ["Bravo"], {"Bravo": ["Charlie"], "Charlie": []}, projects={"Bravo", "Charlie"}),
        }
        self.assertEqual({("D", "Alpha → Charlie")}, found(run(packages, assets)[0]))

    def test_zero_dependency_allows_only_microsoft_extensions_packages(self):
        packages = [package("Alpha", "kernel", "contract", zero="true"), package("Bravo", "kernel", "contract")]
        assets = {"Alpha": make_assets(
            "Alpha", ["Microsoft.Extensions.Options", "Some.Vendor", "Bravo", "Microsoft.CodeAnalysis.PublicApiAnalyzers"],
            {"Microsoft.Extensions.Options": [], "Some.Vendor": [], "Bravo": [], "Microsoft.CodeAnalysis.PublicApiAnalyzers": []},
            projects={"Bravo"}, private={"Microsoft.CodeAnalysis.PublicApiAnalyzers"}, build_only={"Microsoft.CodeAnalysis.PublicApiAnalyzers"})}
        violations, _ = run(packages, assets)
        self.assertEqual({("C", "Alpha → Some.Vendor"), ("C", "Alpha → Bravo")}, found(violations))
        self.assertEqual({"项目引用的不是零依赖的包"}, {v.detail for v in violations if v.dependency == "Bravo"})

    def test_zero_dependency_rejects_project_references_whatever_their_name(self):
        packages = [package("Alpha", "kernel", "contract", zero="true"), package("Microsoft.Extensions.Local", "kernel", "contract")]
        assets = {"Alpha": make_assets("Alpha", ["Microsoft.Extensions.Local"], {"Microsoft.Extensions.Local": []},
                                       projects={"Microsoft.Extensions.Local"})}
        [violation] = run(packages, assets)[0]
        self.assertEqual(("C", "项目引用的不是零依赖的包"), (violation.rule, violation.detail))

    def test_zero_dependency_may_depend_on_other_zero_dependency_packages(self):
        # Kun.Core 与 Kun.Errors 都零依赖，前者引用后者（S1-KUN-023）。项目引用与包引用都按声明算，不看名字。
        packages = [package("Alpha", "kernel", "contract", zero="true"), package("Bravo", "kernel", "contract", zero="true"),
                    package("Charlie", "kernel", "contract", zero="true"), package("Delta", "kernel", "contract")]
        assets = {
            "Alpha": make_assets("Alpha", ["Bravo", "Charlie"], {"Bravo": [], "Charlie": []}, projects={"Bravo"}),
            "Bravo": make_assets("Bravo", ["Delta"], {"Delta": []}, projects={"Delta"}),
        }
        self.assertEqual({("C", "Bravo → Delta")}, found(run(packages, assets)[0]))

    def test_private_build_only_references_do_not_count(self):
        # 分析器之类：PrivateAssets=all 且不带程序集，只在构建时用，规则 D、C 都不计。
        packages = [package("Alpha", "kernel", "contract", zero="true"), package("Bravo", "kernel", "provider")]
        assets = {"Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": []}, private={"Bravo"}, build_only={"Bravo"})}
        self.assertEqual(set(), found(run(packages, assets)[0]))

    def test_private_runtime_library_still_counts(self):
        # PrivateAssets=all 引用运行时库：不进 nuspec，使用方拿不到它，包会坏，所以照样算。
        packages = [package("Alpha", "kernel", "contract", zero="true"), package("Bravo", "kernel", "provider"),
                    package("Charlie", "platform", "contract")]
        assets = {
            "Alpha": make_assets("Alpha", ["Some.Vendor"], {"Some.Vendor": []}, private={"Some.Vendor"}),
            "Charlie": make_assets("Charlie", ["Bravo"], {"Bravo": []}, private={"Bravo"}),
        }
        violations, _ = run(packages, assets)
        self.assertEqual({("C", "Alpha → Some.Vendor"), ("D", "Charlie → Bravo")}, found(violations))
        self.assertIn("nuspec", next(v.detail for v in violations if v.rule == "C"))

    def test_gate_reads_declarations_not_names(self):
        # 名字像契约、声明是提供者：按声明算；名字像提供者、声明是契约：也按声明算。
        packages = [package("Some.Thing.Abstractions", "kernel", "provider"), package("Some.Thing", "kernel", "contract"),
                    package("Consumer.Abstractions", "platform", "contract")]
        assets = {"Consumer.Abstractions": make_assets(
            "Consumer.Abstractions", ["Some.Thing.Abstractions", "Some.Thing"], {"Some.Thing.Abstractions": [], "Some.Thing": []},
            projects={"Some.Thing.Abstractions", "Some.Thing"})}
        self.assertEqual({("D", "Consumer.Abstractions → Some.Thing.Abstractions")}, found(run(packages, assets)[0]))

    def test_package_ids_are_case_insensitive_and_rid_graphs_are_not_double_counted(self):
        packages = [package("Alpha", "kernel", "provider"), package("Charlie", "platform", "contract")]
        assets = {"Alpha": make_assets("Alpha", ["charlie"], {"charlie": []}, projects={"charlie"}, rid_copy=True)}
        violations, _ = run(packages, assets)
        self.assertEqual(1, len(violations))
        self.assertEqual("K", violations[0].rule)


class ExceptionTests(unittest.TestCase):
    def test_exception_covers_the_edge_and_every_path_through_it(self):
        packages = [package("Alpha", "kernel", "provider"),
                    package("Bravo", "kernel", "provider", exceptions=[("Charlie", "K", "B5", "样例")]),
                    package("Charlie", "platform", "contract")]
        assets = {
            "Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": ["Charlie"], "Charlie": []}, projects={"Bravo", "Charlie"}),
            "Bravo": make_assets("Bravo", ["Charlie"], {"Charlie": []}, projects={"Charlie"}),
        }
        violations, statuses = run(packages, assets)
        self.assertEqual([], violations)
        self.assertEqual([("Bravo", "Charlie", "K", "")], [(s.package, s.target, s.rule, s.problem) for s in statuses])

    def test_exception_only_covers_its_own_rule(self):
        packages = [package("Alpha", "kernel", "contract", exceptions=[("Bravo", "K", "B5", "样例")]), package("Bravo", "kernel", "provider")]
        assets = {"Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": []}, projects={"Bravo"})}
        self.assertEqual({("D", "Alpha → Bravo"), ("X", "Alpha → Bravo")}, found(run(packages, assets)[0]))

    def test_stale_and_malformed_exceptions_are_reported(self):
        packages = [package("Alpha", "kernel", "provider", exceptions=[
                        ("Charlie", "K", "B5", "边已没了"), ("Bravo", "K", "B5", "边还在但不违规"),
                        ("Bravo", "D", "B5", ""), ("Bravo", "Z", "B5", "规则写错"), ("Bravo", "K", "B5", "边还在但不违规")]),
                    package("Bravo", "kernel", "provider"), package("Charlie", "platform", "contract")]
        assets = {"Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": []}, projects={"Bravo"})}
        violations, statuses = run(packages, assets)
        self.assertEqual({"X"}, {v.rule for v in violations})
        problems = [s.problem for s in statuses]
        self.assertIn("Alpha 已不直接依赖 Charlie", problems[0])
        self.assertIn("已不违反规则 K", problems[1])
        self.assertIn("缺 Reason", problems[2])
        self.assertIn("Rule 是「Z」", problems[3])
        self.assertIn("重复", problems[4])
        self.assertEqual(5, len(violations))

    def test_exception_on_a_dependency_that_changed_tier_is_stale(self):
        packages = [package("Alpha", "kernel", "provider", exceptions=[("Bravo", "K", "B5", "样例")]), package("Bravo", "kernel", "contract")]
        assets = {"Alpha": make_assets("Alpha", ["Bravo"], {"Bravo": []}, projects={"Bravo"})}
        [violation] = run(packages, assets)[0]
        self.assertEqual("X", violation.rule)
        self.assertIn("已不违反规则 K", violation.detail)


@NEEDS_DOTNET
class SampleProjectTests(unittest.TestCase):
    """端到端：真实还原 tests/gates/depgraph 下的样例项目再检查。"""

    def run_gate(self, solution):
        result = subprocess.run([sys.executable, str(SCRIPT), "--solution", str(SAMPLES / solution), "--json"],
                                capture_output=True, text=True, cwd=ROOT)
        self.assertNotEqual(2, result.returncode, result.stderr)
        return result.returncode, json.loads(result.stdout)

    def test_compliant_samples_pass(self):
        code, report = self.run_gate("good.slnx")
        self.assertEqual(0, code, report["violations"])
        packages = {p["packageId"]: (p["tier"], p["role"]) for p in report["packages"]}
        self.assertEqual({"Gate.Amber", "Gate.Birch", "Gate.Cedar", "Gate.Delta", "Gate.Grove", "Gate.Hazel", "Gate.Tansy"}, set(packages))
        self.assertNotIn("Gate.Host", packages)  # 宿主不打包，不在检查范围
        self.assertEqual([("Gate.Grove", "Gate.Cedar", "K", "")],
                         [(e["package"], e["target"], e["rule"], e["problem"]) for e in report["exceptions"]])

    def test_violating_samples_fail_with_paths(self):
        code, report = self.run_gate("bad.slnx")
        self.assertEqual(1, code)
        self.assertEqual({
            ("K", "Gate.Ivy → Gate.Cedar"),
            ("K", "Gate.Juniper → Gate.Ivy → Gate.Cedar"),
            ("P", "Gate.Kelp → Gate.Delta"),
            ("D", "Gate.Laurel → Gate.Birch"),
            ("D", "Gate.Maple → Gate.Elm.Abstractions"),
            ("C", "Gate.Nectar → Gate.Umber"),  # Gate.Nectar → Gate.Amber 不报：两个都零依赖
            ("C", "Gate.Nectar → Dapper"),
            ("P", "Gate.Rowan → Gate.Delta"),
            ("C", "Gate.Sorrel → Dapper"),
            ("X", "Gate.Oak → Gate.Cedar"),
            ("X", "Gate.Oak → Gate.Birch"),
            ("R", "Gate.Pine"),
            ("R", "Gate.Quince"),
        }, {(v["rule"], " → ".join(v["path"])) for v in report["violations"]})
        oak = [v["detail"] for v in report["violations"] if v["package"] == "Gate.Oak"]
        self.assertEqual(4, len(oak))
        for text in ("已不直接依赖 Gate.Cedar", "已不违反规则 K", "缺 Reason", "Rule 是「Z」"):
            self.assertTrue(any(text in d for d in oak), (text, oak))

    def test_build_rejects_missing_or_invalid_declarations(self):
        cases = {"Gate.Pine": "KUNTIER001", "Gate.Quince": "KUNTIER002", "Gate.Amber": None, "Gate.Host": None}
        for name, code in cases.items():
            with self.subTest(sample=name):
                result = subprocess.run(["dotnet", "msbuild", str(SAMPLES / name / f"{name}.csproj"), "-t:KunRequireTierDeclaration", "-nologo"],
                                        capture_output=True, text=True, env=check_deps._env())
                output = result.stdout + result.stderr
                if code:
                    self.assertNotEqual(0, result.returncode, output)
                    self.assertIn(f"error {code}", output)
                else:
                    self.assertEqual(0, result.returncode, output)
                    self.assertNotIn("error KUNTIER", output)


@NEEDS_DOTNET
class RealGraphTests(unittest.TestCase):
    """读仓库里真实的依赖图（CI 在本步之前已构建 Kun.slnx）。包被移出解决方案、换了包名或改了档位时这里失败，门禁不会静默漏掉它。"""

    @classmethod
    def setUpClass(cls):
        cls.packages = {p.package_id: p for p in check_deps.discover(ROOT / "Kun.slnx", restore=False)}
        cls.declared = {p.package_id.lower(): p for p in cls.packages.values()}
        cls.violations, cls.statuses = check_deps.evaluate(list(cls.packages.values()), check_deps._load)

    def closure(self, package_id):
        package = self.packages[package_id]
        names = set()
        for graph in check_deps.graphs(check_deps._load(package.assets_file), package.project.parent):
            names |= set(graph.names.values())
        return names

    def violations_of(self, package_id):
        return [v for v in self.violations if v.package == package_id]

    def platform_providers(self, names):
        return sorted(n for n in names if (p := self.declared.get(n.lower())) and p.tier == "platform" and p.role == "provider")

    def test_qian_projects_import_the_root_declaration_check(self):
        project = ROOT / "src/Kun.Runtime/Kun.Runtime.csproj"
        result = subprocess.run(["dotnet", "msbuild", str(project), "-t:KunRequireTierDeclaration", "-nologo"],
                                capture_output=True, text=True, env=check_deps._env())
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)

    def test_declared_roles_and_exceptions(self):
        # 计划会话复核附录 A 时定的归类（B3 PR-4）：这 4 个包记契约，组件建在它们上面；Kun.Authorization 本来就记契约。
        # B4 拆插件开发包：声明与命令策略协议一半（Kun.Plugin.Sdk）记内核契约，命令管道一半（Kun.Plugin.Sdk.Commands）记平台契约。
        # B7：Principal 一组的契约包 Kun.Security.Abstractions 记内核契约、零依赖。
        expected = {"Kun.Runtime": ("kernel", "contract"), "Kun.Errors": ("kernel", "contract"),
                    "Kun.Mediator": ("kernel", "contract"), "Kun.Plugin.Sdk.AspNetCore": ("platform", "contract"),
                    "Kun.Authorization": ("kernel", "contract"), "Kun.Core": ("kernel", "contract"),
                    "Kun.Plugin.Sdk": ("kernel", "contract"), "Kun.Plugin.Sdk.Commands": ("platform", "contract"),
                    "Kun.Plugin.Sdk.Generators": ("kernel", "provider"),
                    "Kun.Security.Abstractions": ("kernel", "contract"),
                    "Kun.EntityFrameworkCore": ("kernel", "contract"),
                    "Kun.EntityFrameworkCore.PostgreSQL": ("kernel", "provider")}
        self.assertEqual(expected, {n: (self.packages[n].tier, self.packages[n].role) for n in expected})
        self.assertEqual("true", self.packages["Kun.Core"].zero_dependency)
        self.assertEqual("true", self.packages["Kun.Errors"].zero_dependency)
        self.assertEqual("true", self.packages["Kun.Security.Abstractions"].zero_dependency)
        self.assertEqual([], [v for v in self.violations])
        self.assertEqual(5, len(self.statuses))
        self.assertEqual([("D", "Kun.EventBus", "B6")],
                         [(s.rule, s.target, s.until) for s in self.statuses if s.package == "Kun.EntityFrameworkCore"])
        self.assertEqual([], [s for s in self.statuses if s.problem])
        # 插件开发包没有例外了：命令策略协议的 CommandInvocation 带的 Principal 在内核契约 Kun.Security.Abstractions 里（B7）。
        self.assertEqual([], [(s.rule, s.target, s.until) for s in self.statuses if s.package.startswith("Kun.Plugin.Sdk")])

    def test_reclassification_does_not_hollow_out_rules(self):
        # 假想几条边，按真实的声明判：契约或平台包依赖别的平台组件的提供者、契约直接依赖内核档提供者，都仍要报。
        cases = {
            ("Kun.Plugin.Sdk.AspNetCore", "Qian.Registry"): ["P"],  # 平台档契约 → 别的平台组件的提供者
            ("Qian.Authz.AspNetCore", "Qian.Registry"): ["P"],  # 平台档提供者 → 别的平台组件的提供者
            ("Kun.Runtime", "Qian.Events"): ["D"],  # 内核档契约 → 内核档提供者
            ("Kun.Mediator", "Kun.EventBus"): ["D"],
            ("Kun.Errors", "Kun.Tenancy.Abstractions"): ["K", "C"],  # 内核档 → 平台档；Kun.Errors 零依赖，C 也报
        }
        for (subject, target), rules in cases.items():
            with self.subTest(edge=f"{subject} → {target}"):
                a, b = self.packages[subject], self.packages[target]
                assets = make_assets(subject, [target], {target: []})  # 按包引用合成：规则只看档位与角色，与引用方式无关
                violations, _ = check_deps.evaluate([a, b], lambda path: assets if path == a.assets_file else make_assets(target, [], {}))
                self.assertEqual([(rule, [subject, target]) for rule in rules], [(v.rule, v.path) for v in violations if v.package == subject])

    def test_sdk_closure_has_no_platform_provider(self):
        # S1-RT-014：SDK 包族的完整传递依赖里没有平台档的提供者，也没有平台插件 Qian.Registry / Qian.Lifecycle / Qian.Capabilities。
        for package_id in ("Kun.Plugin.Sdk", "Kun.Plugin.Sdk.Commands", "Kun.Plugin.Sdk.AspNetCore", "Kun.Plugin.Sdk.Generators"):
            with self.subTest(package=package_id):
                self.assertIn(package_id, self.packages, "SDK 包必须在 Kun.slnx 里，依赖门禁才会检查它")
                closure = self.closure(package_id)
                self.assertEqual([], self.platform_providers(closure))
                self.assertFalse({"qian.registry", "qian.lifecycle", "qian.capabilities"} & {n.lower() for n in closure})
                self.assertEqual([], self.violations_of(package_id))
                if package_id != "Kun.Plugin.Sdk.Generators":
                    self.assertIn("Kun.Runtime", closure)  # 读到的是真实依赖图而不是空图

    def test_kernel_half_of_the_sdk_stays_off_the_platform_tier(self):
        # B4：内核档的一半只带声明与命令策略协议，够不着数据契约与 EF。B7 起完整传递依赖里没有平台档的包：
        # CommandInvocation 带的 Principal 在内核契约 Kun.Security.Abstractions 里，不再经身份契约。
        closure = self.closure("Kun.Plugin.Sdk")
        platform = sorted(n for n in closure if (p := self.declared.get(n.lower())) and p.tier == "platform")
        self.assertEqual([], platform)
        self.assertEqual([], sorted(n for n in closure if "entityframeworkcore" in n.lower()))
        self.assertIn("Kun.Runtime", closure)
        self.assertIn("Kun.Security.Abstractions", closure)

    def test_kernel_gateway_closure_has_no_entity_framework(self):
        # 内核档的网关经插件开发包的内核一半接组件，拆包之后完整传递依赖里没有 EF Core（OpenTelemetry 的 EF 埋点包不算 EF）。
        closure = {n.lower() for n in self.closure("Qian.Gateway")}
        self.assertIn("kun.plugin.sdk", closure)
        self.assertEqual([], sorted(n for n in closure if n in {"microsoft.entityframeworkcore", "kun.entityframeworkcore"}
                                    or n.startswith("microsoft.entityframeworkcore.")))

    def test_oidc_connector_does_not_depend_on_identity_server(self):
        # S1-IDP-007：通用 OIDC 连接器只经身份契约接入，完整传递依赖里没有 Qian.Identity.Server（也没有别的平台档提供者）。
        self.assertIn("Qian.Identity.Connector.Oidc", self.packages, "连接器包必须在 Kun.slnx 里，依赖门禁才会检查它")
        closure = self.closure("Qian.Identity.Connector.Oidc")
        self.assertNotIn("qian.identity.server", {n.lower() for n in closure})
        self.assertEqual([], self.platform_providers(closure))
        self.assertEqual([], self.violations_of("Qian.Identity.Connector.Oidc"))
        self.assertTrue({"Qian.Identity.Abstractions", "Kun.Runtime"} <= closure)


if __name__ == "__main__":
    unittest.main()
