> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeDataHosting

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native.Postgres](/sdk/api/kun/kun-data-native-postgres/index.md)\
Assembly: Kun.Data.Native.Postgres.dll

只登记宿主信任的绑定；组件挂载时才校验数据库并提供 NativeData。

```csharp
public static class NativeDataHosting
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeDataHosting](/sdk/api/kun/kun-data-native-postgres/nativedatahosting.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunNativeData(IServiceCollection, NativeDataBinding) \{#Kun_Data_Native_Postgres_NativeDataHosting_AddKunNativeData_Microsoft_Extensions_DependencyInjection_IServiceCollection_Kun_Data_Native_Postgres_NativeDataBinding\_}

登记一个绑定；重复名字、缺表或非保护表达式拒绝，角色与数据库的真实安全属性在挂载时验证。

```csharp
public static IServiceCollection AddKunNativeData(this IServiceCollection services, NativeDataBinding binding)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

`binding` [NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
