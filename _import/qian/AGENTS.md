# AGENTS.md — 给编码代理的入口说明

这个仓库是 **Qian（乾）** 平台的设计与开发仓库。Qian 是基于 Kun（.NET 10 微服务框架）构建的、以「一切皆插件」组织的 PaaS：先做面向开发人员的技术底座（S1），再做低代码构建（S2）、面向客户的应用市场（S3），终态是 AI 原生 Agent 平台（S4）。

## 先读什么

1. `docs/README.md`：文档总索引。
2. `docs/adr/ADR-020-component-model-v2.md`：组件模型（一切皆插件、声明式表面、`qian add` 一键安装、目录市场）。它取代了 ADR-002/003 中被标记的条目。
3. `docs/prd/specs/component-model.md`：开发者规格，实现时以它为准。
4. `docs/prd/07-s1-foundation.md`：S1 每个组件的范围、技术落点与验收。
5. `pm/README.md`：任务怎么管。
6. `docs/adr/ADR-023-kun-qian-merge-blueprint.md`：合并蓝图（Qian 并入 Kun）。落地期间的规则见下一节。

## 本仓库已冻结（ADR-023 合并蓝图落地）

产品负责人 2026-11-17 裁定 Qian 并入 Kun（`pm/tasks/S1-DEC-036.md` 的 D1–D14，蓝图是 `docs/adr/ADR-023-kun-qian-merge-blueprint.md`，Accepted）：`Qian.*` 并入 `Kun.*`，一个包族、一条日历版本线；两边重叠的实现归并成一份；许可证全部改 MIT；前端框架由 yue 更名为 qian。同一天又裁定（`pm/tasks/S1-DEC-038.md` 的 D17、D18）：合并后的仓库是 `CavaiLAB/kun`，不是蓝图原来写的 `idadawn/kun`；前端迁往 `CavaiLAB/qian`；迁移现在开始，不再等「初步可用」里程碑收口。历史导入当天合入 `CavaiLAB/kun`（CavaiLAB/kun#3），产品负责人随后宣布冻结。

