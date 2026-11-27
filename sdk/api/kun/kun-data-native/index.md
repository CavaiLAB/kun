> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Data.Native

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Data.Native.Postgres](/sdk/api/kun/kun-data-native-postgres/index.md)

### Classes

[NativeDataDefinition](/sdk/api/kun/kun-data-native/nativedatadefinition.md)

原生连接与事务的独立 seam，不实现或替代 EF Data。

[NativeDataErrors](/sdk/api/kun/kun-data-native/nativedataerrors.md)

不含连接凭据的原生数据错误词汇。

### Interfaces

[INativeDataService](/sdk/api/kun/kun-data-native/inativedataservice.md)

仅访问受信任宿主登记的连接绑定；不接受连接串或任意角色名。

[INativeDataSession](/sdk/api/kun/kun-data-native/inativedatasession.md)

拥有连接与事务的会话。命令只能来自可信组件；释放回滚未完成事务、清理会话并释放连接，组件卸载同样释放活动会话。
