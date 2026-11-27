> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuthentikFixture.OAuthClient

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

为一个依赖方开出的 OAuth2 客户端。

```csharp
public sealed record AuthentikFixture.OAuthClient : IEquatable<AuthentikFixture.OAuthClient>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuthentikFixture.OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)

#### Implements

[IEquatable\<AuthentikFixture.OAuthClient>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### OAuthClient(string, string, string) \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient__ctor_System_String_System_String_System_String\_}

为一个依赖方开出的 OAuth2 客户端。

```csharp
public OAuthClient(string Issuer, string ClientId, string ClientSecret)
```

#### Parameters

`Issuer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ClientId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ClientSecret` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### ClientId \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_ClientId}

```csharp
public string ClientId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ClientSecret \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_ClientSecret}

```csharp
public string ClientSecret { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Issuer \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_Issuer}

```csharp
public string Issuer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient__Clone\_\_}

```csharp
public AuthentikFixture.OAuthClient <Clone>$()
```

#### Returns

[AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)

### Deconstruct(out string, out string, out string) \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_Deconstruct_System_String__System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Issuer, out string ClientId, out string ClientSecret)
```

#### Parameters

`Issuer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ClientId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ClientSecret` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(OAuthClient?) \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_Equals_Kun_Testing_Containers_AuthentikFixture_OAuthClient\_}

```csharp
public bool Equals(AuthentikFixture.OAuthClient? other)
```

#### Parameters

`other` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(OAuthClient?, OAuthClient?) \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_op_Equality_Kun_Testing_Containers_AuthentikFixture_OAuthClient_Kun_Testing_Containers_AuthentikFixture_OAuthClient\_}

```csharp
public static bool operator ==(AuthentikFixture.OAuthClient? left, AuthentikFixture.OAuthClient? right)
```

#### Parameters

`left` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)?

`right` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(OAuthClient?, OAuthClient?) \{#Kun_Testing_Containers_AuthentikFixture_OAuthClient_op_Inequality_Kun_Testing_Containers_AuthentikFixture_OAuthClient_Kun_Testing_Containers_AuthentikFixture_OAuthClient\_}

```csharp
public static bool operator !=(AuthentikFixture.OAuthClient? left, AuthentikFixture.OAuthClient? right)
```

#### Parameters

`left` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)?

`right` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
