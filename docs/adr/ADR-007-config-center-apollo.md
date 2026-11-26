---
title: 配置中心：Apollo 作 L1 共享组件，自研 AOT 友好的 .NET provider；只存非敏感配置与密钥引用；租户级配置不进 Apollo
status: Accepted
date: 2026-10-06
decision_id: D7
supersedes: none
---

# ADR-007 配置中心 Apollo

> 本文保留当时的决策。方括号中的能力描述用于标出整理时替换的引擎或下游名称。

> 修订（2026-11-25，[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) D1、D2）：单机版默认不装 Apollo，读配置文件；企业版照旧（§9）。交付给客户的产品里，Portal 的登录接身份服务，不带 Authentik（[ADR-017](ADR-017-deployment.md) §12）。正文未改，D7.1 前加了标注。

> 修订（2026-11-27，[S1-DEC-049](../../pm/tasks/S1-DEC-049.md)）：配置中心维持 Apollo、单机版不装，不改用 Nacos（原表第 20 条，照建议定），见 §10。§7 里生产托管 RDS 那一项延期。正文未改，§7 那一项前加了标注。

## 1 背景与问题（Context）

Apollo 是硬性要求。Kun 的 `Kun.Configuration` 只做 `AddKunOptions<T>` 绑定 + 校验，**无任何配置源**，全仓无 `apollo`；现网没有 Apollo、没有 MySQL、没有任何配置中心被应用消费；Nacos 3.0.1 standalone 已部署但只作 AI Registry PoC，且既有边界 ADR 把「动态非敏感配置」权威划给了 Nacos Config，［下游智能体平台］的 `IConfigProvider` 设计也定为 Nacos（基线 K §2.6；基线 P 附录 A-2）。

Apollo 的隐性成本：MySQL 是其唯一官方生产库（PostgreSQL 不受支持）而现网全 PG；官方 .NET 客户端 2.11.0 维护停滞、反射式 STJ 不适配 AOT / 裁剪、启动同步阻塞且无缓存时 30 秒后以空配置继续；Apollo 无租户概念、namespace 有上限、灰度只按实例；.NET 10 运行时的 `IOptionsMonitor` 热重载会重跑校验但不保留上一份有效值，`CurrentValue` 持续抛异常（基线 K §2.6、§7.1 第 4 条、附录 B-7）。版本：≥2.5.2 修复三个 CVE；3.0.0 于 2026-10-03 发布，生产等首个补丁（基线 K §6.3）。参考平台把 100 多个配置键扁平堆放、密钥与参数同表、文件路径与 OCR 地址写在后端配置文件（基线 S §6.2 第 9 条；基线 F §6 第 10 条）。

## 2 决策（Decision）

