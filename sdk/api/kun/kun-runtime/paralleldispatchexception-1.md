> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ParallelDispatchException\<TResult>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

所有并行监听器结束后报告失败；Partial 仅含成功结果，顺序与监听器一致。

```csharp
public sealed class ParallelDispatchException<TResult> : AggregateException, ISerializable
```

#### Type Parameters

`TResult`

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Exception](https://learn.microsoft.com/dotnet/api/system.exception) ←
[AggregateException](https://learn.microsoft.com/dotnet/api/system.aggregateexception) ←
[ParallelDispatchException\<TResult>](/sdk/api/kun/kun-runtime/paralleldispatchexception-1.md)

#### Implements

[ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

#### Inherited Members

[AggregateException.GetBaseException()](https://learn.microsoft.com/dotnet/api/system.aggregateexception.getbaseexception),
[AggregateException.Handle(Func\<Exception, bool>)](https://learn.microsoft.com/dotnet/api/system.aggregateexception.handle),
[AggregateException.Flatten()](https://learn.microsoft.com/dotnet/api/system.aggregateexception.flatten),
[AggregateException.ToString()](https://learn.microsoft.com/dotnet/api/system.aggregateexception.tostring),
[AggregateException.InnerExceptions](https://learn.microsoft.com/dotnet/api/system.aggregateexception.innerexceptions),
[AggregateException.Message](https://learn.microsoft.com/dotnet/api/system.aggregateexception.message),
[Exception.GetBaseException()](https://learn.microsoft.com/dotnet/api/system.exception.getbaseexception),
[Exception.ToString()](https://learn.microsoft.com/dotnet/api/system.exception.tostring),
[Exception.GetType()](https://learn.microsoft.com/dotnet/api/system.exception.gettype),
[Exception.TargetSite](https://learn.microsoft.com/dotnet/api/system.exception.targetsite),
[Exception.Message](https://learn.microsoft.com/dotnet/api/system.exception.message),
[Exception.Data](https://learn.microsoft.com/dotnet/api/system.exception.data),
[Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception),
[Exception.HelpLink](https://learn.microsoft.com/dotnet/api/system.exception.helplink),
[Exception.Source](https://learn.microsoft.com/dotnet/api/system.exception.source),
[Exception.HResult](https://learn.microsoft.com/dotnet/api/system.exception.hresult),
[Exception.StackTrace](https://learn.microsoft.com/dotnet/api/system.exception.stacktrace),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ParallelDispatchException(IEnumerable\<Exception>, IReadOnlyList\<TResult>) \{#Kun_Runtime_ParallelDispatchException_1__ctor_System_Collections_Generic_IEnumerable_System_Exception__System_Collections_Generic_IReadOnlyList__0\_\_}

记录失败与监听器顺序的成功结果快照。

```csharp
public ParallelDispatchException(IEnumerable<Exception> errors, IReadOnlyList<TResult> partial)
```

#### Parameters

`errors` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[Exception](https://learn.microsoft.com/dotnet/api/system.exception)>

`partial` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<TResult>

## Properties

### Partial \{#Kun_Runtime_ParallelDispatchException_1_Partial}

成功结果快照，不含失败项的占位值。

```csharp
public IReadOnlyList<TResult> Partial { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<TResult>
