> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunResults

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors.AspNetCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors.AspNetCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors.AspNetCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors.AspNetCore](/sdk/api/kun/kun-errors-aspnetcore/index.md)\
Assembly: Kun.Errors.AspNetCore.dll

端点返回错误的结果工厂。

```csharp
public static class KunResults
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunResults](/sdk/api/kun/kun-errors-aspnetcore/kunresults.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Error(Error) \{#Kun_Errors_AspNetCore_KunResults_Error_Kun_Errors_Error\_}

把一次出错写成 problem+json 的 ErrorDetail（状态码取错误码的状态码）。

```csharp
public static KunErrorResult Error(Error error)
```

#### Parameters

`error` Error

#### Returns

[KunErrorResult](/sdk/api/kun/kun-errors-aspnetcore/kunerrorresult.md)
