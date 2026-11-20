using Kun.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kun.Configuration.Platform;

/// <summary>固定配置源顺序，复用 Kun 的协议、缓存、签名与重载实现。</summary>
public static class KunConfigurationExtensions
{
    /// <summary>向独立宿主登记同一配置根与只读服务；配置根的释放仍由创建方负责。</summary>
    public static IServiceCollection AddKunConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var entry = services.LastOrDefault(service => service.ServiceType == typeof(ErrorCatalog));
        var catalog = entry?.ImplementationInstance as ErrorCatalog;
        if (entry is not null && catalog is null) throw new InvalidOperationException("ErrorCatalog 必须以实例方式登记。");
        if (catalog is null) { catalog = ErrorCatalog.CreateDefault(); services.AddSingleton(catalog); }
        ConfigErrors.Register(catalog);
        services.AddSingleton(configuration);
        if (!services.Any(service => service.ServiceType == typeof(ConfigReloadNotifier)))
        {
            services.AddSingleton<ConfigReloadNotifier>();
            services.AddSingleton<IObserver<OptionsReloadFailure>>(sp => sp.GetRequiredService<ConfigReloadNotifier>());
        }
        services.AddSingleton<IFeatureFlags>(sp => new FeatureFlagsService(configuration, sp.GetRequiredService<ConfigReloadNotifier>(),
            sp.GetService<ILoggerFactory>()?.CreateLogger("Kun.Configuration.Platform.Flags") ?? NullLogger.Instance));
        services.AddSingleton<IConfigService>(sp => new ConfigurationService(configuration, sp.GetRequiredService<IFeatureFlags>(), sp.GetService<ISecretResolverFactory>()));
        return services;
    }

    internal static ErrorException Invalid(string key) => new(new Error(ConfigErrors.BootstrapInvalid,
        new Dictionary<string, string> { ["key"] = key }));
}
