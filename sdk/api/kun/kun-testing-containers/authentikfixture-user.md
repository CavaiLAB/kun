> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuthentikFixture.User

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

Authentik 里的一个本地用户。

```csharp
public sealed record AuthentikFixture.User : IEquatable<AuthentikFixture.User>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuthentikFixture.User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)

#### Implements

[IEquatable\<AuthentikFixture.User>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### User(string, string, string) \{#Kun_Testing_Containers_AuthentikFixture_User__ctor_System_String_System_String_System_String\_}

Authentik 里的一个本地用户。

```csharp
public User(string Username, string Password, string Email)
```

#### Parameters

`Username` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Password` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Email` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Email \{#Kun_Testing_Containers_AuthentikFixture_User_Email}

```csharp
public string Email { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Password \{#Kun_Testing_Containers_AuthentikFixture_User_Password}

```csharp
public string Password { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Username \{#Kun_Testing_Containers_AuthentikFixture_User_Username}

```csharp
public string Username { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Testing_Containers_AuthentikFixture_User__Clone\_\_}

```csharp
public AuthentikFixture.User <Clone>$()
```

#### Returns

[AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)

### Deconstruct(out string, out string, out string) \{#Kun_Testing_Containers_AuthentikFixture_User_Deconstruct_System_String__System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Username, out string Password, out string Email)
```

#### Parameters

`Username` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Password` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Email` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Testing_Containers_AuthentikFixture_User_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(User?) \{#Kun_Testing_Containers_AuthentikFixture_User_Equals_Kun_Testing_Containers_AuthentikFixture_User\_}

```csharp
public bool Equals(AuthentikFixture.User? other)
```

#### Parameters

`other` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Testing_Containers_AuthentikFixture_User_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Testing_Containers_AuthentikFixture_User_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(User?, User?) \{#Kun_Testing_Containers_AuthentikFixture_User_op_Equality_Kun_Testing_Containers_AuthentikFixture_User_Kun_Testing_Containers_AuthentikFixture_User\_}

```csharp
public static bool operator ==(AuthentikFixture.User? left, AuthentikFixture.User? right)
```

#### Parameters

`left` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)?

`right` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(User?, User?) \{#Kun_Testing_Containers_AuthentikFixture_User_op_Inequality_Kun_Testing_Containers_AuthentikFixture_User_Kun_Testing_Containers_AuthentikFixture_User\_}

```csharp
public static bool operator !=(AuthentikFixture.User? left, AuthentikFixture.User? right)
```

#### Parameters

`left` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)?

`right` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
