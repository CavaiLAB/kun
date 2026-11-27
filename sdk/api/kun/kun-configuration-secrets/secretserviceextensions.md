> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretServiceExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

独立宿主与平台共用的安全解析装配。

```csharp
public static class SecretServiceExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretServiceExtensions](/sdk/api/kun/kun-configuration-secrets/secretserviceextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunSecrets(IServiceCollection, SecretResolverOptions) \{#Kun_Configuration_Secrets_SecretServiceExtensions_AddKunSecrets_Microsoft_Extensions_DependencyInjection_IServiceCollection_Kun_Configuration_Secrets_SecretResolverOptions\_}

登记受控文件存储与不可变权限；宿主必须显式提供 ISecretAccessObserver。

```csharp
public static IServiceCollection AddKunSecrets(this IServiceCollection services, SecretResolverOptions options)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

`options` [SecretResolverOptions](/sdk/api/kun/kun-configuration-secrets/secretresolveroptions.md)

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
