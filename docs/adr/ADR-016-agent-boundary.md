---
title: 与［下游智能体平台］的边界：Qian = 能力提供方 + 业务主数据权威；［下游智能体平台］ = Agent 运行时 + AI 资产控制面；Qian 不写任何 Agent loop
status: Accepted
date: 2026-10-06
decision_id: D16
supersedes: none
---

# ADR-016 与［下游智能体平台］的边界

> 本文保留当时的决策。方括号中的能力描述用于标出整理时替换的引擎或下游名称。

> 修订（2026-11-24，[S1-DEC-038](../../pm/tasks/S1-DEC-038.md) D23、D25）：2026-11-19 平台级裁定 P30 在组件里新增「知识库」内容，与 D16.8 把知识库算作下游智能体平台的 AI 资产有出入，两边怎么分待定，随 S1-DEC-038 的 Q5（本文边界的改写）一起处理；P31 把 Agent 调用平台能力的启用时点由 S4 提前到路线里程碑 M1（D16.9），调用路径不变。正文未改，受影响的条目前加了标注。

> 修订（2026-11-24，[S1-DEC-038](../../pm/tasks/S1-DEC-038.md) D45）：产品负责人 2026-11-24 定 SanYuanAgent 暂缓开发，现在使用 DSH。本文的边界怎么改写（S1-DEC-038 的 Q5）仍待定。正文未改。

> 修订（2026-11-25，[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) D7）：能力条目上标了要人确认的能力，调用方不是人本人时，平台自己不执行，生成一条等人输入的任务，等人点；人点了平台才执行。D16.3 (c) 里「运行时权威仍是［下游智能体平台］的 ToolPolicy / RiskGate」在这一件上受影响，范围见 §9。本文的边界怎么改写（S1-DEC-038 的 Q5），这次只定了这一件，其余仍待定。正文未改，D16.3 前加了标注。

## 1 背景与问题（Context）

［下游智能体平台］是团队基于 Kun 的 Agent 产品（C# 四服务 + Python Harness，React 19 前端）。其 Agent 运行时（Harness Agent loop、Sub-agent / Workflow、编排路由、四层记忆、Skills、工具协议 ToolSpec / ToolPolicy / RiskGate、运行授权快照）已落地；MCP 客户端只有 stdio、钉在 2025-03-26 且未接进运行中的 app；Nacos AI Registry / MCP Catalog / 插件市场 / 组织资源授权仍是设计态或 blocked；沙箱后端自 2026-07-14 起为 CubeSandbox（E2B 兼容），Supervisor 已退役（基线 P §8.1、§10.1 第 9 条、附录 A-5；基线 K §4.1）。［下游智能体平台］已有「两个 Tenant 实体」之痛，Qian 加入前必须先定唯一权威（基线 P §8.6）。［下游智能体平台］冻结了 S2S JWT claim 契约、事件 envelope、错误体 `ErrorDetail{code, message, trace_id}`、契约格式（OpenAPI 3.1 + JSON Schema 2020-12）（基线 K §4.1）。配置中心存在冲突：Qian 硬性 Apollo，［下游智能体平台］ `IConfigProvider` 定为 Nacos Config（仅设计、无代码）（基线 P 附录 A-2）。［下游智能体平台］文档为 CC BY 4.0，C# / 前端为内部项目，Python Harness 为 Apache-2.0（基线 K §4）。

## 2 决策（Decision）

