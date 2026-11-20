using Kun.Audit;
using Kun.Auditing.DependencyInjection;
using Kun.Auditing.Events;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.EntityFrameworkCore;
using Kun.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Auditing;

/// <summary>
/// Default <see cref="IAuditSink"/> implementation that writes
/// <see cref="AuditEventIntegrationEvent"/> directly to the current
/// <see cref="KunDbContext"/>'s Outbox DbSet. The audit row is enrolled in the
/// same EF change tracker batch that the firing <c>SaveChangesAsync</c> is about
/// to persist, so it commits atomically with the business mutation regardless of
/// whether the caller went through <see cref="EfUnitOfWork"/> or invoked
/// <c>DbContext.SaveChangesAsync</c> directly.
/// </summary>
/// <remarks>The interceptor supplies the context currently saving. Direct sink callers use the host's default Kun context.</remarks>
internal sealed class OutboxAuditSink : IAuditSink
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITenantContext _tenantContext;

    public OutboxAuditSink(IServiceProvider serviceProvider, ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(tenantContext);
        _serviceProvider = serviceProvider;
        _tenantContext = tenantContext;
    }

    public ValueTask WriteAsync(AuditEventIntegrationEvent @event, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stage(_serviceProvider.GetRequiredService<KunDbContext>(), @event);
        return ValueTask.CompletedTask;
    }

    internal OutboxMessage Stage(DbContext context, AuditEventIntegrationEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var currentUser = _serviceProvider.GetService<ICurrentUser>();
        var actorId = @event.ActorId ?? (currentUser?.IsAuthenticated == true ? currentUser.UserId : null);
        var actorType = @event.ActorType == AuditActorType.System && currentUser?.IsAuthenticated == true ? AuditActorType.User : @event.ActorType;
        var origin = AuditEventOrigin.Resolve(actorType, actorId);
        var message = KunEnvelopeOutboxWriter.CreateMessage(AuditingServiceCollectionExtensions.AuditEventStableId, @event,
            AuditJsonContext.Default.AuditEventIntegrationEvent, origin, _tenantContext.TenantId, Guid.CreateVersion7(@event.CreatedAt), @event.CreatedAt);
        context.Set<OutboxMessage>().Add(message);
        return message;
    }
}
