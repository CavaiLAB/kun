using Kun.Identity.Abstractions;
using Kun.Security.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Kun.Identity.Client;

/// <summary>
/// 下游服务的 S2S 校验：Kun.Security 负责验签（只收 RS256 / ES256）、issuer、受众与寿命；本类型再按冻结契约
/// （contracts/v0 s2s-claims 与 README §4）逐条检查载荷，通过后把快照放进请求项供应用的当前用户适配器读取。
/// 「每跳一个」受众：aud 必须恰好是本服务，上一跳收到的令牌转发到下一跳一律拒绝；用户访问令牌（typ at+jwt、发行方不同、载荷不闭合）同样进不来。
/// </summary>
internal static partial class IdentityS2sValidation
{
    internal static readonly object PrincipalKey = new();

    internal static void Configure(JwtBearerOptions jwt, string issuer, string audience, TimeProvider clock, ILogger logger)
    {
        jwt.MapInboundClaims = false;
        jwt.TokenValidationParameters.ValidTypes = ["JWT"];
        jwt.TokenValidationParameters.RequireSignedTokens = true;
        jwt.TokenValidationParameters.RequireExpirationTime = true;
        jwt.Events ??= new JwtBearerEvents();
        var previous = jwt.Events.OnTokenValidated;
        jwt.Events.OnTokenValidated = async context =>
        {
            await previous(context).ConfigureAwait(false);
            if (context.Result is { Succeeded: false }) return;
            var reason = Check(context, issuer, audience, clock);
            if (reason is null) return;
            Log.Rejected(logger, reason);
            context.Fail("S2S 令牌不符合契约。");
        };
        var failed = jwt.Events.OnAuthenticationFailed;
        jwt.Events.OnAuthenticationFailed = async context =>
        {
            var type = context.Exception.GetType().Name;
            Log.Rejected(logger, type);
            await failed(context).ConfigureAwait(false);
        };
    }

    // 返回拒绝原因的类别名；只记类别，不记令牌、主体或会话标识。
    private static string? Check(TokenValidatedContext context, string issuer, string audience, TimeProvider clock)
    {
        if (context.SecurityToken is not JsonWebToken token) return "token";
        using var payload = IdentityJsonPayload.Parse(token);
        if (payload is null) return "payload";
        var reason = PrincipalClaims.ReadS2s(context.Principal, payload.RootElement, clock.GetUtcNow(), out var principal);
        if (reason is not null) return reason;
        if (principal is null || principal.Issuer != issuer || principal.Audience[0] != audience) return "audience";
        context.HttpContext.Items[PrincipalKey] = principal;
        return null;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 30, Level = LogLevel.Information, Message = "Identity Client 拒绝 S2S 令牌；reason={Reason}。")]
        internal static partial void Rejected(ILogger logger, string reason);
    }
}
