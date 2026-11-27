> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PrincipalClaims

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security.Authentication](/sdk/api/kun/kun-security-authentication/index.md)\
Assembly: Kun.Security.dll

从已认证的主身份读取授权主体，不拼接不同身份的 claim。

```csharp
public static class PrincipalClaims
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PrincipalClaims](/sdk/api/kun/kun-security-authentication/principalclaims.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### ReadS2s(ClaimsPrincipal?, JsonElement, DateTimeOffset, out Principal?) \{#Kun_Security_Authentication_PrincipalClaims_ReadS2s_System_Security_Claims_ClaimsPrincipal_System_Text_Json_JsonElement_System_DateTimeOffset_Kun_Security_Principal\_\_}

从认证方案已验证的 JWT 载荷与对应的主身份读取 S2S 快照。仅返回符合冻结契约且与该身份一致的快照。
调用方负责验签、核验目标发行方和受众、校验有效期。返回值为错误类别，成功时为 null。

```csharp
public static string? ReadS2s(ClaimsPrincipal? identity, JsonElement payload, DateTimeOffset now, out Principal? snapshot)
```

#### Parameters

`identity` [ClaimsPrincipal](https://learn.microsoft.com/dotnet/api/system.security.claims.claimsprincipal)?

`payload` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`now` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

`snapshot` Principal?

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### TryRead(ClaimsPrincipal?, out string?, out string?, out string?) \{#Kun_Security_Authentication_PrincipalClaims_TryRead_System_Security_Claims_ClaimsPrincipal_System_String__System_String__System_String\_\_}

读取 sub/NameIdentifier、tenant_id 与 principal_kind；缺少类型的旧浏览器身份按 user，重复或冲突字段拒绝。

```csharp
public static bool TryRead(ClaimsPrincipal? principal, out string? subjectId, out string? tenantId, out string? principalKind)
```

#### Parameters

`principal` [ClaimsPrincipal](https://learn.microsoft.com/dotnet/api/system.security.claims.claimsprincipal)?

`subjectId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`tenantId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`principalKind` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
