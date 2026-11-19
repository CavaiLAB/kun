using Kun.Errors;
using Kun.EventBus.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Kun.EventBus;

/// <summary>Registers the event envelope writer and event catalog.</summary>
public static class KunEventsExtensions
{
    /// <summary>
    /// 登记 v1 信封写入端与事件目录。与 AddKunData 的调用顺序无关。
    /// 此入口登记写入端。持有 Outbox 的宿主单独登记投递后台服务。
    /// </summary>
    public static IServiceCollection AddKunEvents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        services.AddOptions();
        if (services.Any(service => service.ServiceType == typeof(EventsHostMarker))) return services;
        services.AddSingleton(new EventsHostMarker());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(ErrorCatalog.CreateDefault());
        RegisterWriter(services);
        services.TryAddSingleton<IEventsService>(sp => new EventsService(sp.GetRequiredService<IEventTypeRegistry>(), sp.GetService<IEventSubscriptionCatalog>()));
        services.AddHostedService<EventsStartupCheck>();
        return services;
    }

    internal static void RegisterWriter(IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IntegrationEventBuffer>();
        services.TryAddScoped(sp => new KunEnvelopeOutboxWriter(sp.GetRequiredService<IntegrationEventBuffer>(), sp.GetRequiredService<IEventTypeRegistry>(),
            sp.GetRequiredService<Kun.Core.MultiTenancy.ITenantContext>(), sp.GetRequiredService<TimeProvider>()));
        services.TryAddScoped<IIntegrationEventBus>(sp => sp.GetRequiredService<KunEnvelopeOutboxWriter>());
        services.TryAddScoped<IOutboxWriter>(sp => sp.GetRequiredService<KunEnvelopeOutboxWriter>());
    }
}

internal interface IEventSubscriptionCatalog
{
    IReadOnlyList<EventSubscriptionDescriptor> Describe(IEventTypeRegistry registry);
}
internal sealed class EventsHostMarker;
internal sealed class EventsStartupCheck(IServiceProvider services, ErrorCatalog catalog) : IHostedService, IDisposable
{
    private readonly Lock _gate = new();
    private readonly List<IDisposable> _registrations = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_registrations.Count == 0)
            {
                _registrations.Add(catalog.Register(EventsErrors.Misconfigured));
                _registrations.Add(catalog.Register(EventsErrors.PublishRejected));
                _registrations.Add(catalog.Register(EventsErrors.ConsumeRejected));
            }
        }
        if (services.GetService<IEventTypeRegistry>() is null)
            throw new KunEventsException(EventsErrors.Misconfigured, "宿主必须以 AddKunEventBus 显式登记事件类型目录。");
        _ = services.GetRequiredService<IEventsService>().Catalog;
        using var scope = services.CreateScope();
        if (scope.ServiceProvider.GetRequiredService<IOutboxWriter>() is not KunEnvelopeOutboxWriter writer ||
            !ReferenceEquals(writer, scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>()))
            throw new KunEventsException(EventsErrors.Misconfigured, "Outbox 写入端被其他装配覆盖，事件不会带上 v1 信封。");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var registration in _registrations) registration.Dispose();
            _registrations.Clear();
        }
    }
}

internal sealed class EventsService(IEventTypeRegistry registry, IEventSubscriptionCatalog? subscriptions) : IEventsService
{
    private readonly Lazy<IReadOnlyList<IntegrationEventDescriptor>> _catalog = new(() => IntegrationEventSchemas.Describe(registry));
    private readonly Lazy<IReadOnlyList<EventSubscriptionDescriptor>> _subscriptions = new(() => subscriptions?.Describe(registry) ?? []);

    public IReadOnlyList<IntegrationEventDescriptor> Catalog => _catalog.Value;

    public IReadOnlyList<EventSubscriptionDescriptor> Subscriptions => _subscriptions.Value;

    public IntegrationEventDescriptor? Find(string type) => Catalog.FirstOrDefault(item => string.Equals(item.Type, type, StringComparison.Ordinal));
}
