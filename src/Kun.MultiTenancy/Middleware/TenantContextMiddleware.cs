using Kun.Core.MultiTenancy;
using Kun.Errors;
using Microsoft.AspNetCore.Http;

namespace Kun.MultiTenancy.Middleware;

/// <summary>为 HTTP 请求打开租户作用域。</summary>
public sealed class TenantContextMiddleware(RequestDelegate next, TenantRequestMode mode)
{
    /// <summary>建立请求上下文后进入后续管道。</summary>
    public async Task InvokeAsync(HttpContext httpContext, ITenantContext tenantContext)
    {
        IDisposable scope;
        try
        {
            scope = new TenantRequestBoundary(tenantContext).Enter(httpContext, mode);
        }
        catch (ErrorException error) when (error.Error.Code.Equals(PlatformErrors.Unauthenticated))
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using (scope)
        {
            await next(httpContext).ConfigureAwait(false);
        }
    }
}
