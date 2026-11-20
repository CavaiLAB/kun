using System.Text.Json;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Kun.Runtime;
using Kun.Errors;

namespace Qian.Registry;

/// <summary>平台 Registry Provider；提供真实数据库 scope 装配与监听生命周期，管理写入另经完整准入。</summary>
public sealed class RegistryPlugin(RegistryRuntimeOptions options) : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key];
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ctx); ArgumentNullException.ThrowIfNull(options);
        ct.ThrowIfCancellationRequested();
        if (ctx.Scope != ScopeKey.Global || options.Profile.Profile != "platform" || options.Profile.ScopePatches.Count != 0 || config.ValueKind != JsonValueKind.Object ||
            (config.EnumerateObject().Count() > 1 || config.EnumerateObject().Any(property => property.Name != "store" || property.Value.ValueKind != JsonValueKind.String || property.Value.GetString() != "pg")))
            throw new ErrorException(new Error(PlatformErrors.InputInvalid));
        var service = new RegistryService(options, ctx.Data(), ctx.Services.GetRequiredService<ITenantContext>(), ctx);
        // 等待后台监听退出，但不在 effect 内调用本装载器的 Mount/Unmount。
        ctx.Effect(service.CancelAndDrainAsync);
        ctx.Effect(RegistryErrors.Register(ctx.Services.GetRequiredService<ErrorCatalog>()));
        ctx.Provide<IRegistryService>(RegistryDefinition.Key, service);
        return ValueTask.CompletedTask;
    }
}
