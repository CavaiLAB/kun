using System.Runtime.CompilerServices;
using Kun.EventBus;
using Kun.EventBus.Outbox;

[assembly: TypeForwardedTo(typeof(IntegrationEventIdAttribute))]
[assembly: TypeForwardedTo(typeof(IEventTypeRegistrar))]
[assembly: TypeForwardedTo(typeof(IEventTypeRegistry))]
[assembly: TypeForwardedTo(typeof(IIntegrationEventBus))]
[assembly: TypeForwardedTo(typeof(IIntegrationEventOutbox))]
[assembly: TypeForwardedTo(typeof(IOutboxWriter))]
[assembly: TypeForwardedTo(typeof(IntegrationEventBuffer))]
[assembly: TypeForwardedTo(typeof(OutboxMessage))]
[assembly: TypeForwardedTo(typeof(OutboxStatus))]
