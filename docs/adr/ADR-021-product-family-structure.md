---
title: 产品族结构：Kun / Qian / ［前端框架］的定位、仓库关系与统一文档
status: Accepted
date: 2026-10-07
decision_id: D21
supersedes: none
amends: ADR-001, ADR-013
superseded_by: ADR-023（部分）
---

# ADR-021 产品族结构：Kun / Qian / ［前端框架］的定位、仓库关系与统一文档

> 本文保留当时的决策。方括号中的能力描述用于标出整理时替换的引擎或下游名称。

> 起因：产品负责人 2026-10-07 提问「Kun 和 Qian 如何定位？是不是创建一个大的项目把各个子项目关联起来？Kun 也需要体现在文档中。」本 ADR 回答这三问，修订 [ADR-001](ADR-001-kun-relationship.md)（补充口径与文档规则，D1.1–D1.8 不变）与 [ADR-013](ADR-013-frontend-framework.md) D13.18（文档站范围扩为产品族）。仓库归属已于同日由产品负责人拍板：产品族统一在 `idadawn`，Kun 迁入（§2 D21.2、§7；执行见任务 S1-PM-003）。

> **2026-11-17 部分被 [ADR-023](ADR-023-kun-qian-merge-blueprint.md) 取代。** D21.1 的三层口径改为「Kun 是框架，内部分内核档与平台档；qian 是前端层（原名［前端框架］）」；D21.2 的不建大单仓改为 Qian 并入 `idadawn/kun`；D21.3 的 `KunVersion` 单点钉版与包源映射在合并阶段 2 构建并轨时作废；D21.4 的 Kun 文档镜像小节在阶段 1 导入后撤销，改为同仓来源。对应阶段落地之前照旧执行。D21.5、D21.6 不变。

> 修订（2026-11-24，[S1-DEC-038](../../pm/tasks/S1-DEC-038.md) D40、D44）：产品负责人 2026-11-24 定产品族的所有者是 CavaiLAB（D40），前端包的作用域是 `@cavailab`（D44），标在 D21.2 第 1 点、D21.6 与 §7「仓库归属」之前。正文未改。

> 修订（2026-11-26，[S1-DEC-043](../../pm/tasks/S1-DEC-043.md) D1、D2、D4）：前端框架 Qian 的仓库 2026-11-25 归档（D1）；产品负责人 2026-11-26 说现在只剩匠心与 Kun 两个项目，SanYuanAgent 保留、先不维护（D2）；Kun 要增加单独的网站（D4）。产品族现在怎么读，见 §9。正文未改，D21.2 第 2 点与 D21.5 前加了标注。

> 修订（2026-11-26，[S1-DEC-047](../../pm/tasks/S1-DEC-047.md) D2）：产品负责人 2026-11-26 定前端框架的组件页从文档站里拿掉；站名随之改成「Kun 开发文档」，首页的一句话定位不再提前端框架的组件文档。D21.2 第 2 点受影响，见 §9 末条。正文未改，D21.2 第 2 点前加了标注。

## 1 背景与问题（Context）

三个名字已经在用：**Kun**（底座框架）、**Qian**（平台）、**［前端框架］**（前端组件库），周边还有官方市场仓库 `qian-plugins`、兄弟系统［下游智能体平台］、部署仓库与模型服务仓库。它们之间的层次关系、仓库怎么关联、Kun 文档从哪来，此前只在 ADR-001 讲了「Kun 是底座 SDK、以 NuGet + CalVer 锁版本消费」，没有统一口径。现状事实：

