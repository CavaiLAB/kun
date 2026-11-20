using Kun.Plugin.Sdk;
using Kun.Surfaces.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Kun.Capabilities;

/// <summary>Capabilities 的宿主装配。</summary>
public static class KunCapabilitiesExtensions
{
    /// <summary>
    /// 登记能力契约的登记期检查（<see cref="ICapabilityContractCheck"/>）：组件 <c>ProvideCapability</c> 时生成目录条目并预编译输入校验器，
    /// 不合格的契约在登记阶段以 <see cref="CapabilityContractException"/> 拒绝；目录与通用执行入口复用同一份缓存。
    /// 宿主须已登记 <see cref="ISurfaceReader"/> 与 <see cref="IToolContractHasher"/>。
    /// </summary>
    public static IServiceCollection AddKunCapabilities(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton(sp => new CapabilityContractCheck(sp.GetRequiredService<ISurfaceReader>(), sp.GetRequiredService<IToolContractHasher>(),
            sp.GetService<ILoggerFactory>()?.CreateLogger<CapabilityContractCheck>()));
        services.TryAddSingleton<ICapabilityContractCheck>(sp => sp.GetRequiredService<CapabilityContractCheck>());
        return services;
    }
}
