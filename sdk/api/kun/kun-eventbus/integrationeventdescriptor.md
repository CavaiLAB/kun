> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class IntegrationEventDescriptor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

事件目录中的一项：稳定 ID、subject、payload 版本与由 JsonTypeInfo 导出的 payload Schema。

```csharp
public sealed record IntegrationEventDescriptor : IEquatable<IntegrationEventDescriptor>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)

#### Implements

[IEquatable\<IntegrationEventDescriptor>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### IntegrationEventDescriptor(string, string, int, JsonElement) \{#Kun_EventBus_IntegrationEventDescriptor__ctor_System_String_System_String_System_Int32_System_Text_Json_JsonElement\_}

事件目录中的一项：稳定 ID、subject、payload 版本与由 JsonTypeInfo 导出的 payload Schema。

```csharp
public IntegrationEventDescriptor(string Type, string Subject, int Version, JsonElement PayloadSchema)
```

#### Parameters

`Type` [string](https://learn.microsoft.com/dotnet/api/system.string)

稳定事件 ID。

`Subject` [string](https://learn.microsoft.com/dotnet/api/system.string)

信封 subject，也是路由键。

`Version` [int](https://learn.microsoft.com/dotnet/api/system.int32)

payload 版本，即事件 ID 末尾的 N。

`PayloadSchema` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

payload 的 JSON Schema（2020-12）。

## Properties

### PayloadSchema \{#Kun_EventBus_IntegrationEventDescriptor_PayloadSchema}

payload 的 JSON Schema（2020-12）。

```csharp
public JsonElement PayloadSchema { get; init; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Subject \{#Kun_EventBus_IntegrationEventDescriptor_Subject}

信封 subject，也是路由键。

```csharp
public string Subject { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Type \{#Kun_EventBus_IntegrationEventDescriptor_Type}

稳定事件 ID。

```csharp
public string Type { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Version \{#Kun_EventBus_IntegrationEventDescriptor_Version}

payload 版本，即事件 ID 末尾的 N。

```csharp
public int Version { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### \<Clone>$() \{#Kun_EventBus_IntegrationEventDescriptor__Clone\_\_}

```csharp
public IntegrationEventDescriptor <Clone>$()
```

#### Returns

[IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)

### Deconstruct(out string, out string, out int, out JsonElement) \{#Kun_EventBus_IntegrationEventDescriptor_Deconstruct_System_String__System_String__System_Int32__System_Text_Json_JsonElement\_\_}

```csharp
public void Deconstruct(out string Type, out string Subject, out int Version, out JsonElement PayloadSchema)
```

#### Parameters

`Type` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Subject` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Version` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`PayloadSchema` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Equals(object?) \{#Kun_EventBus_IntegrationEventDescriptor_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(IntegrationEventDescriptor?) \{#Kun_EventBus_IntegrationEventDescriptor_Equals_Kun_EventBus_IntegrationEventDescriptor\_}

```csharp
public bool Equals(IntegrationEventDescriptor? other)
```

#### Parameters

`other` [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_EventBus_IntegrationEventDescriptor_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_EventBus_IntegrationEventDescriptor_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(IntegrationEventDescriptor?, IntegrationEventDescriptor?) \{#Kun_EventBus_IntegrationEventDescriptor_op_Equality_Kun_EventBus_IntegrationEventDescriptor_Kun_EventBus_IntegrationEventDescriptor\_}

```csharp
public static bool operator ==(IntegrationEventDescriptor? left, IntegrationEventDescriptor? right)
```

#### Parameters

`left` [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)?

`right` [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(IntegrationEventDescriptor?, IntegrationEventDescriptor?) \{#Kun_EventBus_IntegrationEventDescriptor_op_Inequality_Kun_EventBus_IntegrationEventDescriptor_Kun_EventBus_IntegrationEventDescriptor\_}

```csharp
public static bool operator !=(IntegrationEventDescriptor? left, IntegrationEventDescriptor? right)
```

#### Parameters

`left` [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)?

`right` [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
