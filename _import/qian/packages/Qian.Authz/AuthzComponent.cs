using Kun.EntityFrameworkCore;
using Qian.Authz.Abstractions;
using Kun.Data;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using System.Text.Json;

namespace Qian.Authz;

/// <summary>清单分发的稳定入口；程序化适配模式由 AuthzPlugin 保留。</summary>
[KunPlugin("qian.authz", MetadataTypes = [typeof(AuthzRuntimeEvents)])]
[KunConfigSchema(ComponentConfiguration.Schema)]
[DelegatePlugin(typeof(AuthzPlugin))]
[Provides(typeof(AuthzDefinition), nameof(AuthzDefinition.Key))]
[Provides(typeof(AuthzDefinition), nameof(AuthzDefinition.PermissionsKey))]
[Provides(typeof(AuthzDefinition), nameof(AuthzDefinition.SuperadminBypassKey))]
[Provides(typeof(AuthzDefinition), nameof(AuthzDefinition.SuperadminCheckKey))]
[Provides(typeof(AuthzDefinition), nameof(AuthzDefinition.ResourceTypesKey))]
[Provides(typeof(AuthzEnforcement), nameof(AuthzEnforcement.Key))]
[Provides(typeof(TenantRoleGrant), nameof(TenantRoleGrant.Key))]
[Provides(typeof(CommandPolicies), nameof(CommandPolicies.AuthorizationKey))]
public sealed class AuthzComponent : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key];
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        return new AuthzPlugin().ApplyAsync(ctx, config, ct);
    }
}
