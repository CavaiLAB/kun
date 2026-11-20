using Kun.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using System.Text.Json;

namespace Qian.Registry;

/// <summary>清单分发的稳定入口；程序化适配模式由 RegistryPlugin 保留。</summary>
[KunPlugin("qian.registry")]
[KunConfigSchema(ComponentConfiguration.Schema)]
[DelegatePlugin(typeof(RegistryPlugin))]
[Provides(typeof(RegistryDefinition), nameof(RegistryDefinition.Key))]
public sealed class RegistryComponent : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key];
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        return new RegistryPlugin(ctx.Services.GetRequiredService<RegistryRuntimeOptions>()).ApplyAsync(ctx, config, ct);
    }
}