- 本节的「本仓库」指 `idadawn/qian`。在 `CavaiLAB/kun` 的 `_import/qian/` 下读到这里的：本节随历史一起带了过去，冻结的说法在那边不适用，以那边根上的 `AGENTS.md` 为准。
- 迁移分批做，执行计划在 `CavaiLAB/kun` 的 `docs/migration-plan.md`：历史导入（B1，已完成）→ 切换与整理（B2，从冻结开始）→ 构建并轨（B3）→ 七个域的归并与改名（B4–B10）→ 前端目录移出（B11）→ 首个合并版（B12）。对应的任务是 S1-KUN-019（B1、B2）、S1-KUN-018（B2 里旧 kun 仓库的收尾）、S1-KUN-020（B3）、S1-KUN-021（B4–B10）、S1-KUN-022（B12）。前端只迁框架库，计划在 `CavaiLAB/qian` 的 `docs/migration.md`，任务是 S1-YUE-022 至 S1-YUE-026。
- 本仓库自 2026-11-17 起冻结，不再合入任何改动，代码、文档、任务文件都一样。原因：本仓库合入写明去处的 PR 之后就归档，归档时的最终头由最后一次同步带进 `CavaiLAB/kun`；新仓库的过渡目录 `_import/qian/` 在最后一次同步合入之前也不改内容（同步要拿树对象逐个核对）。所以从冻结到最后一次同步合入这段时间，两边都不收改动。唯一的例外是写明去处的那一个 PR（README 顶部的说明、本节，以及 S1-KUN-018、S1-KUN-019 的任务文件），它合入之后，由产品负责人把本仓库归档为只读，再由最后一次同步带进新仓库。
- 最后一次同步合入 `CavaiLAB/kun` 的主干之后，代码、任务文件、决策记录都到那边的 `_import/qian/` 下改，在那边开 PR：任务文件在 `_import/qian/pm/tasks/` 下，`pm.py` 在 `_import/qian/` 下运行。后续批次会把它们提出过渡目录。在那边本文件哪些部分照用，以 `CavaiLAB/kun` 根上的 `AGENTS.md` 为准。
- 本文件其余各节写于冻结之前。其中讲在哪里改的说法（在本仓库认领任务与提交、Kun 的改动去 kun 仓库提 PR 等），冻结之后一律以本节为准；讲构建、测试与代码约定的部分不变。
- 不要抢跑改名。包名、命名空间、目录、许可证字段、yue 的任何标识，都在新仓库的对应批次里改；在那之前包名仍是 `Qian.*`，两道 .NET 门禁、Kun 文档镜像、yue 的目录与包名都不变。按包引用 Kun 与 `KunVersion` 钉版已在构建并轨（B3）里改为项目引用 Kun 源码，见下面「Kun 的引用」。
- 迁往前端仓库的路径自提交 `aafa0cc5ab` 起就已冻结：`packages/yue/*`、`apps/yue-playground`、`packages/qian-plugin-sdk`、`packages/qian-runtime`、`tests/gates/yue`、`scripts/yue-visual.mjs`、`.changeset/`，以及 yue、runtime-ts、runtime-ts-release、plugin-sdk-release 四个工作流。改它们到 `CavaiLAB/qian` 改；它们在 `CavaiLAB/kun` 里也只读，由前端计划的 B11 删除。其余的前端路径（外壳应用与外壳工具包、三个界面包、参考应用的远程模块、文档站）随后端留在 `CavaiLAB/kun`；yue 的标识在这些路径里的改名由前端计划的任务承载。
- 运行期字面不随合并改名：组件 id 前缀 `qian.`、库 schema `plugin_qian_*`、Cookie 名、契约标识 `https://qian.dev/…` 与 `x-qian-*`、`.qian-plugin/plugin.json`、`qian` 命令行、`@qian/*` 导入名（ADR-023 D23.10、D23.18）。
- 按域归并期间（B4–B10），功能任务避开正在归并的域；同一时间一个域只有一个归并批次。

## 任务流程（必须遵守）

- 任务在 `pm/tasks/<id>.md`，一个任务一个文件。开始前认领：
  `python3 scripts/pm.py set <id> --status doing --owner <你的代理名> --by <你的代理名> --note "开始"`
- 日志必须署名：`pm.py set` / `pm.py add` 带 `--by <你的代理名>`（或设环境变量 `PM_ACTOR`），缺省时命令报错；`pm.py validate` 拒绝署名为 someone 的日志行（S1-DEC-019 R71）。
- 先建档再交付：任务以 `todo` / `doing` 建档并认领后再提交实现，不要在创建任务的提交里直接写 `done` 并勾满验收。
- 结束时把「验收」里完成的项改为 `[x]`，改状态（`review` 或 `done`），运行 `python3 scripts/pm.py validate`，只提交任务文件。
- 提交信息末尾带任务 id，如 `feat(runtime): ctx.Effect 逆序释放 [S1-RT-001]`。
- 遇到需要人拍板的问题：不要自作主张改设计。用
  `python3 scripts/pm.py add --stage S1 --area DEC --type decision --wave 0 --by <你的代理名> --title "..."`
  新建决策任务并把当前任务标 `blocked`。
- `pm/index.html`、`pm/tasks.json`、`pm/STATUS.md` 是生成物，不进 git（S1-PM-004），不要提交，也不要手改：
  - 本地想看看板，运行 `python3 scripts/pm.py build`，结果已被 `.gitignore` 忽略；
  - 集群看板由看板 Pod 按 `CavaiLAB/kun` 主干上 `_import/qian/pm/tasks/` 的任务文件自行生成（内网访问，入口见 `deploy/pm-board/README.md`；应用由 Argo CD 管，Application 在部署仓库）；
  - 早先开出的分支合入 `main` 时，若这三个文件出现「一边修改、一边删除」的冲突，直接 `git rm` 它们。