- 【经 S1-DEC-041 D2 修订：单机版默认不装 Apollo，见 §9】【经 S1-DEC-041 D1 修订：交付给客户的产品里 Portal 的登录接身份服务，见 ADR-017 §12】**D7.1 Apollo 作为 L1 共享平台组件**，按现有「vendor chart + 离线渲染入 git」范式部署：configservice + adminservice（每 Apollo 环境一组，2 副本 + podAntiAffinity + PDB）+ portal（2 副本，OIDC + JDBC session）；`JAVA_OPTS=-XX:MaxRAMPercentage=70`；Portal 接 Authentik OIDC、不公网。
- **D7.2 版本**：首发 2.5.x（≥2.5.2）；3.0.x 待首个补丁后评估升级（Q8）。
- **D7.3 MySQL**：staging 先用集群内 operator，生产用托管 RDS（Q8）。
- **D7.4 不与［下游智能体平台］共用一套 Apollo**（Q8）。
- **D7.5 .NET 客户端自研轻量 provider**（`qian.config`，路线 B）：按公开 HTTP 协议 `/configs`、`/notifications/v2` 长轮询（60 秒 hold，需大于 60 秒的空闲超时）、AccessKey HMAC 签名；集群内直连 Service 跳过 Meta Server；AOT 友好、源生成序列化；本地缓存文件；官方 SDK 只作 PoC。
- **D7.6 `Kun.Configuration` 增强**：运行期变更先校验，失败保留上一份有效值并告警；`ValidateOnStart` 分「关键」与「可选」配置；options JSON Schema 导出；失败降级矩阵 L0–L4。
- **D7.7 边界**：Apollo 只存平台 / 服务级**非敏感**配置与密钥引用（`secretRef`）；密钥走 K8s Secret / Infisical，由 `ISecretResolver` 在使用点解析；清单、事件、日志永不出现明文密钥。
- **D7.8 租户级配置不进 Apollo**，由 `qian.tenancy` 的租户配置服务承载（S1 基础 / S3 自助）；功能开关的租户灰度靠租户配置服务，Apollo Label 灰度只按实例。
- **D7.9 映射**：AppId = 一个微服务 / 后端插件；Environment 只建 UAT + PRO；Cluster 默认 `default`（仅区域或专属大租户用）；私有 namespace `application` 与 `qian.plugin.<id>`，公共 namespace `qian.common.*`；插件 `config.schema`（JSON Schema 2020-12）映射为 namespace 骨架与校验器，敏感字段标 `sensitive` 且只能来自 `secretRef`。
- **D7.10 配置源顺序**：appsettings → Apollo（公共 → 插件 → application）→ K8s Secret 文件（KeyPerFile）→ 环境变量 → 命令行；Kustomize 只注入引导项（`Apollo__AppId / Env / Cluster / ConfigServer__0 / Label / CacheDir`、Secret 文件）。
- **D7.11 授权与开关分开存**：授权（Entitled，是否买了）在租户 / 订阅库；特性开关（Enabled，是否灰度开）在 Apollo（OpenFeature 式 targetingKey = tenantId）。
- **D7.12 修订既有边界 ADR**：Nacos 收窄为 AI 资产 / MCP 注册，不再承担配置职责；［下游智能体平台］若坚持 Nacos 则「各用各的、互不越界」，或为其 `IConfigProvider` 端口提供 Apollo 实现。
- **D7.13 治理**：webhook / OpenAPI 扫描拦截疑似密钥键名与值；Apollo 发布事件进 `qian.audit`；实例拉取审计作为「变更是否生效」的回读信号；基线定期导出进 git。

## 3 理由（Rationale）

- Apollo 为硬性要求且已被裁定为 Qian 侧配置权威（基线 K 附录 B-8）；分叉成本在于修订边界 ADR 与新增 MySQL 运维面，但比双配置中心长期并存便宜。
- 自研 provider 的原因是官方客户端停滞且无法通过 AOT / 裁剪门禁、启动阻塞后以空配置继续运行是生产不可接受的行为（基线 K §2.6）。
- 租户级配置不进 Apollo 是 Apollo 自身的能力边界决定的（无租户概念、namespace 上限、灰度粒度），而不是偏好（基线 K §6.3 第 4 条）。

## 4 备选方案与放弃原因（Alternatives）

- **Nacos Config 作为 Qian 配置中心**：违背硬性要求；但若团队最终接受 Nacos 也能满足，这是一个显式分叉点（基线 K §7.1 第 4 条）。放弃。
- **官方 .NET SDK**：维护停滞、反射序列化、启动行为不安全。只作 PoC。
- **只用 K8s ConfigMap + GitOps**：无运行期变更推送、无灰度、无审计回读。放弃。
- **Apollo 承载租户级配置**：namespace 上限与无租户概念使其不可扩展。放弃。

## 5 后果与代价（Consequences）

- 正面：平台 / 服务级配置有统一的发布、审计与回读；插件配置 Schema 化并由清单派生 namespace；密钥与配置彻底分离。
- 负面：新增 MySQL 运维面（现网无平台级 MySQL 经验）与 Apollo 自身的 JVM 资源；与 GitOps 形成双真相，需要基线导出纪律；自研 provider 要自己跟进 Apollo 协议变化。
- 配套：L1 新增 `apollo/` 目录（ns `apollo`）；`qian.tenancy` 租户配置服务；S2 控制台渲染插件配置；S3 灰度 / 审核 / 回滚编排。

