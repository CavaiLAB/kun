using FluentAssertions;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.DependencyInjection;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kun.IntegrationTests.Outbox;

// Pure DI-container tests for the IOutboxWriter primitive introduced in P0-4.
// No database is touched — these assertions only probe that the container
// registration wires IIntegrationEventBus and IOutboxWriter to the same
// scoped envelope writer bound to this scope’s transaction buffer.

public sealed class OutboxWriterDependencyInjectionTests
{
    [Fact]
    public void AddKunEventBus_RegistersIOutboxWriter()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var writer = scope.ServiceProvider.GetService<IOutboxWriter>();

        writer.Should().NotBeNull();
        ((IOutboxBufferBinding)writer!).Buffer.Should().BeSameAs(scope.ServiceProvider.GetRequiredService<IntegrationEventBuffer>());
    }

    [Fact]
    public void IOutboxWriter_And_IIntegrationEventBus_ResolveToSameInstance_PerScope()
    {
        var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var writer = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();
        var bus = scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>();

        writer.Should().BeSameAs(bus);
    }

    [Fact]
    public void IOutboxWriter_ResolvesToFreshInstance_AcrossScopes()
    {
        var provider = BuildProvider();

        IOutboxWriter writerA;
        IOutboxWriter writerB;

        using (var scopeA = provider.CreateScope())
        {
            writerA = scopeA.ServiceProvider.GetRequiredService<IOutboxWriter>();
        }
        using (var scopeB = provider.CreateScope())
        {
            writerB = scopeB.ServiceProvider.GetRequiredService<IOutboxWriter>();
        }

        writerA.Should().NotBeSameAs(writerB);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ITenantContext>(NullTenantContext.Instance);
        services.AddKunEntityFrameworkCore();
        services.AddKunEventBus();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
