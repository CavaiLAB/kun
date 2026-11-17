# AGENTS.md — 给编码代理的入口说明

这个仓库是 **Qian（乾）** 平台的设计与开发仓库。Qian 是基于 Kun（.NET 10 微服务框架）构建的、以「一切皆插件」组织的 PaaS：先做面向开发人员的技术底座（S1），再做低代码构建（S2）、面向客户的应用市场（S3），终态是 AI 原生 Agent 平台（S4）。

## 先读什么

1. `docs/README.md`：文档总索引。
2. `docs/adr/ADR-020-component-model-v2.md`：组件模型（一切皆插件、声明式表面、`qian add` 一键安装、目录市场）。它取代了 ADR-002/003 中被标记的条目。
3. `docs/prd/specs/component-model.md`：开发者规格，实现时以它为准。
4. `docs/prd/07-s1-foundation.md`：S1 每个组件的范围、技术落点与验收。
5. `pm/README.md`：任务怎么管。
6. `docs/adr/ADR-023-kun-qian-merge-blueprint.md`：合并蓝图（Qian 并入 Kun）。落地期间的规则见下一节。

## 合并蓝图落地期间（ADR-023）

产品负责人 2026-11-17 裁定 Qian 并入 Kun（`pm/tasks/S1-DEC-036.md` 的 D1–D14，蓝图是 `docs/adr/ADR-023-kun-qian-merge-blueprint.md`，Accepted）：`Qian.*` 并入 `Kun.*`，一个包族、一条日历版本线；两边重叠的实现归并成一份；许可证全部改 MIT；前端框架由 yue 更名为 qian。同一天又裁定（`pm/tasks/S1-DEC-038.md` 的 D17、D18）：合并后的仓库是 `CavaiLAB/kun`，不是蓝图原来写的 `idadawn/kun`；前端迁往 `CavaiLAB/qian`；迁移现在开始，不再等「初步可用」里程碑收口。

- 迁移分批做，执行计划在 `CavaiLAB/kun` 的 `docs/migration-plan.md`：历史导入（B1）→ 整理与切换（B2）→ 构建并轨（B3）→ 七个域的归并与改名（B4–B10）→ 前端目录移出（B11）→ 首个合并版（B12）。对应的任务是 S1-KUN-019（B1、B2）、S1-KUN-018（B2 里旧 kun 仓库的收尾）、S1-KUN-020（B3）、S1-KUN-021（B4–B10）、S1-KUN-022（B12）。前端的迁移另有计划，任务是 S1-YUE-022。
- 本仓库进入收口期。历史导入合入 `CavaiLAB/kun` 之前，这里照常开发；合入之后，这里只收在途任务的收尾（验收登记、置 done）与紧急修复，新的功能任务到 `CavaiLAB/kun` 开工。收口期合入这里的改动由迁移会话再同步过去，不用自己搬。产品负责人宣布冻结之后，本仓库归档为只读。
- 不要在本仓库里改名。包名、命名空间、目录、许可证字段、yue 的任何标识，都在新仓库的对应批次里改；到归档为止，本仓库的包名仍是 `Qian.*`，两道 .NET 门禁、`KunVersion` 单点钉版、Kun 文档镜像、yue 的目录与包名都不变，本文件其余各节照旧执行。
- 迁移任务（S1-KUN-018 至 S1-KUN-022）的任务文件：历史导入合入之前在本仓库改，合入之后只在 `CavaiLAB/kun` 改，免得再同步时两边改了同一个文件。
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
  - 集群看板由看板 Pod 按 `main` 自行生成；
  - 早先开出的分支合入 `main` 时，若这三个文件出现「一边修改、一边删除」的冲突，直接 `git rm` 它们。

## 写文档与代码的硬规则

