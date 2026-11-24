using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Kun.Security.Authentication;

/// <summary>
/// Extension methods for registering Kun S2S JWT Bearer authentication on downstream services.
/// </summary>
public static class KunServiceAuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication for gateway-issued S2S tokens. It validates the signature
    /// (asymmetric RS256 or ES256 only; HS256 is rejected, consistent with the zero-trust policy),
    /// the issuer, the audience and the lifetime with a 30-second clock skew. The issuer and the
    /// audience must be one of the configured values.
    /// </summary>
    /// <remarks>
    /// For the full S2S contract check (closed claim set, exactly this service as the audience,
    /// identity, roles, scope and the 300-second lifetime limit), register <c>AddKunIdentityS2s</c>
    /// from Kun.Identity.Client, or call <see cref="PrincipalClaims.ReadS2s"/> from the scheme's
    /// <c>OnTokenValidated</c> event and then compare the expected issuer and audience.
    /// </remarks>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="configure">Delegate to configure <see cref="KunServiceAuthenticationOptions"/>.</param>
    /// <returns>The authentication builder for chaining.</returns>
    public static AuthenticationBuilder AddKunServiceAuthentication(
        this AuthenticationBuilder builder,
        Action<KunServiceAuthenticationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var opts = new KunServiceAuthenticationOptions();
        configure(opts);

        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOpts =>
        {
            jwtOpts.Authority = null; // manual validation — no discovery
            jwtOpts.RequireHttpsMetadata = false; // internal network; TLS handled at transport

            var validIssuers = BuildValidIssuers(opts);
            var validAudiences = BuildValidAudiences(opts);

            jwtOpts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = validIssuers,
                ValidateAudience = true,
                ValidAudiences = validAudiences,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                // Reject all symmetric (HS*) algorithms — only RS256/ES256 allowed
                ValidAlgorithms = ["RS256", "ES256"],
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            if (opts.StaticKeySet is not null)
            {
                jwtOpts.TokenValidationParameters.IssuerSigningKeys =
                    opts.StaticKeySet.GetSigningKeys();
            }
            else if (!string.IsNullOrWhiteSpace(opts.JwksUri))
            {
                // JwksUri points at a raw JWKS endpoint (per the option's contract),
                // not an OIDC discovery document — so parse it with
                // JwksConfigurationRetriever. OpenIdConnectConfigurationRetriever would
                // treat the response as discovery metadata and surface zero signing
                // keys (IDX10500). ConfigurationManager still caches the keys and
                // refreshes on an unknown kid.
                var retriever = new HttpDocumentRetriever { RequireHttps = false };
                jwtOpts.TokenValidationParameters.ConfigurationManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>(
                        opts.JwksUri,
                        new JwksConfigurationRetriever(),
                        retriever);
            }
        });

        return builder;
    }

    private static List<string> BuildValidIssuers(KunServiceAuthenticationOptions opts)
    {
        var list = new List<string> { opts.Issuer };
        if (opts.AdditionalValidIssuers is not null)
            list.AddRange(opts.AdditionalValidIssuers);
        return list;
    }

    private static List<string> BuildValidAudiences(KunServiceAuthenticationOptions opts)
    {
        var list = new List<string> { opts.Audience };
        if (opts.AdditionalValidAudiences is not null)
            list.AddRange(opts.AdditionalValidAudiences);
        return list;
    }
}
