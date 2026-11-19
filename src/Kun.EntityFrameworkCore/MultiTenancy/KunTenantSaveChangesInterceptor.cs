using Kun.Core.Domain;
using Kun.Core.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that walks newly added
/// <see cref="IMultiTenant"/> entities and, when their
/// <see cref="IMultiTenant.TenantId"/> is <see langword="null"/> or
/// empty, injects the ambient <see cref="ITenantContext.TenantId"/>.
/// Explicit values must be non-empty UUIDs in lowercase D format.
/// Valid explicit values take precedence over the ambient tenant.
/// </summary>
public sealed partial class KunTenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<KunTenantSaveChangesInterceptor> _logger;

    /// <summary>Creates a new interceptor bound to the supplied tenant context.</summary>
    public KunTenantSaveChangesInterceptor(
        ITenantContext tenantContext,
        ILogger<KunTenantSaveChangesInterceptor> logger)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(logger);
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        if (eventData.Context is { } context)
        {
            InjectTenantIds(context);
        }
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        if (eventData.Context is { } context)
        {
            InjectTenantIds(context);
        }
        return result;
    }

    private void InjectTenantIds(DbContext context)
    {
        var tenant = _tenantContext.TenantId;
        if (tenant == Guid.Empty) throw new InvalidOperationException("A tenant scope requires a non-empty UUID.");
        foreach (var entry in context.ChangeTracker.Entries<IMultiTenant>())
        {
            if (entry.State != EntityState.Added)
            {
                continue;
            }

            var existing = entry.Entity.TenantId;
            if (!string.IsNullOrEmpty(existing))
            {
                if (!Guid.TryParseExact(existing, "D", out var id) || id == Guid.Empty || existing != id.ToString("D"))
                    throw new InvalidOperationException("An explicitly assigned tenant ID must be a canonical non-empty UUID.");
                continue;
            }

            if (tenant is not { } current) continue;
            var tenantId = current.ToString("D");
            // The property setter may be `protected` on aggregate roots;
            // the PropertyEntry API writes through the EF metadata and
            // respects that shape without runtime reflection into the type.
            entry.Property(nameof(IMultiTenant.TenantId)).CurrentValue = tenantId;
            Log.TenantAutoFilled(_logger, entry.Entity.GetType().Name, tenantId);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
            Message = "Kun.MultiTenancy auto-filled TenantId on {EntityType}: {TenantId}")]
        public static partial void TenantAutoFilled(ILogger logger, string entityType, string tenantId);
    }
}
