using Grpc.Core;
using Kun.Errors;
using Kun.Security.Authentication;
using Grpc.Core.Interceptors;

namespace Kun.Grpc.Interceptors;

/// <summary>
/// gRPC server interceptor that enforces authenticated access for multi-tenant calls.
/// Validates that the request has an authenticated <see cref="System.Security.Claims.ClaimsPrincipal"/>
/// with a valid subject. The tenant interceptor establishes the scope, including a platform scope.
/// </summary>
internal sealed class KunServerAuthInterceptor : Interceptor
{
    private static readonly ErrorCatalog Errors = ErrorCatalog.CreateDefault();

    /// <inheritdoc />
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        Authorize(context);
        return continuation(request, context);
    }

    /// <inheritdoc />
    public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Authorize(context);
        return continuation(request, responseStream, context);
    }

    /// <inheritdoc />
    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Authorize(context);
        return continuation(requestStream, context);
    }

    /// <inheritdoc />
    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Authorize(context);
        return continuation(requestStream, responseStream, context);
    }

    private static void Authorize(ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        if (PrincipalClaims.TryRead(user, out _, out _, out _)) return;
        var trace = System.Diagnostics.Activity.Current?.TraceId.ToHexString() ?? System.Diagnostics.ActivityTraceId.CreateRandom().ToHexString();
        var detail = Errors.CreateDetail(new Error(PlatformErrors.Unauthenticated), trace, instance: context.Method);
        throw new RpcException(new Status(StatusCode.Unauthenticated, detail.Code), GrpcErrorTrailers.Create(detail));
    }
}
