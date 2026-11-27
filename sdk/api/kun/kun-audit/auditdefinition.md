> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditDefinition

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

组件间的审计 seam，不是客户端可直接调用的审计写入 HTTP API。

```csharp
public static class AuditDefinition
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditDefinition](/sdk/api/kun/kun-audit/auditdefinition.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Audit_AuditDefinition_Key}

当前 scope 可见的审计服务键。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

## Methods

### Audit(IPluginContext) \{#Kun_Audit_AuditDefinition_Audit_Kun_Runtime_IPluginContext\_}

解析当前插件可见的审计服务。

```csharp
public static IAuditService Audit(this IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

[IAuditService](/sdk/api/kun/kun-audit/iauditservice.md)
