using Kun.Errors;
using Kun.Errors.AspNetCore;
using Yarp.ReverseProxy.Transforms;

namespace Kun.Gateway.Transforms;

/// <summary>Replaces the external authorization header with one token for the selected downstream.</summary>
internal sealed class S2sAuthTransform(S2sTokenIssuer issuer, GatewayRuntime? runtime = null, string? audience = null) : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var http = context.HttpContext;
        var headers = context.ProxyRequest.Headers;
        headers.Remove("Authorization");
        if (runtime is not null)
        {
            headers.Remove("Cookie");
            headers.Remove("Forwarded");
            headers.Remove("X-Forwarded-For");
            headers.Remove("X-Forwarded-Proto");
            headers.Remove("X-Forwarded-Host");
            if (http.Connection.RemoteIpAddress is { } remote)
                headers.TryAddWithoutValidation("X-Forwarded-For", remote.ToString());
            headers.TryAddWithoutValidation("X-Forwarded-Proto", http.Request.Scheme);
            headers.TryAddWithoutValidation("X-Forwarded-Host", new Uri(runtime.Configuration.Issuer).Authority);
            if (!runtime.IsReady)
            {
                http.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }
        if (http.User.Identity?.IsAuthenticated != true)
        {
            if (runtime is not null) await Unauthorized(http).ConfigureAwait(false);
            return;
        }
        try
        {
            var token = audience is null
                ? await issuer.IssueForAsync(http.User, http.RequestAborted).ConfigureAwait(false)
                : await issuer.IssueForAudienceAsync(http.User, audience, http.RequestAborted).ConfigureAwait(false);
            headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
        }
        catch (InvalidOperationException)
        {
            await Unauthorized(http).ConfigureAwait(false);
        }
    }

    private static Task Unauthorized(HttpContext http) =>
        KunResults.Error(new Error(PlatformErrors.Unauthenticated)).ExecuteAsync(http);
}