- 文档一律中文。文档里不得出现参考平台的品牌名（统一写「参考平台」），也不要写对标项目的名字（写「对标项目」）。`python3 scripts/check_docs.py` 会检查禁用词、表格列数与链接，提交前跑一遍。
- Markdown 表格单元格里的反引号内若有竖线，写成 `\|`。
- 组件设计先回答四个问题：文件夹里有哪些表面、装进哪个 profile、属于哪个 seam 的哪个角色、能否 `qian add`。凡是把能力绑死在特权内核、DI 扩展方法或数据库开关上的写法都是倒退。
- 术语：平台插件（不是「系统插件」）、运行时 `Qian.Runtime`（不是「内核」）、profile（不是「可部署单元」）、`.qian-plugin/plugin.json`（不是 `qian-plugin.yaml`）、租户 scope patch（不是「租户开关」）。
- 口径（ADR-023 D23.6，取代 ADR-021 的三层口径）：Kun 是框架，内部分内核档与平台档；qian 是前端层（原名 yue）；「Qian」不再是平台层的名字。新写定位性的表述按这个口径，不再写「Qian 平台基于 Kun 构建」；描述现有代码时照实写现在的包名与目录。总名 Yi 之下怎么写待 S1-DEC-034。Kun 的代码与文档改动去 kun 仓库提 PR，Qian 任务里只回链；文档站的「Kun 框架」小节（`website/docs/sdk/kun/`）是按 `KunVersion` 锁定版本生成的镜像，不要手改（唯一手写页 `with-qian.mdx` 除外）。
- Kun 版本单点：`Directory.Packages.props` 的 `<KunVersion>` 属性（S1-DOCS-005 加入，文档站的「Kun 框架」小节与 Kun API 参考按它生成）。全部 `Kun.*` 的 PackageVersion 引用 `$(KunVersion)`，`NuGet.config` 用 packageSourceMapping 把 `Kun.*` 映射到 nuget.org；Kun 正式版从 nuget.org 还原，只有需要 Kun 预发布版时才加 GitHub Packages 源（文档站的 DocFX 构建也复用此配置，届时 docs-site 工作流也要配该源的读凭据，R62）；之后升级 Kun 只改这一处（ADR-021 D21.3）。
- 代码部分遵循 ADR-019：每个有代码的组件发 `Qian.<X>.Abstractions` + Provider 包；组件库只依赖 `Qian.Runtime`，不依赖 `qian.registry` / `qian.lifecycle`。
- 两道 .NET 门禁会自动拦截（细节见 `tests/README.md`）：
  - 写了 `<IsPackable>true</IsPackable>` 且包名以 `Qian.` 开头的项目就是 Qian 包，必须带 `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`，新公共 API 未登记即编译失败；
  - `scripts/check_deps.py` 要求 Qian 包只依赖 `Qian.Runtime*`、`Qian.Plugin.Sdk*` 与 `*.Abstractions`，依赖 Provider 或 `Qian.Registry` / `Qian.Lifecycle` / `Qian.Capabilities` 即失败（组合包 `Qian.Bundle.*` 除外）。
- 错误响应一律是 `contracts/v0/error-detail.schema.json` 的 ErrorDetail：用 `Qian.Runtime.Errors` 定义与登记错误码（大写下划线、全局唯一、在本组件 Definition 包里定义），HTTP 宿主装 `AddQianErrors()` + `UseQianErrors()`；不要自己拼 ProblemDetails 或 `{ error: ... }`。
- 改了 `contracts/v0/` 的 Schema 必须同时改 fixture 与 `Qian.Runtime.Errors`，`dotnet test` 会校验 Schema、fixture 与内置错误码目录 `error-codes.json` 是否一致。
- 集成测试用 `tests/Qian.Testing.Containers` 的 fixture（PostgreSQL 16 / Redis 7 / RabbitMQ / Apollo 替身），不要各自再写一套容器启动代码。
- 已有决策不要重开。合并蓝图 ADR-023 于 2026-11-17 Accepted（S1-DEC-036 的 D1–D14 由产品负责人逐条裁定），部分取代 ADR-001 / ADR-021、修订 ADR-013 / ADR-019 / ADR-020。D1–D18 已获认可；ADR-019 / 020 / 021 均为 Accepted，ADR-019 / 020 的待拍板项已拍板并记录在各 ADR §7（对应 `S1-DEC-*` 已关闭）；仓库归属已拍板：产品族统一在 `idadawn`，Kun 从 CavaiLAB 迁入（ADR-021 §7，执行见 S1-PM-003）；合并后的仓库在 `CavaiLAB/kun`（S1-DEC-038 D17 改写）。`S1-DEC-001`–`S1-DEC-009` 已关闭；`S1-DEC-010`（contracts/v0 的 8 条默认裁决）待产品负责人拍板，拍板前按 `contracts/v0/README.md` 第 8 节的默认值实现。文档里标「已裁决（R{n}）」的是复核时的默认裁决，有异议时新建决策任务而不是直接改。

