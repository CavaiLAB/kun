using System.Text.Json;
using Kun.Identity.Abstractions;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Kun.Configuration;

namespace Kun.Gateway.Shell;

/// <summary>网关唯一组件入口；宿主准备网络装配，插件挂载与卸载决定业务路由是否可以执行。</summary>
[KunPlugin("qian.gateway")]
[KunConfigSchema(ComponentConfiguration.Schema)]
public sealed class GatewayComponent : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [IdentityDefinition.Key, ConfigDefinition.Key];
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var runtime = ctx.Services.GetRequiredService<GatewayRuntime>();
        if (ctx.Scope != ScopeKey.Global || GatewayConfigurationReader.Serialize(GatewayConfigurationReader.Read(config)) != runtime.SerializedConfiguration)
            throw new InvalidOperationException("网关仅允许 global scope，组件配置必须与宿主已校验的静态装配一致。");
        runtime.Activate();
        ctx.Effect(runtime.Deactivate);
        ctx.Provide<IGatewayService>(GatewayDefinition.Key, runtime);
        return ValueTask.CompletedTask;
    }
}
