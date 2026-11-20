using Kun.Core.MultiTenancy;
using Kun.Data;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EventBus.DependencyInjection;
using Kun.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Scheduler.Tests;

internal sealed class SchedulerDesignTimeFactory : IDesignTimeDbContextFactory<SchedulerDbContext>
{
    public SchedulerDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection(); services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext()); services.AddKunEventBus();
        services.AddKunData<SchedulerDbContext>((_, options) => options.UseNpgsql("Host=localhost;Database=design;Username=design").UseKunSchema(SchedulerDbContext.Schema));
        return services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<SchedulerDbContext>();
    }
}
