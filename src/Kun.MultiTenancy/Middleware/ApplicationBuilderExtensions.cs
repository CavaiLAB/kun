using Microsoft.AspNetCore.Builder;

namespace Kun.MultiTenancy.Middleware;

/// <summary>Registration extensions for <see cref="TenantContextMiddleware"/>.</summary>
public static class KunMultiTenancyApplicationBuilderExtensions
{
    /// <summary>
    /// Inserts the Kun multi-tenancy middleware into the pipeline. Call
    /// after <c>UseAuthentication()</c> so the boundary sees
    /// the authenticated <c>HttpContext.User</c>.
    /// </summary>
    public static IApplicationBuilder UseKunMultiTenancy(this IApplicationBuilder app, TenantRequestMode mode = TenantRequestMode.TenantRequired)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<TenantContextMiddleware>(mode);
    }
}