- **Kun**：仓库 `idadawn/kun`（2026-04 建于此，中间转入 CavaiLAB，2026-10-08 迁回；包元数据与 README 的仓库地址一直是 `idadawn/kun`），私有，MIT；.NET 10 微服务框架，32 个 `Kun.*` 模块；CalVer（`vYYYY.M.D`，最新 `v2026.9.13`）；NuGet 包公开发布在 nuget.org（`Kun.Core` 等，0.1.0 / 2026.6.12 / 2026.9.13），并镜像到仓库所属者的 GitHub Packages。文档是仓库内 `docs/` 的 Markdown（白皮书、模块地图、上手教程、6 篇指南、3 篇安全专题、16 份 Kun 自己的 ADR、发布说明与变更日志），**没有文档站**；Kun 还有其它下游（［下游智能体平台］等），README 与部分文档面向那些下游写。
- **Qian**：仓库 `idadawn/qian`，单仓：`docs/`、`pm/`、`website/`（文档站，S1-DOCS-001 在途），S1 起 `packages/`、`plugins/`、`profiles/`、`samples/`；`Qian.*` SemVer 锁步，Apache-2.0，S1 发 GitHub Packages（`Qian.*` NuGet 随公开仓库公开可见、安装需令牌，ADR-019 D19.7 / R93）。
- **［前端框架］**：随 Qian 单仓以 pnpm workspace 交付（PRD §07 7.15.2），`［前端包系列］`（npm 包名 `［前端包系列］`，2026-10-09 拍板）发 GitHub Packages，Changesets + SemVer。
- **周边**：`qian-plugins` 规定为「与 Qian 同组织下的独立仓库」，但组织名仍是占位（specs/component-model.md §8）；［下游智能体平台］源码在 `［Agent 运行平台仓库］`（ADR-016；其文档里的 CavaiLAB 地址是迁移前的旧地址）；部署仓库 `［部署仓库］`（ADR-017）；模型服务 `idadawn/MaaS`（ADR-015）。除 Kun 外全部在 `idadawn`，两个 GitHub 所有者并存意味着两个 GitHub Packages 命名空间。
- **文档**：docs-site.md 首版把 Kun 写成「外链不重写」，但 Kun 没有可外链的站点，且产品负责人在确认信息架构时就把「Kun SDK」列为 SDK 部分的第一节。

要回答的问题：三层怎么说；「大项目」是什么形态；Kun 文档从哪来、谁维护、怎么与 Qian 锁定的 Kun 版本保持一致。

## 2 决策（Decision）

- 【已被 ADR-023 D23.6 取代】**D21.1 三层口径**。

  | 层 | 名字 | 是什么 | 什么内容归它 | 版本 |
  |---|---|---|---|---|
  | 框架层 | Kun | 面向任何 .NET 微服务的开发框架，Qian 的底座 SDK | 无产品观点的通用基础设施：Mediator、Outbox、Saga、多租户上下文与 RLS、网关与 S2S、存储、缓存、可观测、测试 | CalVer |
  | 平台层 | Qian | 基于 Kun 的多租户 PaaS：极小运行时 `Qian.Runtime` + 平台插件 + profile + `qian` CLI + 目录市场 | 带多租户或业务语义的能力：租户主档、IdP、权限引擎、带配额与策略的文件、配置中心接入、事件桥接、审计装配、低代码、Agent 表面 | SemVer 锁步 |
  | 前端层 | ［前端框架］ | React 组件库与外壳 | UI 组件、表单 / 表格 / 弹层引擎、MF2 外壳与插槽 | SemVer（Changesets） |

  对外一句话：「**Qian 平台基于 Kun 框架构建，前端使用［前端框架］组件库**」。不说「Kun 是 Qian 的子项目」（Kun 有独立下游与发布节奏），也不说「Qian 是 Kun 的扩展」（Qian 有自己的运行时与产品语义）。归属判据沿用决策清单 D19 的分界：无产品观点的通用基础设施上游进 Kun，带多租户 / 业务语义的是 Qian 包。ADR-001 D1.1–D1.8 不变。
