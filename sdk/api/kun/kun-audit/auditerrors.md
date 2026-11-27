> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

审计组件的稳定错误码。

```csharp
public static class AuditErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditErrors](/sdk/api/kun/kun-audit/auditerrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### SchemaInvalid \{#Kun_Audit_AuditErrors_SchemaInvalid}

输入审计 Schema 无法安全解析；不回显 Schema 或输入原值。

```csharp
public static readonly ErrorCode SchemaInvalid
```

#### Field Value

ErrorCode

## Methods

### Register(ErrorCatalog) \{#Kun_Audit_AuditErrors_Register_Kun_Errors_ErrorCatalog\_}

登记本组件错误码；返回可交给 Effect 的撤销句柄。

```csharp
public static IDisposable Register(ErrorCatalog catalog)
```

#### Parameters

`catalog` ErrorCatalog

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