- **D16.1 分工**：Qian = 能力提供方 + 业务主数据权威；［下游智能体平台］ = Agent 运行时 + AI 资产控制面；Higress = 共用数据面；Kun = 共用身份 / 审计 / Outbox 基座。
- **D16.2 Qian 不写任何 Agent loop、沙箱、记忆、技能引擎**；S4 经 MCP / HTTP 对接［下游智能体平台］运行时；沙箱对接 CubeSandbox（E2B 兼容 API），不按已退役的 Supervisor 端点设计。
- 【经 S1-DEC-041 D7 修订：(c) 里要不要等人确认这一件，标了要人确认的能力由平台自己停下来等人点，见 §9；其余各项不变】**D16.3 对齐项**：(a) S2S JWT claim 形状 `iss / aud[] / sub / tenant_id / project_id? / roles[] / scope 串 / session_id`，`exp − iat ≤ 300s`，「加 scope 不加 claim」；(b) 事件信封 `version / event_id / subject / occurred_at / tenant_id / payload`；(c) 工具元数据 `sideEffect / policyHint / requiredScopes / requiredSecrets(名字) / schemaSha256 / contractVersion`，［下游智能体平台］据此生成默认 ToolPolicy，运行时权威仍是［下游智能体平台］的 ToolPolicy / RiskGate；(d) 契约格式 OpenAPI 3.1 + `operationId`、JSON Schema 2020-12 + `additionalProperties:false`，`x-status: draft` 期允许破坏性修订、定稿后 additive-only；(e) 错误体 `ErrorDetail`；(f) 注册表形状 Package / Release / Publication / Installation 四层。
- **D16.4 MCP 以 2026-07-28 为目标版本**（无状态、`server/discover`、头部路由、列表可缓存、Tasks / Apps / Skills 官方扩展），通过 `server/discover` 兼容旧客户端；与［下游智能体平台］制定协议对齐计划（其客户端仍钉在旧版）。
- **D16.5 共用边界**（Q15）：租户主数据权威归 **Qian**；共用 Authentik（仅运维侧）与 Higress；S2S 各签各的但 claim 同形（若共用需规划内部服务自铸的密钥与 kid 管理）；Usage Ledger / 租户 API Key 是否共用待 S3。
- **D16.6 代码复用授权**（Q40）：［下游智能体平台］与 Qian 同属一个所有者，允许搬代码，但只搬四处——审计拦截器、租户主数据模型（Tenant / TenantMember / Project，去掉 Agent 专属字段）、OIDC 授权码实现、通知通道（typed port）范式；其余只借鉴思路。
- **D16.7 配置中心**：Qian 侧权威 = Apollo；可为［下游智能体平台］的 `IConfigProvider` 端口提供 Apollo 实现，或约定互不越界；Nacos 不进入 Qian 的配置依赖（见 ADR-007）。
- 【经 S1-DEC-038 D23 标注：组件里新增「知识库」内容（平台级裁定 P30），与本条知识库的归属有出入；怎么分待定，随 S1-DEC-038 的 Q5 处理】**D16.8 AI 资产 / MCP 注册**：Qian 自身能力以 Qian 注册表为真相源，向［下游智能体平台］ Platform facade / Nacos AI Registry 的注册是经 adapter 的投影，对外形态兼容官方 Registry OpenAPI；［下游智能体平台］自有 AI 资产（智能体、提示词、知识库、Skills、工具策略）的权威归［下游智能体平台］，Qian 不复制、不改写；两个市场是否合并待拍板。
- 【经 S1-DEC-038 D25 修订：启用时点由 S4 提前到路线里程碑 M1（平台级裁定 P31）；经 Higress MCP 路由的调用路径不变】**D16.9 Agent 调用路径**（S4）：Agent 不直连 Qian 服务裸 URL，一律经 Higress MCP 路由；令牌受众绑定（RFC 8707）、禁止透传、最小 scope + 逐步提权；「知道 ID 不构成权限」，Qian RBAC 接受 S2S JWT 的 `sub + tenant_id + scope` 做用户态鉴权；写类工具必须幂等。
- **D16.10 发现与暴露**：工具**定义**由能力目录机械生成（保证 schema 不漂移），**暴露**经 `agent.expose` 与意图式 description 策划；平台另提供能力目录搜索工具与代码模式；MCP 服务器按租户逻辑门面托管在 Higress，而非每插件一个进程。
- **D16.11 生成**：Agent 生成应用 = 生成 `package`，经 schema 校验 + TCK + 差异审阅 + 人工确认；Agent 生成的代码只落 `function` 沙箱；记录 `generatedBy / promptHash / 审核人` 并按生成内容标识要求标识。
- **D16.12 对接以［下游智能体平台］冻结契约（`docs/contracts/v0`）与代码为准**，不依赖其设计态能力；Qian 可作为首个批量注册方推动［下游智能体平台］的 MCP Catalog 落地。

## 3 理由（Rationale）

- ［下游智能体平台］的 Agent 引擎 / 记忆 / Skills 已落地且 Harness 为 Apache-2.0 可独立运行，重复建设无价值（基线 P §8.1）。
- 租户权威归 Qian：Qian 是业务主数据与租户生命周期的所有者（ADR-004、ADR-005），［下游智能体平台］的 Tenant 实体服务于 Agent 场景；第三套租户实体会放大已有的双真相之痛。
- 对齐 claim、信封与工具元数据而非共用实现，使两个产品可独立演进又能互相调用。
- 以 2026-07-28 为目标而非迁就旧版，是因为无状态端点、按头路由与列表可缓存直接决定 K3s 水平扩展与网关限流的实现方式（基线 P §8.4）。

## 4 备选方案与放弃原因（Alternatives）

- **Qian 自建 Agent 运行时**：重复建设，且与［下游智能体平台］分裂工具协议与门禁。放弃。
- **合并为一个产品 / 一个仓库**：产品定位不同（低代码 PaaS vs Agent 平台），发布节奏不同。放弃。
- **租户权威归［下游智能体平台］ Edge 签发 S2S**：业务主数据在 Qian，权威反置会让每次开户跨产品。放弃。
- **迁就 MCP 2025-03-26**：会把有状态 SSE 与服务端反向请求的假设带进 S4 设计。放弃。

## 5 后果与代价（Consequences）

- 正面：Qian 聚焦能力与元数据；S4 对接只需 MCP 门面 + 契约对齐；审计、事件、S2S 在两个产品间同形。
- 负面：Qian 的 S4 进度受［下游智能体平台］ MCP 客户端与 Catalog 的成熟度制约；Qian 注册表与［下游智能体平台］ / Nacos 的投影需要 adapter 维护；若最终共用 S2S，需管理内部服务自铸密钥的 kid。
- 配套：协议对齐计划与时间表；`docs/contracts/v0` 的联合维护；C# MCP SDK 能否过 Kun `PublishAot /warnaserror` 门禁需实测，不能则 MCP 端点放在非 AOT 独立服务或由 Higress 按契约生成。

