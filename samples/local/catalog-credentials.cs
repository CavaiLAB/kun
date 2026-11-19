#:project ../../src/Kun.Gateway/Kun.Gateway.csproj
#:property PublishAot=false
#:property IsPackable=false

using System.Security.Claims;
using System.Text.Json;
using Kun.Gateway;
using Kun.Gateway.Identity;
using Kun.Gateway.Jwks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

if (args is not [var directory] || !Directory.Exists(directory))
{
    Console.Error.WriteLine("Pass the path of a private local directory created for this demo.");
    return 1;
}

var options = new GatewayConfiguration
{
    // This local issuer has no upstream identity provider.
    IdentityIssuer = string.Empty,
    IdentityAudience = string.Empty,
    ExternalJwksUri = string.Empty,
    SigningKeyPath = Path.Combine(directory, "private.pem"),
    PublicKeyPath = Path.Combine(directory, "public.pem"),
    Issuer = "kun-sample",
    InternalAudience = "kun-sample-catalog",
    InternalTokenLifetime = TimeSpan.FromMinutes(5),
};
var host = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = Environments.Development });
using var issuer = new S2sTokenIssuer(Options.Create(options), host.Environment, NoOpS2sPrincipalResolver.Instance);
var identity = new ClaimsIdentity([
    new Claim("sub", "sample-order"),
    new Claim("principal_kind", "service"),
    new Claim("tenant_id", "5d72f90d-099e-4894-bd5b-e2f7d68a1162"),
], "LocalSample");
var token = await issuer.IssueForAsync(new ClaimsPrincipal(identity), CancellationToken.None);
var jwks = new JwksResponseBuilder().Build(options.PublicKeyPath);
File.WriteAllText(Path.Combine(directory, "public.jwks"), JsonSerializer.Serialize(jwks));
var pendingToken = Path.Combine(directory, Path.GetRandomFileName());
File.WriteAllText(pendingToken, token);
File.Move(pendingToken, Path.Combine(directory, "catalog.token"), overwrite: true);
return 0;
