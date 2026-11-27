> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class Principal

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security](/sdk/api/kun/kun-security/index.md)\
Assembly: Kun.Security.Abstractions.dll

冻结 S2S payload 的 DTO，不承担 JWT 验签、发行或权限判定。

```csharp
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class Principal
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Principal](/sdk/api/kun/kun-security/principal.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### Principal() \{#Kun_Security_Principal__ctor}

```csharp
public Principal()
```

## Properties

### Actor \{#Kun_Security_Principal_Actor}

预留委托链。

```csharp
[JsonPropertyName("act")]
public DelegationActor? Actor { get; init; }
```

#### Property Value

[DelegationActor](/sdk/api/kun/kun-security/delegationactor.md)?

### Audience \{#Kun_Security_Principal_Audience}

服务受众。

```csharp
[JsonPropertyName("aud")]
public required IReadOnlyList<string> Audience { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### ExpiresAt \{#Kun_Security_Principal_ExpiresAt}

NumericDate，签发方须保证有效期不超过 300 秒。

```csharp
[JsonPropertyName("exp")]
public required long ExpiresAt { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

### IdentityRevision \{#Kun_Security_Principal_IdentityRevision}

用户身份修订。

```csharp
[JsonPropertyName("identity_revision")]
public long? IdentityRevision { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)?

### IssuedAt \{#Kun_Security_Principal_IssuedAt}

NumericDate。

```csharp
[JsonPropertyName("iat")]
public required long IssuedAt { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

### Issuer \{#Kun_Security_Principal_Issuer}

签发方。

```csharp
[JsonPropertyName("iss")]
public required string Issuer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Kind \{#Kun_Security_Principal_Kind}

主体类型。

```csharp
[JsonPropertyName("principal_kind")]
public required PrincipalKind Kind { get; init; }
```

#### Property Value

[PrincipalKind](/sdk/api/kun/kun-security/principalkind.md)

### MembershipId \{#Kun_Security_Principal_MembershipId}

用户租户成员；机器主体不得携带。

```csharp
[JsonPropertyName("membership_id")]
public string? MembershipId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### NotBefore \{#Kun_Security_Principal_NotBefore}

NumericDate。

```csharp
[JsonPropertyName("nbf")]
public required long NotBefore { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

### PluginId \{#Kun_Security_Principal_PluginId}

可选插件主体。

```csharp
[JsonPropertyName("plugin_id")]
public string? PluginId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### ProjectId \{#Kun_Security_Principal_ProjectId}

可选项目。

```csharp
[JsonPropertyName("project_id")]
public string? ProjectId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Roles \{#Kun_Security_Principal_Roles}

粗粒度角色。

```csharp
[JsonPropertyName("roles")]
public required IReadOnlyList<string> Roles { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### Scope \{#Kun_Security_Principal_Scope}

空格分隔 scope。

```csharp
[JsonPropertyName("scope")]
public required string Scope { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### SessionId \{#Kun_Security_Principal_SessionId}

会话 UUID；机器主体使用 jti。

```csharp
[JsonPropertyName("session_id")]
public required string SessionId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Subject \{#Kun_Security_Principal_Subject}

person/client 标识。

```csharp
[JsonPropertyName("sub")]
public required string Subject { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantId \{#Kun_Security_Principal_TenantId}

租户 UUID 字符串；平台主体必须显式输出 null。

```csharp
[JsonPropertyName("tenant_id")]
[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
public required string? TenantId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### TokenId \{#Kun_Security_Principal_TokenId}

令牌 UUID。

```csharp
[JsonPropertyName("jti")]
public required string TokenId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### ToString() \{#Kun_Security_Principal_ToString}

避免默认字符串化把 session_id、jti 或委托链写入日志。

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
