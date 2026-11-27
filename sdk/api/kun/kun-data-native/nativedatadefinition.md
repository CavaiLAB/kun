> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeDataDefinition

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native](/sdk/api/kun/kun-data-native/index.md)\
Assembly: Kun.Data.Native.Abstractions.dll

原生连接与事务的独立 seam，不实现或替代 EF Data。

```csharp
public static class NativeDataDefinition
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeDataDefinition](/sdk/api/kun/kun-data-native/nativedatadefinition.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Data_Native_NativeDataDefinition_Key}

NativeData 服务键。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

## Methods

### NativeData(IPluginContext) \{#Kun_Data_Native_NativeDataDefinition_NativeData_Kun_Runtime_IPluginContext\_}

解析当前可见的原生数据服务。

```csharp
public static INativeDataService NativeData(this IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

[INativeDataService](/sdk/api/kun/kun-data-native/inativedataservice.md)
