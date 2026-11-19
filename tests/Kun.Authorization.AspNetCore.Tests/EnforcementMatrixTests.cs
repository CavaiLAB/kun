using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Kun.Authorization.Rbac.Abstractions;
using Kun.Errors;
using Kun.Errors.AspNetCore;
using Kun.Grpc.DependencyInjection;
using Kun.Identity.Abstractions;
using Kun.Plugin.Sdk.AspNetCore;
using Kun.Runtime;
using Kun.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Kun.Authorization.AspNetCore.Tests;

public sealed class EnforcementMatrixTests
{
    private const string Permission = "sample.matrix:item:read";

    // 每行按相同的已认证输入比较两种模式；HTTP 状态也用于核对 gRPC ErrorDetail。
    public static TheoryData<string, string, string, int> Cases()
    {
        (string Scenario, int Platform, int? Library)[] cases =
        [
            ("anonymous", 401, 401), ("denied", 403, 403), ("granted", 200, 200),
            ("stale", 401, 200), ("delegated", 403, 403), ("platform-admin", 200, 403),
            ("platform-denied", 403, 403), ("audit-unavailable", 404, null),
            ("missing-engine", 404, 403), ("missing-registration", 403, 403),
            ("undeclared-production", 403, 403), ("undeclared-development", 200, 403)
        ];
        var data = new TheoryData<string, string, string, int>();
        foreach (var (scenario, platform, library) in cases)
        {
            foreach (var transport in new[] { "http", "grpc" })
            {
                data.Add(transport, "platform", scenario, platform);
                if (library is not null) data.Add(transport, "library", scenario, library.Value);
            }
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Matrix(string transport, string mode, string scenario, int expected)
    {
        var ct = TestContext.Current.CancellationToken;
        var state = new MatrixState(scenario);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(state);
        builder.Services.AddAuthentication("matrix").AddScheme<AuthenticationSchemeOptions, MatrixAuthentication>("matrix", _ => { });
        builder.Services.AddKunErrors();
        builder.Services.AddAuthorization();
        if (scenario == "missing-registration") builder.Services.AddKunGrpc();
        else
        {
            if (mode == "platform") state.RegisterPlatform(builder.Services);
            else state.RegisterLibrary(builder.Services);
            builder.Services.AddKunGrpc(options => options.AddKunAuthorizationInterceptor());
        }

        await using var app = builder.Build();
        app.UseKunErrors();
        app.UseAuthentication();
        app.UseAuthorization();
        // 在 HTTP 分支装 HTTP 中间件；RPC 方法授权由 gRPC 自身管道负责。
        if (scenario != "missing-registration")
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/test"), branch => branch.UseKunAuthorization());
        var http = app.MapGet("/test", state.Execute);
        var grpc = app.MapGrpcService<EchoService>();
        if (state.Undeclared)
        {
            var binding = new PluginEndpointBinding("matrix", "sample.matrix", new ServiceKey("matrix"));
            http.WithMetadata(binding);
            grpc.WithMetadata(binding);
        }
        else
        {
            http.RequireKunPermission(Permission);
            grpc.WithMetadata(new RequiresPermissionAttribute(Permission));
        }
        await app.StartAsync(ct);

        if (transport == "http")
        {
            using var client = app.GetTestClient();
            using var response = await client.GetAsync("/test", ct);
            Assert.Equal(expected, (int)response.StatusCode);
            if (expected == 200) Assert.Equal("executed", await response.Content.ReadAsStringAsync(ct));
            else if (scenario != "missing-registration")
            {
                Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
                var detail = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
                Assert.Equal(ErrorCodeFor(expected), detail.GetProperty("code").GetString());
                Assert.Equal(expected, detail.GetProperty("status").GetInt32());
            }
        }
        else
        {
            using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = app.GetTestServer().CreateHandler() });
            using var call = channel.CreateCallInvoker().AsyncUnaryCall(EchoBindings.Call, null, new CallOptions(cancellationToken: ct), "test");
            if (expected == 200) Assert.Equal("executed", await call.ResponseAsync);
            else
            {
                var error = await Assert.ThrowsAsync<RpcException>(() => call.ResponseAsync);
                Assert.Equal(expected switch { 401 => StatusCode.Unauthenticated, 404 => StatusCode.NotFound, _ => StatusCode.PermissionDenied }, error.StatusCode);
                Assert.Equal(ErrorCodeFor(expected), error.Trailers.GetValue("kun-error-code"));
                var bytes = Assert.Single(error.Trailers, entry => entry.Key == AuthzGrpcServerInterceptor.ErrorDetailTrailer).ValueBytes;
                var detail = ErrorDetailJson.Deserialize(Encoding.UTF8.GetString(bytes));
                Assert.NotNull(detail);
                Assert.Equal((ErrorCodeFor(expected), expected), (detail.Code, detail.Status));
            }
        }
        Assert.Equal(expected == 200 ? 1 : 0, state.Executions);
        if (mode == "platform" && scenario is not ("anonymous" or "missing-registration" or "missing-engine" or "undeclared-development"))
        {
            var request = Assert.Single(state.Requests);
            Assert.Equal(transport, request.EntryPoint);
            Assert.Equal(state.Undeclared ? null : Permission, request.Permission);
            Assert.Equal(scenario == "delegated", request.Delegated);
            Assert.Equal(state.Principal.IdentityRevision, request.IdentityRevision);
            Assert.Equal(state.Principal.TenantId, request.Subject.TenantId);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ServiceScopedRegistrationIsDiscoveredOnlyForTheMappedService(bool configuredForMappedService)
    {
        var ct = TestContext.Current.CancellationToken;
        var state = new MatrixState("granted");
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(state);
        builder.Services.AddAuthentication("matrix").AddScheme<AuthenticationSchemeOptions, MatrixAuthentication>("matrix", _ => { });
        state.RegisterLibrary(builder.Services);
        var grpc = builder.Services.AddKunGrpc();
        if (configuredForMappedService) grpc.AddServiceOptions<EchoService>(options => options.AddKunAuthorizationInterceptor());
        else grpc.AddServiceOptions<OtherService>(options => options.AddKunAuthorizationInterceptor());

        await using var app = builder.Build();
        app.UseAuthentication();
        app.MapGrpcService<EchoService>().WithMetadata(new RequiresPermissionAttribute(Permission));
        await app.StartAsync(ct);
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = app.GetTestServer().CreateHandler() });
        using var call = channel.CreateCallInvoker().AsyncUnaryCall(EchoBindings.Call, null, new CallOptions(cancellationToken: ct), "test");
        if (configuredForMappedService) Assert.Equal("executed", await call.ResponseAsync);
        else Assert.Equal(StatusCode.PermissionDenied, (await Assert.ThrowsAsync<RpcException>(() => call.ResponseAsync)).StatusCode);
        Assert.Equal(configuredForMappedService ? 1 : 0, state.Executions);
    }

