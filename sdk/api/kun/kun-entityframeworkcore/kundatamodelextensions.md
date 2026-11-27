> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunDataModelExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

安装不可由派生上下文遗漏的 Kun 模型约定。

```csharp
public static class KunDataModelExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunDataModelExtensions](/sdk/api/kun/kun-entityframeworkcore/kundatamodelextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### RequireKunSchemaPrefix(DbContextOptionsBuilder, string) \{#Kun_EntityFrameworkCore_KunDataModelExtensions_RequireKunSchemaPrefix_Microsoft_EntityFrameworkCore_DbContextOptionsBuilder_System_String\_}

要求 `UseKunSchema` 指定的 schema 以 `prefix` 开头，前缀之后仍须小写字母开头、只含小写字母、数字和下划线，总长不超过 63 个字符。
不调用时 [KunStrictDbContext](/sdk/api/kun/kun-entityframeworkcore/kunstrictdbcontext.md) 只要求合法的小写标识符。前缀设定后不能改成别的，再次设定不同的前缀即拒绝。

```csharp
public static DbContextOptionsBuilder RequireKunSchemaPrefix(this DbContextOptionsBuilder options, string prefix)
```

#### Parameters

`options` [DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)

数据上下文的配置。

`prefix` [string](https://learn.microsoft.com/dotnet/api/system.string)

小写字母开头、只含小写字母、数字和下划线的前缀，最长 62 个字符。

#### Returns

[DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)

### UseKunDataModel(DbContextOptionsBuilder) \{#Kun_EntityFrameworkCore_KunDataModelExtensions_UseKunDataModel_Microsoft_EntityFrameworkCore_DbContextOptionsBuilder\_}

在业务映射之后应用 Kun 数据约定与 schema 安全检查。

```csharp
public static DbContextOptionsBuilder UseKunDataModel(this DbContextOptionsBuilder options)
```

#### Parameters

`options` [DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)

#### Returns

[DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)
