using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EntityFrameworkCore.Interceptors;
using Kun.EntityFrameworkCore.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests;

// 严格基类构造时逐项核对装配：整套装齐可以构造；只缺其中一项时拒绝，异常文字指出缺的是哪一项。规则在构造时校验，不连库。
public sealed class KunStrictAssemblyRuleTests
{
    [Fact]
    public void CompleteAssembly_Constructs()
    {
        using var context = new StrictContext(Options(omit: null));
        Assert.NotNull(context);
    }

    [Theory]
    [InlineData("rls-connection", "必须同时装配 RLS 连接版与事务版拦截器。")]
    [InlineData("rls-transaction", "必须同时装配 RLS 连接版与事务版拦截器。")]
    [InlineData("audit-fields", "必须装配 Kun 的审计字段拦截器。")]
    [InlineData("soft-delete", "必须在 Kun 审计字段拦截器之前装配 Kun 软删除拦截器。")]
    [InlineData("tenant-field", "必须装配 Kun 的租户字段拦截器。")]
    [InlineData("application-role", "必须装配在线应用角色校验拦截器。")]
    [InlineData("model-cache-key", "必须使用 KunMultiTenantModelCacheKeyFactory。")]
    [InlineData("model-customizer", "必须装配 Kun 数据模型约定。")]
    public void OnePieceMissing_IsRejectedNamingThatPiece(string omit, string message)
    {
        var error = Assert.Throws<KunDataMisconfiguredException>(() => new StrictContext(Options(omit)));
        Assert.Contains(message, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SoftDeleteAfterAuditFields_IsRejected()
    {
        var error = Assert.Throws<KunDataMisconfiguredException>(
            () => new StrictContext(Options(omit: null, softDeleteFirst: false)));
        Assert.Contains("必须在 Kun 审计字段拦截器之前装配 Kun 软删除拦截器。", error.Message, StringComparison.Ordinal);
    }

    private static DbContextOptions Options(string? omit, bool softDeleteFirst = true)
    {
        var tenant = NullTenantContext.Instance;
        var user = AnonymousCurrentUser.Instance;
        var builder = new DbContextOptionsBuilder();
        builder.UseNpgsql("Host=localhost;Database=unused").UseKunSchema("catalog");
        if (omit != "model-customizer") builder.UseKunDataModel();
        if (omit != "model-cache-key") builder.ReplaceService<IModelCacheKeyFactory, KunMultiTenantModelCacheKeyFactory>();
        var interceptors = new List<(string Name, IInterceptor Interceptor)>
        {
            ("rls-connection", new TenantRlsConnectionInterceptor(tenant, NullLogger<TenantRlsConnectionInterceptor>.Instance)),
            ("rls-transaction", new TenantRlsTransactionInterceptor(tenant)),
            ("soft-delete", new KunSoftDeleteInterceptor(user, TimeProvider.System)),
            ("audit-fields", new AuditFieldsSaveChangesInterceptor(user, TimeProvider.System)),
            ("tenant-field", new KunTenantSaveChangesInterceptor(tenant, NullLogger<KunTenantSaveChangesInterceptor>.Instance)),
            ("application-role", new KunApplicationRoleInterceptor()),
        };
        if (!softDeleteFirst) (interceptors[2], interceptors[3]) = (interceptors[3], interceptors[2]);
        builder.AddInterceptors(interceptors.Where(item => item.Name != omit).Select(item => item.Interceptor));
        return builder.Options;
    }

    private sealed class StrictContext(DbContextOptions options) : KunStrictDbContext(options, NullTenantContext.Instance);
}
