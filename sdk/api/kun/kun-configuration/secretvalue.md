> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretValue

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

密钥的受控内存副本；默认打印和 JSON 序列化不暴露内容。

```csharp
[JsonConverter(typeof(SecretValueJsonConverter))]
public sealed class SecretValue : IDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretValue](/sdk/api/kun/kun-configuration/secretvalue.md)

#### Implements

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretValue(ReadOnlySpan\<byte>) \{#Kun_Configuration_SecretValue__ctor_System_ReadOnlySpan_System_Byte\_\_}

复制字节；调用方仍负责清理自己持有的输入。

```csharp
public SecretValue(ReadOnlySpan<byte> value)
```

#### Parameters

`value` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<[byte](https://learn.microsoft.com/dotnet/api/system.byte)>

## Properties

### Bytes \{#Kun_Configuration_SecretValue_Bytes}

显式取得只读字节，释放后本对象持有的缓冲归零。

```csharp
public ReadOnlyMemory<byte> Bytes { get; }
```

#### Property Value

[ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory-1)\<[byte](https://learn.microsoft.com/dotnet/api/system.byte)>

## Methods

### Dispose() \{#Kun_Configuration_SecretValue_Dispose}

清零并释放本对象的副本；无法清零调用方已创建的字符串。

```csharp
public void Dispose()
```

### RevealUtf8() \{#Kun_Configuration_SecretValue_RevealUtf8}

显式取得 UTF-8 字符串，调用方负责其使用和生命周期，不能写入日志或配置导出。

```csharp
public string RevealUtf8()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ToString() \{#Kun_Configuration_SecretValue_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
