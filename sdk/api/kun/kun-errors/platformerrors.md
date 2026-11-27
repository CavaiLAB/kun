> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PlatformErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

平台内置错误码（归属 `qian.runtime`），冻结在 contracts/v0/error-codes.json。
协议错误覆盖宿主框架会产生的全部状态码；业务错误只有两个通用兜底，具体业务错误码由各组件在 Definition 包登记。

```csharp
public static class PlatformErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PlatformErrors](/sdk/api/kun/kun-errors/platformerrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### AuthzDenied \{#Kun_Errors_PlatformErrors_AuthzDenied}

403：当前主体无权执行该操作。

```csharp
public static readonly ErrorCode AuthzDenied
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### BadGateway \{#Kun_Errors_PlatformErrors_BadGateway}

502：上游服务返回无效响应。

```csharp
public static readonly ErrorCode BadGateway
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### BadRequest \{#Kun_Errors_PlatformErrors_BadRequest}

400：请求无法解析（JSON 语法、内容类型、参数类型）。

```csharp
public static readonly ErrorCode BadRequest
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### BusinessRuleViolated \{#Kun_Errors_PlatformErrors_BusinessRuleViolated}

422：请求合法但被业务规则拒绝；组件应优先登记更具体的业务错误码。

```csharp
public static readonly ErrorCode BusinessRuleViolated
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Conflict \{#Kun_Errors_PlatformErrors_Conflict}

409：资源状态已变化或与现有数据冲突（并发修改、唯一键重复）。

```csharp
public static readonly ErrorCode Conflict
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### DependencyCycle \{#Kun_Errors_PlatformErrors_DependencyCycle}

400：启动配置中的依赖成环。

```csharp
public static readonly ErrorCode DependencyCycle
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### DependencyMissing \{#Kun_Errors_PlatformErrors_DependencyMissing}

400：启动配置引用了没有 Provider 的服务键。

```csharp
public static readonly ErrorCode DependencyMissing
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### FormatInvalid \{#Kun_Errors_PlatformErrors_FormatInvalid}

不是合法的声明格式（`format`，如 uuid、date-time、email）。

```csharp
public static readonly ErrorCode FormatInvalid
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### GatewayTimeout \{#Kun_Errors_PlatformErrors_GatewayTimeout}

504：上游服务超时。

```csharp
public static readonly ErrorCode GatewayTimeout
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### InputInvalid \{#Kun_Errors_PlatformErrors_InputInvalid}

400：输入不符合能力的输入 Schema，逐项见 errors。

```csharp
public static readonly ErrorCode InputInvalid
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### InsufficientScope \{#Kun_Errors_PlatformErrors_InsufficientScope}

403：令牌缺少该操作所需的 scope（WWW-Authenticate 头给出所需 scope）。

```csharp
public static readonly ErrorCode InsufficientScope
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Internal \{#Kun_Errors_PlatformErrors_Internal}

500：服务内部错误（不暴露异常细节）。

```csharp
public static readonly ErrorCode Internal
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### MethodNotAllowed \{#Kun_Errors_PlatformErrors_MethodNotAllowed}

405：资源不支持此 HTTP 方法。

```csharp
public static readonly ErrorCode MethodNotAllowed
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### NotAcceptable \{#Kun_Errors_PlatformErrors_NotAcceptable}

406：无法按 Accept 头返回。

```csharp
public static readonly ErrorCode NotAcceptable
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### NotFound \{#Kun_Errors_PlatformErrors_NotFound}

404：资源不存在或对调用者不可见（未挂载与不存在同此响应，R32）。

```csharp
public static readonly ErrorCode NotFound
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### OutOfRange \{#Kun_Errors_PlatformErrors_OutOfRange}

取值或长度超出范围（`minimum` / `maximum` / `minLength` / `maxLength` / `minItems` / `maxItems`）。

```csharp
public static readonly ErrorCode OutOfRange
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Owner \{#Kun_Errors_PlatformErrors_Owner}

内置错误码的归属组件。

```csharp
public const string Owner = "qian.runtime"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PatternMismatch \{#Kun_Errors_PlatformErrors_PatternMismatch}

不匹配模式（`pattern`）。

```csharp
public static readonly ErrorCode PatternMismatch
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### PayloadTooLarge \{#Kun_Errors_PlatformErrors_PayloadTooLarge}

413：请求体过大。

```csharp
public static readonly ErrorCode PayloadTooLarge
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### PreconditionFailed \{#Kun_Errors_PlatformErrors_PreconditionFailed}

412：If-Match 等前置条件不成立。

```csharp
public static readonly ErrorCode PreconditionFailed
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### RateLimited \{#Kun_Errors_PlatformErrors_RateLimited}

429：超出限流阈值（Retry-After 头给出等待时间）。

```csharp
public static readonly ErrorCode RateLimited
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### RegistrationConflict \{#Kun_Errors_PlatformErrors_RegistrationConflict}

409：同 scope 同名注册已存在。

```csharp
public static readonly ErrorCode RegistrationConflict
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### RequestTimeout \{#Kun_Errors_PlatformErrors_RequestTimeout}

408：请求超时。

```csharp
public static readonly ErrorCode RequestTimeout
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Required \{#Kun_Errors_PlatformErrors_Required}

缺少必填项（`required`）。

```csharp
public static readonly ErrorCode Required
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### TypeMismatch \{#Kun_Errors_PlatformErrors_TypeMismatch}

类型不符（`type`）。

```csharp
public static readonly ErrorCode TypeMismatch
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Unauthenticated \{#Kun_Errors_PlatformErrors_Unauthenticated}

401：缺少有效凭据或令牌已失效。

```csharp
public static readonly ErrorCode Unauthenticated
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Unavailable \{#Kun_Errors_PlatformErrors_Unavailable}

503：服务暂不可用。

```csharp
public static readonly ErrorCode Unavailable
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### UnknownProperty \{#Kun_Errors_PlatformErrors_UnknownProperty}

含 Schema 未声明的成员（`additionalProperties: false`）。

```csharp
public static readonly ErrorCode UnknownProperty
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### UnsupportedMediaType \{#Kun_Errors_PlatformErrors_UnsupportedMediaType}

415：不支持的 Content-Type。

```csharp
public static readonly ErrorCode UnsupportedMediaType
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### ValueNotAllowed \{#Kun_Errors_PlatformErrors_ValueNotAllowed}

取值不在允许集合内（`enum` / `const`），可选值见 allowed。

```csharp
public static readonly ErrorCode ValueNotAllowed
```

#### Field Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

## Properties

### All \{#Kun_Errors_PlatformErrors_All}

全部内置错误码。

```csharp
public static IReadOnlyList<ErrorCode> All { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)>

## Methods

### ForStatus(int) \{#Kun_Errors_PlatformErrors_ForStatus_System_Int32\_}

某个 HTTP 状态码的兜底错误码：用于宿主框架自己产生、没有带错误码的错误响应。
没有专门错误码的 4xx 落到 [PlatformErrors.BadRequest](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_BadRequest)、5xx 落到 [PlatformErrors.Internal](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_Internal)，响应状态码保持原值。

```csharp
public static ErrorCode ForStatus(int status)
```

#### Parameters

`status` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)