## 写文档与代码的硬规则

- 文档一律中文。文档里不得出现参考平台的品牌名（统一写「参考平台」），也不要写对标项目的名字（写「对标项目」）。`python3 scripts/check_docs.py` 会检查禁用词、表格列数与链接，提交前跑一遍。
- Markdown 表格单元格里的反引号内若有竖线，写成 `\|`。
- 组件设计先回答四个问题：文件夹里有哪些表面、装进哪个 profile、属于哪个 seam 的哪个角色、能否 `qian add`。凡是把能力绑死在特权内核、DI 扩展方法或数据库开关上的写法都是倒退。
- 术语：平台插件（不是「系统插件」）、运行时 `Kun.Runtime`（不是「内核」）、profile（不是「可部署单元」）、`.qian-plugin/plugin.json`（不是 `qian-plugin.yaml`）、租户 scope patch（不是「租户开关」）。
- 口径（ADR-023 D23.6，取代 ADR-021 的三层口径）：Kun 是框架，内部分内核档与平台档；qian 是前端层（原名 yue）；「Qian」不再是平台层的名字。新写定位性的表述按这个口径，不再写「Qian 平台基于 Kun 构建」；描述现有代码时照实写现在的包名与目录。总名 Yi 之下怎么写待 S1-DEC-034。Kun 的代码与文档改动去 kun 仓库提 PR，Qian 任务里只回链；文档站的「Kun 框架」小节（`website/docs/sdk/kun/`）是按 Kun 最新的发布版（仓库根 `CHANGELOG.md` 里最新的发布段）生成的镜像，不要手改（唯一手写页 `with-qian.mdx` 除外）。
- Kun 的引用：构建并轨（S1-KUN-020）之后，`_import/qian/` 下的项目改为项目引用仓库根 `src/` 下的 Kun 源码，不再按包引用 Kun，也不再有 `Kun.*` 的版本条目。原来留给文档站读的 `<KunVersion>` 也已删除：文档站（「Kun 框架」小节、Kun API 参考、版本矩阵）改读仓库根 `CHANGELOG.md` 里 Kun 最新的发布版，`Directory.Packages.props` 只剩导入根上那份，`tests/gates/test_build_merge.py` 保证没有 MSBuild 文件再定义或引用 `KunVersion`；原来的版本单点写法见 ADR-021 D21.3（已被 ADR-023 D23.12 取代）。包源配置在仓库根的 `NuGet.config`，只有 nuget.org。
- 代码部分遵循 ADR-019：每个有代码的组件发 `Qian.<X>.Abstractions` + Provider 包；组件库只依赖 `Kun.Runtime`，不依赖 `qian.registry` / `qian.lifecycle`。
- 三道 .NET 门禁会自动拦截（细节见 `tests/README.md`，都在仓库根的 `Directory.Build.targets` 与 `scripts/` 下，不看包名）：
  - 可打包的项目（写了 `<IsPackable>true</IsPackable>`）都必须带 `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`，新公共 API 未登记即编译失败；
  - 可打包的项目都要在项目文件里写档位与角色（`KunTier`、`KunRole`），仓库根的 `scripts/check_deps.py` 按它们检查依赖，不看包名：内核档不依赖平台档，平台档只依赖内核档与平台档的契约包，契约包直接依赖的内核档包只能是契约。写法、例外与本机跑法见仓库根 `AGENTS.md`「档位与依赖」；
  - 禁用符号与平台分析器（RS0030、QIAN0002–QIAN0006）接平台档的包与宿主（项目文件里写 `<KunHost>true</KunHost>`）；过渡期本目录下的项目全部接入，去内核档的包也在内，各域批次把包搬出本目录时再按归属收窄。新写的宿主要写 `KunHost`。
