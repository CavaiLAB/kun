> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PrincipalKindConverter

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Security](/sdk/api/kun/kun-security/index.md)\
Assembly: Kun.Security.Abstractions.dll

主体类型的 JSON 转换器：只收 user、service、agent 三个小写字符串，数字与其它值一律报错。

```csharp
public sealed class PrincipalKindConverter : JsonConverter<PrincipalKind>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[JsonConverter](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter) ←
[JsonConverter\<PrincipalKind>](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1) ←
[PrincipalKindConverter](/sdk/api/kun/kun-security/principalkindconverter.md)

#### Inherited Members

[JsonConverter\<PrincipalKind>.CanConvert(Type)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.canconvert),
[JsonConverter\<PrincipalKind>.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.read),
[JsonConverter\<PrincipalKind>.Write(Utf8JsonWriter, PrincipalKind, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.write),
[JsonConverter\<PrincipalKind>.ReadAsPropertyName(ref Utf8JsonReader, Type, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.readaspropertyname),
[JsonConverter\<PrincipalKind>.WriteAsPropertyName(Utf8JsonWriter, PrincipalKind, JsonSerializerOptions)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.writeaspropertyname),
[JsonConverter\<PrincipalKind>.HandleNull](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.handlenull),
[JsonConverter\<PrincipalKind>.Type](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter-1.type),
[JsonConverter.CanConvert(Type)](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter.canconvert),
[JsonConverter.Type](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonconverter.type),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

公开是因为别的程序集里的 JSON 源生成上下文要引用它。不能换成 [`JsonStringEnumConverter`](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonstringenumconverter-1)：以特性指定时它也收数字。

## Constructors

### PrincipalKindConverter() \{#Kun_Security_PrincipalKindConverter__ctor}

```csharp
public PrincipalKindConverter()
```

## Methods

### Read(ref Utf8JsonReader, Type, JsonSerializerOptions) \{#Kun_Security_PrincipalKindConverter_Read_System_Text_Json_Utf8JsonReader__System_Type_System_Text_Json_JsonSerializerOptions\_}

```csharp
public override PrincipalKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` [Utf8JsonReader](https://learn.microsoft.com/dotnet/api/system.text.json.utf8jsonreader)

`typeToConvert` [Type](https://learn.microsoft.com/dotnet/api/system.type)

`options` [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

#### Returns

[PrincipalKind](/sdk/api/kun/kun-security/principalkind.md)

### Write(Utf8JsonWriter, PrincipalKind, JsonSerializerOptions) \{#Kun_Security_PrincipalKindConverter_Write_System_Text_Json_Utf8JsonWriter_Kun_Security_PrincipalKind_System_Text_Json_JsonSerializerOptions\_}

```csharp
public override void Write(Utf8JsonWriter writer, PrincipalKind value, JsonSerializerOptions options)
```

#### Parameters

`writer` [Utf8JsonWriter](https://learn.microsoft.com/dotnet/api/system.text.json.utf8jsonwriter)

`value` [PrincipalKind](/sdk/api/kun/kun-security/principalkind.md)

`options` [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)
