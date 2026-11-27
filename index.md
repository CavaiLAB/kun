> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.



# Kun 开发文档

Kun 是为 Agent 时代构建的多租户平台框架。本站收录 Kun 的 .NET 包与平台组件的文档；前端框架 Qian 的仓库已归档，它的组件页不在本站。匠心工业智能体平台的 Web 与桌面端采用 DSH 外壳。

## 三条起步命令

构建 Kun 命令行后，可按需安装组件、创建技能；模型调用需先配置网关。

### ① 装一个组件

```bash
qian add identity authz files
```
在自己的 ASP.NET Core 宿主里组合身份、授权和文件能力。[30 分钟上手 →](/start/quickstart.md)

### ② 配置你的编码代理

```bash
pnpm exec qian init --template skill-only --id com.example.guide --name Guide --cwd ./guide
```
在空白目录生成含 `AGENTS.md` 和技能内容的无代码组件。[详见下一节 ↓](#agents)

### ③ 调一个模型 S1·P1

```bash
curl "$MODEL_GATEWAY/v1/chat/completions" \
  -H "Authorization: Bearer $MODEL_GATEWAY_KEY" \
  -H "Content-Type: application/json" \
  -d '{"model": "chat-default", "messages": [{"role": "user", "content": "你好"}]}'
```
经模型网关的 OpenAI 兼容端点发一条请求，用逻辑模型名。[模型网关 →](/sdk/ai/gateway.md)

## 配置你的编码代理

Agent 原生表面从 S1 起就是一等公民：组件带的技能、约定与 MCP 可以直接交给编码代理。

### Qian 技能

安装时可通过 `qian add <组件> --surfaces skills --no-code` 选择技能文件。[表面 →](/start/concepts/surface.md)

### AGENTS.md 约定

每个仓库与每个包都带 `AGENTS.md`，`qian init` 生成骨架；代理动手前先读它。[ADR-020 D20.9 →](/design/adr/adr-020-component-model-v2.md)

### Qian MCP

组件的 `.mcp.json` 表面随组件安装；文档检索 MCP 为 S1 可选；每租户能力门面（S4）。[MCP →](/sdk/ai/mcp.md)

### 提示词模板

让代理按任务文件工作：读 `AGENTS.md`、认领任务、按验收逐条完成并开 PR。模板见[仓库 pm/README 第 7 节](https://github.com/CavaiLAB/kun/blob/main/pm/README.md#7-怎么下发任务)。

## 最新动态

- 2026-11-24 [Kun 2026.11.24](/sdk/kun/releases/index.md)：端点权限回调的校验；权限执行要求的标记接口；gRPC 权限点在启动时校验，共 13 项
- 2026-11-21 [Kun 2026.11.21](/sdk/kun/releases/index.md)：.NET 构建、测试与打包；平台分析器；样例、开发工具与测试宿主，共 48 项
- 2026-10-25 [Kun 2026.10.25](/sdk/kun/releases/index.md)：数据约定与实体审计不再占用当前主体 / 租户端口
- 2026-10-23 [Kun 2026.10.23](/sdk/kun/releases/index.md)：普通网关包的 JWKS NativeAOT 消费；工具依赖的安全补丁

[Kun 版本记录 →](/sdk/kun/releases/index.md)

## 产品能力

按用途找能力：每项一句话，正文只在一个部分写一次。

### 用 AI 构建

- [模型网关](/sdk/ai/gateway.md)：一个端点调所有模型，配额、回退、计量（S1·P1）
- [AI SDK](/sdk/ai/index.md)：调模型与 Embedding，写 Agent 表面，声明工具副作用与策略
- [Agent 运行时对接](/sdk/ai/agent-runtime.md)：Qian 不写 Agent loop，运行时为下游智能体平台
- [MCP](/sdk/ai/mcp.md)：组件的 `.mcp.json` 表面；每租户能力门面（S4）
- [流程与自动化](/components/platform/qian.workflow.md)：审批内核与触发器（S2）
- [沙箱](/sdk/ai/sandbox.md)：隔离执行不可信或 Agent 生成的代码（S4）

### 构建应用

- [宿主与框架](/start/hosts.md)：独立模式、platform profile、React + qian 组件库、[内核与平台组件](/sdk/kun/with-qian.md)
- [Kun 框架](/sdk/kun/index.md)的内核模块、平台组件集成和 [API 参考](/sdk/api/kun/index.md)
- [组件与 qian add](/components/index.md)：一条命令安装，装上即可用
- [能力与函数](/sdk/dotnet/capabilities.md)：能力目录；`function` 表面（S2）
- [事件与中间件](/sdk/dotnet/events.md)：五种分发模式，`waterfall` 即环绕中间件
- [profile 与 patch](/start/concepts/profile.md)：环境 = profile + 分层 patch
- [前端 SDK 与微前端](/sdk/frontend/index.md)：旧外壳的插件树与远程模块
- [存储](/components/platform/qian.files.md)：文件与数据，租户隔离默认开启
- [低代码](/design/prd/08-s2-lowcode.md)：表单与列表（S2）

### 多租户平台

- [租户](/components/platform/qian.tenancy.md)：租户目录、隔离策略与生命周期
- [身份与 SSO](/components/platform/qian.identity.md)：OIDC 端点与双层账号
- [鉴权](/components/platform/qian.authz.md)：权限点注册表与租户域 RBAC
- [权益](/components/platform/qian.entitlement.md)：授权 v0；计费（S3）
- [配置中心](/components/platform/qian.config.md)：Apollo 与热更新
- [事件与消息](/components/platform/qian.events.md)：Outbox 可靠发布
- [调度与实时](/components/platform/qian.scheduler.md)：持久化作业与实时推送
- [市场](/marketplace/index.md)：上架、信任等级与签名（S3）

### 保护应用

- [网关防护](/components/platform/qian.gateway.md)：强制认证、路由级限流、可信代理、出网 SSRF 基线
- [发布保护](/ops/release.md)：制品验签、修订回切、灰度开关
- [审计](/components/platform/qian.audit.md)：命令级哈希链与实体级同事务审计
- [密钥](/ops/secrets.md)：`secretRef`、按范围授权、轮换

### 部署与运维

- [GitOps 部署](/ops/deploy.md)：Argo CD app-of-apps、Kustomize、digest 写回
- [灰度发布](/ops/release.md)：灰度开关 + profile 滚动更新
- [修订回切](/ops/release.md)：回到任一保留的修订
- [可观测](/components/platform/qian.observability.md)：OTel、租户 / 用户 / 插件 baggage、计量事件
- [模型网关接线](/ops/model-gateway.md)：GPUStack → Higress（S1·P1）

## 指南与教程

### Agent 栈

Qian 提供能力与主数据，下游智能体平台跑 Agent，模型与 MCP 流量走 Higress。[阅读 →](/start/guides/index.md#agent-stack)

### 后端模式

seam 三角色、只经 Abstractions 引用、五种事件分发模式。[阅读 →](/start/guides/index.md#backend)

### 多 profile 组合

gateway、identity、platform 等 profile 各自部署、各自升级。[阅读 →](/start/guides/index.md#profiles)

### 安全

RLS 租户隔离、默认拒绝匿名、`secretRef`、信任等级。[阅读 →](/start/guides/index.md#security)

### 连接器与集成

为 seam 提供 Provider、声明出网目标的连接器组件。[阅读 →](/start/guides/index.md#integrations)

- [站点地图](/sitemap.md)
- 产品族仓库：[Kun](https://github.com/CavaiLAB/kun)、[Qian](https://github.com/CavaiLAB/qian)、qian-plugins（待建）
- 许可证：[Kun MIT](https://github.com/CavaiLAB/kun/blob/main/LICENSE)、[Qian MIT](https://github.com/CavaiLAB/qian/blob/main/LICENSE)

