using Qian.Config;
using Kun.Identity.Abstractions;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Errors;
using System.Text.Json;

namespace Kun.Identity.Connector.Oidc;

/// <summary>清单分发的稳定入口；程序化适配模式由 OidcConnectorPlugin 保留。</summary>
[KunPlugin("qian.identity.connector.oidc")]
[KunConfigSchema(ComponentConfiguration.Schema)]
[DelegatePlugin(typeof(OidcConnectorPlugin))]
[Contributes(typeof(IdentityDefinition), nameof(IdentityDefinition.ConnectorsKey), "oidc")]
public sealed class OidcConnectorComponent : IKunPlugin
{
    /// <inheritdoc />
    public IReadOnlyList<ServiceKey> Inject => [IdentityDefinition.Key, ConfigDefinition.Key];
    /// <inheritdoc />
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        if (OidcConnectorOptions.FromConfig(config).Name != "oidc")
            throw new ErrorException(new Error(PlatformErrors.InputInvalid)
            { Errors = [new FieldError(PlatformErrors.ValueNotAllowed, "#/name")] });
        return new OidcConnectorPlugin().ApplyAsync(ctx, config, ct);
    }
}