- 【已被 ADR-023 D23.1 取代，阶段 1 导入之前照旧执行】**D21.2 不建大单仓、不建 submodule 元仓库**（ADR-001 §4 已否决单仓）。「大项目」= **产品族**，用四样东西关联：
  1. 【经 S1-DEC-038 D40 修订：产品族的所有者是 CavaiLAB，四个仓库都在它之下】**一个 GitHub 所有者：`idadawn`**（产品负责人 2026-10-07 拍板）。［下游智能体平台］源码、部署仓库、MaaS、Qian 都已在 `idadawn`，只有 Kun 还在公司级、内容驳杂的 CavaiLAB 组织，所以是 **Kun 迁回 `idadawn`**，而不是 Qian 迁出。迁后产品族全部仓库同一所有者：一个 GitHub Packages 命名空间（`Qian.*` 的 S1 包、Kun 的镜像与预发布包；`Qian.*` 包公开可见但安装需令牌，R93）、一段源配置加一份 `packageSourceMapping`；官方市场仓库定名 `idadawn/qian-plugins`。Kun 正式版的主渠道是公开的 nuget.org，迁移不影响任何消费者。执行与核验见 S1-PM-003。
  2. 【经 S1-DEC-043 D4 标注：Kun 要增加单独的网站，放使用文档与给大模型用的文档，见 §9】【经 S1-DEC-047 D2 标注：前端框架的组件页 2026-11-26 起从站里拿掉，站名改成「Kun 开发文档」，见 §9 末条；本条不改】**一个文档站**：「Qian 文档」覆盖整个产品族。Kun 为第 3 部分 SDK 的第一节 `/sdk/kun/`（导航名「Kun 框架」），内容按 Qian 锁定的 Kun 版本从 kun 仓库镜像并经 DocFX 生成 API（D21.4）。站名不改，首页一句话定位点明基于 Kun、［前端框架］。
  3. **一张版本矩阵**：`/start/versions` 自动生成：每个 Qian 版本对应的 Kun CalVer、`Qian.*`、`［前端包系列］`、.NET SDK、Node、K3s 基线；来源是仓库内的钉版文件（`Directory.Packages.props`、`global.json`、pnpm catalog）。
  4. **一个看板**：跨仓库工作在 Qian 的 `pm/` 跟踪（KUN 领域），kun 仓库的 PR 写进任务 `refs` 与进度日志回链；不另建 GitHub Project。
- 【已被 ADR-023 D23.12 取代，阶段 2 构建并轨之前照旧执行】**D21.3 Kun 版本单点钉死**。`Directory.Packages.props` 定义 `<KunVersion>`，全部 `Kun.*` 的 PackageVersion 引用 `$(KunVersion)`；`NuGet.config` 的 Kun 源与 `packageSourceMapping` 在首个引用 `Kun.*` 的任务加入（既有约定）；文档同步脚本与版本矩阵读同一属性。升级 Kun = 改这一处 → TCK 两种模式通过 → Kun 文档镜像自动跟随（CI 比对 tag）。补注（S1-DEC-019 R65，2026-10-13）：Kun 的同日修复版 `-hotfix.N` 是推送 nuget.org 的正式修复，NuGet 按预发布排序；`Kun.*` 一律以 `[$(KunVersion)]` 精确钉定，不依赖最新稳定版排序，钉在修复版上时于 Kun 下一个无后缀正式版发布后升级回来。
- 【已被 ADR-023 D23.13 取代，阶段 1 导入之前照旧执行】**D21.4 Kun 文档进站规则**（细则见 [specs/docs-site.md §2b](../prd/specs/docs-site.md)）：
  - 真相源在 kun 仓库 `docs/`，Qian 站**只镜像不改写**，与第 7 部分镜像 `docs/` 同一原则；改 Kun 文档去 kun 仓库提 PR。Qian 自写的只有一页「与 Qian 的关系」（Kun 模块 ↔ 平台插件 / seam 映射、canonical API 选择、Qian 的强制装配与禁用项、补丁包状态）以及首页与导航文案。
  - 镜像范围是白名单：README 的定位与模块矩阵、上手教程、模块地图、设计说明与 Kun 的 ADR、指南、可观测、安全、白皮书、变更日志与发布说明。不镜像面向其它下游的差距分析、路线图与 backlog、AOT 审计与基准报告、贡献者文档、各级 `AGENTS.md`。
  - 每页横幅注明来源路径与 Kun 版本、「Kun 文档与代码不一致处以代码为准（ADR-001 D1.8）」、指向「与 Qian 的关系」页；frontmatter `project: kun`、`version: <CalVer>`、`source: <owner>/kun@<tag>:<path>`。
  - API 参考由 DocFX 从锁定版本的 `Kun.*` 包生成，挂 `/sdk/api/kun/`，与 `Qian.*` 的 `/sdk/api/dotnet/` 并列。
  - 门禁：`sdk/kun/` 为生成目录不接受手改；镜像 tag 与 `KunVersion` 不一致、白名单外文件进站、`KunVersion` 缺失，均构建失败。
  - 公开时点耦合：文档站公开（ADR-019 §7：S2 末或首个外部宿主试用时）会连带公开 Kun 文档；届时 Kun 仓库须同步公开或 Kun 维护人书面同意，否则公开站的 Kun 小节降级为「版本矩阵 + 与 Qian 的关系」两页。
