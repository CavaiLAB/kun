using Kun.Errors;

namespace Kun.Validation;

/// <summary>
/// Result of running a validator against an instance. <see cref="IsValid"/> is true iff
/// <see cref="Errors"/> is empty. Each error is a <see cref="FieldError"/>: a field-level error code
/// registered in the error catalog (such as <see cref="PlatformErrors.Required"/>) plus the JSON Pointer
/// of the offending input (<see cref="ErrorPointer.Input"/>, e.g. <c>#/items/0/qty</c>).
/// </summary>
public readonly record struct ValidationResult
{
    /// <summary>Errors accumulated by the validator. Empty collection means valid.</summary>
    public IReadOnlyList<FieldError> Errors { get; }

    /// <summary>True iff <see cref="Errors"/> is empty.</summary>
    public bool IsValid => Errors.Count == 0;

    private ValidationResult(IReadOnlyList<FieldError> errors)
    {
        Errors = errors;
    }

    /// <summary>A successful validation with no errors.</summary>
    public static ValidationResult Success { get; } = new(Array.Empty<FieldError>());

    /// <summary>Create a failing validation result from the given errors.</summary>
    public static ValidationResult Failure(IReadOnlyList<FieldError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
            throw new ArgumentException("Failure must carry at least one error.", nameof(errors));
        return new ValidationResult(errors);
    }
}

/// <summary>Turns the collected field errors into the single <see cref="Error"/> both validation behaviors report.</summary>
internal static class ValidationErrors
{
    /// <summary>ErrorDetail 的 <c>errors[]</c> 至多 100 项（contracts/v0）。</summary>
    internal const int Max = 100;

    /// <summary><see cref="PlatformErrors.InputInvalid"/>（400）带逐项字段错误，按校验器的顺序保留前 100 项。</summary>
    internal static Error ToError(List<FieldError> errors) => new(PlatformErrors.InputInvalid)
    {
        Errors = errors.Count > Max ? errors.GetRange(0, Max) : errors,
    };
}
