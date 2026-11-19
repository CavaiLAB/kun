using Kun.EntityFrameworkCore;
using Kun.Identity.Abstractions;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Tenancy;
using System.Text.Json;

namespace Kun.Identity.Server;

/// <summary>清单分发的稳定入口；程序化适配模式由 IdentityServerPlugin 保留。</summary>
[KunPlugin("qian.identity.server", MetadataTypes = [typeof(IdentityRuntimeEvents), typeof(DirectoryDisableCapability), typeof(DirectoryAddCapability)])]
[KunConfigSchema(ComponentConfiguration.Schema)]
[CapabilitySource(typeof(DirectoryDisableCapability), nameof(DirectoryDisableCapability.Definition))]
[CapabilitySource(typeof(DirectoryAddCapability), nameof(DirectoryAddCapability.Definition))]
[DelegatePlugin(typeof(IdentityServerPlugin))]
[Provides(typeof(IdentityDefinition), nameof(IdentityDefinition.Key))]
[Provides(typeof(IdentityDefinition), nameof(IdentityDefinition.AccountsKey))]
[Provides(typeof(IdentityDefinition), nameof(IdentityDefinition.DirectoryKey))]
[Provides(typeof(IdentityDefinition), nameof(IdentityDefinition.SignInKey))]
[Provides(typeof(IdentityDefinition), nameof(IdentityDefinition.FactorsKey))]
[Provides(typeof(TenantProvisioning), nameof(TenantProvisioning.MembershipKey))]
[Provides(typeof(TenantSettings), nameof(TenantSettings.UserCounterKey))]
public sealed class IdentityServerComponent : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key, TenancyDefinition.Key];
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        return new IdentityServerPlugin().ApplyAsync(ctx, config, ct);
    }
}