- 【经 S1-DEC-043 D1、D2 标注：在用的项目只剩匠心与 Kun，前端框架的仓库已归档；本仓库 README 的产品族表与文档站产品族页的表里，前端框架一行已去掉，表下面加了说明，见 §9】**D21.5 产品族可见性**。各仓库 README 开头放同一段「产品族地图」（Qian 立即落实；kun、`qian-plugins` 等在下次变更时补一行回链，不作 S1 退出条件）；文档站第 1 部分有 `/start/family` 产品族地图页；全站 frontmatter 增加 `project`（`qian` / `kun` / `yue`），`llms.txt` 按它分节、知识库按它过滤。
- 【经 S1-DEC-038 D44 修订：物理包名的作用域是 `@cavailab`，即 `@cavailab/qian-runtime`】**D21.6 TS 运行时的物理发布名与逻辑名**（S1-DEC-014，产品负责人 2026-10-11 确认）。GitHub Packages 的物理包名为 `@idadawn/qian-runtime`，关联 `idadawn/qian`，S1 保持私有。公共逻辑导入名仍为 `@qian/runtime`，消费者通过 `"@qian/runtime": "npm:@idadawn/qian-runtime@<固定版本>"` 映射，不另发同功能的第二份包。发版必须完成私有源真实安装、别名导入与类型声明验收；不把本地打包代替已发布证明。

## 3 理由（Rationale）

- **同一所有者才有一段源配置。** ADR-019 的目标是「装一个包就能用」。Kun 正式版在 nuget.org，但 `Qian.*` 的 S1 包（公开可见、安装需令牌，R93）、Kun 的预发布包（ADR-001 D1.3 要求的 `-beta.N` feed）与 `qian-plugins` 都挂在 GitHub 所有者名下；两个所有者就要配两段源加映射，第三方第一次还原就会卡住。选 `idadawn` 而不是 CavaiLAB，是因为要动的只有 Kun 一个仓库，而且 Kun 本来就建在那里。
- **镜像而不是外链或改写。** Kun 没有站点；写 Qian 插件就是在用 Kun 的 Mediator、Outbox、RLS，读者高度重叠；单站搜索与 S2 的「问 AI」才能跨两层回答。复制改写会制造双真相源，而 Kun 文档失真正是 ADR-001 记录过的教训，所以只镜像、带横幅、以代码为准。
- **版本单点与自动镜像。** Qian 对 Kun 是锁版本消费（D1.2），文档必须与锁定版本一致，否则读者看到的是另一个 Kun。
- **不建单仓。** ADR-001 §4 已否决；Kun 有独立下游与发布节奏；在单人维护的现实下合并仓库只会放大耦合。

## 4 备选方案与放弃原因（Alternatives）

