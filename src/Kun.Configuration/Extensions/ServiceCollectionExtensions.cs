using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Kun.Configuration.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Kun.Configuration.Extensions;

/// <summary>
/// Service-collection extensions that register Kun typed-options
/// validation.
/// </summary>
public static class KunConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TOptions"/> bound to
    /// <paramref name="section"/>, validates DataAnnotations
    /// attributes at host start, and fails fast with
    /// section-qualified messages.
    /// </summary>
    /// <typeparam name="TOptions">
    /// Options type; must be a reference type. Annotated with
    /// <see cref="DynamicallyAccessedMemberTypes.All"/> so the
    /// trimmer preserves the properties DataAnnotations walks.
    /// </typeparam>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="section">Configuration section path (e.g. <c>"Authentik"</c>, <c>"Database:Primary"</c>).</param>
    /// <returns>The underlying <see cref="OptionsBuilder{TOptions}"/> for further chaining.</returns>
    [RequiresUnreferencedCode("DataAnnotations-based options validation walks properties by reflection.")]
    [RequiresDynamicCode("DataAnnotations-based options validation walks properties by reflection.")]
    [UnconditionalSuppressMessage("ConfigurationBinder", "SYSLIB1100",
        Justification = "Consumer opts into reflection-based binding by calling this annotated entry point.")]
    [UnconditionalSuppressMessage("ConfigurationBinder", "SYSLIB1101",
        Justification = "Consumer opts into reflection-based binding by calling this annotated entry point.")]
    [UnconditionalSuppressMessage("ConfigurationBinder", "SYSLIB1104",
        Justification = "Consumer opts into reflection-based binding by calling this annotated entry point.")]
    public static OptionsBuilder<TOptions> AddKunOptions<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this IServiceCollection services,
        string section)
        where TOptions : class
    {
        return services.AddKunOptions<TOptions>(section, critical: true);
    }
    /// <summary>关键配置在启动时校验；可选配置在首次读取时校验，未使用时不阻断宿主启动。</summary>
    [RequiresUnreferencedCode("配置绑定与 DataAnnotations 校验需要反射。")]
    [RequiresDynamicCode("配置绑定与 DataAnnotations 校验需要反射。")]
    public static OptionsBuilder<TOptions> AddKunOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this IServiceCollection services, string section, bool critical) where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(section);

        services.AddSingleton<IValidateOptions<TOptions>>(sp =>
        {
            var validators = sp.GetServices<IKunConfigurationValidator<TOptions>>();
            return new KunOptionsValidator<TOptions>(section, validators);
        });

        services.AddLogging();
        RegisterMonitor<TOptions>(services, section, null);

        var builder = services.AddOptions<TOptions>().BindConfiguration(section);
        return critical ? builder.ValidateOnStart() : builder;
    }

#pragma warning disable RS0026 // 两个既有签名的第二个必要参数分别为 JsonTypeInfo<T> 和绑定委托。
    /// <summary>绑定并校验配置，同时登记基于源生成 JSON 元数据的 Schema 2020-12。</summary>
    [RequiresUnreferencedCode("与既有 AddKunOptions 相同，绑定及 DataAnnotations 需要反射。Schema 导出本身使用源生成元数据。")]
    [RequiresDynamicCode("与既有 AddKunOptions 相同，调用方显式选择反射绑定。")]
    public static OptionsBuilder<TOptions> AddKunOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this IServiceCollection services, string section, JsonTypeInfo<TOptions> typeInfo, bool critical = true) where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        var builder = services.AddKunOptions<TOptions>(section, critical);
        services.AddSingleton(new OptionsSchema(section, typeInfo));
        return builder;
    }

    /// <summary>
    /// 登记 <typeparamref name="T"/> 绑定到 <paramref name="section"/>：<paramref name="bind"/> 收到该配置节；校验器照常以 IValidateOptions 登记
    /// （DataAnnotations 可用 OptionsValidator 源生成器）。critical 为 true 时宿主启动即校验，否则首次读取时校验；给出 <paramref name="schema"/> 时同时登记
    /// JSON Schema 2020-12 导出。同一类型只能登记一次。
    /// </summary>
    public static OptionsBuilder<T> AddKunOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
        this IServiceCollection services, string section, Action<IConfiguration, T> bind, bool critical = true,
        JsonTypeInfo<T>? schema = null, string? plugin = null) where T : class
#pragma warning restore RS0026
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        ArgumentNullException.ThrowIfNull(bind);
        if (plugin is not null) ArgumentException.ThrowIfNullOrWhiteSpace(plugin);
        if (services.Any(service => service.ServiceType == typeof(OptionsMarker<T>))) throw new InvalidOperationException("同一配置类型不能重复登记。");
        services.AddSingleton(new OptionsMarker<T>());
        services.AddSingleton<IOptionsChangeTokenSource<T>>(sp => new SectionChangeTokenSource<T>(sp.GetRequiredService<IConfiguration>().GetSection(section)));
        RegisterMonitor<T>(services, section, plugin);
        if (schema is not null) services.AddSingleton(new OptionsSchema(section, schema, plugin));
        var builder = services.AddOptions<T>().Configure<IConfiguration>((options, root) => bind(root.GetSection(section), options));
        return critical ? builder.ValidateOnStart() : builder;
    }

    private static void RegisterMonitor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
        IServiceCollection services, string section, string? plugin) where T : class
    {
        services.TryAddSingleton<ReloadFailureSource>();
        services.TryAddSingleton<IObservable<OptionsReloadFailure>>(sp => sp.GetRequiredService<ReloadFailureSource>());
        services.AddSingleton<IOptionsMonitor<T>>(sp => new LastGoodOptionsMonitor<T>(sp.GetRequiredService<IOptionsFactory<T>>(),
            sp.GetServices<IOptionsChangeTokenSource<T>>(), section, plugin, sp.GetRequiredService<ReloadFailureSource>(),
            sp.GetService<ILoggerFactory>()?.CreateLogger("Kun.Configuration.Options") ?? NullLogger.Instance).Start());
    }

    private sealed class OptionsMarker<T>;

    private sealed class SectionChangeTokenSource<T>(IConfiguration section) : IOptionsChangeTokenSource<T>
    {
        public string Name => Options.DefaultName;
        public IChangeToken GetChangeToken() => section.GetReloadToken();
    }
}
