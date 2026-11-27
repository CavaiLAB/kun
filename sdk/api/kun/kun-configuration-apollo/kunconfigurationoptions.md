> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunConfigurationOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Apollo` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Apollo`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Apollo)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Apollo](/sdk/api/kun/kun-configuration-apollo/index.md)\
Assembly: Kun.Configuration.Apollo.dll

配置装配参数；引导值来自宿主源，Apollo 不能改变自己的访问地址或访问密钥引用。

```csharp
public sealed class KunConfigurationOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunConfigurationOptions](/sdk/api/kun/kun-configuration-apollo/kunconfigurationoptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunConfigurationOptions() \{#Kun_Configuration_Apollo_KunConfigurationOptions__ctor}

```csharp
public KunConfigurationOptions()
```

## Properties

### ApolloConfigurationSchema \{#Kun_Configuration_Apollo_KunConfigurationOptions_ApolloConfigurationSchema}

合成配置 Schema；敏感字段扫描始终启用，自定义校验只能追加约束。

```csharp
public JsonElement? ApolloConfigurationSchema { get; init; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)?

### Arguments \{#Kun_Configuration_Apollo_KunConfigurationOptions_Arguments}

命令行覆盖项，优先级最高。

```csharp
public IReadOnlyList<string> Arguments { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### BasePath \{#Kun_Configuration_Apollo_KunConfigurationOptions_BasePath}

appsettings 文件与相对缓存路径的根目录。

```csharp
public required string BasePath { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### EnvironmentName \{#Kun_Configuration_Apollo_KunConfigurationOptions_EnvironmentName}

可选 appsettings.\{EnvironmentName}.json；不等同于 Apollo UAT/PRO 环境。

```csharp
public string? EnvironmentName { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### EnvironmentPrefix \{#Kun_Configuration_Apollo_KunConfigurationOptions_EnvironmentPrefix}

可选环境变量前缀，默认无前缀。

```csharp
public string? EnvironmentPrefix { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### HttpClient \{#Kun_Configuration_Apollo_KunConfigurationOptions_HttpClient}

可选宿主管理的客户端，超时必须大于 60 秒且不能向不可信目标重定向。

```csharp
public HttpClient? HttpClient { get; init; }
```

#### Property Value

[HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)?

### Logger \{#Kun_Configuration_Apollo_KunConfigurationOptions_Logger}

可选日志器；底层失败日志不输出配置或凭据。

```csharp
public ILogger? Logger { get; init; }
```

#### Property Value

[ILogger](https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger)?

### ResolveAccessKey \{#Kun_Configuration_Apollo_KunConfigurationOptions_ResolveAccessKey}

每次签名前由宿主解析 AccessKey 的 secretRef；不接受明文引导值。

```csharp
public Func<string, string?>? ResolveAccessKey { get; init; }
```

#### Property Value

[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>?

### SecretDirectory \{#Kun_Configuration_Apollo_KunConfigurationOptions_SecretDirectory}

已挂载的 K8s Secret 文件目录；指定后缺目录即失败。

```csharp
public string? SecretDirectory { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### UseApollo \{#Kun_Configuration_Apollo_KunConfigurationOptions_UseApollo}

独立模式可显式禁用 Apollo，仅使用标准配置源。

```csharp
public bool UseApollo { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### ValidateApolloSnapshot \{#Kun_Configuration_Apollo_KunConfigurationOptions_ValidateApolloSnapshot}

可选候选 Apollo 快照校验；拒绝时保留上一份快照和缓存。

```csharp
public Func<IReadOnlyDictionary<string, string?>, bool>? ValidateApolloSnapshot { get; init; }
```

#### Property Value

[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)>?
