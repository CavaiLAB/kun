using System.Security.Claims;
using Kun.Core.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Kun.MultiTenancy;

/// <summary>请求入口允许的主体范围。</summary>
public enum TenantRequestMode
{
    /// <summary>请求需要一个已认证的租户身份。</summary>
    TenantRequired,
    /// <summary>同时允许已认证的平台身份；匿名请求交给宿主的授权策略处理。</summary>
    PlatformPrincipalAllowed
}

/// <summary>按已认证主身份的 tenant_id 建立本次请求的租户作用域。</summary>
public sealed class TenantRequestBoundary(ITenantContext tenant)
{
    /// <summary>在租户作用域内执行请求，完成、异常或取消后还原上下文。</summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        using var scope = Enter(context, TenantRequestMode.TenantRequired);
        await next(context).ConfigureAwait(false);
    }

    internal IDisposable Enter(HttpContext context, TenantRequestMode mode)
    {
        ArgumentNullException.ThrowIfNull(context);
        Guid? id = null;
        if (context.User.Identity is ClaimsIdentity { IsAuthenticated: true } identity)
        {
            var claims = identity.Claims.Where(claim => claim.Type == "tenant_id").Take(2).ToArray();
            if (claims.Length != 1) throw TenantContextPropagator.Denied();
            if (claims[0].ValueType != "JSON_NULL") id = TenantContextPropagator.Parse(claims[0].Value);
        }

        if (id is null && mode != TenantRequestMode.PlatformPrincipalAllowed)
            throw TenantContextPropagator.Denied();
        if (tenant.TenantId is { } existing && TenantContextPropagator.Parse(existing) != id)
            throw TenantContextPropagator.Denied();
        return tenant.BeginScope(id?.ToString("D"));
    }
}
