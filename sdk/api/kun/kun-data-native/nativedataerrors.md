> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeDataErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native](/sdk/api/kun/kun-data-native/index.md)\
Assembly: Kun.Data.Native.Abstractions.dll

不含连接凭据的原生数据错误词汇。

```csharp
public static class NativeDataErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeDataErrors](/sdk/api/kun/kun-data-native/nativedataerrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Misconfigured \{#Kun_Data_Native_NativeDataErrors_Misconfigured}

连接绑定、受限角色、表或 RLS 保护不符合要求。

```csharp
public static readonly ErrorCode Misconfigured
```

#### Field Value

ErrorCode
