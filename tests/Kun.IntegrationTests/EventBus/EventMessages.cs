using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Kun.EventBus;
using Kun.EventBus.Outbox;

namespace Kun.IntegrationTests.EventBus;

internal static class EventMessages
{
    internal static OutboxMessage Create<T>(Guid id, string type, T payload, JsonTypeInfo<T> info, string? tenant = null, string? traceParent = null)
    {
        var now = DateTimeOffset.UtcNow;
        var envelope = new KunEventEnvelope
        {
            Version = KunEventEnvelope.CurrentVersion,
            EventId = id,
            Type = type,
            Subject = IntegrationEventTypes.SubjectOf(type),
            OccurredAt = now,
            TenantId = tenant is null ? null : Guid.Parse(tenant),
            Producer = "kun.integration-tests",
            Actor = EventActor.Service("integration-tests"),
            TraceParent = traceParent ?? "00-" + ActivityTraceId.CreateRandom() + "-" + ActivitySpanId.CreateRandom() + "-01",
            Payload = JsonSerializer.SerializeToElement(payload, info),
        };
        return new OutboxMessage(id, type, envelope.ToJson(), tenant, envelope.TraceParent, null, now);
    }
}