    private sealed class OtherService { }

    private static string ErrorCodeFor(int status) => status switch { 401 => "UNAUTHENTICATED", 403 => "AUTHZ_DENIED", 404 => "NOT_FOUND", _ => throw new ArgumentOutOfRangeException(nameof(status)) };

    public sealed class MatrixState(string scenario)
    {
        public int Executions { get; private set; }
        internal List<EnforcementRequest> Requests { get; } = [];
        internal bool Undeclared => scenario.StartsWith("undeclared-", StringComparison.Ordinal);
        internal bool Anonymous => scenario == "anonymous";
        internal Principal Principal { get; } = new()
        {
            Issuer = "matrix",
            Audience = ["matrix"],
            Subject = "test-user",
            Kind = PrincipalKind.User,
            TenantId = scenario.StartsWith("platform-", StringComparison.Ordinal) ? null : "00000000-0000-4000-8000-00000000000a",
            IdentityRevision = scenario == "stale" ? 1 : null,
            Actor = scenario == "delegated" ? new DelegationActor { Kind = PrincipalKind.Service, Subject = "test-service" } : null,
            Roles = [],
            Scope = "",
            SessionId = "test-session",
            TokenId = "test-token",
            IssuedAt = 1,
            NotBefore = 1,
            ExpiresAt = 2
        };

