using Kun.Errors;
using Kun.Mediator;

namespace Kun.Validation.Behaviors;

/// <summary>
/// Pipeline behavior that runs every registered <see cref="IValidator{T}"/> against the
/// incoming request and throws <see cref="ErrorException"/> if any report errors. The carried
/// <see cref="Error"/> has code <see cref="PlatformErrors.InputInvalid"/> (400) and lists the field errors
/// (at most 100) in <see cref="Error.Errors"/>; Kun.Errors.AspNetCore writes it as an ErrorDetail.
/// <para>
/// For Result-based semantics (returning a failed <c>Result</c> carrying the same error instead of
/// throwing), register <see cref="ValidationResultPipelineBehavior{TRequest, TValue}"/> instead.
/// </para>
/// </summary>
/// <typeparam name="TRequest">The request type being validated.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : Kun.Core.Cqrs.IRequest<TResponse>
{
    private readonly IValidator<TRequest>[] _validators;

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationPipelineBehavior{TRequest,TResponse}"/>.
    /// </summary>
    /// <param name="validators">All registered validators for <typeparamref name="TRequest"/>.</param>
    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators);
        _validators = validators.ToArray();
    }

    /// <inheritdoc/>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Length == 0) return await next();

        var allErrors = new List<FieldError>();
        for (var i = 0; i < _validators.Length; i++)
        {
            var result = await _validators[i].ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
            {
                for (var j = 0; j < result.Errors.Count; j++)
                    allErrors.Add(result.Errors[j]);
            }
        }

        if (allErrors.Count > 0)
            throw new ErrorException(ValidationErrors.ToError(allErrors));

        return await next();
    }
}