## 仓库布局

```text
docs/        设计文档：decisions（决策清单）、adr、prd（含 specs）、migration、research、ROADMAP.md
contracts/   冻结契约 v0：S2S claim、事件信封、ErrorDetail 与错误码目录、能力条目的 JSON Schema + fixture（见 contracts/v0/README.md）
packages/    .NET 组件库（NuGet 包），如 Qian.Runtime.Errors；packages/yue/ 是 yue 前端工作区（@yue/* 八个包、分层矩阵，见 packages/yue/README.md 与 packages/yue/AGENTS.md）
apps/        前端应用：yue-playground（Base UI 验证门与门禁靶场）
pm/          任务与看板（见 pm/README.md）
scripts/     pm.py、check_docs.py、check_deps.py（依赖图门禁）
tests/       Testcontainers fixture、集成测试、门禁自测（见 tests/README.md）；tests/gates/yue/ 是 yue 门禁自测
website/     文档站（Rspress 2.0，见 website/README.md 与 website/AGENTS.md）；components/、cli/、design/、sdk/kun/（with-qian.mdx 除外）、sdk/api/ 为生成目录
deploy/      开发期工具的 K3s 清单：pm-board/（看板）、docs-site/（文档站）；images/ 是 profile 镜像模板与供应链门禁（见 deploy/images/README.md）
.github/     CI：文档门禁 + 任务校验 + 看板新鲜度；dotnet 构建、门禁与测试；文档站构建、预览与发布；yue 八项门禁矩阵；镜像供应链自检；本机租户运营手测环境冒烟
Qian.slnx、global.json、Directory.Build.props / .targets、Directory.Packages.props、NuGet.config
             .NET 构建基线：net10.0、警告即错误、中央包版本、PublicAPI 门禁
```

代码目录 `packages/` 已建立（一个包一个目录：`packages/<包名>/<包名>.csproj`，测试在 `tests/<包名>.Tests/`）；`plugins/`、`profiles/`、`samples/` 在 S1 第一波任务中建立，布局以 `docs/prd/specs/component-model.md` §2 与 §7 为准。

## 本地检查清单

```bash
python3 scripts/check_docs.py && python3 scripts/pm.py validate && python3 scripts/pm.py build
```

改了文档站（`website/`）或想看设计文档在站内的效果时再跑（需要 Node、pnpm、.NET SDK 与 kun 仓库的读权限：`KUN_DOCS_PATH` 指向本机 kun 克隆，见 `website/README.md` §1）：

```bash
pnpm install && pnpm --filter website typecheck && pnpm --filter website test && pnpm --filter website build && pnpm --filter website check
```

改了 yue 前端工作区（`packages/yue/`、`apps/`）再跑八项门禁（Playwright Chromium 与本机 Chrome 见 `packages/yue/README.md` §3）：

```bash
pnpm yue:gates
python3 scripts/check_docs.py website/docs/sdk/kun website/docs/sdk/api
```

改了 .NET 代码时再跑（集成测试需要 Docker）：

```bash
dotnet build Qian.slnx -c Release && python3 scripts/check_deps.py --no-restore && python3 -m unittest discover -s tests/gates && dotnet test --solution Qian.slnx -c Release --no-build
```

<!-- BEGIN:turborepo-agent-rules -->

# This is NOT the Turborepo you know

Turborepo configuration, task behavior, and CLI commands can vary between installed versions and may differ from your training data. Resolve the `turbo` package from this file's directory or relevant workspace; in monorepos, it may not be visible from the repository root. For example, run `node -p "require.resolve('turbo/package.json')"` from a workspace that depends on `turbo`.

Read `docs/README.md` inside that installed package first, then read the relevant pages from its `docs/` directory before changing Turborepo configuration or commands. Heed deprecation notices. These bundled docs match the installed package version and are available without network access.

This block is written and re-added by `turbo` before repository-scoped commands when an AI agent is detected. In the Turborepo source repository, its template is defined in `crates/turborepo-cli/src/cli/agent_guidance.rs`. Removing the managed block while updates are enabled means a later qualifying invocation will add it again. Set `"agentGuidance": false` in the root `turbo.json` or `turbo.jsonc` to opt out; this does not remove an existing block. Keep the block committed with your work to avoid an uncommitted change on the next agent invocation.
<!-- END:turborepo-agent-rules -->
