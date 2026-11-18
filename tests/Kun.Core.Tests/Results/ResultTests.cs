using FluentAssertions;
using Kun.Core.Results;
using Kun.Errors;
using Kun.Testing.Results;
using Xunit;

namespace Kun.Core.Tests.Results;

public class ResultTests
{
    private static readonly Error NotFound = new(PlatformErrors.NotFound);
    private static readonly Error Conflict = new(PlatformErrors.Conflict);

    // ── Result (non-generic) ────────────────────────────────────────────────

    [Fact]
    public void Ok_IsSuccessTrue()
    {
        // Act
        var result = Result.Ok();

        // Assert — smoke-adopting Kun.Testing.Results.ResultAssertions (ADR-012 AC-7)
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Ok_Error_Throws()
    {
        // Act
        var act = () => Result.Ok().Error;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_IsSuccessFalseAndErrorMatches()
    {
        // Act
        var result = Result.Fail(NotFound);

        // Assert — smoke-adopting Kun.Testing.Results.ResultAssertions (ADR-012 AC-7)
        result.ShouldHaveErrorCode("NOT_FOUND").Should().BeSameAs(NotFound);
    }

    [Fact]
    public void Fail_WithNullError_Throws()
    {
        // Act
        var act = () => Result.Fail(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Default_IsFailureWithoutError_ErrorThrows()
    {
        // Act
        var act = () => default(Result).Error;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToString_DoesNotThrowAndShowsTheErrorCode()
    {
        Result.Ok().ToString().Should().Contain("IsSuccess = True");
        Result.Fail(NotFound).ToString().Should().Contain("NOT_FOUND");
        Result<int>.Ok(42).ToString().Should().Contain("Value = 42");
        Result<int>.Fail(Conflict).ToString().Should().Contain("CONFLICT");
    }

    [Fact]
    public void Equality_ComparesOutcomeAndCarriedError()
    {
        Result.Ok().Should().Be(Result.Ok());
        Result.Fail(NotFound).Should().Be(Result.Fail(NotFound));
        Result.Fail(NotFound).Should().NotBe(Result.Fail(Conflict));
        Result<int>.Ok(1).Should().Be(Result<int>.Ok(1));
        Result<int>.Ok(1).Should().NotBe(Result<int>.Fail(NotFound));
    }

    // ── Result<T> ───────────────────────────────────────────────────────────

    [Fact]
    public void ResultT_Ok_ValueAccessibleAndIsSuccess()
    {
        // Act
        var result = Result<int>.Ok(42);

        // Assert — ShouldBeSuccess<T> returns the value so chaining is terse
        result.ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public void ResultT_Fail_IsSuccessFalseAndErrorMatches()
    {
        // Act
        var result = Result<int>.Fail(NotFound);

        // Assert — the error code assert documents the expected shape in one line
        result.ShouldHaveErrorCode(PlatformErrors.NotFound).Should().BeSameAs(NotFound);
    }

    [Fact]
    public void ResultT_Value_OnFailure_Throws()
    {
        // Arrange
        var result = Result<int>.Fail(NotFound);

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultT_Error_OnSuccess_Throws()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var act = () => result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultT_ImplicitConversion_FromValue_IsSuccess()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_ImplicitConversion_FromError_IsFailure()
    {
        // Act
        Result<int> result = NotFound;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(NotFound);
    }

    [Fact]
    public void EnsureSuccess_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<string>.Ok("hello");

        // Act
        var value = result.EnsureSuccess();

        // Assert
        value.Should().Be("hello");
    }

    [Fact]
    public void EnsureSuccess_OnFailure_ThrowsErrorException_WithMatchingError()
    {
        // Arrange
        var result = Result<string>.Fail(Conflict);

        // Act
        var act = () => result.EnsureSuccess();

        // Assert
        act.Should().Throw<ErrorException>()
            .Which.Error.Should().BeSameAs(Conflict);
    }
}
