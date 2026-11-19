using Microsoft.Extensions.Options;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// Default <see cref="IDbConnectionResolver"/> backed by <see cref="TenantsConnectionOptions"/>.
/// Reads the current snapshot on every call via <see cref="IOptionsMonitor{TOptions}"/>
/// so on-the-fly config reloads reach new tenants without restarting the host.
/// </summary>
public sealed class OptionsBasedTenantConnectionResolver : IDbConnectionResolver
{
    private readonly IOptionsMonitor<TenantsConnectionOptions> _options;

    /// <summary>Creates a new resolver.</summary>
    public OptionsBasedTenantConnectionResolver(IOptionsMonitor<TenantsConnectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
    public string ResolveConnectionString(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId must be a non-empty UUID.", nameof(tenantId));
        }

        var map = _options.CurrentValue.ConnectionStrings;
        if (!map.TryGetValue(tenantId, out var cs) || string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                $"No connection string configured for tenant '{tenantId}'. " +
                $"Add an entry under '{TenantsConnectionOptions.SectionName}:ConnectionStrings' " +
                "or register a custom IDbConnectionResolver.");
        }

        return cs;
    }
}
