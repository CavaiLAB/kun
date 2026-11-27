> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum TenantIsolation

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Core` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Core`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Core)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Core.MultiTenancy](/sdk/api/kun/kun-core-multitenancy/index.md)\
Assembly: Kun.Core.dll

多租户数据隔离策略。数值用于宿主配置，小写 JSON 值用于租户契约。

```csharp
[JsonConverter(typeof(JsonStringEnumConverter<TenantIsolation>))]
public enum TenantIsolation
```

## Fields

`Database = 2`

每个租户使用单独的数据库。

`Row = 0`

共享数据库与 schema，以租户列隔离数据。

`Schema = 1`

每个租户使用单独的 schema。