- **大单仓 / submodule 元仓库**：否决。前者见 ADR-001 §4；后者对 Codex、Claude Code 的任务流程不友好，锁定的 SHA 容易漂移，也解决不了 feed 问题。
- **Qian 迁入 CavaiLAB**（本 ADR 首版的推荐）：否决。［下游智能体平台］源码与部署仓库都在 `idadawn`，CavaiLAB 是公司级多项目组织；迁 Qian 要改看板与 ArgoCD 的四处地址，迁 Kun 只动 Kun 自己。
- **新建专用组织**：公开形象更干净，但要同时迁 Kun 的 runner 与 Packages 命名空间，个人账号下的仓库也都要再搬一次。留作 S3 公开前的再评估项：届时包已在 nuget.org / npm，组织只影响源码与市场仓库的地址。
- **Kun 自建文档站**：单人维护没有带宽，读者重叠，两套搜索。否决。
- **文档站改用产品族名**：三个名字已经够用，站名「Qian 文档」加一句副标题即可。否决。

## 5 后果与代价（Consequences）

- 正面：三层口径统一，README、PRD、文档站、AGENTS.md 同一说法；Kun 读者有站可读；版本矩阵让「哪个 Qian 配哪个 Kun」一眼可见；一个 feed、一处钉版。
- 负面：CI 需要对私有 kun 仓库的只读访问（deploy key 或细粒度 token）；镜像页继承 Kun 文档的失真，只能靠横幅与 D1.8 兜底；文档站多版本构建时间增加（每个 Qian 版本一份 Kun 镜像）。
- Kun 迁入 `idadawn` 的影响：git 与网页地址自动重定向；issues、PR、releases、secrets（`NUGET_API_KEY`）随仓库走；CavaiLAB 组织成员派生的权限（另两名管理员与 12 名只读成员）不随迁，需要的人要重新邀请为协作者、且个人账号仓库只有写权限一档；仓库级自托管 runner 随仓库 id 一起迁移（已核验在线，无需重新注册）；CavaiLAB 名下的 Kun 镜像包不随迁也不删，下次发版自动发到 `idadawn` 名下，消费者走 nuget.org 不受影响；Kun 的 CHANGELOG 比较链接与［下游智能体平台］文档中的旧地址择机更新（旧地址仍重定向，不阻塞）。Qian 侧的看板与 ArgoCD 地址不变。
- 配套任务：S1-DOCS-005（Kun 小节镜像 + API）、S1-DOCS-006（产品族地图、版本矩阵、`project` 字段）、S1-DEC-009（仓库归属，已拍板关闭）、S1-PM-003（Kun 迁入 `idadawn` 的执行与核验）；S1-RT-001 验收补 `KunVersion`；AGENTS.md 硬规则两条。

## 6 对各阶段的影响

- **S1**：Kun 小节随文档站第一版上线（S1-DOCS-005 依赖 S1-DOCS-001）；`KunVersion` 在首个引用 `Kun.*` 的任务落地；Kun 迁入 `idadawn`（S1-PM-003）。
- **S2**：「问 AI」可跨 Kun / Qian 回答；公开前处理 D21.4 的公开耦合。
- **S3**：ISV 文档用同一站；组织形态再评估。
- **S4**：版本矩阵加入 MCP 协议版本与 Agent 运行时（［下游智能体平台］）版本。

## 7 待确认事项

- 【经 S1-DEC-038 D40 修订：产品族的所有者是 CavaiLAB（产品负责人 2026-11-24 定）；Kun 的源码以 `CavaiLAB/kun` 为准（D17）】**仓库归属**：已拍板（产品负责人，2026-10-07）：产品族所有者 = `idadawn`，Kun 从 CavaiLAB 迁入，理由是［下游智能体平台］源码与其余仓库都在 `idadawn`；specs 中的组织占位已替换为 `idadawn`。**已执行**（2026-10-08）：`idadawn/kun` 就位，旧地址重定向，runner / secrets / releases 完好；协作者邀请名单待产品负责人决定（S1-PM-003）。
- **Kun 仓库公开时点**：随文档站公开，需 Kun 维护人确认。
- **［前端框架］是否拆独立仓库**：默认不拆，随 Qian 单仓；S3 公开时再议。

## 8 参考

