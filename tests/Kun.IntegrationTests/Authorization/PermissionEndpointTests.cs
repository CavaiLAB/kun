using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Kun.Authorization;
using Kun.Authorization.AspNetCore;
using Kun.IntegrationTests.EventBus;
using Kun.Plugin.Sdk.AspNetCore;
using Kun.Runtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Kun.IntegrationTests.Authorization;

public sealed class PermissionEndpointTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ASecondAuthorizationRegistrationIsRejected(bool firstPlatform, bool secondPlatform)
    {
        var services = new ServiceCollection();
        void Register(bool platform)
        {
            if (platform) services.AddKunAuthorization(_ => throw new InvalidOperationException());
            else services.AddKunAuthorization();
        }

        Register(firstPlatform);
        Assert.Throws<InvalidOperationException>(() => Register(secondPlatform));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LibraryAuthorizationRejectsAPluginHostAtStartup(bool registerPluginFirst)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        if (registerPluginFirst) builder.Services.AddKunPluginEndpoints(_ => throw new InvalidOperationException());
        builder.Services.AddKunAuthorization();
        if (!registerPluginFirst) builder.Services.AddKunPluginEndpoints(_ => throw new InvalidOperationException());
        await using var app = builder.Build();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => app.StartAsync(TestContext.Current.CancellationToken));
        Assert.Contains("平台模式", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LibraryAuthorizationRejectsBoundEndpointsEvenWhenTheCheckerAllows(bool declarePermission)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthentication>("test", _ => { });
        var logs = new CapturingLoggerProvider();
        builder.Logging.AddProvider(logs);
        builder.Services.AddKunAuthorization();
        var checker = Substitute.For<IPermissionChecker>();
#pragma warning disable CA2012
        checker.CheckAsync(Arg.Any<PermissionRequest>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult(new PermissionDecision(true, "test", [])));
#pragma warning restore CA2012
        builder.Services.AddSingleton(checker);
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        var binding = new PluginEndpointBinding("catalog", "sample.crm", new ServiceKey("catalog"));
        app.UseKunAuthorization();
        var endpoint = app.MapGet("/protected", () => "result").WithMetadata(binding);
        if (declarePermission) endpoint.RequireKunPermission("sample.crm:item:read");
        await app.StartAsync(TestContext.Current.CancellationToken);
        using var response = await app.GetTestClient().GetAsync("/protected", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var startup = Assert.Single(logs.Entries, entry => entry.Category == "Kun.Authorization.AspNetCore.LibraryAuthorizationStartup" && entry.EventId == 5);
        Assert.Equal(LogLevel.Information, startup.Level);
        Assert.Contains("库模式", startup.Message, StringComparison.Ordinal);
        Assert.Contains("不写决策审计", startup.Message, StringComparison.Ordinal);
        Assert.Contains("不比对身份修订", startup.Message, StringComparison.Ordinal);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4 && entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task EndpointPermissionRequiresAQualifiedPluginIdAtDeclaration()
    {
        var builder = WebApplication.CreateBuilder();
        await using var app = builder.Build();
        var endpoint = app.MapGet("/protected", () => "result");
        Assert.Throws<ArgumentException>(() => endpoint.RequireKunPermission("sample:item:read"));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.Forbidden)]
    [InlineData(true, HttpStatusCode.OK)]
    public async Task ProtectedHttpEndpoint_MissingOrDenyingEngineNeverExecutesHandler(bool? allow, HttpStatusCode expected)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthentication>("test", _ => { });
        builder.Services.AddKunAuthorization();
        var calls = new List<string>();
        if (allow is not null)
        {
            var checker = Substitute.For<IPermissionChecker>();
#pragma warning disable CA2012 // 设置 mock 回调，不消费业务 ValueTask。
            checker.CheckAsync(Arg.Any<PermissionRequest>(), Arg.Any<CancellationToken>()).Returns(info =>
            {
                var request = info.Arg<PermissionRequest>();
                Assert.Equal("user-1", request.Subject.Id);
                Assert.Equal("tenant-a", request.Subject.TenantId);
                calls.Add("check");
                return ValueTask.FromResult(new PermissionDecision(allow.Value, "test", []));
            });
#pragma warning restore CA2012
            builder.Services.AddSingleton(checker);
        }
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/protected", () => { calls.Add("handler"); return "ok"; }).RequireKunPermission("sample.test:item:read");
        await app.StartAsync(TestContext.Current.CancellationToken);
        var response = await app.GetTestClient().GetAsync("/protected", TestContext.Current.CancellationToken);
        Assert.Equal(expected, response.StatusCode);
        if (allow == true) Assert.Equal(["check", "handler"], calls);
        else Assert.DoesNotContain("handler", calls);
    }

    private sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1"), new Claim("tenant_id", "tenant-a")], "test")), "test")));
    }
}
