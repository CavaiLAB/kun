> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretValueJsonConverter

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

禁止把已解析密钥当作普通 JSON 数据。

```csharp
public sealed class SecretValueJsonConverter : JsonConverter<SecretValue>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[JsonConverter](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter) ←
[JsonConverter\<SecretValue>](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1) ←
[SecretValueJsonConverter](/sdk/api/kun/kun-configuration/secretvaluejsonconverter.md)

#### Inherited Members

[JsonConverter\<SecretValue>.CanConvert(Type)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.canconvert),
[JsonConverter\<SecretValue>.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.read),
[JsonConverter\<SecretValue>.Write(Utf8JsonWriter, SecretValue, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.write),
[JsonConverter\<SecretValue>.ReadAsPropertyName(ref Utf8JsonReader, Type, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.readaspropertyname),
[JsonConverter\<SecretValue>.WriteAsPropertyName(Utf8JsonWriter, SecretValue, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.writeaspropertyname),
[JsonConverter\<SecretValue>.HandleNull](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.handlenull),
[JsonConverter\<SecretValue>.Type](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.type),
[JsonConverter.CanConvert(Type)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter.canconvert),
[JsonConverter.Type](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter.type),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretValueJsonConverter() \{#Kun_Configuration_SecretValueJsonConverter__ctor}

```csharp
public SecretValueJsonConverter()
```

## Methods

### Read(ref Utf8JsonReader, Type, JsonSerializerOptions) \{#Kun_Configuration_SecretValueJsonConverter_Read_System_Text_Json_Utf8JsonReader__System_Type_System_Text_Json_JsonSerializerOptions\_}

```csharp
public override SecretValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` [Utf8JsonReader](https://learn.microsoft.com/dotnet/api/system.text.json.utf8jsonreader)

`typeToConvert` [Type](https://learn.microsoft.com/dotnet/api/system.type)

`options` [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

#### Returns

[SecretValue](/sdk/api/kun/kun-configuration/secretvalue.md)

### Write(Utf8JsonWriter, SecretValue, JsonSerializerOptions) \{#Kun_Configuration_SecretValueJsonConverter_Write_System_Text_Json_Utf8JsonWriter_Kun_Configuration_SecretValue_System_Text_Json_JsonSerializerOptions\_}

```csharp
public override void Write(Utf8JsonWriter writer, SecretValue value, JsonSerializerOptions options)
```

#### Parameters

`writer` [Utf8JsonWriter](https://learn.microsoft.com/dotnet/api/system.text.json.utf8jsonwriter)

`value` [SecretValue](/sdk/api/kun/kun-configuration/secretvalue.md)

`options` [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)
