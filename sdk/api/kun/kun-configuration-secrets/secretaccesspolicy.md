> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretAccessPolicy

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

调用方的声明与审批策略，两者都必须允许访问。

```csharp
public sealed record SecretAccessPolicy : IEquatable<SecretAccessPolicy>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)

#### Implements

[IEquatable\<SecretAccessPolicy>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretAccessPolicy(string, IReadOnlyList\<SecretGrant>, IReadOnlyList\<SecretGrant>) \{#Kun_Configuration_Secrets_SecretAccessPolicy__ctor_System_String_System_Collections_Generic_IReadOnlyList_Kun_Configuration_SecretGrant__System_Collections_Generic_IReadOnlyList_Kun_Configuration_SecretGrant\_\_}

调用方的声明与审批策略，两者都必须允许访问。

```csharp
public SecretAccessPolicy(string Id, IReadOnlyList<SecretGrant> Declared, IReadOnlyList<SecretGrant> Approved)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Declared` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<SecretGrant>

`Approved` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<SecretGrant>

## Properties

### Approved \{#Kun_Configuration_Secrets_SecretAccessPolicy_Approved}

```csharp
public IReadOnlyList<SecretGrant> Approved { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<SecretGrant>

### Declared \{#Kun_Configuration_Secrets_SecretAccessPolicy_Declared}

```csharp
public IReadOnlyList<SecretGrant> Declared { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<SecretGrant>

### Id \{#Kun_Configuration_Secrets_SecretAccessPolicy_Id}

```csharp
public string Id { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Configuration_Secrets_SecretAccessPolicy__Clone\_\_}

```csharp
public SecretAccessPolicy <Clone>$()
```

#### Returns

[SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)

### Deconstruct(out string, out IReadOnlyList\<SecretGrant>, out IReadOnlyList\<SecretGrant>) \{#Kun_Configuration_Secrets_SecretAccessPolicy_Deconstruct_System_String__System_Collections_Generic_IReadOnlyList_Kun_Configuration_SecretGrant___System_Collections_Generic_IReadOnlyList_Kun_Configuration_SecretGrant\_\_\_}

```csharp
public void Deconstruct(out string Id, out IReadOnlyList<SecretGrant> Declared, out IReadOnlyList<SecretGrant> Approved)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Declared` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<SecretGrant>

`Approved` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<SecretGrant>

### Equals(object?) \{#Kun_Configuration_Secrets_SecretAccessPolicy_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(SecretAccessPolicy?) \{#Kun_Configuration_Secrets_SecretAccessPolicy_Equals_Kun_Configuration_Secrets_SecretAccessPolicy\_}

```csharp
public bool Equals(SecretAccessPolicy? other)
```

#### Parameters

`other` [SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Configuration_Secrets_SecretAccessPolicy_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Configuration_Secrets_SecretAccessPolicy_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(SecretAccessPolicy?, SecretAccessPolicy?) \{#Kun_Configuration_Secrets_SecretAccessPolicy_op_Equality_Kun_Configuration_Secrets_SecretAccessPolicy_Kun_Configuration_Secrets_SecretAccessPolicy\_}

```csharp
public static bool operator ==(SecretAccessPolicy? left, SecretAccessPolicy? right)
```

#### Parameters

`left` [SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)?

`right` [SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(SecretAccessPolicy?, SecretAccessPolicy?) \{#Kun_Configuration_Secrets_SecretAccessPolicy_op_Inequality_Kun_Configuration_Secrets_SecretAccessPolicy_Kun_Configuration_Secrets_SecretAccessPolicy\_}

```csharp
public static bool operator !=(SecretAccessPolicy? left, SecretAccessPolicy? right)
```

#### Parameters

`left` [SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)?

`right` [SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
