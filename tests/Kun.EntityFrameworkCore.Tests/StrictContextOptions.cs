using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EntityFrameworkCore.Interceptors;
using Kun.EntityFrameworkCore.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kun.EntityFrameworkCore.Tests;

/// <summary>
/// 不经 Kun.Data，只用内核的部件装出 <see cref="KunStrictDbContext"/> 构造时要求的整套配置：
/// 行级安全双拦截器、软删在审计字段之前、租户字段、应用角色、模型缓存键与模型约定。连接串只占位，不连库。
/// </summary>
internal static class StrictContextOptions
{
    public static DbContextOptionsBuilder Builder(string schema)
    {
        var tenant = NullTenantContext.Instance;
        var user = AnonymousCurrentUser.Instance;
        var builder = new DbContextOptionsBuilder();
        builder.UseNpgsql("Host=localhost;Database=unused").UseKunSchema(schema);
        builder.UseKunDataModel()
            .ReplaceService<IModelCacheKeyFactory, KunMultiTenantModelCacheKeyFactory>()
            .AddInterceptors(
                new TenantRlsConnectionInterceptor(tenant, NullLogger<TenantRlsConnectionInterceptor>.Instance),
                new TenantRlsTransactionInterceptor(tenant),
                new KunSoftDeleteInterceptor(user, TimeProvider.System),
                new AuditFieldsSaveChangesInterceptor(user, TimeProvider.System),
                new KunTenantSaveChangesInterceptor(tenant, NullLogger<KunTenantSaveChangesInterceptor>.Instance),
                new KunApplicationRoleInterceptor());
        return builder;
    }
}
