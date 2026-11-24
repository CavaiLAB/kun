namespace Kun.Auditing;

/// <summary>
/// Request-scoped DTO that carries ambient audit context from the outer host
/// (<see cref="Middleware.AuditContextMiddleware"/>) into the EF Core
/// <see cref="AuditingSaveChangesInterceptor"/>.
/// </summary>
/// <remarks>
/// All fields are nullable; the middleware (or any caller) populates them before the command
/// handler opens its unit of work, and hosts without an HTTP context may leave them unset.
/// The actor is the exception: when a save happens outside an <see cref="Kun.EventBus.EventEnvelopeScope"/>,
/// <see cref="ActorId"/> is empty and the current user is not authenticated, the interceptor rejects
/// the save with a <see cref="Kun.EventBus.KunEventsException"/> carrying the error code
/// <c>EVENTS_PUBLISH_REJECTED</c>. Background jobs and anonymous requests must first open an
/// <see cref="Kun.EventBus.EventEnvelopeScope"/> with a service actor.
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
    /// Kind of actor: <c>"user"</c>, <c>"agent"</c>, <c>"service"</c> or <c>"system"</c>; defaults to
    /// <c>"system"</c>, and is recorded as <c>"user"</c> when the current user is authenticated and the value
    /// is still the default. The default does not stand in for an actor: when <see cref="ActorId"/> is empty,
    /// the current user is not authenticated and the save happens outside an
    /// <see cref="Kun.EventBus.EventEnvelopeScope"/>, the save is rejected.
    /// </summary>
    public string ActorType { get; set; } = "system";
}
