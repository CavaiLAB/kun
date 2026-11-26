---
title: 网关分层：Traefik（北向入口）→ qian.gateway（Kun.Gateway 加固，业务 API 边界）；Higress 只管模型与 MCP 流量
status: Accepted
date: 2026-10-06
decision_id: D10
supersedes: none
---

# ADR-010 网关分层

> 本文保留当时的决策。方括号中的能力描述用于标出整理时替换的引擎或下游名称。

> 修订（2026-11-25，[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) D4）：匠心工业智能体平台 Web 端里 Agent 工作区的域名，由匠心的工作区服务直接接，不经过 `qian.gateway`。这是对 D10.1 的一处有意的偏离，范围与原因见 §9。正文未改，D10.1 前加了标注。

## 1 背景与问题（Context）

现网有两个 IngressClass：`traefik`（业务北向）与 `higress`（AI 南向），各有 MetalLB VIP；集群不直接暴露公网，公网 nginx 终结 TLS 后经 WireGuard 直连；集群内无 cert-manager，Ingress 全 HTTP；Traefik `forwardedHeaders.trustedIPs` 必须配置否则 OIDC issuer 变成 http（基线 K §6.1）。Higress 2.2.3 两副本 + PDB，是模型流量的 chokepoint，对租户无感（基线 K §6.4）。

`Kun.Gateway` 是 YARP 反代 + 外部 JWT 校验（单 issuer）+ `S2sTokenIssuer`（RS256 / 60 秒）+ JWKS + 三种限流策略 + `IS2sPrincipalResolver` + `ForwardedClaimTypes` 白名单 + gRPC-Web，可执行亦可作库；但**默认装配不可上线**：不强制认证、匿名透传下游；`kun-default` 限流未挂到任何路由；IP 分区信任 `X-Forwarded-For`；换私钥须重启；无动态 `IProxyConfigProvider`；网关宿主未接 OTel（基线 K §1 Gateway 行、附录 B-11）。S2S 签发器有三套（网关 RS256 / 60 秒；`Kun.Security.S2SJwt` ES256 / 300 秒含 `scope / session_id / membership_id / project_id`；`Kun.Identity.KunJwtIssuer`），且默认 issuer 值写死为下游产品名（基线 K §1 Security 行、§5.1 第 7 条）。白皮书声称的 `/s2s/issue` 机器身份端点不存在（基线 K 附录 B-13）。gRPC 三个拦截器只覆盖 Unary，流式调用绕过认证与 `tenant_id` 校验（基线 K §2.4 ⑤）。参考平台开放接口用用户级 AppSecret 并允许「公有」免鉴权；接口体 AES 加密中间件本质是混淆（基线 D §9.1 第 8 条；基线 P §B.2）。

## 2 决策（Decision）

