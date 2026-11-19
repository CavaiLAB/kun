using Kun.Core.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// <see cref="ITenantProvisioner"/> implementation for the
/// <see cref="TenantIsolation.Schema"/> strategy. On
/// <see cref="ProvisionAsync"/> executes <c>CREATE SCHEMA IF NOT EXISTS "{schema}"</c>
/// and runs EF Core migrations under that schema; on <see cref="DeprovisionAsync"/>
/// drops the schema when <see cref="KunMultiTenancyOptions.AllowDestructiveDeprovision"/>
/// is enabled, else throws.
/// </summary>
/// <remarks>
/// The provisioner and the context factory use <see cref="KunMultiTenancyOptions.GetSchemaName"/>
/// to derive the same schema from the tenant UUID and configured prefix.
/// </remarks>
/// <typeparam name="TContext">The KunDbContext-derived context whose migrations to apply.</typeparam>
public sealed partial class SchemaPerTenantProvisioner<TContext> : ITenantProvisioner
    where TContext : DbContext
{
    private readonly ITenantDbContextFactory<TContext> _factory;
    private readonly KunMultiTenancyOptions _options;
    private readonly ILogger<SchemaPerTenantProvisioner<TContext>> _logger;

    /// <summary>Creates a new provisioner.</summary>
    public SchemaPerTenantProvisioner(
        ITenantDbContextFactory<TContext> factory,
        IOptions<KunMultiTenancyOptions> options,
        ILogger<SchemaPerTenantProvisioner<TContext>>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(options);
        _factory = factory;
        _options = options.Value;
        _logger = logger ?? NullLogger<SchemaPerTenantProvisioner<TContext>>.Instance;
    }

    /// <inheritdoc />
    public async Task ProvisionAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        var schema = _options.GetSchemaName(tenant.TenantId);

        await using var context = _factory.CreateForTenant(tenant.TenantId);
        LogProvisioning(_logger, tenant.TenantId);

        // Create the schema on the server before running migrations so the migrations-history
        // table lands inside the tenant's schema (matches UseKunSchema contract).
        // Schema name is validated above; interpolation is safe.
        var createSchemaSql = $"CREATE SCHEMA IF NOT EXISTS \"{schema}\"";
        await context.Database.ExecuteSqlRawAsync(createSchemaSql, cancellationToken).ConfigureAwait(false);

        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeprovisionAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var schema = _options.GetSchemaName(tenantId);

        if (!_options.AllowDestructiveDeprovision)
        {
            throw new NotSupportedException(
                "Deprovisioning is disabled. Set KunMultiTenancyOptions.AllowDestructiveDeprovision = true " +
                "to enable DROP SCHEMA CASCADE. This is a data-loss operation and must be an explicit opt-in.");
        }

        await using var context = _factory.CreateForTenant(tenantId);
        LogDeprovisioning(_logger, tenantId);

        var dropSchemaSql = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
        await context.Database.ExecuteSqlRawAsync(dropSchemaSql, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "SchemaPerTenantProvisioner: provisioning tenant {TenantId}.")]
    private static partial void LogProvisioning(ILogger logger, Guid tenantId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "SchemaPerTenantProvisioner: DROP SCHEMA for tenant {TenantId} (AllowDestructiveDeprovision=true).")]
    private static partial void LogDeprovisioning(ILogger logger, Guid tenantId);
}
