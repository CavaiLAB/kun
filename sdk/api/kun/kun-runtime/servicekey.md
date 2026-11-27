> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Struct ServiceKey

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

Definition 拥有的服务键；具名注册可用 Key:name 声明就绪依赖。

```csharp
public readonly struct ServiceKey : IEquatable<ServiceKey>
```

#### Implements

[IEquatable\<ServiceKey>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ServiceKey(string) \{#Kun_Runtime_ServiceKey__ctor_System_String\_}

Definition 拥有的服务键；具名注册可用 Key:name 声明就绪依赖。

```csharp
public ServiceKey(string Name)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Name \{#Kun_Runtime_ServiceKey_Name}

```csharp
public string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Deconstruct(out string) \{#Kun_Runtime_ServiceKey_Deconstruct_System_String\_\_}

```csharp
public void Deconstruct(out string Name)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object) \{#Kun_Runtime_ServiceKey_Equals_System_Object\_}

```csharp
public override bool Equals(object obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(ServiceKey) \{#Kun_Runtime_ServiceKey_Equals_Kun_Runtime_ServiceKey\_}

```csharp
public bool Equals(ServiceKey other)
```

#### Parameters

`other` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Runtime_ServiceKey_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Runtime_ServiceKey_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(ServiceKey, ServiceKey) \{#Kun_Runtime_ServiceKey_op_Equality_Kun_Runtime_ServiceKey_Kun_Runtime_ServiceKey\_}

```csharp
public static bool operator ==(ServiceKey left, ServiceKey right)
```

#### Parameters

`left` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`right` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(ServiceKey, ServiceKey) \{#Kun_Runtime_ServiceKey_op_Inequality_Kun_Runtime_ServiceKey_Kun_Runtime_ServiceKey\_}

```csharp
public static bool operator !=(ServiceKey left, ServiceKey right)
```

#### Parameters

`left` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`right` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