        internal string Execute() { Executions++; return "executed"; }

        internal void RegisterPlatform(IServiceCollection services)
        {
            var user = Substitute.For<ICurrentUser>();
            user.IsAuthenticated.Returns(!Anonymous);
            user.UserId.Returns(Principal.Subject);
            user.Principal.Returns(Principal);
            services.AddSingleton(user);
            var port = Substitute.For<IAuthzEnforcement>();
#pragma warning disable CA2012 // 在替身上登记 ValueTask 返回值。
            port.EnforceAsync(Arg.Any<EnforcementRequest>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                var request = call.Arg<EnforcementRequest>();
                Requests.Add(request);
                var reason = scenario switch { "stale" => AuthzEnforcement.StaleIdentityRevision, "audit-unavailable" => AuthzEnforcement.AuditUnavailable, _ => "test" };
                var decision = new PermissionDecision(scenario is "granted" or "platform-admin", reason, []);
                return ValueTask.FromResult(new AuthorizationDecision(Guid.NewGuid(), request, new AuthorizationExplanation(decision, [], [], []),
                    scenario == "platform-admin", true, DateTimeOffset.UtcNow));
            });
#pragma warning restore CA2012
            var source = Substitute.For<IPluginContext>();
            source.Inject<IAuthzEnforcement>(AuthzEnforcement.Key).Returns(port);
            services.AddKunAuthorization(_ => scenario == "missing-engine" ? throw new ServiceNotReadyException() : source,
                options => options.Production = scenario != "undeclared-development");
        }

        internal void RegisterLibrary(IServiceCollection services)
        {
            services.AddKunAuthorization();
            if (scenario == "missing-engine") return;
            var checker = Substitute.For<IPermissionChecker>();
#pragma warning disable CA2012 // 在替身上登记 ValueTask 返回值。
            checker.CheckAsync(Arg.Any<PermissionRequest>(), Arg.Any<CancellationToken>()).Returns(
                ValueTask.FromResult(new PermissionDecision(scenario is "granted" or "stale" or "delegated" || Undeclared, "test", [])));
#pragma warning restore CA2012
            services.AddSingleton(checker);
        }
    }

    private sealed class MatrixAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, MatrixState state)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (state.Anonymous) return Task.FromResult(AuthenticateResult.NoResult());
            var principal = state.Principal;
            var claims = new List<Claim> { new("sub", principal.Subject), new("principal_kind", "user") };
            claims.Add(principal.TenantId is { } tenantId ? new Claim("tenant_id", tenantId) : new Claim("tenant_id", "", "JSON_NULL"));
            if (principal.Actor is not null) claims.Add(new("act", "{}"));
            if (principal.IdentityRevision is not null) claims.Add(new("identity_revision", "1"));
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }

    [BindServiceMethod(typeof(EchoBindings), nameof(EchoBindings.BindService))]
    public class EchoService(MatrixState state)
    {
        public virtual Task<string> Echo(string request, ServerCallContext context) => Task.FromResult(state.Execute());
    }

    public static class EchoBindings
    {
        private static readonly Marshaller<string> Marshaller = Marshallers.Create(Encoding.UTF8.GetBytes, Encoding.UTF8.GetString);
        public static readonly Method<string, string> Call = new(MethodType.Unary, "matrix.Service", "Echo", Marshaller, Marshaller);
        public static void BindService(ServiceBinderBase binder, EchoService? service) => binder.AddMethod(Call, service is null ? null : service.Echo);
    }
}
