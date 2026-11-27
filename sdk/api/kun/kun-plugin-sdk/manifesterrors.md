> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ManifestErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

可信组件清单构建的错误目录；不改变市场准入或运行权限。

```csharp
public static class ManifestErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ManifestErrors](/sdk/api/kun/kun-plugin-sdk/manifesterrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### ComponentMismatch \{#Kun_Plugin_Sdk_ManifestErrors_ComponentMismatch}

清单 ID 没有匹配的组件入口。

```csharp
public static readonly ErrorCode ComponentMismatch
```

#### Field Value

ErrorCode

### Field \{#Kun_Plugin_Sdk_ManifestErrors_Field}

输入错误的具体位置与原因。

```csharp
public static readonly ErrorCode Field
```

#### Field Value

ErrorCode

### HandwrittenConflict \{#Kun_Plugin_Sdk_ManifestErrors_HandwrittenConflict}

派生文件与手写文件冲突。

```csharp
public static readonly ErrorCode HandwrittenConflict
```

#### Field Value

ErrorCode

### ManifestDuplicate \{#Kun_Plugin_Sdk_ManifestErrors_ManifestDuplicate}

出现第二份组件清单。

```csharp
public static readonly ErrorCode ManifestDuplicate
```

#### Field Value

ErrorCode

### ManifestInvalid \{#Kun_Plugin_Sdk_ManifestErrors_ManifestInvalid}

组件文件或 Schema 无效。

```csharp
public static readonly ErrorCode ManifestInvalid
```

#### Field Value

ErrorCode

### MetadataInvalid \{#Kun_Plugin_Sdk_ManifestErrors_MetadataInvalid}

生成元数据重复、跨组件或不完整。

```csharp
public static readonly ErrorCode MetadataInvalid
```

#### Field Value

ErrorCode

### MetadataMissing \{#Kun_Plugin_Sdk_ManifestErrors_MetadataMissing}

程序集缺少组件注解或派生工厂。

```csharp
public static readonly ErrorCode MetadataMissing
```

#### Field Value

ErrorCode

### PathEscape \{#Kun_Plugin_Sdk_ManifestErrors_PathEscape}

路径越界或经过符号链接。

```csharp
public static readonly ErrorCode PathEscape
```

#### Field Value

ErrorCode
