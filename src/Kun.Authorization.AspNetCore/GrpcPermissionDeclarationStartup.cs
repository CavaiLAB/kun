using Grpc.AspNetCore.Server;
using Kun.Authorization.Rbac.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Authorization.AspNetCore;

/// <summary>
/// 宿主启动时核对 gRPC 方法上 <see cref="RequiresPermissionAttribute"/> 的权限点都是 <c>&lt;plugin-id&gt;:&lt;resource&gt;:&lt;action&gt;</c> 全名。
/// HTTP 端点在 <see cref="AuthzEnforcementExtensions.RequireKunPermission{TBuilder}"/> 声明时已校验；gRPC 的权限点写在特性或端点元数据上，
/// 在映射完端点、开始接收请求之前于此处校验，不是全名即抛出。
/// </summary>
internal sealed class GrpcPermissionDeclarationStartup : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);
        if (app.ApplicationServices.GetService<EndpointDataSource>() is { } source) Validate(source.Endpoints);
    };

    internal static void Validate(IEnumerable<Endpoint> endpoints)
    {
        var invalid = new List<string>();
        foreach (var endpoint in endpoints)
        {
            if (endpoint.Metadata.GetMetadata<GrpcMethodMetadata>() is not { } method) continue;
            foreach (var declared in endpoint.Metadata.GetOrderedMetadata<RequiresPermissionAttribute>())
            {
                if (!PermissionCode.IsValid(declared.Permission)) invalid.Add(method.Method.FullName + " 的权限点 \"" + declared.Permission + "\"");
            }
        }
        if (invalid.Count > 0)
            throw new InvalidOperationException("gRPC 方法声明的权限点必须是 <plugin-id>:<resource>:<action> 全名：" + string.Join("；", invalid.Distinct(StringComparer.Ordinal)) + "。");
    }
}
