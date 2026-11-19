using Kun.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kun.EventBus.EntityFrameworkCore;

/// <summary>Registers the EF-backed event relay and its host settings.</summary>
public static class KunEventRelayExtensions
{
    /// <summary>Registers the event writer and validates the configured relay options when the host starts.</summary>
    public static IServiceCollection AddKunEvents(this IServiceCollection services, Action<KunEventsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.AddKunEvents();
        services.Configure(configure);
        services.AddHostedService<RelayOptionsCheck>();
        return services;
    }

    /// <summary>Publishes the outboxes of registered strict data contexts through the configured broker.</summary>
    public static IServiceCollection AddKunOutboxRelay(this IServiceCollection services)
    {
        services.AddKunEvents();
        services.AddHostedService<RelayOptionsCheck>();
        services.AddHostedService(sp => new KunOutboxRelay(sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetService<IDataContextCatalog>() ?? NoDataContexts.Instance, sp.GetRequiredService<IIntegrationEventPublisher>(),
            sp.GetRequiredService<IOptions<KunEventsOptions>>(), sp.GetRequiredService<TimeProvider>(),
            sp.GetRequiredService<ILogger<KunOutboxRelay>>(), sp));
        return services;
    }
}

internal sealed class RelayOptionsCheck(IServiceProvider services, IOptions<KunEventsOptions> options) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        options.Value.Validate();
        if (options.Value.OutboxLock == OutboxLockMode.Redis && services.GetService<StackExchange.Redis.IConnectionMultiplexer>() is null)
            throw new KunEventsException(EventsErrors.Misconfigured, "outbox.lock 为 redis 时宿主必须登记 IConnectionMultiplexer。");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class NoDataContexts : IDataContextCatalog
{
    public static readonly NoDataContexts Instance = new();

    public IReadOnlyList<Type> ContextTypes => [];
}