过渡态（2026-10-06）：［下游智能体平台］ MCP 客户端钉在 2025-03-26 且只有 stdio；MCP Catalog / AI Registry / 插件市场为设计态；［下游智能体平台］ Platform 在 prod 是否常驻未确认。

## 6 对各阶段的影响

- **S1**：claim、信封、错误体、契约格式对齐；统一主体模型与委托链字段落库；搬运四处代码。
- **S2**：工具元数据字段随能力目录落库；工作流即能力；低代码连接器与第三方出网点的出网 SSRF 基线（平台自有出网点在 S1）。
- **S3**：授权服务器对齐 MCP（RFC 9728 / 8414 / 8707、PKCE、`iss` 回传、CIMD 规划）；市场预留 MCP registry 字段；Usage Ledger 共用方案。
- **S4**：每租户 MCP 门面、Agent 主体与 `act` 链、CubeSandbox 对接、Skills over MCP、A2A 可选。

## 7 待确认事项

- Qian 应用市场与［下游智能体平台］插件市场是否合成一个注册表、谁审核、谁签名（Q15 默认未覆盖此项）。
- Usage Ledger / 租户 API Key 是否共用（待 S3）。
- ［下游智能体平台］ Platform 在 prod 是否常驻 HTTP；切到 CubeSandbox 后 SecretScope、逐域名出网白名单、沙箱审计事件由何承接。
- 官方 C# MCP SDK 的 AOT 门禁实测。

## 8 参考

- 相关 ADR：ADR-002、ADR-005、ADR-007、ADR-008、ADR-009、ADR-010、ADR-014、ADR-015。
- PRD：`PRD §10`、`PRD §12`、`PRD §13`、`PRD §17`（借鉴声明）。
- 基线：P §0 第 7/10 条、§8.1–§8.7、§10.1 第 9/10 条、§10.2 第 3/5 条、附录 A-1/A-2/A-3/A-5/A-6、附录 C；K §4.1、§7.1 第 10 条、§7.2 第 2 条、附录 B-9/B-10。

## 9 标了要人确认的能力，平台自己停下来等人点（2026-11-25）

产品负责人 2026-11-25 定（[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) D7；平台级裁定 P73，原文在 `CavaiLAB/jiangxin` 的 [2026-11-25 平台级裁定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/decisions/2026-11-25-platform.md)）：能力条目上标了要人确认的能力，调用方不是人本人时，平台自己不执行，生成一条等人输入的任务，等人点；人点了平台才执行。问的是「与 Kun 现行写法对不上的两处，都按提议定吗？」，答「都按提议定」；这是其中一处，另一处见 ADR-012 §9。这是对 §2 D16.3 (c) 的修订，只在这一件上。还没有实现。

是什么：

- 「标了要人确认」指能力条目上 `agent.requiresConfirmation` 为真。这个字段在冻结的契约 `contracts/v0` 里已经有，现在只登记，平台不按它拦调用。
- 「不是人本人」指调用方是 Agent，或者带着委托链。人自己在界面上直接调同一个能力，不拦。
- 等人输入的任务就是 PRD §08 8.12 节的 Operation，状态 `input_required`。人点了以后由平台执行。

原因：平台的事要平台自己从人那里拿到答案，见那份平台级裁定第 3.7 节。

修订的范围：

- D16.3 (c) 写「［下游智能体平台］据此生成默认 ToolPolicy，运行时权威仍是［下游智能体平台］的 ToolPolicy / RiskGate」。这次改的只是其中一件：标了要人确认的能力，等不等人点不由调用方一侧决定，平台自己停。
- 整理者的读法，产品负责人没有逐句确认：这次没有把 ToolPolicy / RiskGate 整个搬到平台，D16.3 在别的事情上照旧；调用方自己的策略可以更严。
- 冻结的契约没有动。`contracts/v0` 的对齐差异说明第 4 节引本条 (c) 的一句，照本节读。

需求文档里的对应：

- 同一口径、已加修订注的：§10 的 10.3 节「范围与边界」末一条、10.1.2 节表的第一行、10.6 节表的第二行。
- 同向、没有展开成契约的三句：§10 的 10.3 节「危险操作 Agent 只能发起申请（「待确认票据」契约）」；§13 的 13.5 节把 confirm-required 解释为「Agent 发起时平台返回 `input_required`，由人确认后继续」；§03 的 3.10 节的例子「Agent 调用一个需人工确认的工具，同样收到 `input_required`」。

没有定的：

- 接口的形状，即路径、字段、回什么状态、错误码怎么登记，是提议，随契约定稿。
- `agent` 主体启用之前怎么演示，谁做、什么时候做。看板上没有新建实现任务。
- 本文的边界怎么改写（S1-DEC-038 的 Q5）：这次只定了这一件。本文其余各条怎么改写、「Agent SDK 指什么」、服务层与媒体适配层的许可、D16.8 里知识库的归属，都仍待定。
