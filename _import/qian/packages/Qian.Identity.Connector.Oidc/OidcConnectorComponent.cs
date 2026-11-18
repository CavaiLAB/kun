using Qian.Config;
using Qian.Identity.Abstractions;
using Qian.Plugin.Sdk;
using Qian.Runtime;
using Kun.Errors;
using System.Text.Json;

namespace Qian.Identity.Connector.Oidc;

/// <summary>清单分发的稳定入口；程序化适配模式由 OidcConnectorPlugin 保留。</summary>
[QianPlugin("qian.identity.connector.oidc")]
[QianConfigSchema(ComponentConfiguration.Schema)]
[DelegatePlugin(typeof(OidcConnectorPlugin))]
[Contributes(typeof(IdentityDefinition), nameof(IdentityDefinition.ConnectorsKey), "oidc")]
public sealed class OidcConnectorComponent : IQianPlugin
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
