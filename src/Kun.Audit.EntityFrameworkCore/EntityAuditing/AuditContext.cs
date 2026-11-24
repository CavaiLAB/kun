namespace Kun.Auditing;

/// <summary>
/// Request-scoped DTO that carries ambient audit context from the outer host
/// (<see cref="Middleware.AuditContextMiddleware"/>) into the EF Core
/// <see cref="AuditingSaveChangesInterceptor"/>.
/// </summary>
/// <remarks>
/// 字段都可为空；中间件（或其他调用方）在命令处理器打开工作单元之前填好，没有 HTTP 上下文的宿主可以不填。
/// 主体例外：不在 <see cref="Kun.EventBus.EventEnvelopeScope"/> 内保存时，<see cref="ActorId"/> 为空且当前用户未认证，
/// 拦截器拒绝这次保存，抛出错误码为 <c>EVENTS_PUBLISH_REJECTED</c> 的 <see cref="Kun.EventBus.KunEventsException"/>。
/// 后台作业与匿名请求要先以服务主体打开 <see cref="Kun.EventBus.EventEnvelopeScope"/>。
/// </remarks>
public sealed class AuditContext
{
    /// <summary>Remote IP address (e.g. <c>HttpContext.Connection.RemoteIpAddress.ToString()</c>).</summary>
    public string? IpAddress { get; set; }

    /// <summary>Client User-Agent header value, when present.</summary>
    public string? UserAgent { get; set; }

    /// <summary>W3C trace id (<c>Activity.Current?.TraceId.ToString()</c>), when an activity is active.</summary>
    public string? TraceId { get; set; }

    /// <summary>Subject identifier of the acting principal (<c>sub</c> claim, user id, service id, …).</summary>
    public string? ActorId { get; set; }

    /// <summary>
    /// 主体类型：<c>"user"</c>、<c>"agent"</c>、<c>"service"</c> 或 <c>"system"</c>，缺省 <c>"system"</c>；
    /// 当前用户已认证且仍是缺省值时按 <c>"user"</c> 记。缺省值不能代替主体：<see cref="ActorId"/> 为空、当前用户未认证、
    /// 又不在 <see cref="Kun.EventBus.EventEnvelopeScope"/> 内时，保存会被拒绝。
    /// </summary>
    public string ActorType { get; set; } = "system";
}
