---
title: 指南
description: Kun 的使用文档：哪些已经能照着做、哪些还写不了（依赖的能力没有实现），以及后端模式、安全、集成等要点与权威来源。
part: start
project: platform
audience: [developer, operator, decision-maker, agent]
updated: 2026-11-26
---

# 指南

## 使用文档 {#usage}

下表是 Kun 的使用文档。「已有」的那几篇在本机照着跑通过；「写不了」的是依赖的能力还没有实现，只写现状与入口。

| 要做的 | 现状 | 看哪里 |
|---|---|---|
| 上手：在 ASP.NET Core 里用上 Kun | 已有（2026-11-26 核对，Kun 2026.11.24） | [上手](/start/getting-started) |
| 写一个组件：入口、能力、清单派生、装进宿主 | 已有（同上）；无代码组件只写到生成骨架 | [写一个组件](/start/guides/write-a-component) |
| 在 .NET 里接权限：命令、HTTP、gRPC 三种入口 | 已有（同上）；权限引擎的平台模式写了现状与入口 | [在 .NET 里接权限](/start/guides/authorization-dotnet) |
| 接登录 | 有指南，没有按步骤核对 | [Identity 客户端（Kun 框架指南）](/sdk/kun/guides/identity-client) |
| Kun 宿主以外的系统接权限 | 写不了：远程的授权接口与客户端包是规划 | [在 .NET 里接权限 · 还没有的](/start/guides/authorization-dotnet#planned) |
| 接模型服务 | 写不了：模型服务的标准接口还没有实现，组件 [qian.model-gateway](/components/platform/qian.model-gateway) 只有配置骨架 | 此前的设计：[ADR-015 模型网关](/design/adr/adr-015-model-gateway) |
| 两种形态与两个版本怎么装 | 写不了：两种形态（带模型推理的集群，通过 API 接第三方模型服务）与多租户、单租户两个版本是已定的方向，各自怎么装配是规划 | 现有的部署说明：[运维](/ops/) |
| 接口参考 | 部分已有：内核档的 .NET API、清单与命令行字段；平台档的 .NET API、平台 HTTP 接口的参考没有 | [API 参考](/sdk/api/)、[CLI 与规格](/cli/) |

两种形态、两个版本与模型服务的标准接口，见 2026-11-26 的平台级裁定在 Kun 一侧的登记 [S1-DEC-043](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-043.md) D5。

## 要点与权威来源

下面几节写于 2026-10，每节给出要点与权威来源，完整指南随相关组件交付陆续补齐。节里的「Qian」是当时的平台名；现在 Qian 只是前端框架的名字，它的仓库已归档。

### Agent 栈 {#agent-stack}

Qian 是能力提供方与业务主数据权威，Agent 运行时是下游智能体平台，模型与 MCP 流量走共用的 Higress；Qian 不写 Agent loop、沙箱、记忆与技能引擎（[ADR-016](/design/adr/adr-016-agent-boundary)）。组件的 skills、tools 等 Agent 原生表面在 S1 可用；每租户 MCP 能力门面在 S4。开发入口见 [AI SDK](/sdk/ai/)。

### 后端模式 {#backend}

组件以 seam 组织：Definition、Provider、Consumer 三个角色，组件之间只经 `*.Abstractions` 引用；进程内事件按五种分发模式之一分发，跨进程的集成事件经 Outbox 可靠发布。见 [seam](/start/concepts/seam)、[事件与中间件](/sdk/dotnet/events)、[qian.events](/components/platform/qian.events)。

### 多 profile 组合 {#profiles}

一套部署通常由多个 profile 组成：`gateway` 在边缘做认证与限流，`identity` 是唯一 issuer，`platform` 承载平台插件。各 profile 独立部署、独立升级。见 [宿主与框架](/start/hosts)、[GitOps 部署](/ops/deploy)。

### 安全 {#security}

默认共享库 + `tenant_id` + PostgreSQL RLS 隔离租户；网关默认拒绝匿名；密钥只以 `secretRef://` 引用出现；第三方代码不进共享进程，按信任等级限制运行形态。见 [PRD §13 安全](/design/prd/13-security)、[密钥](/ops/secrets)。

### 连接器与集成 {#integrations}

连接器是为某个 seam 提供 Provider、且需要出网的组件，例如身份 seam 的 `oidc`、`wecom` 连接器：向注册表子服务按名称注册，并在清单 `policy.egress[]` 声明允许的出网目标。见 [组件清单 plugin.json](/cli/plugin-json)。

### 从参考平台迁移 {#migration}

迁移方案按租户 / 身份 / 权限、应用表单与列表、流程、数据集成与展示、前端五个领域组织，见 [迁移方案](/design/migration/)。
