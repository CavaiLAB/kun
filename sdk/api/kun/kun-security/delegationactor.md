> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class DelegationActor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security](/sdk/api/kun/kun-security/index.md)\
Assembly: Kun.Security.Abstractions.dll

RFC 8693 委托链，保持与 v0 Actor 定义同形。

```csharp
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DelegationActor
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[DelegationActor](/sdk/api/kun/kun-security/delegationactor.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### DelegationActor() \{#Kun_Security_DelegationActor__ctor}

```csharp
public DelegationActor()
```

## Properties

### Actor \{#Kun_Security_DelegationActor_Actor}

更早的行事方。

```csharp
[JsonPropertyName("act")]
public DelegationActor? Actor { get; init; }
```

#### Property Value

[DelegationActor](/sdk/api/kun/kun-security/delegationactor.md)?

### Kind \{#Kun_Security_DelegationActor_Kind}

主体类型。

```csharp
[JsonPropertyName("principal_kind")]
public required PrincipalKind Kind { get; init; }
```

#### Property Value

[PrincipalKind](/sdk/api/kun/kun-security/principalkind.md)

### Subject \{#Kun_Security_DelegationActor_Subject}

行事主体。

```csharp
[JsonPropertyName("sub")]
public required string Subject { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### ToString() \{#Kun_Security_DelegationActor_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
