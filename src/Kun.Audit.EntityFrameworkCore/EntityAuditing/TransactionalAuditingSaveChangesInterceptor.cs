using System.Diagnostics;
using Kun.Audit;
using Kun.Auditing.DependencyInjection;
using Kun.Auditing.Events;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kun.Auditing;

/// <summary>在业务上下文中同时暂存 AuditLog 与 Outbox，任何一方失败均使整个保存失败。</summary>
public sealed class TransactionalAuditingSaveChangesInterceptor(IAuditableEntityRegistry registry,
    ITenantContext tenant, ICurrentUser user, AuditContext auditContext, TimeProvider clock) : SaveChangesInterceptor
{
    private readonly StagedAuditRows _staged = new();

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stage(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stage(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Stage(DbContext? context)
    {
        if (context is null) return;
        var pending = _staged.Begin(context);
        var changes = context.ChangeTracker.Entries().Where(e =>
            e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted &&
            registry.RegisteredTypes.Contains(e.Metadata.ClrType)).ToArray();
        try
        {
            StageChanges(context, changes, pending);
        }
        catch
        {
            _staged.Rollback(context);
            throw;
        }
    }

    private void StageChanges(DbContext context, EntityEntry[] changes, List<object> pending)
    {
        foreach (var entry in changes)
        {
            var now = clock.GetUtcNow();
            var id = Guid.CreateVersion7(now);
            var actor = auditContext.ActorId ?? (user.IsAuthenticated ? user.UserId : null);
            var actorType = auditContext.ActorType == AuditActorType.System && user.IsAuthenticated
                ? AuditActorType.User : auditContext.ActorType;
            var trace = auditContext.TraceId ?? Activity.Current?.TraceId.ToString();
            var resourceId = PrimaryKey(entry);
            var @event = new AuditEventIntegrationEventV2(id, tenant.TenantId?.ToString("D"),
                user.IsAuthenticated ? user.UserId : null, actorType, actor, entry.State.ToString(),
                entry.Metadata.ClrType.Name, resourceId, trace, auditContext.IpAddress, now);
            var log = new AuditLog
            {
                Id = id,
                TenantId = tenant.TenantId?.ToString("D"),
                ActorId = actor,
                ActorType = actorType,
                Action = @event.Action,
                ResourceType = @event.ResourceType,
                ResourceId = resourceId,
                TraceId = trace,
                Timestamp = now,
                IpAddress = auditContext.IpAddress
            };
            var origin = AuditEventOrigin.Resolve(actorType, actor);
            var message = KunEnvelopeOutboxWriter.CreateMessage(AuditingServiceCollectionExtensions.AuditEventV2StableId, @event,
                AuditJsonContext.Default.AuditEventIntegrationEventV2, origin, tenant.TenantId, id, now);
            context.Set<AuditLog>().Add(log);
            pending.Add(log);
            context.Set<OutboxMessage>().Add(message);
            pending.Add(message);
        }
    }

    private static string? PrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        return key is null ? null : string.Join('|', key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString()));
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        _staged.Complete(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default) => ValueTask.FromResult(SavedChanges(eventData, result));

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => _staged.Rollback(eventData.Context);
    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _staged.Rollback(eventData.Context);
        return Task.CompletedTask;
    }
    /// <inheritdoc />
    public override void SaveChangesCanceled(DbContextEventData eventData) => _staged.Rollback(eventData.Context);
    /// <inheritdoc />
    public override Task SaveChangesCanceledAsync(DbContextEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _staged.Rollback(eventData.Context);
        return Task.CompletedTask;
    }
}
