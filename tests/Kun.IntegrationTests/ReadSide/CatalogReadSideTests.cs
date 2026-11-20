using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.Extensions;
using Kun.Sample.Catalog.Application.Queries;
using Kun.Sample.Catalog.Domain;
using Kun.Sample.Catalog.Infrastructure;
using Kun.Testing.Containers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Kun.IntegrationTests.ReadSide;

// Catalog 样例的读侧：查询处理器从读侧 NpgsqlDataSource 取连接、用 Dapper 查询。
// 写侧落库的商品按 DTO 原样读回；软删除的与不存在的商品都读不到。
public sealed class CatalogReadSideTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task GetProductById_ReadsWrittenProductThroughDataSource_AndHidesSoftDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .UseKunSchema("catalog")
            .Options;

        var visible = Product.Create(Guid.NewGuid(), "可见商品", "读侧要读到", 12.5m, Guid.NewGuid(), 7);
        var deleted = Product.Create(Guid.NewGuid(), "已删商品", "读侧读不到", 3m, Guid.NewGuid(), 1);
        deleted.Delete();
        await using (var db = new CatalogDbContext(options, NullTenantContext.Instance))
        {
            await db.Database.EnsureCreatedAsync(ct);
            db.Products.AddRange(visible, deleted);
            await db.SaveChangesAsync(ct);
        }

        await using var readDataSource = NpgsqlDataSource.Create(postgres.ConnectionString);
        var handler = new GetProductByIdQueryHandler(readDataSource);

        var dto = await handler.Handle(new GetProductByIdQuery(visible.Id), ct);

        Assert.Equal(new ProductDto(visible.Id, "可见商品", "读侧要读到", 12.5m, visible.CategoryId, 7), dto);
        Assert.Null(await handler.Handle(new GetProductByIdQuery(deleted.Id), ct));
        Assert.Null(await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), ct));
    }
}
