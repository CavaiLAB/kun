using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Kun.Grpc.Interceptors;
using Kun.MultiTenancy;
using Kun.MultiTenancy.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kun.Grpc.DependencyInjection;

/// <summary>
/// Extension methods for registering Kun gRPC services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class KunGrpcServiceCollectionExtensions
{
    /// <summary>
    /// Registers Kun gRPC services and interceptors:
    /// <list type="bullet">
    ///   <item><see cref="KunTracingInterceptor"/> — starts an <c>Activity</c> per RPC call.</item>
    ///   <item><see cref="KunServerAuthInterceptor"/> — requires an authenticated subject.</item>
    ///   <item><see cref="TenantGrpcServerInterceptor"/> — establishes an authenticated tenant or platform scope.</item>
    ///   <item><see cref="RequiredPermissionInterceptorGuard"/> — verifies the permission interceptor registration.</item>
    ///   <item><see cref="KunResultToRpcInterceptor"/> — maps <c>ErrorException</c> to <c>RpcException</c>.</item>
    /// </list>
    /// Optionally call <c>AddKunSecurity()</c> separately to wire up <c>JwtCurrentUser</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional callback to further configure <see cref="GrpcServiceOptions"/>.</param>
    /// <returns>The <see cref="IGrpcServerBuilder"/> for further gRPC configuration.</returns>
    public static IGrpcServerBuilder AddKunGrpc(
        this IServiceCollection services,
        Action<GrpcServiceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKunMultiTenancy();
        services.TryAddSingleton<ServicePermissionInterceptors>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>), typeof(ServicePermissionInterceptorDiscovery<>)));
        return services.AddGrpc(options =>
        {
            options.Interceptors.Add<KunTracingInterceptor>();
            options.Interceptors.Add<KunServerAuthInterceptor>();
            options.Interceptors.Add<TenantGrpcServerInterceptor>(TenantRequestMode.PlatformPrincipalAllowed);
            options.Interceptors.Add<RequiredPermissionInterceptorGuard>();
            options.Interceptors.Add<KunResultToRpcInterceptor>();
            configure?.Invoke(options);
        });
    }
}
