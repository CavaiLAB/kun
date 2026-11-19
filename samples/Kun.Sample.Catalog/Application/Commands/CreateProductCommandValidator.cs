using Kun.Errors;
using Kun.Validation;

namespace Kun.Sample.Catalog.Application.Commands;

/// <summary>
/// Validates <see cref="CreateProductCommand"/> before the handler runs. Each violation is a field-level
/// error code plus the JSON Pointer of the offending member (camelCase, as clients post it); the behavior
/// turns them into one INPUT_INVALID error.
/// </summary>
public sealed class CreateProductCommandValidator : IValidator<CreateProductCommand>
{
    /// <inheritdoc/>
    public ValueTask<ValidationResult> ValidateAsync(
        CreateProductCommand instance,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<FieldError>();

        if (string.IsNullOrWhiteSpace(instance.Name))
            errors.Add(new FieldError(PlatformErrors.Required, "#/name"));
        else if (instance.Name.Length > 200)
            errors.Add(new FieldError(PlatformErrors.OutOfRange, "#/name"));

        if (instance.Price < 0)
            errors.Add(new FieldError(PlatformErrors.OutOfRange, "#/price"));

        if (instance.CategoryId == Guid.Empty)
            errors.Add(new FieldError(PlatformErrors.Required, "#/categoryId"));

        return errors.Count == 0
            ? ValueTask.FromResult(ValidationResult.Success)
            : ValueTask.FromResult(ValidationResult.Failure(errors));
    }
}
