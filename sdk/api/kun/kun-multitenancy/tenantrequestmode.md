> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum TenantRequestMode

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.MultiTenancy](/sdk/api/kun/kun-multitenancy/index.md)\
Assembly: Kun.MultiTenancy.dll

请求入口允许的主体范围。

```csharp
public enum TenantRequestMode
```

## Fields

`PlatformPrincipalAllowed = 1`

同时允许已认证的平台身份；匿名请求交给宿主的授权策略处理。

`TenantRequired = 0`

请求需要一个已认证的租户身份。
