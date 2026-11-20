using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EventBus.DependencyInjection;
using Kun.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Kun.Data;

namespace Kun.Capabilities.Tests;

// dotnet ef 的设计时入口：经 AddKunData 的同一装配取上下文（命名约定、租户过滤、Qian 迁移生成器），迁移模型与运行时一致。
// 不连接数据库；连接串只用于选定 Npgsql 提供程序。用法见仓库根 src/Kun.Data/README.md「迁移执行与 RLS 核查」：
//   dotnet ef migrations has-pending-model-changes --project packages/Qian.Capabilities --startup-project tests/Qian.Capabilities.Tests --context CapabilitiesDbContext
internal sealed class CapabilitiesDesignTimeFactory : IDesignTimeDbContextFactory<CapabilitiesDbContext>
{
    public CapabilitiesDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext());
        services.AddKunEventBus();
        services.AddKunData<CapabilitiesDbContext>((_, options) => options.UseNpgsql("Host=localhost;Database=design;Username=design").UseKunSchema(CapabilitiesDbContext.Schema));
        return services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<CapabilitiesDbContext>();
    }
}
