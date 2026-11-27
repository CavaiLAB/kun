> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Struct Result

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Core` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Core`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Core)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Core.Results](/sdk/api/kun/kun-core-results/index.md)\
Assembly: Kun.Core.dll

无返回值的 Result。Handler 在不需要返回数据时使用。失败时携带 [Error](/sdk/api/kun/kun-errors/error.md)：
一个登记在错误码目录里的错误码，加文案参数与出错位置。

```csharp
public readonly struct Result : IEquatable<Result>
```

#### Implements

[IEquatable\<Result>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

#### Extension Methods

[ResultExtensions.Bind(Result, Func\<Result>)](/sdk/api/kun/kun-core-results/resultextensions.md#Kun_Core_Results_ResultExtensions_Bind_Kun_Core_Results_Result_System_Func_Kun_Core_Results_Result__),
[ResultExtensions.Ensure(Result, Func\<bool>, Error)](/sdk/api/kun/kun-core-results/resultextensions.md#Kun_Core_Results_ResultExtensions_Ensure_Kun_Core_Results_Result_System_Func_System_Boolean__Kun_Errors_Error_),
[ResultExtensions.Match\<TOut>(Result, Func\<TOut>, Func\<Error, TOut>)](/sdk/api/kun/kun-core-results/resultextensions.md#Kun_Core_Results_ResultExtensions_Match__1_Kun_Core_Results_Result_System_Func___0__System_Func_Kun_Errors_Error___0__),
[ResultExtensions.Tap(Result, Action)](/sdk/api/kun/kun-core-results/resultextensions.md#Kun_Core_Results_ResultExtensions_Tap_Kun_Core_Results_Result_System_Action_)

## Properties

### Error \{#Kun_Core_Results_Result_Error}

失败结果携带的错误。仅在 [Result.IsFailure](/sdk/api/kun/kun-core-results/result.md#Kun_Core_Results_Result_IsFailure) 为 [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) 时可访问，
成功结果调用此属性会抛异常——应先用 [Result.IsFailure](/sdk/api/kun/kun-core-results/result.md#Kun_Core_Results_Result_IsFailure) 判断。

```csharp
public Error Error { get; }
```

#### Property Value

Error

### IsFailure \{#Kun_Core_Results_Result_IsFailure}

[true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) when the operation failed.

```csharp
public bool IsFailure { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### IsSuccess \{#Kun_Core_Results_Result_IsSuccess}

[true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) when the operation succeeded.

```csharp
public bool IsSuccess { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### Equals(object) \{#Kun_Core_Results_Result_Equals_System_Object\_}

```csharp
public override bool Equals(object obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(Result) \{#Kun_Core_Results_Result_Equals_Kun_Core_Results_Result\_}

```csharp
public bool Equals(Result other)
```

#### Parameters

`other` [Result](/sdk/api/kun/kun-core-results/result.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Fail(Error) \{#Kun_Core_Results_Result_Fail_Kun_Errors_Error\_}

Creates a failure result carrying `error`.

```csharp
public static Result Fail(Error error)
```

#### Parameters

`error` Error

#### Returns

[Result](/sdk/api/kun/kun-core-results/result.md)

### GetHashCode() \{#Kun_Core_Results_Result_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Ok() \{#Kun_Core_Results_Result_Ok}

Creates a successful result.

```csharp
public static Result Ok()
```

#### Returns

[Result](/sdk/api/kun/kun-core-results/result.md)

### ToString() \{#Kun_Core_Results_Result_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(Result, Result) \{#Kun_Core_Results_Result_op_Equality_Kun_Core_Results_Result_Kun_Core_Results_Result\_}

```csharp
public static bool operator ==(Result left, Result right)
```

#### Parameters

`left` [Result](/sdk/api/kun/kun-core-results/result.md)

`right` [Result](/sdk/api/kun/kun-core-results/result.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### implicit operator Result(Error) \{#Kun_Core_Results_Result_op_Implicit_Kun_Errors_Error__Kun_Core_Results_Result}

Implicit conversion from [Error](/sdk/api/kun/kun-errors/error.md) to a failure result.

```csharp
public static implicit operator Result(Error error)
```

#### Parameters

`error` Error

#### Returns

[Result](/sdk/api/kun/kun-core-results/result.md)

### operator !=(Result, Result) \{#Kun_Core_Results_Result_op_Inequality_Kun_Core_Results_Result_Kun_Core_Results_Result\_}

```csharp
public static bool operator !=(Result left, Result right)
```

#### Parameters

`left` [Result](/sdk/api/kun/kun-core-results/result.md)

`right` [Result](/sdk/api/kun/kun-core-results/result.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
