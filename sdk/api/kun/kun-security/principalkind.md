> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum PrincipalKind

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security](/sdk/api/kun/kun-security/index.md)\
Assembly: Kun.Security.Abstractions.dll

统一主体模型；Agent 在 S1 只预留，不因此开放签发。

```csharp
[JsonConverter(typeof(PrincipalKindConverter))]
public enum PrincipalKind
```

## Fields

`Agent = 2`

Agent 客户端，S4 启用。

`Service = 1`

机器服务。

`User = 0`

自然人。
