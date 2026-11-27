> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IFeatureFlags

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

只读租户灰度求值；不读取权益，不改变租户 scope 的组件挂载。

```csharp
public interface IFeatureFlags
```

## Methods

### IsEnabled(string, Guid) \{#Kun_Configuration_IFeatureFlags_IsEnabled_System_String_System_Guid\_}

求值指定开关。名称区分大小写，只接受小写 ASCII 名称；tenantId 必须非空。
未定义的开关返回 false；规则依次检查租户白名单、稳定百分比桶，最后使用 default。

```csharp
bool IsEnabled(string name, Guid tenantId)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`tenantId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
