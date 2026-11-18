using FluentAssertions;
using Kun.Errors;
using Xunit;

namespace Kun.Validation.Tests;

// ---------------------------------------------------------------------------
// Shared domain types
// ---------------------------------------------------------------------------

internal sealed record CreateUserCommand(string Email, int Age);

internal sealed class CreateUserValidator : IValidator<CreateUserCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(CreateUserCommand instance, CancellationToken ct = default)
    {
        var errors = new List<FieldError>();
        if (string.IsNullOrWhiteSpace(instance.Email))
            errors.Add(new FieldError(PlatformErrors.Required, ErrorPointer.Input("email")));
        else if (!instance.Email.Contains('@'))
            errors.Add(new FieldError(PlatformErrors.FormatInvalid, ErrorPointer.Input("email")));
        if (instance.Age < 0)
            errors.Add(new FieldError(PlatformErrors.OutOfRange, ErrorPointer.Input("age")));
        return ValueTask.FromResult(errors.Count == 0 ? ValidationResult.Success : ValidationResult.Failure(errors));
    }
}

// ---------------------------------------------------------------------------
// ValidationResult unit tests
// ---------------------------------------------------------------------------

public sealed class ValidationResultTests
{
    [Fact]
    public void ValidationResult_Success_IsValidTrueAndErrorsEmpty()
    {
        var result = ValidationResult.Success;

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_Failure_WithEmptyErrors_Throws()
    {
        var act = () => ValidationResult.Failure(Array.Empty<FieldError>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one error*");
    }

    [Fact]
    public void ValidationResult_Failure_WithErrors_IsValidFalseAndMatchesInput()
    {
        var errors = new[] { new FieldError(PlatformErrors.Required, ErrorPointer.Input("email")) };

        var result = ValidationResult.Failure(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeSameAs(errors[0]);
    }
}

// ---------------------------------------------------------------------------
// IValidator unit tests
// ---------------------------------------------------------------------------

public sealed class ValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public async Task Validator_ValidInput_ReturnsSuccess()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand("user@example.com", 25), TestContext.Current.CancellationToken);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_EmptyEmail_ReturnsRequiredAtEmail()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand("", 25), TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.Required && e.Pointer == "#/email");
    }

    [Fact]
    public async Task Validator_MissingAtSign_ReturnsFormatInvalidAtEmail()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand("notanemail", 25), TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.FormatInvalid && e.Pointer == "#/email");
    }

    [Fact]
    public async Task Validator_NegativeAge_ReturnsOutOfRangeAtAge()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand("user@example.com", -1), TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == PlatformErrors.OutOfRange && e.Pointer == "#/age");
    }

    [Fact]
    public async Task Validator_MultipleFaults_AccumulatesAllErrors()
    {
        var result = await _validator.ValidateAsync(new CreateUserCommand("", -5), TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.Pointer).Should().Equal("#/email", "#/age");
    }
}
