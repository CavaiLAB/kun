> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Read-Side Patterns: Writing Query Handlers

:::info 发布版本来源
本页由本仓库的发布标签生成，源文件为 [`docs/guides/read-side-patterns.md`](https://github.com/CavaiLAB/kun/blob/v2026.11.24/docs/guides/read-side-patterns.md)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

This guide covers the recommended patterns for query handlers in Kun. It assumes you understand
the write/read split; for the reasoning behind that split see
[docs/design/why-cqrs.md](/sdk/kun/architecture/why-cqrs.md).

***

## The Write/Read Rule

Query handlers must not use `KunDbContext`, `IUnitOfWork`, or any EF Core type. They open a
connection from an `NpgsqlDataSource`, run raw SQL with Dapper, and return flat DTOs. Kun does not
ship a read-side package: the application references Npgsql and Dapper directly. The rule is not
enforced by the build, so reviewers check that query handlers take no EF Core types.

The short version:

- Command handler: inject `IRepository<T>` or `KunDbContext` (via EF). Transactional.
- Query handler: inject `NpgsqlDataSource`. No change tracking, no aggregates.

***

## Recommended Query Handler Pattern

The standard shape for a Kun query handler:

```csharp
using Dapper;
using Kun.Core.Cqrs;
using Npgsql;

public sealed class GetProductByIdQueryHandler(NpgsqlDataSource readDataSource)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await readDataSource.OpenConnectionAsync(cancellationToken);

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
```

This is the handler from
`samples/Kun.Sample.Catalog/Application/Queries/GetProductByIdQueryHandler.cs`.

Key points:

- **`NpgsqlDataSource.OpenConnectionAsync`** returns an open connection from the data source's
  pool. Dispose it with `await using`; disposing returns it to the pool.
- **Parameterized SQL** via the anonymous object `new { request.Id }`. Never interpolate values
  directly into the SQL string.
- **Pass the cancellation token** through `CommandDefinition`, so a cancelled request also cancels
  the query.
- **Column names match the DTO property names** exactly (case-sensitive on PostgreSQL). The
  catalog's EF mapping keeps PascalCase column names, so the query selects them quoted; with
  snake_case columns, alias each one with `AS "PropertyName"`.
- **Schema-qualify table names.** The catalog tables live in the `catalog` schema, and the read
  connection does not set `search_path`.
- **Soft-delete filter** (`"IsDeleted" = false`) applied directly in SQL. There is no EF Core
  global filter on the read side — the SQL is the filter.
- The handler returns `null` when no row matches; the endpoint turns that into `Results.NotFound()`,
  and `Kun.Errors.AspNetCore` (`UseKunErrors()`) writes the empty 404 as an ErrorDetail with the
  error code `NOT_FOUND`.

***

## Registering the Read-Side Data Source

Register one `NpgsqlDataSource` for the read side in `Program.cs`:

```csharp
var readCs = builder.Configuration.GetConnectionString("CatalogDbRead") ?? writeCs;

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(readCs));
```

- The container owns the data source and disposes it on shutdown.
- Give the read side its own connection string to point it at a replica; the catalog sample
  falls back to the write connection when `ConnectionStrings:CatalogDbRead` is not set.
- If the application talks to more than one PostgreSQL database, register each data source under
  a key (`AddKeyedSingleton`) and resolve it with `[FromKeyedServices]`, so a handler cannot pick
  up the wrong one.

***

## Pagination

Kun does not ship paging types. Define the request and result shapes your API needs next to the
query:

```csharp
public sealed record ListProductsQuery(int Page = 1, int PageSize = 20) : IQuery<ProductPage>;

public sealed record ProductSummaryDto(Guid Id, string Name, decimal Price, int Stock);

public sealed record ProductPage(IReadOnlyList<ProductSummaryDto> Items, int TotalCount);
```

Run the count and the page on the same connection. Dapper's `QueryMultiple` does both in one
round trip:

```csharp
public sealed class ListProductsQueryHandler(NpgsqlDataSource readDataSource)
    : IRequestHandler<ListProductsQuery, ProductPage>
{
    public async ValueTask<ProductPage> Handle(
        ListProductsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await readDataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(*) FROM "catalog"."catalog_products" WHERE "IsDeleted" = false;
            SELECT "Id", "Name", "Price", "Stock"
            FROM   "catalog"."catalog_products"
            WHERE  "IsDeleted" = false
            ORDER  BY "Name", "Id"
            LIMIT  @PageSize OFFSET @Offset;
            """;

        var parameters = new { request.PageSize, Offset = (request.Page - 1) * request.PageSize };
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalCount = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<ProductSummaryDto>()).ToArray();
        return new ProductPage(items, totalCount);
    }
}
```

- `Page` is 1-based; validate `Page` and `PageSize` in the query's validator.
- Order by a unique key last (`"Id"` above), so rows do not move between pages.
- For large tables, prefer keyset pagination (`WHERE ("Name", "Id") > (@AfterName, @AfterId)`)
  over `OFFSET`, which reads and discards every skipped row.

***

## DTO Design

DTOs on the read side are intentionally flat. They are shaped for the API consumer, not for the
domain model.

Rules:

- **No aggregate nesting.** A `ProductDto` does not contain an `OrderDto`. If a view needs data
  from two aggregates, join the tables in SQL.
- **No domain types.** DTOs use primitive types (`string`, `decimal`, `Guid`, `int`) or simple
  value types. Never expose `Money`, `Address`, or other value objects from the domain layer in a
  DTO — that would create a dependency on `Kun.Core` in the read result.
- **Name for the consumer, not the table.** Column aliases can differ from table column names.
  Shape the DTO for what the API consumer needs to display or act on.
- **Immutable records.** Use `sealed record` with positional properties for DTOs. Records are
  value-comparable, serializable, and clearly signal "data container, not entity".

The `ProductDto` from the catalog sample:

```csharp
// samples/Kun.Sample.Catalog/Application/Queries/ProductDto.cs
public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    int Stock);
```

The query from `GetProductByIdQuery`:

```csharp
// samples/Kun.Sample.Catalog/Application/Queries/GetProductByIdQuery.cs
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto?>;
```

`IQuery<TResponse>` is a marker interface in `Kun.Core.Cqrs` that extends `IRequest<TResponse>`.
Using `IQuery` rather than `IRequest` directly signals intent and allows pipeline behaviors to
discriminate between commands and queries at registration time.

***

## Anti-Patterns

### Do not inject KunDbContext in a query handler

```csharp
// Wrong — do not do this
public sealed class GetProductByIdQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(...)
    {
        return await db.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto(...))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

Problems with this pattern:

1. `CatalogDbContext` inherits `KunDbContext` which has global query filters, change tracking, and
   concurrency checks active. A query handler does not need any of this and pays the overhead.
2. `Kun.EntityFrameworkCore` is not AOT-compatible (no `IsAotCompatible` in its csproj). Injecting it into a
   query handler that runs in an AOT-published service will fail at publish time with trim warnings
   or at runtime with reflection errors.
3. `UnitOfWorkPipelineBehavior` calls `SaveChangesAsync` after every registered handler in the
   write pipeline. If a query handler is registered in the same pipeline stack as a command
   handler that has `UnitOfWorkPipelineBehavior`, the query will trigger a `SaveChangesAsync`
   call. This is harmless but wasteful — and it signals a pipeline configuration mistake.

### Do not load aggregates and project at runtime

```csharp
// Wrong — loads the entire aggregate graph, then discards most of it
var product = await _repository.GetByIdAsync(request.Id, ct);
return new ProductDto(product.Id, product.Name, ...);
```

Problems:

1. Loading an aggregate through `IRepository<T>` uses EF Core change tracking. Every property on
   the entity is loaded even if the DTO only needs three of them.
2. If the aggregate has navigation properties (owned entities, collections), EF Core may issue
   additional queries. The N+1 problem manifests here.
3. The aggregate is loaded into memory, partially read, then immediately discarded. The CPU and
   allocation cost is wasted.
4. Kills AOT compatibility for the same reasons as injecting `KunDbContext` directly.

Write the SQL to return only the columns you need. If the SQL feels verbose, that verbosity is
honest — it reflects exactly what data is fetched.

***

## AOT Note

Dapper's classic API (`QueryAsync<T>`, `QuerySingleOrDefaultAsync<T>`) builds the row reader for
`T` with runtime reflection, so it is not trim- or AOT-safe. Kun does not wrap Dapper. If a
service that runs query handlers is published with Native AOT, choose one of these in that
service's own project:

- add Dapper's source generator (the `Dapper.AOT` package) and mark the query methods with
  `[DapperAot]`, so the reader is generated at build time; or
- read rows with `NpgsqlDataReader` directly and construct the DTOs by hand.

Either way, `dotnet publish -p:PublishAot=true` with warnings as errors catches a reflection-based
read path before deployment. The catalog sample is not AOT-published and uses the classic API.
