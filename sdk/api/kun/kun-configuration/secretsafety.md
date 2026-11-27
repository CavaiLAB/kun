> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretSafety

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

配置契约级敏感数据扫描，可供发布门禁与 Apollo 快照验证共用。

```csharp
public static class SecretSafety
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretSafety](/sdk/api/kun/kun-configuration/secretsafety.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Scan(JsonElement, JsonElement?) \{#Kun_Configuration_SecretSafety_Scan_System_Text_Json_JsonElement_System_Nullable_System_Text_Json_JsonElement\_\_}

扫描 JSON；本地 Schema 引用和组合分支中的敏感标记都参与检查。

```csharp
public static IReadOnlyList<SecretDiagnostic> Scan(JsonElement value, JsonElement? schema = null)
```

#### Parameters

`value` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`schema` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)?

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)>

### ScanConfiguration(IReadOnlyDictionary\<string, string?>, JsonElement?) \{#Kun_Configuration_SecretSafety_ScanConfiguration_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement\_\_}

扫描标准 IConfiguration 的扁平层次键，冒号分隔路径；不把原值放进诊断。

```csharp
public static IReadOnlyList<SecretDiagnostic> ScanConfiguration(IReadOnlyDictionary<string, string?> values, JsonElement? schema = null)
```

#### Parameters

`values` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>

`schema` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)?

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)>
