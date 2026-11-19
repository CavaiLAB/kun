using FluentAssertions;
using Kun.Core.Cqrs;
using Kun.Core.Results;
using Kun.Errors;
using Kun.Mediator;
using Kun.Mediator.DependencyInjection;
using Kun.Validation.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kun.Validation.Tests;

// ---------------------------------------------------------------------------
// Domain types for Result-variant integration tests
// ---------------------------------------------------------------------------

internal sealed record CreateOrderCommand(string Sku, int Quantity) : IRequest<Result<string>>;

internal sealed class CreateOrderResultHandler : IRequestHandler<CreateOrderCommand, Result<string>>
{
    private readonly ResultInvocationTracker _tracker;

    public CreateOrderResultHandler(ResultInvocationTracker tracker) => _tracker = tracker;

    public ValueTask<Result<string>> Handle(CreateOrderCommand r, CancellationToken ct)
    {
        _tracker.WasInvoked = true;
        // Let the test control whether the handler itself returns success or failure.
        return ValueTask.FromResult(_tracker.ReturnFailure
            ? Result<string>.Fail(new Error(PlatformErrors.NotFound))
            : Result<string>.Ok("order:" + r.Sku));
    }
}

internal sealed class ResultInvocationTracker
{
    public bool WasInvoked { get; set; }
    public bool ReturnFailure { get; set; }
}

internal sealed class SkuRequiredValidator : IValidator<CreateOrderCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(CreateOrderCommand instance, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(instance.Sku))
        {
            var errors = new[] { new FieldError(PlatformErrors.Required, ErrorPointer.Input("sku")) };
            return ValueTask.FromResult(ValidationResult.Failure(errors));
        }
        return ValueTask.FromResult(ValidationResult.Success);
    }
}

internal sealed class QuantityPositiveValidator : IValidator<CreateOrderCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(CreateOrderCommand instance, CancellationToken ct = default)
    {
        if (instance.Quantity <= 0)
        {
            var errors = new[] { new FieldError(PlatformErrors.OutOfRange, ErrorPointer.Input("quantity")) };
            return ValueTask.FromResult(ValidationResult.Failure(errors));
        }
        return ValueTask.FromResult(ValidationResult.Success);
    }
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

