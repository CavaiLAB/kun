using System.Net;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using FluentAssertions;
using Kun.Gateway.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.Gateway.Tests;

/// <summary>
/// <see cref="KunGatewayServiceCollectionExtensions.AddKunGateway"/> 拒绝匿名请求的两项缺省：宿主映射的端点没有鉴权元数据时按缺省鉴权策略要求登录；
/// 代理路由没有声明策略时，网关给它补上 <c>Default</c> 策略。两条用例各自只依赖其中一项。
/// </summary>
public sealed class GatewayAuthorizationDefaultsTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "kun-gateway-authz-" + Guid.NewGuid().ToString("N"));

    public GatewayAuthorizationDefaultsTests()
    {
        Directory.CreateDirectory(_directory);
        using var key = RSA.Create(2048);
        File.WriteAllText(Path.Combine(_directory, "private.pem"), key.ExportRSAPrivateKeyPem());
        File.WriteAllText(Path.Combine(_directory, "public.pem"), key.ExportRSAPublicKeyPem());
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public async Task HostEndpointWithoutAuthorizationMetadata_RejectsAnonymousRequest()
    {
        await using var app = await StartAsync(keepFallbackPolicy: true);
        using var client = app.GetTestClient();

        using var response = await client.GetAsync("/host/status", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProxyRouteWithoutPolicy_RejectsAnonymousRequestWithoutFallbackPolicy()
    {
        await using var app = await StartAsync(keepFallbackPolicy: false);
        using var client = app.GetTestClient();

        // 缺省鉴权策略已去掉：宿主端点放行，所以代理路由的 401 只能来自网关补上的 Default 策略。
        using (var host = await client.GetAsync("/host/status", TestContext.Current.CancellationToken))
            host.StatusCode.Should().Be(HttpStatusCode.OK);
        using var proxied = await client.GetAsync("/protected/resource", TestContext.Current.CancellationToken);

        proxied.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<WebApplication> StartAsync(bool keepFallbackPolicy)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        // 路由不写 AuthorizationPolicy；目标地址不可达，请求一旦被转发就不会是 401。
        var reverseProxy = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Routes:protected:ClusterId"] = "backend",
            ["Routes:protected:Match:Path"] = "/protected/{**rest}",
            ["Clusters:backend:Destinations:one:Address"] = "http://127.0.0.1:9/"
        }).Build();
        builder.Services.AddKunGateway(new GatewayConfiguration
        {
            IdentityIssuer = "https://login.example.com",
            IdentityAudience = "my-api",
            ExternalJwksUri = "https://login.example.com/.well-known/openid-configuration",
            SigningKeyPath = Path.Combine(_directory, "private.pem"),
            PublicKeyPath = Path.Combine(_directory, "public.pem")
        }, reverseProxyConfig: reverseProxy);
        builder.Services.AddAuthentication(NoCredentials.Name)
            .AddScheme<AuthenticationSchemeOptions, NoCredentials>(NoCredentials.Name, configureOptions: null);
        if (!keepFallbackPolicy)
            builder.Services.Configure<AuthorizationOptions>(options => options.FallbackPolicy = null);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapGet("/host/status", () => "ok");
        app.MapReverseProxy();
        await app.StartAsync(TestContext.Current.CancellationToken);
        return app;
    }

    /// <summary>不认任何凭据的认证方案：请求一律匿名，质询返回 401。</summary>
    private sealed class NoCredentials(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        internal const string Name = "no-credentials";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
    }
}
