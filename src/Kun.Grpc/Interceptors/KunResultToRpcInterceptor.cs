using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kun.Errors;

namespace Kun.Grpc.Interceptors;

/// <summary>
/// gRPC server interceptor that catches <see cref="ErrorException"/> — thrown by business code or by
/// <c>Result&lt;T&gt;.EnsureSuccess()</c> — in any RPC handler and translates the carried
/// <see cref="Error"/> into the appropriate <see cref="RpcException"/>.
/// </summary>
internal sealed class KunResultToRpcInterceptor : Interceptor
{
    /// <inheritdoc />
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context).ConfigureAwait(false);
        }
        catch (ErrorException ex)
        {
            throw ToRpcException(ex.Error);
        }
    }

    /// <inheritdoc />
    public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        => MapAsync(() => continuation(request, responseStream, context));

    /// <inheritdoc />
    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try { return await continuation(requestStream, context).ConfigureAwait(false); }
        catch (ErrorException error) { throw ToRpcException(error.Error); }
    }

    /// <inheritdoc />
    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        => MapAsync(() => continuation(requestStream, responseStream, context));

    private static async Task MapAsync(Func<Task> next)
    {
        try { await next().ConfigureAwait(false); }
        catch (ErrorException error) { throw ToRpcException(error.Error); }
    }

    /// <summary>
    /// Maps an <see cref="Error"/> to an <see cref="RpcException"/>: the status code follows the HTTP status
    /// of the error code (<see cref="MapStatus"/>), the status detail is <see cref="Error.Message"/>, and the
    /// error code (<see cref="ErrorCode.Code"/>, e.g. <c>NOT_FOUND</c>) is included in the response trailers
    /// as <c>kun-error-code</c>.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>An <see cref="RpcException"/> with the mapped status code.</returns>
    public static RpcException ToRpcException(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        // A gRPC service can return its own codes without registering an HTTP error catalog.
        var catalog = new ErrorCatalog();
        catalog.Register(error.Code);
        foreach (var field in error.Errors) catalog.Register(field.Code);
        var trace = Activity.Current?.TraceId.ToHexString() ?? ActivityTraceId.CreateRandom().ToHexString();
        var detail = catalog.CreateDetail(error, trace);
        return new RpcException(new Status(MapStatus(error.Code.Status), error.Message), GrpcErrorTrailers.Create(detail));
    }

    /// <summary>
    /// Maps the HTTP status of an error code (<see cref="ErrorCode.Status"/>) to the corresponding gRPC
    /// <see cref="StatusCode"/>; statuses without a closer gRPC equivalent map to <see cref="StatusCode.Internal"/>.
    /// </summary>
    /// <param name="httpStatus">The HTTP status of the error code (field-level codes have none).</param>
    /// <returns>The gRPC status code.</returns>
    public static StatusCode MapStatus(int? httpStatus) => httpStatus switch
    {
        400 => StatusCode.InvalidArgument,
        401 => StatusCode.Unauthenticated,
        403 => StatusCode.PermissionDenied,
        404 => StatusCode.NotFound,
        408 or 504 => StatusCode.DeadlineExceeded,
        409 or 412 or 422 => StatusCode.FailedPrecondition,
        413 or 429 => StatusCode.ResourceExhausted,
        502 or 503 => StatusCode.Unavailable,
        _ => StatusCode.Internal
    };
}
