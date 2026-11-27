> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class IdentityClaimNames

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security](/sdk/api/kun/kun-security/index.md)\
Assembly: Kun.Security.Abstractions.dll

身份 claim 的唯一词汇表；OIDC 用户令牌与冻结 S2S 令牌的字段集合分别列出。

```csharp
public static class IdentityClaimNames
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[IdentityClaimNames](/sdk/api/kun/kun-security/identityclaimnames.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Actor \{#Kun_Security_IdentityClaimNames_Actor}

委托链的预留字段，S1 不签发。

```csharp
public const string Actor = "act"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Audience \{#Kun_Security_IdentityClaimNames_Audience}

受众数组。

```csharp
public const string Audience = "aud"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### AuthenticationContext \{#Kun_Security_IdentityClaimNames_AuthenticationContext}

认证上下文等级。

```csharp
public const string AuthenticationContext = "acr"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### AuthenticationMethods \{#Kun_Security_IdentityClaimNames_AuthenticationMethods}

认证方法。

```csharp
public const string AuthenticationMethods = "amr"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ExpiresAt \{#Kun_Security_IdentityClaimNames_ExpiresAt}

过期时刻。

```csharp
public const string ExpiresAt = "exp"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### IdentityRevision \{#Kun_Security_IdentityClaimNames_IdentityRevision}

身份修订。

```csharp
public const string IdentityRevision = "identity_revision"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### IssuedAt \{#Kun_Security_IdentityClaimNames_IssuedAt}

签发时刻。

```csharp
public const string IssuedAt = "iat"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Issuer \{#Kun_Security_IdentityClaimNames_Issuer}

发行方。

```csharp
public const string Issuer = "iss"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### MembershipId \{#Kun_Security_IdentityClaimNames_MembershipId}

租户成员。

```csharp
public const string MembershipId = "membership_id"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### NotBefore \{#Kun_Security_IdentityClaimNames_NotBefore}

生效时刻。

```csharp
public const string NotBefore = "nbf"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PluginId \{#Kun_Security_IdentityClaimNames_PluginId}

服务主体的插件标识。

```csharp
public const string PluginId = "plugin_id"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PrincipalKind \{#Kun_Security_IdentityClaimNames_PrincipalKind}

统一主体类型。

```csharp
public const string PrincipalKind = "principal_kind"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Profile \{#Kun_Security_IdentityClaimNames_Profile}

身份切换上下文的预留字段，S1 不启用。

```csharp
public const string Profile = "profile"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ProjectId \{#Kun_Security_IdentityClaimNames_ProjectId}

项目透传字段。

```csharp
public const string ProjectId = "project_id"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Roles \{#Kun_Security_IdentityClaimNames_Roles}

粗粒度角色，不是权限点集合。

```csharp
public const string Roles = "roles"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Scope \{#Kun_Security_IdentityClaimNames_Scope}

空格分隔的 OAuth scope。

```csharp
public const string Scope = "scope"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### SessionId \{#Kun_Security_IdentityClaimNames_SessionId}

S2S 会话标识；机器主体等于 jti。

```csharp
public const string SessionId = "session_id"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Sid \{#Kun_Security_IdentityClaimNames_Sid}

OIDC 会话标识；换签时映射至 session_id。

```csharp
public const string Sid = "sid"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Subject \{#Kun_Security_IdentityClaimNames_Subject}

用户、服务或 Agent 主体。

```csharp
public const string Subject = "sub"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantId \{#Kun_Security_IdentityClaimNames_TenantId}

租户，平台主体为 null。

```csharp
public const string TenantId = "tenant_id"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TokenId \{#Kun_Security_IdentityClaimNames_TokenId}

令牌标识。

```csharp
public const string TokenId = "jti"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### IdentityToken \{#Kun_Security_IdentityClaimNames_IdentityToken}

用户身份令牌词汇；其中 profile 与 act 是未启用的预留位置。

```csharp
public static IReadOnlyList<string> IdentityToken { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### S2sOptional \{#Kun_Security_IdentityClaimNames_S2sOptional}

冻结 S2S 契约允许的附加字段，不能加入 sid、amr、acr 或 profile。

```csharp
public static IReadOnlyList<string> S2sOptional { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### S2sRequired \{#Kun_Security_IdentityClaimNames_S2sRequired}

与 contracts/v0/s2s-claims.schema.json 的 required 精确一致。

```csharp
public static IReadOnlyList<string> S2sRequired { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>
