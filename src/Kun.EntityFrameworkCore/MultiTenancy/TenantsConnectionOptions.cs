using Microsoft.Extensions.Configuration;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// Static tenant → connection-string mapping consumed by
/// <see cref="OptionsBasedTenantConnectionResolver"/>. Suitable for small deployments
/// where the full tenant list is known at configuration time (appsettings.json or
/// Kubernetes ConfigMap).
/// </summary>
/// <remarks>
/// <para>
/// Bind the section with <c>Configure&lt;TenantsConnectionOptions&gt;(configuration.GetSection(TenantsConnectionOptions.SectionName))</c>.
/// Under <c>ConnectionStrings</c>, each key is a tenant ID as a non-empty UUID in lowercase D format.
/// The configuration binder cannot populate <see cref="Guid"/>-keyed dictionaries, so the section lands in
/// <see cref="ConfiguredConnectionStrings"/>. <c>AddKunMultiTenancy&lt;TContext&gt;</c> with
/// <c>TenantIsolation.Database</c> merges those entries into <see cref="ConnectionStrings"/>. The host fails to start when a key is not
/// a canonical UUID or when code and configuration give one tenant different connection strings.
/// </para>
/// <para>
/// For scale, swap in a custom <see cref="IDbConnectionResolver"/> implementation backed
/// by a secret manager + tenant-metadata service. See the P2-D Part 3 notes in
/// <c>docs/architecture/adr-p2d-multitenancy.md</c>.
/// </para>
/// </remarks>
public sealed class TenantsConnectionOptions
{
    /// <summary>Config section name bound via <c>Configure&lt;TenantsConnectionOptions&gt;</c>.</summary>
    public const string SectionName = "Kun:MultiTenancy:Tenants";

    /// <summary>
    /// Maps tenant IDs to ADO.NET connection strings. Fill it from code; entries from
    /// configuration are merged in from <see cref="ConfiguredConnectionStrings"/>.
    /// </summary>
    public IDictionary<Guid, string> ConnectionStrings { get; set; } =
        new Dictionary<Guid, string>();

    /// <summary>
    /// Receives the <c>ConnectionStrings</c> configuration section. Keys are tenant IDs as
    /// non-empty UUIDs in lowercase D format. Code should write <see cref="ConnectionStrings"/> instead.
    /// </summary>
    [ConfigurationKeyName("ConnectionStrings")]
    public IDictionary<string, string> ConfiguredConnectionStrings { get; set; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
