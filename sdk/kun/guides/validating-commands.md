> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Command 校验指南（Kun.Validation）

:::info 发布版本来源
本页由本仓库的发布标签生成，源文件为 [`docs/guides/validating-commands.md`](https://github.com/CavaiLAB/kun/blob/v2026.11.24/docs/guides/validating-commands.md)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

> 怎么在一个 Command/Query Handler 上挂校验，并且让失败以 `Result.Fail(...)`（错误码 `INPUT_INVALID`，带逐项字段错误）而不是异常的形式流到调用方。
>
> 参考：`docs/adr/kun/ADR-011-Result-async-extensions.md` · `docs/adr/kun/ADR-014-Validation-Result-behavior.md`

## 适用对象

在下游服务里新加一条 Command/Query，打算：

- 用 `IRequest<Result>` 或 `IRequest<Result<TValue>>` 返回业务结果（而不是抛 `ErrorException` 让 HTTP / gRPC 宿主翻译）；
- 在 Handler 之外把"请求形状合法性"（非空、长度、枚举范围等）挡住；
- 写单元测试时能直接断言 `result.Error.Code` 是 `PlatformErrors.InputInvalid`，不需要 `Assert.ThrowsAsync<ErrorException>`。

如果你只写内部小工具、Handler 返回 `TResponse` 且可以接受异常语义，见下方 `ValidationPipelineBehavior` 路径；否则默认选 Result 路径。

## 选型决策树

Kun.Validation 目前出三个 Pipeline Behavior，按 Handler 响应类型二选一：

| Handler 响应签名                    | 选的 Behavior                                          | 失败时的行为                          | 注册扩展                                              |
| ------------------------------- | ---------------------------------------------------- | ------------------------------- | ------------------------------------------------- |
| `IRequest<TResponse>`（非 Result） | `ValidationPipelineBehavior<TRequest, TResponse>`    | 抛 `ErrorException`              | `AddValidationBehavior<TRequest, TResponse>()`    |
| `IRequest<Result<TValue>>`      | `ValidationResultPipelineBehavior<TRequest, TValue>` | 返回 `Result<TValue>.Fail(error)` | `AddValidationResultBehavior<TRequest, TValue>()` |
| `IRequest<Result>`（无返回值命令）      | `ValidationResultPipelineBehavior<TRequest>`         | 返回 `Result.Fail(error)`         | `AddValidationResultBehavior<TRequest>()`         |

三个 Behavior 给出的是同一个 `Kun.Errors.Error`：错误码 `PlatformErrors.InputInvalid`（`INPUT_INVALID`，400），`Errors` 按校验器的注册顺序列出各校验器给的字段错误，至多 100 项（ErrorDetail 契约的上限，多出的丢弃）。HTTP 宿主装了 `Kun.Errors.AspNetCore`（`AddKunErrors()` 加 `UseKunErrors()`）时，抛出版的异常、Result 版经 `EnsureSuccess()` 抛出的异常都写成 problem+json 的 ErrorDetail，`errors[]` 的每一项是字段错误码加 JSON Pointer。

> **铁律**：同一 `TRequest` 不要同时注册抛出版 + Result 版。Pipeline 会按注册顺序串成链，同一组校验器跑两遍，先注册的那个决定失败的形状（抛异常还是返回 Result），后一个形同虚设。Kun 不在运行时拦截该错配（成本高、场景罕见），只靠 Code Review + 这份文档挡。

## 例 1：`Result<TValue>` 命令

场景：下单命令 `CreateOrderCommand → Result<OrderDto>`，两个校验器（数量 > 0、产品 ID 不为空）。

### Command + Validator

```csharp
public sealed record CreateOrderCommand(Guid ProductId, int Quantity)
    : IRequest<Result<OrderDto>>;

public sealed class CreateOrderQuantityValidator : IValidator<CreateOrderCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(
        CreateOrderCommand instance,
        CancellationToken cancellationToken = default)
    {
        if (instance.Quantity > 0) return ValueTask.FromResult(ValidationResult.Success);

        return ValueTask.FromResult(ValidationResult.Failure(
        [
            new FieldError(PlatformErrors.OutOfRange, ErrorPointer.Input("quantity")),
        ]));
    }
}

public sealed class CreateOrderProductIdValidator : IValidator<CreateOrderCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(
        CreateOrderCommand instance,
        CancellationToken cancellationToken = default)
    {
        if (instance.ProductId != Guid.Empty) return ValueTask.FromResult(ValidationResult.Success);

        return ValueTask.FromResult(ValidationResult.Failure(
        [
            new FieldError(PlatformErrors.Required, ErrorPointer.Input("productId")),
        ]));
    }
}
```

一项字段错误是两样东西：错误码与出错位置。

- 错误码用平台内置的字段级错误码，与 JSON Schema 的关键字一一对应：`REQUIRED`、`TYPE_MISMATCH`、`VALUE_NOT_ALLOWED`、`OUT_OF_RANGE`、`PATTERN_MISMATCH`、`FORMAT_INVALID`、`UNKNOWN_PROPERTY`（`PlatformErrors` 上的同名字段）；也可以用组件自己登记的业务错误码。
- 位置是指向请求体的 JSON Pointer，按客户端提交的成员名写（这里请求体是 camelCase）。用 `ErrorPointer.Input(...)` 拼，数组下标传整数：`ErrorPointer.Input("items", 0, "qty")` 得到 `#/items/0/qty`。
- 取值受限时可以带上允许值：`new FieldError(PlatformErrors.ValueNotAllowed, ErrorPointer.Input("purpose")) { Allowed = AllowedValues.Of("attachment", "avatar") }`。

校验器不写文案：响应里的 `message` 按请求语言从错误码目录里取。

### DI 注册

```csharp
services.AddValidator<CreateOrderCommand, CreateOrderQuantityValidator>();
services.AddValidator<CreateOrderCommand, CreateOrderProductIdValidator>();

services.AddKunMediator(mediator =>
{
    mediator.AddValidationResultBehavior<CreateOrderCommand, OrderDto>();
    // mediator.AddRequestHandler<...>() — 由源生成器自动填
});
```

`AddValidator<,>` 挂在 `IServiceCollection`，`AddValidationResultBehavior<,>` 挂在 `KunMediatorBuilder`——分开是因为 Validator 是按请求类型注册的 transient，Behavior 是 Mediator pipeline 链上的一环。

### Handler

Handler 直接写成功路径；Validator 失败时 Behavior 已经短路，Handler 不会被调到。Handler 里的业务失败优先用组件登记的具体错误码（`ErrorCode.Business(...)` 定义，`AddKunErrors(catalog => catalog.Register(...))` 登记）；通用的 404、409 可以直接用 `PlatformErrors.NotFound`、`PlatformErrors.Conflict`。

```csharp
public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 走到这里时 Quantity > 0 且 ProductId != Guid.Empty 已成立。
        var product = await _products.GetAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<OrderDto>.Fail(new Error(PlatformErrors.NotFound));

        var order = await _orders.CreateAsync(product, request.Quantity, cancellationToken);
        return Result<OrderDto>.Ok(OrderDto.From(order));
    }
}
```

### 测试

```csharp
[Fact]
public async Task Validator_fails_returns_input_invalid()
{
    var services = new ServiceCollection();
    services.AddValidator<CreateOrderCommand, CreateOrderQuantityValidator>();
    services.AddKunMediator(m =>
    {
        m.AddRequestHandler<CreateOrderCommand, Result<OrderDto>, CreateOrderCommandHandler>();
        m.AddValidationResultBehavior<CreateOrderCommand, OrderDto>();
    });
    var sender = services.BuildServiceProvider().GetRequiredService<ISender>();

    var result = await sender.Send(new CreateOrderCommand(Guid.NewGuid(), 0), CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Same(PlatformErrors.InputInvalid, result.Error.Code);
    var field = Assert.Single(result.Error.Errors);
    Assert.Same(PlatformErrors.OutOfRange, field.Code);
    Assert.Equal("#/quantity", field.Pointer);
}
```

更多断言帮助（`ResultAssertions.ShouldBeFailure(...)`、`ShouldHaveErrorCode(PlatformErrors.InputInvalid)` 等）在 `Kun.Testing`。

## 例 2：`Result` 命令（无返回值）

场景：删除用户命令 `DeleteUserCommand → Result`，一个校验器（`UserId` 非空）。适用于幂等写命令、删除、标记为已读等"跑完了就行、没什么可返回"的业务。

```csharp
public sealed record DeleteUserCommand(Guid UserId) : IRequest<Result>;

public sealed class DeleteUserValidator : IValidator<DeleteUserCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(
        DeleteUserCommand instance,
        CancellationToken cancellationToken = default)
    {
        if (instance.UserId != Guid.Empty) return ValueTask.FromResult(ValidationResult.Success);

        return ValueTask.FromResult(ValidationResult.Failure(
        [
            new FieldError(PlatformErrors.Required, ErrorPointer.Input("userId")),
        ]));
    }
}

// DI：
services.AddValidator<DeleteUserCommand, DeleteUserValidator>();
services.AddKunMediator(m =>
{
    m.AddValidationResultBehavior<DeleteUserCommand>();  // 注意：只有一个类型参数
});

// Handler：
public sealed class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteUserCommand r, CancellationToken ct)
    {
        var user = await _users.GetAsync(r.UserId, ct);
        if (user is null) return Result.Fail(new Error(PlatformErrors.NotFound));
        await _users.DeleteAsync(user, ct);
        return Result.Ok();
    }
}
```

`AddValidationResultBehavior<TRequest>()`（单参数）闭合在 `IRequest<Result>` 上；`AddValidationResultBehavior<TRequest, TValue>()`（双参数）闭合在 `IRequest<Result<TValue>>` 上。名字一样，只靠类型参数数量区分。

## 反模式

### 1. 同一 `TRequest` 注册两个 Behavior

```csharp
// ❌ 不要这样
m.AddValidationBehavior<CreateOrderCommand, Result<OrderDto>>();          // 抛出版
m.AddValidationResultBehavior<CreateOrderCommand, OrderDto>();            // Result 版
```

Pipeline 会串成链，同一组校验器跑两遍：先注册的抛出版先抛 `ErrorException`，Result 版永远走不到。跑不出来预期结果，IDE 也不会警告。

### 2. 在 `Result`-Handler 里抛 `ErrorException`

```csharp
// ❌ 不要这样
public async ValueTask<Result<OrderDto>> Handle(...)
{
    if (request.Quantity < 0)
        throw new ErrorException(/* ... */);   // 业务层不该为校验失败抛异常
    // ...
}
```

把校验放进 Validator；Handler 里用 `Result<TValue>.Fail(error)` 直接返回。根 [AGENTS.md](https://github.com/CavaiLAB/kun/blob/v2026.11.24/AGENTS.md#运行时约定)「运行时约定」：业务失败返回 `Result` / `Result<T>`，异常只留给系统性故障（并发、网络、基础设施）。抛异常会破坏下游 `Match / Bind / Tap` 组合链（Kun ADR-011）。

### 3. 在 Validator 里调数据库 / 外部服务

```csharp
// ❌ 不要这样
public async ValueTask<ValidationResult> ValidateAsync(CreateOrderCommand r, CancellationToken ct)
{
    var product = await _db.Products.FindAsync(r.ProductId, ct);   // 跨层了
    if (product is null) return ValidationResult.Failure(/* ... */);
    // ...
}
```

Validator 只处理"请求形状合法性"（类型、范围、非空、正则）。凡是需要外部状态（"ProductId 对应的产品是否存在"、"该用户是否有权限"）的判断都是语义校验，归 Handler。Validator 纯函数化保证：AOT 友好、单元测试不需要 mock 任何 DbContext、Pipeline 中多个 Validator 可以并行跑。

## 排错

- **Behavior 没生效**：检查 `AddKunMediator(m => …)` 里是否真的调了 `AddValidation…Behavior`；源生成器只注册 Handler 本身，Behavior 需要显式挂到 builder。
- **打了 `AddValidator<,>()` 但 Validator 没跑**：确认 Validator 实现了 `IValidator<T>`（not `IValidator<TRequest, TResponse>`），且 `T` 精确匹配 Command 类型（`CreateOrderCommand` 不是 `object`）。
- **响应里的 `errors[]` 少了几项**：ErrorDetail 至多 100 项，Behavior 按校验器的注册顺序保留前 100 项。
- **错误响应不是 problem+json**：HTTP 宿主没装 `Kun.Errors.AspNetCore`。`AddKunErrors()` 登记错误码目录与异常处理，`UseKunErrors()` 放在管道最前面。

## 相关

- [Kun ADR-011 Result async extensions](/sdk/kun/architecture/adr-011-result-async-extensions.md)—`Result<T>` 的 `Map/Bind/Tap/Ensure/Match` 组合面。
- [Kun ADR-014 Validation Result behavior](/sdk/kun/architecture/adr-014-validation-result-behavior.md)—两个 Result 版 Behavior 的设计决策。
- [src/Kun.Validation/](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Validation)—`IValidator<T>`、`ValidationResult` 与三个 Behavior。
- [src/Kun.Errors/](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)—错误码与错误码目录、`Error`、`FieldError`、`ErrorPointer`、平台内置错误码 `PlatformErrors`。
- [src/Kun.Testing/](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing)—`ResultAssertions.ShouldBeFailure` 等断言帮助。
- [docs/design/when-to-use-currentuser.md](/sdk/kun/architecture/when-to-use-currentuser.md)—Kun 选型指南的样板格式，本文跟它的结构一致。