- 错误响应一律是 `contracts/v0/error-detail.schema.json` 的 ErrorDetail：用 `Kun.Errors` 定义与登记错误码（大写下划线、全局唯一、在本组件 Definition 包里定义），HTTP 宿主装 `AddKunErrors()` + `UseKunErrors()`；不要自己拼 ProblemDetails 或 `{ error: ... }`。
- 改了 `contracts/v0/` 的 Schema 必须同时改 fixture 与 `Kun.Errors`，`dotnet test` 会校验 Schema、fixture 与内置错误码目录 `error-codes.json` 是否一致。
- 集成测试用 `tests/Qian.Testing.Containers` 的 fixture（PostgreSQL 16 / Redis 7 / RabbitMQ / Apollo 替身），不要各自再写一套容器启动代码。
- 已有决策不要重开。合并蓝图 ADR-023 于 2026-11-17 Accepted（S1-DEC-036 的 D1–D14 由产品负责人逐条裁定），部分取代 ADR-001 / ADR-021、修订 ADR-013 / ADR-019 / ADR-020。D1–D18 已获认可；ADR-019 / 020 / 021 均为 Accepted，ADR-019 / 020 的待拍板项已拍板并记录在各 ADR §7（对应 `S1-DEC-*` 已关闭）；仓库归属已拍板：产品族统一在 `idadawn`，Kun 从 CavaiLAB 迁入（ADR-021 §7，执行见 S1-PM-003）；合并后的仓库在 `CavaiLAB/kun`（S1-DEC-038 D17 改写）。`S1-DEC-001`–`S1-DEC-009` 已关闭；`S1-DEC-010`（contracts/v0 的 8 条默认裁决）待产品负责人拍板，拍板前按 `contracts/v0/README.md` 第 8 节的默认值实现。文档里标「已裁决（R{n}）」的是复核时的默认裁决，有异议时新建决策任务而不是直接改。

## 仓库布局

```text
docs/        设计文档：decisions（决策清单）、adr、prd（含 specs）、migration、research、ROADMAP.md
contracts/   冻结契约 v0：S2S claim、事件信封、ErrorDetail 与错误码目录、能力条目的 JSON Schema + fixture（见 contracts/v0/README.md）
packages/    .NET 组件库（NuGet 包），如 Kun.Runtime；packages/qian-* 是 Node 包：命令行、声明式表面校验库、外壳接线与三个界面包。
             前端框架（原 yue，现名 qian）已迁到 CavaiLAB/qian，这里装已发布的 @cavailab/qian-*（见 README.md「前端依赖与包源」）
apps/        外壳应用 qian-shell（模块联邦宿主，见 apps/qian-shell/README.md）
pm/          任务与看板（见 pm/README.md）
scripts/     pm.py、check_docs.py、dotnet_ci.py 等（依赖门禁在仓库根的 scripts/check_deps.py）
tests/       Testcontainers fixture、集成测试、门禁自测（见 tests/README.md）；tests/gates/ui/ 是外壳与界面包的门禁自测，tests/directory-e2e/ 是真实目录验收页
website/     文档站（Rspress 2.0，见 website/README.md 与 website/AGENTS.md）；components/、cli/、design/、sdk/kun/（with-qian.mdx 除外）、sdk/api/ 为生成目录
deploy/      开发期工具的 K3s 清单：pm-board/（看板）、docs-site/（文档站）；images/ 是 profile 镜像模板与供应链门禁（见 deploy/images/README.md）
.github/     原 Qian 的工作流原件，在 CavaiLAB/kun 里 GitHub 不读：检查接在仓库根的 .github/workflows/qian-*.yml
             （文档门禁、任务校验与看板生成；dotnet 构建、门禁与测试；文档站构建与发布；本机租户运营手测环境冒烟等），
             在 _import/qian/ 下运行；文档站的 PR 预览、镜像供应链自检与 dotnet-release 发布工作流没有接（见根 AGENTS.md 与迁移执行计划）。
             yue 八项门禁与 TS 运行时、插件开发包的工作流随前端框架迁到 CavaiLAB/qian；外壳与界面包的门禁在仓库根的 qian-ui.yml（没有原件）
Qian.slnf、Directory.Build.props / .targets、Directory.Packages.props
             .NET 构建基线：net10.0、警告即错误、中央包版本。目标框架、质量门槛、中央包版本、NuGet.config、
             公共 API 门禁、AOT 守卫、禁用符号清单与分析器作用域已并到 CavaiLAB/kun 仓库根（S1-KUN-020），
             这里的 Directory.Build.props / .targets 与 Directory.Packages.props 先导入根上的，只留本目录的打包设置、
             发版脚本读的包判定与网关引用的处理。解决方案是仓库根的 Kun.slnx，
             Qian.slnf 是它的筛选文件：列出原 Qian 的项目，也是脚本与测试定位本目录的锚，新加项目要两处都加。
             SDK 版本只钉在仓库根的 global.json，本目录没有另一份
```

