using Kun.Core.Cqrs;
using Kun.Core.Results;
using Kun.EntityFrameworkCore;
using Kun.Errors;
using Kun.Sample.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kun.Sample.Catalog.Application.Commands;

/// <summary>
/// Handles <see cref="AdjustStockCommand"/>: loads the product, adjusts stock,
/// and commits (domain event → integration event → outbox row in one transaction).
/// </summary>
public sealed class AdjustStockCommandHandler(
    CatalogDbContext db,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdjustStockCommand, Result>
{
    /// <inheritdoc/>
    public async ValueTask<Result> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            return Result.Fail(new Error(PlatformErrors.NotFound));

        product.AdjustStock(request.Delta);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
