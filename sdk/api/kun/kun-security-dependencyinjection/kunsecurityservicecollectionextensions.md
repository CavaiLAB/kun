> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunSecurityServiceCollectionExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security.DependencyInjection](/sdk/api/kun/kun-security-dependencyinjection/index.md)\
Assembly: Kun.Security.dll

Extension methods for registering Kun Security services with an [`IServiceCollection`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection).

```csharp
public static class KunSecurityServiceCollectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunSecurityServiceCollectionExtensions](/sdk/api/kun/kun-security-dependencyinjection/kunsecurityservicecollectionextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunSecurity(IServiceCollection) \{#Kun_Security_DependencyInjection_KunSecurityServiceCollectionExtensions_AddKunSecurity_Microsoft_Extensions_DependencyInjection_IServiceCollection\_}

Registers Kun Security services:

- [`IHttpContextAccessor`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.ihttpcontextaccessor) (if not already registered)
- Scoped [ICurrentUser](/sdk/api/kun/kun-core-security/icurrentuser.md) → [JwtCurrentUser](/sdk/api/kun/kun-security-security/jwtcurrentuser.md)

Call `AddAuthentication().AddKunServiceAuthentication(...)` separately to validate the
signature, issuer, audience and lifetime of S2S tokens; use `AddKunIdentityS2s` from
Kun.Identity.Client when the full S2S contract check is needed.

```csharp
public static IServiceCollection AddKunSecurity(this IServiceCollection services)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

The service collection.

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

The same [`IServiceCollection`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) for chaining.
