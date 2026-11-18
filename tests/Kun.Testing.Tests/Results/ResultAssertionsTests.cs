using FluentAssertions;
using Kun.Core.Results;
using Kun.Errors;
using Kun.Testing.Results;
using Xunit;

namespace Kun.Testing.Tests.Results;

public class ResultAssertionsTests
{
    private static readonly Error NotFound = new(PlatformErrors.NotFound);

    // ── Result (non-generic) ────────────────────────────────────────────────

    [Fact]
    public void ShouldBeSuccess_OnSuccess_DoesNotThrow()
    {
        var act = () => Result.Ok().ShouldBeSuccess();
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeSuccess_OnFailure_ThrowsWithErrorDetails()
    {
        var act = () => Result.Fail(NotFound).ShouldBeSuccess();
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldBeSuccess*expected success*failure*Code=NOT_FOUND*Message=*");
    }

    [Fact]
    public void ShouldBeFailure_OnFailure_ReturnsErrorForChaining()
    {
        var err = Result.Fail(NotFound).ShouldBeFailure();
        err.Should().BeSameAs(NotFound);
    }

    [Fact]
    public void ShouldBeFailure_OnSuccess_Throws()
    {
        var act = () => Result.Ok().ShouldBeFailure();
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldBeFailure*expected failure*saw success*");
    }

    [Fact]
    public void ShouldHaveErrorCode_OnMatchingFailure_ReturnsError()
    {
        var err = Result.Fail(NotFound).ShouldHaveErrorCode("NOT_FOUND");
        err.Code.Should().BeSameAs(PlatformErrors.NotFound);
    }

    [Fact]
    public void ShouldHaveErrorCode_OnMismatchedCode_ThrowsWithExpectedAndActual()
    {
        var act = () => Result.Fail(NotFound).ShouldHaveErrorCode("CONFLICT");
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldHaveErrorCode*expected*CONFLICT*saw*NOT_FOUND*");
    }

    [Fact]
    public void ShouldHaveErrorCode_OnSuccess_Throws()
    {
        var act = () => Result.Ok().ShouldHaveErrorCode("ANY_CODE");
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldHaveErrorCode*expected failure*ANY_CODE*saw success*");
    }

    [Fact]
    public void ShouldHaveErrorCode_NullCode_Throws()
    {
        var act = () => Result.Fail(NotFound).ShouldHaveErrorCode((string)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldHaveErrorCode_ByErrorCode_OnMatch_ReturnsError()
    {
        var err = Result.Fail(NotFound).ShouldHaveErrorCode(PlatformErrors.NotFound);
        err.Should().BeSameAs(NotFound);
    }

    [Fact]
    public void ShouldHaveErrorCode_ByErrorCode_OnMismatch_ThrowsWithBothCodes()
    {
        var act = () => Result.Fail(NotFound).ShouldHaveErrorCode(PlatformErrors.Conflict);
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldHaveErrorCode*expected*CONFLICT*saw*NOT_FOUND*");
    }

    [Fact]
    public void ShouldHaveErrorCode_ByErrorCode_NullCode_Throws()
    {
        var act = () => Result.Fail(NotFound).ShouldHaveErrorCode((ErrorCode)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Result<T> ───────────────────────────────────────────────────────────

    [Fact]
    public void ShouldBeSuccess_T_OnSuccess_ReturnsValue()
    {
        var value = Result<int>.Ok(42).ShouldBeSuccess();
        value.Should().Be(42);
    }

    [Fact]
    public void ShouldBeSuccess_T_OnFailure_ThrowsWithTypeNameAndErrorDetails()
    {
        var act = () => Result<int>.Fail(NotFound).ShouldBeSuccess();
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldBeSuccess<Int32>*NOT_FOUND*");
    }

    [Fact]
    public void ShouldBeFailure_T_OnFailure_ReturnsError()
    {
        var err = Result<int>.Fail(NotFound).ShouldBeFailure();
        err.Should().BeSameAs(NotFound);
    }

    [Fact]
    public void ShouldBeFailure_T_OnSuccess_ThrowsWithValueInMessage()
    {
        var act = () => Result<int>.Ok(99).ShouldBeFailure();
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldBeFailure<Int32>*saw success*99*");
    }

    [Fact]
    public void ShouldHaveErrorCode_T_OnMatch_ReturnsError()
    {
        var err = Result<string>.Fail(NotFound).ShouldHaveErrorCode("NOT_FOUND");
        err.Should().BeSameAs(NotFound);
    }

    [Fact]
    public void ShouldHaveErrorCode_T_OnMismatch_Throws()
    {
        var act = () => Result<string>.Fail(NotFound).ShouldHaveErrorCode("CONFLICT");
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldHaveErrorCode<String>*CONFLICT*NOT_FOUND*");
    }

    [Fact]
    public void ShouldHaveErrorCode_T_ByErrorCode_OnMatch_ReturnsError()
    {
        var err = Result<int>.Fail(NotFound).ShouldHaveErrorCode(PlatformErrors.NotFound);
        err.Code.Should().BeSameAs(PlatformErrors.NotFound);
    }

    [Fact]
    public void ShouldHaveErrorCode_T_ByErrorCode_OnSuccess_ThrowsWithValueInMessage()
    {
        var act = () => Result<int>.Ok(7).ShouldHaveErrorCode(PlatformErrors.InputInvalid);
        act.Should().Throw<KunAssertionException>()
            .WithMessage("*ShouldHaveErrorCode<Int32>*INPUT_INVALID*saw success*7*");
    }
}
