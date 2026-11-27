> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeAuditRoles

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit.Postgres](/sdk/api/kun/kun-audit-postgres/index.md)\
Assembly: Kun.Audit.Postgres.dll

离线迁移和在线核验共用的专用角色声明；角色名不是凭据。

```csharp
public sealed class NativeAuditRoles
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeAuditRoles](/sdk/api/kun/kun-audit-postgres/nativeauditroles.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### NativeAuditRoles(string, string, string?) \{#Kun_Audit_Postgres_NativeAuditRoles__ctor_System_String_System_String_System_String\_}

指定独立租户登录、NOLOGIN 函数所有者及可选的平台登录。

```csharp
public NativeAuditRoles(string tenantLogin, string functionOwner, string? platformLogin = null)
```

#### Parameters

`tenantLogin` [string](https://learn.microsoft.com/dotnet/api/system.string)

`functionOwner` [string](https://learn.microsoft.com/dotnet/api/system.string)

`platformLogin` [string](https://learn.microsoft.com/dotnet/api/system.string)?

## Properties

### FunctionOwner \{#Kun_Audit_Postgres_NativeAuditRoles_FunctionOwner}

不能登录、不能继承且没有成员关系的函数所有者。

```csharp
public string FunctionOwner { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PlatformLogin \{#Kun_Audit_Postgres_NativeAuditRoles_PlatformLogin}

显式平台登录；null 表示关闭平台路径。

```csharp
public string? PlatformLogin { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### TenantLogin \{#Kun_Audit_Postgres_NativeAuditRoles_TenantLogin}

只读表与调用租户追加函数的受限登录。

```csharp
public string TenantLogin { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
