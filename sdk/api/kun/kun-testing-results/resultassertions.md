> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ResultAssertions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Results](/sdk/api/kun/kun-testing-results/index.md)\
Assembly: Kun.Testing.dll

Fluent assertion extensions for [Result](/sdk/api/kun/kun-core-results/result.md) and
[Result\<T>](/sdk/api/kun/kun-core-results/result-1.md). On mismatch, throws
[KunAssertionException](/sdk/api/kun/kun-testing/kunassertionexception.md) with a precise message that
includes the helper name, the expected value, and the actual
observed value. See ADR-012 §Error-message contract.

```csharp
public static class ResultAssertions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ResultAssertions](/sdk/api/kun/kun-testing-results/resultassertions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### ShouldBeFailure(Result) \{#Kun_Testing_Results_ResultAssertions_ShouldBeFailure_Kun_Core_Results_Result\_}

Asserts the result is a failure and returns its [Error](/sdk/api/kun/kun-errors/error.md) for further inspection.

```csharp
public static Error ShouldBeFailure(this Result result)
```

#### Parameters

`result` Result

#### Returns

Error

### ShouldBeFailure\<T>(Result\<T>) \{#Kun_Testing_Results_ResultAssertions_ShouldBeFailure__1_Kun_Core_Results_Result___0\_\_}

Asserts the result is a failure and returns its [Error](/sdk/api/kun/kun-errors/error.md) for further inspection.

```csharp
public static Error ShouldBeFailure<T>(this Result<T> result)
```

#### Parameters

`result` Result\<T>

#### Returns

Error

#### Type Parameters

`T`

### ShouldBeSuccess(Result) \{#Kun_Testing_Results_ResultAssertions_ShouldBeSuccess_Kun_Core_Results_Result\_}

Asserts the result is a success. Throws
[KunAssertionException](/sdk/api/kun/kun-testing/kunassertionexception.md) carrying the observed
[Error](/sdk/api/kun/kun-errors/error.md) on a failure input.

```csharp
public static void ShouldBeSuccess(this Result result)
```

#### Parameters

`result` Result

### ShouldBeSuccess\<T>(Result\<T>) \{#Kun_Testing_Results_ResultAssertions_ShouldBeSuccess__1_Kun_Core_Results_Result___0\_\_}

Asserts the result is a success and returns its
[Result\<T>.Value](/sdk/api/kun/kun-core-results/result-1.md#Kun_Core_Results_Result_1_Value) for further inspection. Throws
[KunAssertionException](/sdk/api/kun/kun-testing/kunassertionexception.md) carrying the observed
[Error](/sdk/api/kun/kun-errors/error.md) on a failure input.

```csharp
public static T ShouldBeSuccess<T>(this Result<T> result)
```

#### Parameters

`result` Result\<T>

#### Returns

T

#### Type Parameters

`T`

### ShouldHaveErrorCode(Result, string) \{#Kun_Testing_Results_ResultAssertions_ShouldHaveErrorCode_Kun_Core_Results_Result_System_String\_}

Asserts the result is a failure whose error code ([ErrorCode.Code](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_Code))
equals `code` (ordinal comparison). Returns
the [Error](/sdk/api/kun/kun-errors/error.md) for further inspection.

```csharp
public static Error ShouldHaveErrorCode(this Result result, string code)
```

#### Parameters

`result` Result

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

Error

### ShouldHaveErrorCode(Result, ErrorCode) \{#Kun_Testing_Results_ResultAssertions_ShouldHaveErrorCode_Kun_Core_Results_Result_Kun_Errors_ErrorCode\_}

Asserts the result is a failure carrying `code`
(compared by [ErrorCode.Code](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_Code)). Returns the [Error](/sdk/api/kun/kun-errors/error.md)
for further inspection.

```csharp
public static Error ShouldHaveErrorCode(this Result result, ErrorCode code)
```

#### Parameters

`result` Result

`code` ErrorCode

#### Returns

Error

### ShouldHaveErrorCode\<T>(Result\<T>, string) \{#Kun_Testing_Results_ResultAssertions_ShouldHaveErrorCode__1_Kun_Core_Results_Result___0__System_String\_}

Asserts the result is a failure whose error code ([ErrorCode.Code](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_Code))
equals `code` (ordinal comparison). Returns
the [Error](/sdk/api/kun/kun-errors/error.md) for further inspection.

```csharp
public static Error ShouldHaveErrorCode<T>(this Result<T> result, string code)
```

#### Parameters

`result` Result\<T>

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

Error

#### Type Parameters

`T`

### ShouldHaveErrorCode\<T>(Result\<T>, ErrorCode) \{#Kun_Testing_Results_ResultAssertions_ShouldHaveErrorCode__1_Kun_Core_Results_Result___0__Kun_Errors_ErrorCode\_}

Asserts the result is a failure carrying `code`
(compared by [ErrorCode.Code](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_Code)). Returns the [Error](/sdk/api/kun/kun-errors/error.md)
for further inspection.

```csharp
public static Error ShouldHaveErrorCode<T>(this Result<T> result, ErrorCode code)
```

#### Parameters

`result` Result\<T>

`code` ErrorCode

#### Returns

Error

#### Type Parameters

`T`
