> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum NativeDataAccess

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native.Postgres](/sdk/api/kun/kun-data-native-postgres/index.md)\
Assembly: Kun.Data.Native.Postgres.dll

宿主批准的连接用途，不能由请求或插件 JSON 改变。

```csharp
public enum NativeDataAccess
```

## Fields

`Platform = 1`

专用平台登录，只允许有效 global 调用方且没有租户。

`Tenant = 0`

必须具有有效租户上下文。