代码目录 `packages/` 已建立（一个包一个目录：`packages/<包名>/<包名>.csproj`，测试在 `tests/<包名>.Tests/`）；`plugins/`、`profiles/`、`samples/` 在 S1 第一波任务中建立，布局以 `docs/prd/specs/component-model.md` §2 与 §7 为准。

## 本地检查清单

```bash
python3 scripts/check_docs.py && python3 scripts/pm.py validate && python3 scripts/pm.py build
```

改了文档站（`website/`）或想看设计文档在站内的效果时再跑（需要 Node、pnpm、.NET SDK 与 kun 仓库的读权限：`KUN_DOCS_PATH` 指向本机 kun 克隆；还要 CavaiLAB/qian 的检出：`QIAN_DOCS_PATH`，见 `website/README.md` §1；装依赖要 GitHub Packages 的令牌，见 `README.md`「前端依赖与包源」）：

```bash
pnpm install && pnpm --filter website typecheck && pnpm --filter website test && pnpm --filter website build && pnpm --filter website check
```

改了外壳与界面包（`apps/`、`packages/qian-tenancy-ui`、`packages/qian-tenancy-signup-ui`、`packages/qian-identity-ui`、`packages/qian-shell-kit`、`samples/qian-sample-crm-ui`、`tests/directory-e2e`）再跑门禁（界面包的浏览器测试要 Playwright Chromium：`pnpm --filter @qian/identity-ui exec playwright install chromium`；外壳联邦验收要 .NET SDK 与 Docker）：

```bash
pnpm ui:gates
python3 scripts/check_docs.py website/docs/sdk/kun website/docs/sdk/api
```

改了 .NET 代码时再跑（集成测试需要 Docker）：

```bash
dotnet build Qian.slnf -c Release && python3 ../../scripts/check_deps.py --no-restore && python3 -m unittest discover -s tests/gates && dotnet test --solution Qian.slnf -c Release --no-build
```

<!-- BEGIN:turborepo-agent-rules -->

# This is NOT the Turborepo you know

Turborepo configuration, task behavior, and CLI commands can vary between installed versions and may differ from your training data. Resolve the `turbo` package from this file's directory or relevant workspace; in monorepos, it may not be visible from the repository root. For example, run `node -p "require.resolve('turbo/package.json')"` from a workspace that depends on `turbo`.

Read `docs/README.md` inside that installed package first, then read the relevant pages from its `docs/` directory before changing Turborepo configuration or commands. Heed deprecation notices. These bundled docs match the installed package version and are available without network access.

This block is written and re-added by `turbo` before repository-scoped commands when an AI agent is detected. In the Turborepo source repository, its template is defined in `crates/turborepo-cli/src/cli/agent_guidance.rs`. Removing the managed block while updates are enabled means a later qualifying invocation will add it again. Set `"agentGuidance": false` in the root `turbo.json` or `turbo.jsonc` to opt out; this does not remove an existing block. Keep the block committed with your work to avoid an uncommitted change on the next agent invocation.
<!-- END:turborepo-agent-rules -->
