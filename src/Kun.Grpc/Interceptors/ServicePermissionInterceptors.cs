using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Kun.Authorization;
using Microsoft.Extensions.Options;

namespace Kun.Grpc.Interceptors;

internal sealed class ServicePermissionInterceptors
{
    private readonly ConcurrentDictionary<Type, bool> _services = new();

    internal void Record(Type service, GrpcServiceOptions options) => _services[service] = ContainsEnforcement(options);
    internal bool Has(Type service) => _services.TryGetValue(service, out var installed) && installed;
    internal static bool ContainsEnforcement(GrpcServiceOptions options) =>
        options.Interceptors.Any(item => typeof(IPermissionEnforcementInterceptor).IsAssignableFrom(item.Type));
}

// 在 gRPC 发现每个服务时读取同一份按服务配置的选项。TService 由 gRPC 本身提供，无须反射构造泛型。
internal sealed class ServicePermissionInterceptorDiscovery<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TService>(
    IOptions<GrpcServiceOptions<TService>> options,
    ServicePermissionInterceptors services) : IServiceMethodProvider<TService> where TService : class
{
    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context) => services.Record(typeof(TService), options.Value);
}
