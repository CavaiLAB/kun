> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Audit

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Audit.Postgres](/sdk/api/kun/kun-audit-postgres/index.md)

### Classes

[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)

持久化的链节点；序号和前序都参与哈希验证。

[AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)

全链验证结果；BrokenAt 指出最早不一致的位置，不回显敏感记录。

[AuditDefinition](/sdk/api/kun/kun-audit/auditdefinition.md)

组件间的审计 seam，不是客户端可直接调用的审计写入 HTTP API。

[AuditErrors](/sdk/api/kun/kun-audit/auditerrors.md)

审计组件的稳定错误码。

[AuditInputProjection](/sdk/api/kun/kun-audit/auditinputprojection.md)

由可信能力契约构造的输入审计策略；元数据不能来自调用请求。脱敏后才计算输入哈希。

[AuditProtocol](/sdk/api/kun/kun-audit/auditprotocol.md)

审计 v1 的确定性 JSON：键按 ordinal 排序，精确十进制数规整，数组顺序保留，重复键拒绝。

[AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

进入审计链的不可变记录；输入只保留脱敏后的哈希，Payload 须由可信调用方按审计契约构造。

[NativeAuditErrors](/sdk/api/kun/kun-audit/nativeauditerrors.md)

原生审计 Provider 的安全装配错误，不包含连接凭据。

### Interfaces

[IAuditReader](/sdk/api/kun/kun-audit/iauditreader.md)

组件内读取端口；面向用户的查询必须由宿主另行鉴权并审计。

[IAuditService](/sdk/api/kun/kun-audit/iauditservice.md)

Audit seam 的读写服务。

[IAuditWriter](/sdk/api/kun/kun-audit/iauditwriter.md)

可信组件生成记录后的追加端口；独立追加不替代业务同事务写入。
