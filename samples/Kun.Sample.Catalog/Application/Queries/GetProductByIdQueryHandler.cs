using Dapper;
using Kun.Core.Cqrs;
using Npgsql;

namespace Kun.Sample.Catalog.Application.Queries;

/// <summary>
/// Read-side handler for <see cref="GetProductByIdQuery"/>.
/// Opens a connection from the read-side <see cref="NpgsqlDataSource"/> and queries it with
/// Dapper — no EF change tracking.
/// </summary>
public sealed class GetProductByIdQueryHandler(NpgsqlDataSource readDataSource)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    /// <inheritdoc/>
    public async ValueTask<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await readDataSource.OpenConnectionAsync(cancellationToken);

        // Schema-qualify — CatalogDbContext.DefaultSchema = "catalog", so the table
        // lives at "catalog"."catalog_products", and unqualified lookups depend on
        // search_path being set to include the schema (the read connection doesn't
        // get that).
        const string sql = """
            SELECT "Id",
                   "Name",
                   "Description",
                   "Price",
                   "CategoryId",
                   "Stock"
            FROM   "catalog"."catalog_products"
            WHERE  "Id" = @Id
              AND  "IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProductDto>(
            new CommandDefinition(sql, new { request.Id }, cancellationToken: cancellationToken));
    }
}
