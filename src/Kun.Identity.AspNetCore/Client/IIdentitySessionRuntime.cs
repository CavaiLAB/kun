using Kun.Identity.Implementations;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Kun.Identity.Client;

/// <summary>The relying-party settings and services used by the ASP.NET Core session adapter.</summary>
internal interface IIdentitySessionRuntime
{
    bool Active { get; }
    bool BrowserSessionsEnabled { get; }
    string Issuer { get; }
    string? Resource { get; }
    TimeSpan RefreshBeforeExpiry { get; }
    TimeSpan SessionLifetime { get; }
    ConfigurationManager<OpenIdConnectConfiguration> Metadata { get; }
    ValueTask<KunOidcClient> ClientAsync(CancellationToken cancellationToken);
}
