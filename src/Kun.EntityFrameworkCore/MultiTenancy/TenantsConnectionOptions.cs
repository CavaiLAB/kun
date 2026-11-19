namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// Static tenant → connection-string mapping consumed by
/// <see cref="OptionsBasedTenantConnectionResolver"/>. Suitable for small deployments
/// where the full tenant list is known at configuration time (appsettings.json or
/// Kubernetes ConfigMap).
/// </summary>
/// <remarks>
/// For scale, swap in a custom <see cref="IDbConnectionResolver"/> implementation backed
/// by a secret manager + tenant-metadata service. See the P2-D Part 3 notes in
/// <c>docs/architecture/adr-p2d-multitenancy.md</c>.
/// </remarks>
public sealed class TenantsConnectionOptions
{
    /// <summary>Config section name bound via <c>Configure&lt;TenantsConnectionOptions&gt;</c>.</summary>
    public const string SectionName = "Kun:MultiTenancy:Tenants";

    /// <summary>
    /// Maps tenant IDs to ADO.NET connection strings.
    /// </summary>
    public IDictionary<Guid, string> ConnectionStrings { get; set; } =
        new Dictionary<Guid, string>();
}
