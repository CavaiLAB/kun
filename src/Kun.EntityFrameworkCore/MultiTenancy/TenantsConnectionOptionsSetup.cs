using Microsoft.Extensions.Options;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// Copies entries from <see cref="TenantsConnectionOptions.ConfiguredConnectionStrings"/> into
/// <see cref="TenantsConnectionOptions.ConnectionStrings"/>. Validation rejects keys that are not
/// non-empty UUIDs in lowercase D format, and tenants whose code and configuration entries disagree.
/// </summary>
internal sealed class TenantsConnectionOptionsSetup :
    IPostConfigureOptions<TenantsConnectionOptions>,
    IValidateOptions<TenantsConnectionOptions>
{
    public void PostConfigure(string? name, TenantsConnectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var (key, value) in options.ConfiguredConnectionStrings)
        {
            // Malformed keys and conflicting values stay out of the map; Validate reports them.
            if (TryParseTenantId(key, out var tenantId))
            {
                options.ConnectionStrings.TryAdd(tenantId, value);
            }
        }
    }

    public ValidateOptionsResult Validate(string? name, TenantsConnectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<string>? failures = null;
        foreach (var (key, value) in options.ConfiguredConnectionStrings)
        {
            if (!TryParseTenantId(key, out var tenantId))
            {
                (failures ??= []).Add(
                    $"TenantsConnectionOptions.ConnectionStrings key '{key}' must be a non-empty UUID in lowercase D format.");
            }
            else if (options.ConnectionStrings.TryGetValue(tenantId, out var existing) &&
                     !string.Equals(existing, value, StringComparison.Ordinal))
            {
                (failures ??= []).Add(
                    $"TenantsConnectionOptions.ConnectionStrings key '{key}' has a different connection string in code and in configuration.");
            }
        }

        return failures is null ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool TryParseTenantId(string key, out Guid tenantId) =>
        Guid.TryParseExact(key, "D", out tenantId) &&
        tenantId != Guid.Empty &&
        string.Equals(key, tenantId.ToString("D"), StringComparison.Ordinal);
}
