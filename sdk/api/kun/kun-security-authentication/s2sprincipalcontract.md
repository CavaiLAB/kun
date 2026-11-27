> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class S2sPrincipalContract

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security.Authentication](/sdk/api/kun/kun-security-authentication/index.md)\
Assembly: Kun.Security.dll

已认证的主体快照在外部身份投影、网关签发与服务端接收之间共用的契约。

```csharp
public static class S2sPrincipalContract
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[S2sPrincipalContract](/sdk/api/kun/kun-security-authentication/s2sprincipalcontract.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Validate(Principal, DateTimeOffset) \{#Kun_Security_Authentication_S2sPrincipalContract_Validate_Kun_Security_Principal_System_DateTimeOffset\_}

核对每跳服务令牌的主体、受众、角色与时间窗。签名、密钥、发行方及目标受众绑定由认证方案核验。

```csharp
public static string? Validate(Principal principal, DateTimeOffset now)
```

#### Parameters

`principal` Principal

`now` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### ValidateIdentity(Principal) \{#Kun_Security_Authentication_S2sPrincipalContract_ValidateIdentity_Kun_Security_Principal\_}

核对身份字段，返回首个不符合契约的字段或规则名。此方法不校验令牌签名。

```csharp
public static string? ValidateIdentity(Principal principal)
```

#### Parameters

`principal` Principal

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)?
