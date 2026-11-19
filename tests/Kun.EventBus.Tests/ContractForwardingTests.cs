using Kun.EventBus.Outbox;
using Xunit;

namespace Kun.EventBus.Tests;

public sealed class ContractForwardingTests
{
    [Theory]
    [InlineData("Kun.EventBus.IntegrationEventIdAttribute", typeof(IntegrationEventIdAttribute))]
    [InlineData("Kun.EventBus.IEventTypeRegistrar", typeof(IEventTypeRegistrar))]
    [InlineData("Kun.EventBus.IEventTypeRegistry", typeof(IEventTypeRegistry))]
    [InlineData("Kun.EventBus.IIntegrationEventBus", typeof(IIntegrationEventBus))]
    [InlineData("Kun.EventBus.Outbox.IIntegrationEventOutbox", typeof(IIntegrationEventOutbox))]
    [InlineData("Kun.EventBus.Outbox.IOutboxWriter", typeof(IOutboxWriter))]
    [InlineData("Kun.EventBus.Outbox.IntegrationEventBuffer", typeof(IntegrationEventBuffer))]
    [InlineData("Kun.EventBus.Outbox.OutboxMessage", typeof(OutboxMessage))]
    [InlineData("Kun.EventBus.Outbox.OutboxStatus", typeof(OutboxStatus))]
    public void LegacyAssemblyReference_WithMovedContract_ShouldResolveSameType(string typeName, Type expected)
    {
        var resolved = Type.GetType($"{typeName}, Kun.EventBus", throwOnError: true);

        Assert.Same(expected, resolved);
        Assert.Equal("Kun.EventBus.Abstractions", resolved!.Assembly.GetName().Name);
    }
}
