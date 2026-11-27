> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PluginDefinition

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

装载输入；Provides 来自清单，不通过试运行 Apply 推测。

```csharp
public sealed record PluginDefinition : IEquatable<PluginDefinition>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)

#### Implements

[IEquatable\<PluginDefinition>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### PluginDefinition(PluginEntry, IKunPlugin, IReadOnlyList\<ServiceKey>) \{#Kun_Runtime_PluginDefinition__ctor_Kun_Runtime_PluginEntry_Kun_Runtime_IKunPlugin_System_Collections_Generic_IReadOnlyList_Kun_Runtime_ServiceKey\_\_}

装载输入；Provides 来自清单，不通过试运行 Apply 推测。

```csharp
public PluginDefinition(PluginEntry Entry, IKunPlugin Plugin, IReadOnlyList<ServiceKey> Provides)
```

#### Parameters

`Entry` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

`Plugin` [IKunPlugin](/sdk/api/kun/kun-runtime/ikunplugin.md)

`Provides` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)>

## Properties

### Entry \{#Kun_Runtime_PluginDefinition_Entry}

```csharp
public PluginEntry Entry { get; init; }
```

#### Property Value

[PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

### Plugin \{#Kun_Runtime_PluginDefinition_Plugin}

```csharp
public IKunPlugin Plugin { get; init; }
```

#### Property Value

[IKunPlugin](/sdk/api/kun/kun-runtime/ikunplugin.md)

### Provides \{#Kun_Runtime_PluginDefinition_Provides}

```csharp
public IReadOnlyList<ServiceKey> Provides { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)>

## Methods

### \<Clone>$() \{#Kun_Runtime_PluginDefinition__Clone\_\_}

```csharp
public PluginDefinition <Clone>$()
```

#### Returns

[PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)

### Deconstruct(out PluginEntry, out IKunPlugin, out IReadOnlyList\<ServiceKey>) \{#Kun_Runtime_PluginDefinition_Deconstruct_Kun_Runtime_PluginEntry__Kun_Runtime_IKunPlugin__System_Collections_Generic_IReadOnlyList_Kun_Runtime_ServiceKey\_\_\_}

```csharp
public void Deconstruct(out PluginEntry Entry, out IKunPlugin Plugin, out IReadOnlyList<ServiceKey> Provides)
```

#### Parameters

`Entry` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

`Plugin` [IKunPlugin](/sdk/api/kun/kun-runtime/ikunplugin.md)

`Provides` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)>

### Equals(object?) \{#Kun_Runtime_PluginDefinition_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(PluginDefinition?) \{#Kun_Runtime_PluginDefinition_Equals_Kun_Runtime_PluginDefinition\_}

```csharp
public bool Equals(PluginDefinition? other)
```

#### Parameters

`other` [PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Runtime_PluginDefinition_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Runtime_PluginDefinition_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(PluginDefinition?, PluginDefinition?) \{#Kun_Runtime_PluginDefinition_op_Equality_Kun_Runtime_PluginDefinition_Kun_Runtime_PluginDefinition\_}

```csharp
public static bool operator ==(PluginDefinition? left, PluginDefinition? right)
```

#### Parameters

`left` [PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)?

`right` [PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(PluginDefinition?, PluginDefinition?) \{#Kun_Runtime_PluginDefinition_op_Inequality_Kun_Runtime_PluginDefinition_Kun_Runtime_PluginDefinition\_}

```csharp
public static bool operator !=(PluginDefinition? left, PluginDefinition? right)
```

#### Parameters

`left` [PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)?

`right` [PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
