using Kun.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qian.Config;
using Kun.Plugin.Sdk;
using Qian.Registry;
using Kun.Runtime;
using System.Text.Json;

namespace Qian.Lifecycle;

/// <summary>清单分发的稳定入口；程序化适配模式由 LifecyclePlugin 保留。</summary>
[KunPlugin("qian.lifecycle")]
[KunConfigSchema(ComponentConfiguration.Schema)]
[DelegatePlugin(typeof(LifecyclePlugin))]
[Provides(typeof(LifecycleDefinition), nameof(LifecycleDefinition.Key))]
public sealed class LifecycleComponent : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key, RegistryDefinition.Key, ConfigDefinition.Key];
    /// <summary>可选依赖（清单 optionalInject）：Audit 与 AuthzSuperadminBypass，含义见 <see cref="LifecyclePlugin.OptionalInject"/>。</summary>
    public static IReadOnlyList<ServiceKey> OptionalInject => LifecyclePlugin.OptionalInject;
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        return new LifecyclePlugin(ctx.Services.GetService<LifecycleRuntimeOptions>() ?? new LifecycleRuntimeOptions()).ApplyAsync(ctx, config, ct);
    }
}
