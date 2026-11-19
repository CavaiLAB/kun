using Grpc.AspNetCore.Server;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kun.Authorization;
using Kun.Grpc.DependencyInjection;
using Kun.Grpc.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Kun.Grpc.Tests;

public sealed class PermissionInterceptorTests
{
    [Theory]
    [InlineData("missing", false)]
    [InlineData("global-before", true)]
    [InlineData("global-after", true)]
    [InlineData("same-service", true)]
    [InlineData("other-service", false)]
    public async Task AllRpcShapes_RequireAnEnforcementInterceptorForThisService(string position, bool installed)
    {
        var services = new ServiceCollection();
        if (position == "global-before") services.Configure<GrpcServiceOptions>(opts => opts.Interceptors.Add<Enforcement>());
        services.AddKunGrpc();
        if (position == "global-after") services.Configure<GrpcServiceOptions>(opts => opts.Interceptors.Add<Enforcement>());
        if (position == "same-service") services.Configure<GrpcServiceOptions<FirstService>>(opts => opts.Interceptors.Add<Enforcement>());
        if (position == "other-service") services.Configure<GrpcServiceOptions<OtherService>>(opts => opts.Interceptors.Add<Enforcement>());
        using var provider = services.BuildServiceProvider();
        var registrations = provider.GetRequiredService<ServicePermissionInterceptors>();
        new ServicePermissionInterceptorDiscovery<FirstService>(provider.GetRequiredService<IOptions<GrpcServiceOptions<FirstService>>>(), registrations)
            .OnServiceMethodDiscovery(null!);
        new ServicePermissionInterceptorDiscovery<OtherService>(provider.GetRequiredService<IOptions<GrpcServiceOptions<OtherService>>>(), registrations)
            .OnServiceMethodDiscovery(null!);
        var http = new DefaultHttpContext { RequestServices = provider };
        http.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new RequiresPermissionAttribute("sample.test:item:read"),
            new GrpcMethodMetadata(typeof(FirstService), Substitute.For<IMethod>())), "test"));
        var context = Substitute.For<ServerCallContext>();
        context.UserState.Returns(new Dictionary<object, object> { ["__HttpContext"] = http });
        var interceptor = new RequiredPermissionInterceptorGuard(provider.GetRequiredService<IOptions<GrpcServiceOptions>>(), registrations,
            NullLogger<RequiredPermissionInterceptorGuard>.Instance);
        var reader = Substitute.For<IAsyncStreamReader<string>>();
        var writer = Substitute.For<IServerStreamWriter<string>>();
        var executed = 0;
        Task<string> Unary(string _, ServerCallContext __) { executed++; return Task.FromResult("ok"); }
        Task Server(string _, IServerStreamWriter<string> __, ServerCallContext ___) { executed++; return Task.CompletedTask; }
        Task<string> Client(IAsyncStreamReader<string> _, ServerCallContext __) { executed++; return Task.FromResult("ok"); }
        Task Duplex(IAsyncStreamReader<string> _, IServerStreamWriter<string> __, ServerCallContext ___) { executed++; return Task.CompletedTask; }
        Func<Task>[] invoke = [
            async () => await interceptor.UnaryServerHandler("request", context, Unary),
            () => interceptor.ServerStreamingServerHandler("request", writer, context, Server),
            async () => await interceptor.ClientStreamingServerHandler(reader, context, Client),
            () => interceptor.DuplexStreamingServerHandler(reader, writer, context, Duplex)
        ];
        foreach (var call in invoke)
        {
            if (installed) await call();
            else Assert.Equal(StatusCode.PermissionDenied, (await Assert.ThrowsAsync<RpcException>(call)).StatusCode);
        }
        Assert.Equal(installed ? 4 : 0, executed);
        await reader.DidNotReceive().MoveNext(Arg.Any<CancellationToken>());
    }
    private sealed class Enforcement : Interceptor, IPermissionEnforcementInterceptor { }
    private sealed class FirstService { }
    private sealed class OtherService { }
}
