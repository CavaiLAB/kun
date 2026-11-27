> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Data.Native.Postgres

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)

受信任宿主的具名连接绑定。凭据工厂仅在组件挂载时读取，密钥轮换后重挂载；对象打印不暴露工厂或凭据。

[NativeDataComponent](/sdk/api/kun/kun-data-native-postgres/nativedatacomponent.md)

qian.data.native 唯一入口；仅挂载于 global，提供独立 NativeData 键。

[NativeDataHosting](/sdk/api/kun/kun-data-native-postgres/nativedatahosting.md)

只登记宿主信任的绑定；组件挂载时才校验数据库并提供 NativeData。

[NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)

固定迁移契约：精确的 pg_get_expr 输出必须来自受审查的迁移真相源，不能从当前数据库学习后回填。

### Enums

[NativeDataAccess](/sdk/api/kun/kun-data-native-postgres/nativedataaccess.md)

宿主批准的连接用途，不能由请求或插件 JSON 改变。