- 相关 ADR：ADR-001（Kun 关系）、ADR-013 D13.18（文档站）、ADR-016（［下游智能体平台］边界）、ADR-017（部署）、ADR-019 §7（许可与 feed）、ADR-020 §7 第 4 条（`qian-plugins`）。
- PRD：[§01 愿景](../prd/01-vision.md) 1.1、[§07](../prd/07-s1-foundation.md) 7.1 与 7.15.2；[specs/docs-site.md](../prd/specs/docs-site.md) §2b。
- 流程：[pm/README.md](../../pm/README.md)、[deploy/pm-board/README.md](../../deploy/pm-board/README.md)、[仓库 README](../../README.md)「产品族」一节。

## 9 现在只剩匠心与 Kun 两个项目（2026-11-26）

产品负责人 2026-11-26 说（[S1-DEC-043](../../pm/tasks/S1-DEC-043.md) D2；平台级裁定 P78，原文在 `CavaiLAB/jiangxin` 的 [2026-11-26 平台级裁定](https://github.com/CavaiLAB/jiangxin/blob/c7b32a4dea2d1455fd2b9835e1cc0848c9d036c2/docs/decisions/2026-11-26-platform.md)）：「经过一段考虑，前端 Qian 归档，SanyuanAgent 保留先不维护，现在只剩下  Jiangxin 和 Kun」。

- 「前端 Qian 归档」说的是 2026-11-25 已经定下并做完的一件（S1-DEC-043 D1；平台级裁定 P77）：前端框架的仓库归档，只读，已发布的包留着，不再发新版。
- 读法（整理者的读法，产品负责人没有逐句确认）：在用、在维护的项目是匠心（`CavaiLAB/jiangxin`）与 Kun 两个。SanYuanAgent 的仓库留着、不归档，代码和文档停在现在的样子。四个仓库都还在。
- 产品族的口径：本文 D21.1 的三层口径已被 [ADR-023](ADR-023-kun-qian-merge-blueprint.md) D23.6 取代；那里说的前端层，仓库已归档，D23.6 前加了标注。Kun 自己的定位不变。平台怎么描述、各项目之间的关系，以 `CavaiLAB/jiangxin` 的平台级裁定为准，本仓库只引用。
- D21.5 的产品族说明：本仓库 README 的产品族表与文档站产品族页（`/start/family`）的表里，前端框架一行去掉了（S1-DEC-043 D1：产品负责人 2026-11-26 答「连文档站一起去掉」）；两张表下面各加了一句，写明前端框架的仓库已归档、已发布的包留着。文档站的其余部分没有动。README 的第一行与「定位」一节也照这次的说法改了（S1-DEC-043 D2、D3）。
- 文档站（D21.2 第 2 点）：他同一天说「Kun 增加单独网站增加使用文档和说明。方便后续其他人使用，增加给大模型使用的类似 llm.txt 这种文档。方便进行调用。」（S1-DEC-043 D4；平台级裁定 P82）。范围与没有定的见 [ADR-022](ADR-022-rspress-unified-component-docs.md) §9。
- 文档站里前端框架的组件页（2026-11-26）：产品负责人定从站里拿掉（[S1-DEC-047](../../pm/tasks/S1-DEC-047.md) D2），由 [S1-DOCS-011](../../pm/tasks/S1-DOCS-011.md) 落地：`/components/qian/` 只留一页说明，原来的地址都落到这一页，组件文档以已归档仓库的标签 `@cavailab/qian-core@0.2.1` 为准。站里剩下的是 Kun 自己的文档，外加讲旧外壳与前端 SDK 的几页，站名随之从「Kun 与 Qian 开发文档」改成「Kun 开发文档」，首页的一句话定位不再提前端框架的组件文档；用哪个站名是落地时定的写法，产品负责人没有定。D21.5 的 `project` 字段照旧，`llms.txt` 里前端框架这一组只剩那页说明与「设计与路线」里前端框架规格的镜像，组名改成「前端框架 Qian（已归档）」。