public sealed class ValidationResultPipelineBehaviorTests
{
    private static ServiceProvider BuildServices(Action<IServiceCollection>? extra = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(new ResultInvocationTracker());

        services.AddKunMediator(b => b
            .AddValidationResultBehavior<CreateOrderCommand, string>()
            .AddRequestHandler<CreateOrderCommand, Result<string>, CreateOrderResultHandler>());

        extra?.Invoke(services);

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task NoValidatorsRegistered_PassesThrough_ReturnsHandlerResult()
    {
        await using var sp = BuildServices();
        var sender = sp.GetRequiredService<ISender>();
        var tracker = sp.GetRequiredService<ResultInvocationTracker>();

        var result = await sender.Send(
            new CreateOrderCommand("sku-1", 3),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("order:sku-1");
        tracker.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatorReturnsNoErrors_HandlerInvoked_ReturnsSuccess()
    {
        await using var sp = BuildServices(svc =>
            svc.AddValidator<CreateOrderCommand, SkuRequiredValidator>());

        var sender = sp.GetRequiredService<ISender>();
        var tracker = sp.GetRequiredService<ResultInvocationTracker>();

        var result = await sender.Send(
            new CreateOrderCommand("sku-2", 1),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        tracker.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatorReturnsErrors_ReturnsResultFail_HandlerNotInvoked()
    {
        await using var sp = BuildServices(svc =>
            svc.AddValidator<CreateOrderCommand, SkuRequiredValidator>());

        var sender = sp.GetRequiredService<ISender>();
        var tracker = sp.GetRequiredService<ResultInvocationTracker>();

        var result = await sender.Send(
            new CreateOrderCommand(Sku: "", Quantity: 1),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().BeSameAs(PlatformErrors.InputInvalid);
        result.Error.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.Required && e.Pointer == "#/sku");
        tracker.WasInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task MultipleValidatorsEachReportErrors_AggregatesIntoSingleError()
    {
        await using var sp = BuildServices(svc =>
        {
            svc.AddValidator<CreateOrderCommand, SkuRequiredValidator>();
            svc.AddValidator<CreateOrderCommand, QuantityPositiveValidator>();
        });

        var sender = sp.GetRequiredService<ISender>();
        var tracker = sp.GetRequiredService<ResultInvocationTracker>();

        var result = await sender.Send(
            new CreateOrderCommand(Sku: "", Quantity: -5),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().BeSameAs(PlatformErrors.InputInvalid);
        result.Error.Errors.Select(e => e.Pointer).Should().Equal("#/sku", "#/quantity");
        tracker.WasInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task AllValidatorsPass_HandlerReturnsResultFail_BehaviorPropagatesHandlerFailure()
    {
        await using var sp = BuildServices(svc =>
            svc.AddValidator<CreateOrderCommand, SkuRequiredValidator>());

        var tracker = sp.GetRequiredService<ResultInvocationTracker>();
        tracker.ReturnFailure = true;
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(
            new CreateOrderCommand("sku-3", 1),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        // Handler-produced error survives — behaviour must NOT overwrite it.
        result.Error.Code.Should().BeSameAs(PlatformErrors.NotFound);
        tracker.WasInvoked.Should().BeTrue();
    }

    // ── Non-generic Result variant (ADR-014 §F1, KN-SELF-012) ──────────────

    internal sealed record DeleteUserCommand(string UserId) : IRequest<Result>;

    internal sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly ResultInvocationTracker _tracker;
        public DeleteUserHandler(ResultInvocationTracker tracker) => _tracker = tracker;
        public ValueTask<Result> Handle(DeleteUserCommand r, CancellationToken ct)
        {
            _tracker.WasInvoked = true;
            return ValueTask.FromResult(_tracker.ReturnFailure
                ? Result.Fail(new Error(PlatformErrors.NotFound))
                : Result.Ok());
        }
    }

    internal sealed class UserIdRequiredValidator : IValidator<DeleteUserCommand>
    {
        public ValueTask<ValidationResult> ValidateAsync(DeleteUserCommand instance, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(instance.UserId))
            {
                var errors = new[] { new FieldError(PlatformErrors.Required, ErrorPointer.Input("userId")) };
                return ValueTask.FromResult(ValidationResult.Failure(errors));
            }
            return ValueTask.FromResult(ValidationResult.Success);
        }
    }

    internal sealed class UserIdLengthValidator : IValidator<DeleteUserCommand>
    {
        public ValueTask<ValidationResult> ValidateAsync(DeleteUserCommand instance, CancellationToken ct = default)
        {
            if (instance.UserId is { Length: > 20 })
            {
                var errors = new[] { new FieldError(PlatformErrors.OutOfRange, ErrorPointer.Input("userId")) };
                return ValueTask.FromResult(ValidationResult.Failure(errors));
            }
            return ValueTask.FromResult(ValidationResult.Success);
        }
    }

    private static ServiceProvider BuildNonGenericServices(Action<IServiceCollection>? extra = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(new ResultInvocationTracker());
        services.AddKunMediator(b => b
            .AddValidationResultBehavior<DeleteUserCommand>()
            .AddRequestHandler<DeleteUserCommand, Result, DeleteUserHandler>());
        extra?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task NonGeneric_ValidatorReturnsErrors_ReturnsResultFail_HandlerNotInvoked()
    {
        await using var sp = BuildNonGenericServices(svc =>
            svc.AddValidator<DeleteUserCommand, UserIdRequiredValidator>());

        var sender = sp.GetRequiredService<ISender>();
        var tracker = sp.GetRequiredService<ResultInvocationTracker>();

        var result = await sender.Send(
            new DeleteUserCommand(UserId: ""),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().BeSameAs(PlatformErrors.InputInvalid);
        result.Error.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.Required && e.Pointer == "#/userId");
        tracker.WasInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task NonGeneric_EachValidatorFailure_SurfacesAsInputInvalid()
    {
        await using var sp = BuildNonGenericServices(svc =>
        {
            svc.AddValidator<DeleteUserCommand, UserIdRequiredValidator>();
            svc.AddValidator<DeleteUserCommand, UserIdLengthValidator>();
        });

        var sender = sp.GetRequiredService<ISender>();
        var tracker = sp.GetRequiredService<ResultInvocationTracker>();

        // Whitespace fails only UserIdRequiredValidator; a 25-character id fails only UserIdLengthValidator.
        var r1 = await sender.Send(
            new DeleteUserCommand(UserId: "   "),
            TestContext.Current.CancellationToken);
        r1.IsFailure.Should().BeTrue();
        r1.Error.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.Required && e.Pointer == "#/userId");

        var r2 = await sender.Send(
            new DeleteUserCommand(UserId: new string('x', 25)),
            TestContext.Current.CancellationToken);
        r2.IsFailure.Should().BeTrue();
        r2.Error.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.OutOfRange && e.Pointer == "#/userId");

        tracker.WasInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task NonGeneric_AllValidatorsPass_HandlerReturnsResultFail_BehaviorPropagates()
    {
        await using var sp = BuildNonGenericServices(svc =>
            svc.AddValidator<DeleteUserCommand, UserIdRequiredValidator>());

        var tracker = sp.GetRequiredService<ResultInvocationTracker>();
        tracker.ReturnFailure = true;
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(
            new DeleteUserCommand("u-1"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().BeSameAs(PlatformErrors.NotFound);
        tracker.WasInvoked.Should().BeTrue();
    }
}
