using System.Diagnostics;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kun.Authorization;
using Kun.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kun.Grpc.Interceptors;

/// <summary>方法声明了权限时，确认全局或该服务的管道内已装配执行权限的拦截器。</summary>
internal sealed partial class RequiredPermissionInterceptorGuard(IOptions<GrpcServiceOptions> global,
    ServicePermissionInterceptors services, ILogger<RequiredPermissionInterceptorGuard> logger) : Interceptor
{
    private void Check(ServerCallContext context)
    {
        var endpoint = context.GetHttpContext().GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequiresPermissionAttribute>() is null) return;
        if (ServicePermissionInterceptors.ContainsEnforcement(global.Value)) return;
        if (endpoint.Metadata.GetMetadata<GrpcMethodMetadata>() is { } method && services.Has(method.ServiceType)) return;
        MissingInterceptor(logger, context.Method);
        var catalog = context.GetHttpContext().RequestServices.GetService<ErrorCatalog>() ?? ErrorCatalog.CreateDefault();
        var trace = Activity.Current?.TraceId is { } id && id != default ? id.ToHexString() : ActivityTraceId.CreateRandom().ToHexString();
        var detail = catalog.CreateDetail(new Error(PlatformErrors.AuthzDenied), trace, instance: context.Method);
        throw new RpcException(new Status(StatusCode.PermissionDenied, detail.Code), GrpcErrorTrailers.Create(detail));
    }

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "gRPC 方法 {Method} 声明了权限，但该服务没有登记授权拦截器。")]
    private static partial void MissingInterceptor(ILogger logger, string method);

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        Check(context);
        return await continuation(request, context).ConfigureAwait(false);
    }
    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Check(context);
        await continuation(request, responseStream, context).ConfigureAwait(false);
    }
    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Check(context);
        return await continuation(requestStream, context).ConfigureAwait(false);
    }
    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Check(context);
        await continuation(requestStream, responseStream, context).ConfigureAwait(false);
    }
}
