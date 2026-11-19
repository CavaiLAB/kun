using Kun.Core.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kun.MultiTenancy.DependencyInjection;

/// <summary>租户上下文的服务登记。</summary>
public static class KunMultiTenancyServiceCollectionExtensions
{
    /// <summary>为当前宿主提供可显式打开作用域的租户上下文。</summary>
    public static IServiceCollection AddKunMultiTenancy(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<AsyncLocalTenantContext>();
        services.TryAddSingleton<ITenantContext>(sp => sp.GetRequiredService<AsyncLocalTenantContext>());
        return services;
    }
}