- 【经 S1-DEC-041 D4 标注：匠心 Web 端的工作区域名由匠心的工作区服务直接接，不经过 `qian.gateway`，是有意的偏离，见 §9；三层的分工不变】**D10.1 三层网关，职责从 S1 画清、不互相承担**：Traefik（北向公网入口）→ `qian.gateway`（业务 API 边界）→ Higress（模型与 MCP 流量）。
- **D10.2 Traefik 不做鉴权**，只做入口路由与 TLS 终结衔接；`trustedIPs` 必配；ingress 全 HTTP 直到引入 cert-manager。
- **D10.3 `qian.gateway` 以库方式引用 `Kun.Gateway` 并加固**：认证回退策略（默认拒绝匿名）；路由级 `RateLimiterPolicy` 与全局限流器；可信代理网段；私钥热轮换（JWKS 双钥重叠）；Cookie 会话换 S2S（`IS2sPrincipalResolver` 注入租户 / 应用上下文）；按上游 cluster 铸造 per-service audience；`AddKunObservability`；`/healthz /readyz`；OpenAPI 聚合与能力目录端点（生产不公开 OpenAPI）。
- **D10.4 S2S 签发器收敛为一套**：保留网关 `S2sTokenIssuer`（短 TTL、JWKS），吸收 `scope / session_id / membership_id / project_id` claim；claim 形状对齐［下游智能体平台］契约 `iss / aud[] / sub / tenant_id / project_id? / roles[] / scope 串 / session_id / iat,nbf,exp,jti`，`exp − iat ≤ 300s`；「新能力授权 = 加 scope 串，不加 claim」；`roles[]` 作为与［下游智能体平台］同形的例外保留，但限定为粗粒度角色代码而非权限点集合，Qian 自身服务不得仅凭 `roles[]` 放行、必须调用 `Kun.Authorization` 决策端口，时效性由 `identity_revision` 管；`project_id` 保留为可选 claim（［下游智能体平台］侧语义，Qian 只透传不解释）（R24）；`Kun.Identity.KunJwtIssuer` 与 `Kun.Security.S2SJwt` 标记废弃；去产品化（issuer 默认值等约 30 处）。
- **D10.5 机器身份签发**：`client_credentials` 由 `qian.identity` 提供（S1 必做）；后台任务 / 作业以服务自持私钥签发为过渡，S4 前统一经签发端点。
- **D10.6 多 Pod 共享限流计数**：Redis `PartitionedRateLimiter`，S1 可选、S2 必做。
- **D10.7 S3 动态路由 provider**：租户目录 × 已安装应用 → YARP routes / clusters，由插件清单 `components.service.protocols` 与 `tenancy` 派生；「未启用」在网关路由、SDK 入口、前端外壳三处同时拦截；域名 → 租户解析 S1 可选 / S3 必做。
- **D10.8 开放接口凭据**（Q35）：废弃用户级 AppSecret，统一为网关的应用级客户端凭据（`client_credentials` / API Key，经 `qian.identity` 客户端注册表）；取消「公有」免鉴权。
- **D10.9 gRPC 流式拦截器补齐**（server / client / duplex streaming），`service` 型插件用流式 RPC 前必须完成。
- **D10.10 Higress 只管模型与 MCP 流量**，对租户无感；不在 S1 把 SSO / S2S 交给 Higress；网关策略类插件直接用 Higress `WasmPlugin` CRD，不自造。
- **D10.11 不做**：接口体 AES 加密中间件（下沉网关 TLS）；网关解析 JSON 体做鉴权（MCP 按 `Mcp-Method / Mcp-Name` 头路由，敏感参数不入头）。
- **D10.12 gateway 的原生持久化组合**（S1-DEC-030，2026-10-23 负责人确认 A）：完整 Linux NativeAOT 探针已暴露现有 EF Data/Audit 的动态模型与查询阻塞。新增 `NativeData` 连接/事务 Definition 与 `qian.data.native` PostgreSQL Provider，并以 `qian.audit.native` 实现现有 `Audit` 契约；gateway 采用这组原生 Provider，platform / scheduler 保持原 EF/RLS 路径。审计 Schema、规范化哈希、幂等、平台专用角色与事务语义保持一致，在线宿主不执行迁移。完整 Apollo、Redis、受限 PostgreSQL 和网关安全路径通过 Linux 原生运行前，开发模板继续 JIT，GW-001 / DEP-008 的 AOT 门不关闭。证据及拆分边界见 [实施路径](https://github.com/CavaiLAB/kun/blob/97d521b6df237b092990d7bed8586bda18aebbbc/docs/decisions/gateway-native-aot-options.md)。

## 3 理由（Rationale）

- 两个网关是不同层次：`qian.gateway` 是「外部 JWT → S2S」的业务边界，Higress 是模型 / MCP 入口；混用会让租户语义泄漏进 Higress 或让模型流量绕过平台计量（基线 K §6.4 与 `Kun.Gateway` 的关系）。
- Kun 的安全默认值必须写成内核强制项而不是沿用默认装配，这是基线 P §5 的明确结论。
- 收敛签发器是插件生态不分裂的前提；选网关签发器是因为它已有 JWKS 与短 TTL 实践，缺的只是 claim 形状（基线 K §5.1 第 7 条）。

## 4 备选方案与放弃原因（Alternatives）

- **Higress 统一所有流量与认证**：Higress 对租户无感，认证收编（Wasm）已被［下游智能体平台］明确延后；会把 S1 的身份边界交给第三方网关。放弃。
- **单层 Traefik + 服务各自鉴权**：参考平台「鉴权分散」的反例。放弃。
- **保留 `Kun.Security.S2SJwt` 作唯一签发器**（基线 P §5 的建议）：与网关 JWKS 轮换链路重复；本 ADR 采 K 的方案并吸收其 claim。
- **每服务独立网关**：三节点集群不可承受。放弃。

## 5 后果与代价（Consequences）

- 正面：匿名流量在 S1 起就被挡在业务边界外；S2S 令牌一种形状；模型流量与业务流量的计量各归其位。
- 负面：`qian.gateway` 承担动态路由后成为有状态的配置消费者，需与注册表一致性设计；签发器收敛需要 Kun 上游配合或补丁包；Cookie 会话换 S2S 的性能与会话一致性需要验证。
- 配套：JWKS 双钥轮换运维手册；路由级限流基线值；S3 动态路由的灰度与回滚。

过渡态（2026-10-06）：集群内无 cert-manager、Ingress 全 HTTP；网关宿主未接 OTel；三套签发器并存。S1 完成 D10.3–D10.4 前，Qian 不对外提供任何租户可见 API。

## 6 对各阶段的影响

- **S1**：D10.2–D10.5、D10.9、D10.10；OpenAPI 聚合与能力目录端点。
- **S2**：Redis 共享限流；低代码数据引擎 API 经网关统一鉴权；外链能力令牌经网关匿名路由 + 服务端校验。
- **S3**：动态路由 provider；按租户选 IdP 多 issuer（如需）；OPA 粗粒度 ABAC 可选。
- 【经 S1-DEC-042 D1 标注：M1 之前，确认卡的最小一圈先用替身演示，不经 MCP 路由，是过渡，M1 做好以后撤掉，见 ADR-016 §10；本行不改】**S4**：Higress 托管每租户 MCP 逻辑门面；Agent 一律经 Higress MCP 路由，令牌受众绑定、禁透传。

## 7 待确认事项

- 已裁决（R5）：保留 Kun.Gateway 的 `S2sTokenIssuer`，吸收 `scope / session_id / membership_id / project_id` claim，`Kun.Security.S2SJwt` 与 `Kun.Identity.KunJwtIssuer` 标记废弃（即 D10.4）。
- 域名 → 租户解析是否进入 S1。
- cert-manager 引入时点（TLS 进集群）。

## 8 参考

- 相关 ADR：ADR-001、ADR-003、ADR-005、ADR-006、ADR-015、ADR-016、ADR-017。
- PRD：`PRD §07`、`PRD §10`、`PRD §12`、`PRD §13`。
- 基线：K §1 Gateway / Security 行、§2.4 ④⑤⑥、§5.1 第 4/5/7 条、§6.1、§6.4、附录 B-11/B-13；P §5 主体身份行、§6 `qian.gateway` 行、§8.2 调用、§8.6；D §9.1 第 8 条。

## 9 匠心的工作区域名不经过 `qian.gateway`（2026-11-25）

产品负责人 2026-11-25 定（[S1-DEC-041](../../pm/tasks/S1-DEC-041.md) D4；平台级裁定 P64，原文在 `CavaiLAB/jiangxin` 的 [2026-11-25 平台级裁定](https://github.com/CavaiLAB/jiangxin/blob/c7b32a4dea2d1455fd2b9835e1cc0848c9d036c2/docs/decisions/2026-11-25-platform.md)）：匠心工业智能体平台的 Web 端里，Agent 工作区的域名由匠心自己的工作区服务直接接，不经过 `qian.gateway`。问的是「工作区这个域名的入口，由谁来接？」，答「工作区服务直接接」。这是对 §2 D10.1 的一处有意的偏离。工作区服务还没有实现。

是什么：

- 匠心的 Web 端分成两块，放在两个域名上（平台级裁定 P63）。管理系统是多用户的网页应用，它的域名照旧过 `qian.gateway`，D10.3 的 Cookie 会话换 S2S 照旧。
- Agent 工作区后面是托管的 DSH 实例，每个正在用的人一个。Traefik 按域名把工作区的流量直接交给工作区服务，由它认人，按登录的人找到他的实例并转发，WebSocket 一并转发。
- 工作区服务装本仓库发布的登录组件 `Kun.Identity.Client` 认人，在工作区的域名下建立自己的会话；登录仍然到身份服务。这个包的说明写明，独立的宿主只装它就能对接任意 OIDC 发行方。本仓库不用为此改代码。

偏离的范围：

- 只有工作区这一个域名上浏览器到 Agent 实例的流量。
- 三层的分工不变：Traefik 不鉴权（D10.2）；管理系统与平台的业务接口仍以 `qian.gateway` 为边界；Higress 只管模型与 MCP 流量（D10.10）。

原因：

- 现有的网关宿主接不了这条流量。2026-11-25 对着主干（提交 `2ba5eb82e`）实测并读了代码，卡在四处：
  1. 路由只按路径匹配，接不了第二个域名。
  2. 一个宿主只有一个登录回调地址，会话 Cookie 只限本主机名，第二个域名登录不了。
  3. 带着会话 Cookie 的非 GET 请求必须带防伪令牌的请求头，宿主的配置里关不掉，也不能按路由关。DSH 的客户端调用都是 POST，不带这个头，会全部被拒。这一条和域名无关，退回到一个域名、按路径分流也过不去。
  4. 长连接空闲满 100 秒被掐断，宿主里配不了。DSH 的长连接约半分钟有一次心跳，撑得住，但没有余量。
- 实测的边界：测的是照测试宿主的办法装起来的同一条网关管线，会话是预置的；没有走真实的登录，也没有经过 Traefik。
- 定位：Kun 是通用的组件，不负责任何业务（S1-DEC-041 D6）。「把登录的人送到他自己的 Agent 实例」是匠心的业务逻辑，不该写进本仓库的网关。

没有选的另一条路：先给网关宿主补上按主机名路由、多个主机名各自登录、按路由选择防伪的方式、空闲超时可配这几项通用的能力，再让工作区走它。这几项以后做 D10.7 的域名到租户解析时也要补；补完以后要不要把工作区并回来，可以再议。

代价：工作区服务成了直接面向浏览器的入口。可信代理网段、限流、会话存储要它自己装配，可以用本仓库的包。这条流量不经过 `qian.gateway` 的限流与审计。
