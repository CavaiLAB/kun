> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ComponentMetadataDeclarations

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

生成代码使用的声明校验；只读取静态声明，不执行插件。

```csharp
public static class ComponentMetadataDeclarations
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ComponentMetadataDeclarations](/sdk/api/kun/kun-plugin-sdk/componentmetadatadeclarations.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Event\<TPayload, TResult>(KunEvent\<TPayload, TResult>, DispatchMode, string) \{#Kun_Plugin_Sdk_ComponentMetadataDeclarations_Event__2_Kun_Runtime_KunEvent___0___1__Kun_Runtime_DispatchMode_System_String\_}

核验事件属性与实际强类型事件的模式一致，并产生可追溯的元数据项。

```csharp
public static JsonObject Event<TPayload, TResult>(KunEvent<TPayload, TResult> value, DispatchMode mode, string source)
```

#### Parameters

`value` KunEvent\<TPayload, TResult>

`mode` DispatchMode

`source` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

#### Type Parameters

`TPayload`

`TResult`

### Event\<TPayload, TResult>(KunEvent\<TPayload, TResult>, DispatchMode, string, string) \{#Kun_Plugin_Sdk_ComponentMetadataDeclarations_Event__2_Kun_Runtime_KunEvent___0___1__Kun_Runtime_DispatchMode_System_String_System_String\_}

在同源事件声明上保留唯一 Definition 归属；多个 Provider 可引用同一事件。

```csharp
public static JsonObject Event<TPayload, TResult>(KunEvent<TPayload, TResult> value, DispatchMode mode, string source, string definitionComponent)
```

#### Parameters

`value` KunEvent\<TPayload, TResult>

`mode` DispatchMode

`source` [string](https://learn.microsoft.com/dotnet/api/system.string)

`definitionComponent` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

#### Type Parameters

`TPayload`

`TResult`
