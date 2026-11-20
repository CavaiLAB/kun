using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Kun.Core.MultiTenancy;

namespace Kun.EventBus.Outbox;

/// <summary>
/// Outbox 写入端：入队时就生成完整 v1 信封并写进 Outbox 行的 payload，
/// 发布器原样发出。来源（producer、actor、因果链）取自 <see cref="EventEnvelopeScope"/>，缺失即拒绝，
/// 这是「集成事件只能在命令 handler 同 scope 发出」的运行时兜底。
/// </summary>
internal sealed class KunEnvelopeOutboxWriter(IntegrationEventBuffer buffer, IEventTypeRegistry registry, ITenantContext tenant, TimeProvider clock)
    : IIntegrationEventBus, IOutboxWriter, IOutboxBufferBinding
{
    public IntegrationEventBuffer Buffer => buffer;

    public void Publish<TEvent>(TEvent @event) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);
        var origin = EventEnvelopeScope.Current
            ?? throw new KunEventsException(EventsErrors.PublishRejected, "集成事件只能在命令 handler 的同一作用域内发出；后台发布方须以服务主体打开 EventEnvelopeScope。");
        var type = registry.GetEventType(typeof(TEvent))
            ?? throw new KunEventsException(EventsErrors.PublishRejected, "事件类型未在 Kun 事件目录登记。");
        if (registry.GetJsonTypeInfo(typeof(TEvent)) is not JsonTypeInfo<TEvent> typeInfo)
            throw new KunEventsException(EventsErrors.PublishRejected, "事件必须以源生成 JsonTypeInfo 登记，payload Schema 才能与发出的 JSON 一致。");
        var occurredAt = clock.GetUtcNow();
        buffer.Add(CreateMessage(type, @event, typeInfo, origin, tenant.TenantId, Guid.CreateVersion7(occurredAt), occurredAt));
    }

    // EF SavingChanges runs after the unit of work has flushed the buffer. Transactional interceptors
    // can stage the same envelope directly in the active context, using their audit row's existing ID.
    internal static OutboxMessage CreateMessage<TEvent>(string type, TEvent value, JsonTypeInfo<TEvent> typeInfo, EventOrigin origin,
        Guid? tenantId, Guid id, DateTimeOffset occurredAt) where TEvent : class
    {
        var subject = IntegrationEventTypes.SubjectOf(type);
        var payload = JsonSerializer.SerializeToElement(value, typeInfo);
        if (tenantId == Guid.Empty) throw new KunEventsException(EventsErrors.PublishRejected, "租户标识必须是非空 UUID。");
        var activity = Activity.Current;
        var traceParent = origin.TraceParent ?? (activity is { IdFormat: ActivityIdFormat.W3C } && KunEventEnvelope.IsTraceParent(activity.Id) ? activity.Id : null)
            ?? "00-" + ActivityTraceId.CreateRandom().ToHexString() + "-" + ActivitySpanId.CreateRandom().ToHexString() + "-00";
        var envelope = new KunEventEnvelope
        {
            Version = KunEventEnvelope.CurrentVersion,
            EventId = id,
            Subject = subject,
            Type = type,
            OccurredAt = occurredAt,
            TenantId = tenantId,
            Producer = origin.Producer,
            CorrelationId = origin.CorrelationId,
            CausationId = origin.CausationId,
            Actor = origin.Actor,
            TraceParent = traceParent,
            Payload = payload,
        };
        return new OutboxMessage(envelope.EventId, type, envelope.ToJson(), tenantId?.ToString("D"), traceParent, activity?.TraceStateString, occurredAt);
    }

    public void Enqueue<TEvent>(TEvent @event) where TEvent : class => Publish(@event);

}
