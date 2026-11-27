> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEvent

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

事件定义工厂。

```csharp
public static class KunEvent
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEvent](/sdk/api/kun/kun-runtime/kunevent.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Define\<TPayload, TResult>(string, DispatchMode) \{#Kun_Runtime_KunEvent_Define__2_System_String_Kun_Runtime_DispatchMode\_}

定义一个事件；同一运行时内同名 Definition 必须复用同一实例。

```csharp
public static KunEvent<TPayload, TResult> Define<TPayload, TResult>(string name, DispatchMode mode)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

#### Returns

[KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>

#### Type Parameters

`TPayload`

`TResult`
