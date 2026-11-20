using Kun.Audit;
using Kun.EventBus;

namespace Kun.Auditing;

internal static class AuditEventOrigin
{
    internal static EventOrigin Resolve(string actorType, string? actorId)
    {
        if (EventEnvelopeScope.Current is { } command) return command with { Producer = "qian.audit" };
        if (string.IsNullOrWhiteSpace(actorId))
            throw new KunEventsException(EventsErrors.PublishRejected, "实体审计需要已认证主体，后台保存请通过 EventEnvelopeScope 指定服务主体。");

        var actor = actorType switch
        {
            AuditActorType.User => EventActor.User(actorId),
            AuditActorType.Agent => EventActor.Agent(actorId),
            AuditActorType.System or AuditActorType.Service => EventActor.Service(actorId),
            _ => throw new KunEventsException(EventsErrors.PublishRejected, "实体审计主体类型未登记。"),
        };
        return new EventOrigin("qian.audit", actor);
    }
}