过渡态（2026-10-06）：现网无 Apollo、无 MySQL，边界 ADR 仍指向 Nacos Config；S1 第一个迭代完成 L1 部署与 ADR 修订前，Qian 服务以 appsettings + K8s Secret 启动。

## 6 对各阶段的影响

- **S1**：Apollo L1 部署 + MySQL；自研 provider；热更新语义；`secretRef`；配置 Schema 声明；租户配置服务基础。
- **S2**：控制台渲染插件配置；配置变更审批。
- **S3**：配置灰度 / 审核 / 回滚编排；租户自助配置。
- **S4**：配置 Schema 作为 Agent 可读元数据的一部分，写入走审批。

## 7 待确认事项

- 3.0.x 升级时点与 2.5.x 的 CVE 跟进节奏。
- 【S1-DEC-049（2026-11-27）：延期（成批的丁批），前置条件：先有生产部署的规划；本项不改】生产托管 RDS 的供应商与网络打通方式。
- Infisical 运行时注入（Operator / ESO）未接通前，`secretRef` 的运行时权威只能是 K8s Secret。
- Apollo 实例级灰度与租户级开关之间的联动规则需在 S2 前验证。

## 8 参考

- 相关 ADR：ADR-001、ADR-002、ADR-014、ADR-016、ADR-017。
- PRD：`PRD §07`、`PRD §12`、`PRD §13`。
- 基线：K §2.6、§6.2 配置 / 密钥行、§6.3、§7.1 第 4 条、附录 B-7/B-8；P §5 配置绑定行、附录 A-2；S §6.2 第 9 条、§11 第 7 条。

## 9 单机版默认不装 Apollo（2026-11-25）

产品负责人 2026-11-25 定（[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) D2；平台级裁定 P58，原文在 `CavaiLAB/jiangxin` 的 [2026-11-25 平台级裁定](https://github.com/CavaiLAB/jiangxin/blob/c7b32a4dea2d1455fd2b9835e1cc0848c9d036c2/docs/decisions/2026-11-25-platform.md)）：单机版默认不装 Apollo，读配置文件；要在运行期改配置的客户可以加装。企业版照旧装。问的是「与旧决策有出入的甲 1 至甲 3，都按提议定吗？」，答「都按提议定」，这是其中的甲 2。

- 单机版指给小企业的、一台服务器上能跑的部署，企业版指跑在集群上的全套部署，见 [ADR-017](ADR-017-deployment.md) §13。
- 修订的是 §1 开头的「Apollo 是硬性要求」与 §2 D7.1：对企业版不变；对单机版，Apollo 由必装改为可选。ADR-017 D17.3 的 L1 清单随之按本节读。其余条目不变，装了 Apollo 就照 D7.5 至 D7.13 办。
- 单机版读什么：D7.10 的顺序去掉 Apollo 一层，即 appsettings、K8s Secret 文件、环境变量、命令行。两个宿主现在都能这样启动（已有）：网关宿主的配置源设为 `host` 时只读宿主配置；平台宿主的 config 条目现在读的就是宿主配置根。
- 理由：单机版少装一套组件，即 Apollo 连同它用的 MySQL；少备份一个库。
- 代价：单机版没有运行期的配置推送、配置灰度和配置发布的审计。改配置靠改文件，一般要重启服务。§4 放弃「只用 K8s ConfigMap + GitOps」时列的就是这几条，单机版接受它们。
- 更正：裁定时给的理由里还有省内存一条，估的是约 3 GB。这个数是按内存的请求量估的，偏高；裁定之后核对，省下的内存比当时说的少。上面两条理由不变。

## 10 维持 Apollo（2026-11-27）

产品负责人 2026-11-27 答复《决策表》（[S1-DEC-049](../../pm/tasks/S1-DEC-049.md)）：其余逐条采用原表建议。原表第 20 条问配置中心，建议是「维持 Apollo，单机版不装。」另可选是「改 Nacos。」照建议定：

- 企业版的配置中心维持 Apollo，本文 §2 不变；单机版不装，与 §9 一致。
- 不改用 Nacos。[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) 里「配置中心是否改用 Nacos」那一问随之了结。
- 看板上的任务没有动。
