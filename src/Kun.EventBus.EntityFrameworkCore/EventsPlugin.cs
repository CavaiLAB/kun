using System.Text.Json;
using Kun.Errors;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kun.EventBus.EntityFrameworkCore;

/// <summary>qian.events 的真实 Provider：挂载时核对组件配置与宿主装配一致，提供 Events 服务键；卸载撤销服务与错误码。</summary>
[KunPlugin("qian.events")]
[KunConfigSchema(ComponentConfiguration.Schema)]
public sealed class EventsPlugin : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [];

    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ct.ThrowIfCancellationRequested();
        if (ctx.Scope != ScopeKey.Global && ctx.Scope.Kind != "tenant")
            throw new KunEventsException(EventsErrors.Misconfigured, "Events 只允许挂载在 global 或 tenant scope。");
        var settings = EventsConfiguration.Read(config);
        _ = ctx.Services.GetService<EventsHostMarker>()
            ?? throw new KunEventsException(EventsErrors.Misconfigured, "宿主必须先以 AddKunEvents 装配信封写入端。");
        var options = ctx.Services.GetRequiredService<IOptions<KunEventsOptions>>().Value;
        if (options.OutboxLock != settings.Lock)
            throw new KunEventsException(EventsErrors.Misconfigured, "组件配置 outbox.lock 与宿主装配的领取方式不一致。");
        var broker = ctx.Services.GetService<EventsBrokerBinding>();
        if (settings.Broker is not null && broker?.Reference != settings.Broker)
            throw new KunEventsException(EventsErrors.Misconfigured, "组件配置的 broker 引用没有在宿主绑定。");
        var catalog = ctx.Services.GetService<ErrorCatalog>();
        if (catalog is not null)
        {
            ctx.Effect(catalog.Register(EventsErrors.Misconfigured));
            ctx.Effect(catalog.Register(EventsErrors.PublishRejected));
            ctx.Effect(catalog.Register(EventsErrors.ConsumeRejected));
        }
        ctx.Provide<IEventsService>(EventsDefinition.Key, ctx.Services.GetRequiredService<IEventsService>());
        return ValueTask.CompletedTask;
    }
}

internal sealed record EventsSettings(string? Broker, OutboxLockMode Lock);

internal static class EventsConfiguration
{
    internal static EventsSettings Read(JsonElement config)
    {
        if (config.ValueKind != JsonValueKind.Object) throw Invalid("Events 配置必须为对象。");
        string? broker = null;
        var mode = OutboxLockMode.SkipLocked;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in config.EnumerateObject())
        {
            if (!names.Add(property.Name)) throw Invalid("Events 配置不能重复声明字段。");
            switch (property.Name)
            {
                case "broker" when property.Value.ValueKind == JsonValueKind.String:
                    broker = property.Value.GetString();
                    EventsBrokerBinding.ValidateReference(broker!);
                    break;
                case "outbox" when property.Value.ValueKind == JsonValueKind.Object:
                    foreach (var item in property.Value.EnumerateObject())
                    {
                        mode = item is { Name: "lock", Value.ValueKind: JsonValueKind.String } ? item.Value.GetString() switch
                        {
                            "skip-locked" => OutboxLockMode.SkipLocked,
                            "redis" => OutboxLockMode.Redis,
                            _ => throw Invalid("outbox.lock 只能是 skip-locked 或 redis。")
                        } : throw Invalid("outbox 只允许 lock 字段。");
                    }
                    break;
                default:
                    throw Invalid("Events 配置包含未知字段或不受支持的值。");
            }
        }
        return new EventsSettings(broker, mode);
    }

    private static KunEventsException Invalid(string reason) => new(EventsErrors.Misconfigured, reason);
}
