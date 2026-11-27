> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretReference

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

公开的规范引用；Kubernetes 定位只在受信任存储绑定中使用。

```csharp
public sealed class SecretReference
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretReference](/sdk/api/kun/kun-configuration/secretreference.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### Path \{#Kun_Configuration_SecretReference_Path}

规范化的引用路径，不是文件系统路径。

```csharp
public string Path { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Scope \{#Kun_Configuration_SecretReference_Scope}

platform、tenant 或 dev。

```csharp
public string Scope { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### IsValid(string?) \{#Kun_Configuration_SecretReference_IsValid_System_String\_}

格式检查，不输出无效输入。

```csharp
public static bool IsValid(string? value)
```

#### Parameters

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Parse(string) \{#Kun_Configuration_SecretReference_Parse_System_String\_}

解析引用；拒绝内嵌凭据、查询、fragment、空段和穿越。

```csharp
public static SecretReference Parse(string value)
```

#### Parameters

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[SecretReference](/sdk/api/kun/kun-configuration/secretreference.md)

### ToString() \{#Kun_Configuration_SecretReference_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
