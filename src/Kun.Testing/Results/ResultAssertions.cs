using Kun.Core.Results;
using Kun.Errors;

namespace Kun.Testing.Results;

/// <summary>
/// Fluent assertion extensions for <see cref="Result"/> and
/// <see cref="Result{T}"/>. On mismatch, throws
/// <see cref="KunAssertionException"/> with a precise message that
/// includes the helper name, the expected value, and the actual
/// observed value. See ADR-012 §Error-message contract.
/// </summary>
public static class ResultAssertions
{
    // ── Result (non-generic) ────────────────────────────────────────────────

    /// <summary>
    /// Asserts the result is a success. Throws
    /// <see cref="KunAssertionException"/> carrying the observed
    /// <see cref="Error"/> on a failure input.
    /// </summary>
    public static void ShouldBeSuccess(this Result result)
    {
        if (result.IsSuccess) return;
        throw new KunAssertionException(
            $"ShouldBeSuccess — expected success but saw failure with Error {{ Code={result.Error.Code.Code}, Message={result.Error.Message} }}.");
    }

    /// <summary>Asserts the result is a failure and returns its <see cref="Error"/> for further inspection.</summary>
    public static Error ShouldBeFailure(this Result result)
    {
        if (result.IsFailure) return result.Error;
        throw new KunAssertionException("ShouldBeFailure — expected failure but saw success.");
    }

    /// <summary>
    /// Asserts the result is a failure whose error code (<see cref="ErrorCode.Code"/>)
    /// equals <paramref name="code"/> (ordinal comparison). Returns
    /// the <see cref="Error"/> for further inspection.
    /// </summary>
    public static Error ShouldHaveErrorCode(this Result result, string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (result.IsSuccess)
            throw new KunAssertionException($"ShouldHaveErrorCode — expected failure with Code={code} but saw success.");
        if (!string.Equals(result.Error.Code.Code, code, StringComparison.Ordinal))
            throw new KunAssertionException(
                $"ShouldHaveErrorCode — expected Code={code} but saw Code={result.Error.Code.Code}.");
        return result.Error;
    }

    /// <summary>
    /// Asserts the result is a failure carrying <paramref name="code"/>
    /// (compared by <see cref="ErrorCode.Code"/>). Returns the <see cref="Error"/>
    /// for further inspection.
    /// </summary>
    public static Error ShouldHaveErrorCode(this Result result, ErrorCode code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return result.ShouldHaveErrorCode(code.Code);
    }

    // ── Result<T> ───────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts the result is a success and returns its
    /// <see cref="Result{T}.Value"/> for further inspection. Throws
    /// <see cref="KunAssertionException"/> carrying the observed
    /// <see cref="Error"/> on a failure input.
    /// </summary>
    public static T ShouldBeSuccess<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value;
        throw new KunAssertionException(
            $"ShouldBeSuccess<{typeof(T).Name}> — expected success but saw failure with Error {{ Code={result.Error.Code.Code}, Message={result.Error.Message} }}.");
    }

    /// <summary>Asserts the result is a failure and returns its <see cref="Error"/> for further inspection.</summary>
    public static Error ShouldBeFailure<T>(this Result<T> result)
    {
        if (result.IsFailure) return result.Error;
        throw new KunAssertionException(
            $"ShouldBeFailure<{typeof(T).Name}> — expected failure but saw success (Value={result.Value}).");
    }

    /// <summary>
    /// Asserts the result is a failure whose error code (<see cref="ErrorCode.Code"/>)
    /// equals <paramref name="code"/> (ordinal comparison). Returns
    /// the <see cref="Error"/> for further inspection.
    /// </summary>
    public static Error ShouldHaveErrorCode<T>(this Result<T> result, string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (result.IsSuccess)
            throw new KunAssertionException(
                $"ShouldHaveErrorCode<{typeof(T).Name}> — expected failure with Code={code} but saw success (Value={result.Value}).");
        if (!string.Equals(result.Error.Code.Code, code, StringComparison.Ordinal))
            throw new KunAssertionException(
                $"ShouldHaveErrorCode<{typeof(T).Name}> — expected Code={code} but saw Code={result.Error.Code.Code}.");
        return result.Error;
    }

    /// <summary>
    /// Asserts the result is a failure carrying <paramref name="code"/>
    /// (compared by <see cref="ErrorCode.Code"/>). Returns the <see cref="Error"/>
    /// for further inspection.
    /// </summary>
    public static Error ShouldHaveErrorCode<T>(this Result<T> result, ErrorCode code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return result.ShouldHaveErrorCode(code.Code);
    }
}
