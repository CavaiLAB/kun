> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ApolloConfigurationDescription

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Apollo` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Apollo`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Apollo)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Apollo](/sdk/api/kun/kun-configuration-apollo/index.md)\
Assembly: Kun.Configuration.Apollo.dll

已装配的 Apollo 配置源与其脱敏的发布记录。

```csharp
public sealed class ApolloConfigurationDescription
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ApolloConfigurationDescription](/sdk/api/kun/kun-configuration-apollo/apolloconfigurationdescription.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### AppId \{#Kun_Configuration_Apollo_ApolloConfigurationDescription_AppId}

应用标识。

```csharp
public string AppId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Environment \{#Kun_Configuration_Apollo_ApolloConfigurationDescription_Environment}

引导阶段选定的环境。

```csharp
public string Environment { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Namespaces \{#Kun_Configuration_Apollo_ApolloConfigurationDescription_Namespaces}

按覆盖顺序排列的命名空间。

```csharp
public IReadOnlyList<string> Namespaces { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### Servers \{#Kun_Configuration_Apollo_ApolloConfigurationDescription_Servers}

按引导顺序排列的配置服务器。

```csharp
public IReadOnlyList<Uri> Servers { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<Uri>

## Methods

### From(IConfigurationProvider) \{#Kun_Configuration_Apollo_ApolloConfigurationDescription_From_Microsoft_Extensions_Configuration_IConfigurationProvider\_}

返回指定提供程序的来源信息；其他配置源返回空。

```csharp
public static ApolloConfigurationDescription? From(IConfigurationProvider provider)
```

#### Parameters

`provider` [IConfigurationProvider](https://learn.microsoft.com/dotnet/api/microsoft.extensions.configuration.iconfigurationprovider)

#### Returns

[ApolloConfigurationDescription](/sdk/api/kun/kun-configuration-apollo/apolloconfigurationdescription.md)?

### ReadReleaseAsync(CancellationToken) \{#Kun_Configuration_Apollo_ApolloConfigurationDescription_ReadReleaseAsync_System_Threading_CancellationToken\_}

从本实例的有界队列消费下一条发布记录，宿主为每个源指定一个读取方。

```csharp
public ValueTask<ApolloReleaseRecord> ReadReleaseAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[ApolloReleaseRecord](/sdk/api/kun/kun-configuration-apollo/apolloreleaserecord.md)>
