> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum LocalTransferWriteResult

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Storage.Local` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Storage.Local`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Storage.Local)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Storage.Local](/sdk/api/kun/kun-storage-local/index.md)\
Assembly: Kun.Storage.Local.dll

直传结果，宿主映射为 HTTP 状态码。

```csharp
public enum LocalTransferWriteResult
```

## Fields

`Conflict = 2`

对象已存在，不覆盖（409）。

`Rejected = 1`

签名不符、已过期或缺少必需参数（404，不区分原因）。

`Stored = 0`

已写入（201）。

`TooLarge = 3`

请求体超过签名里的上限（413）。
