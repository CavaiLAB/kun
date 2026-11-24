using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kun.Identity.Configuration;
using Kun.Identity.Implementations;
using Kun.Identity.Tests.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Kun.Identity.Tests;

public sealed class KunOidcClientTests
{
    [Fact]
    public async Task CodeExchange_SendsPkceAndValidatesSignedNonceBoundIdentity()
    {
        using var provider = new ProtocolFixture();
        var tokens = await provider.Client.ExchangeCodeAsync(provider.Request, TestContext.Current.CancellationToken);
        tokens.ValidatedPrincipal!.FindFirst("sub")!.Value.Should().Be("user-a");
        provider.Handler.LastForm.Should().Contain("code_verifier=" + provider.Request.CodeVerifier).And.Contain("grant_type=authorization_code");
        provider.Handler.LastAuthorization.Should().Be("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("test-client:s%3Ae+c")));
    }

    [Fact]
    public void Defaults_ExposeTheSupportedSigningAlgorithmsAndClockSkew()
    {
        var options = new KunOidcClientOptions();
        options.AllowedSigningAlgorithms.Should().BeEquivalentTo(
            "RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "ES512");
        options.ClockSkew.Should().Be(TimeSpan.FromMinutes(2));
        options.AllowAdditionalAudiences.Should().BeFalse();
    }

    [Fact]
    public async Task CodeExchange_UsesConfiguredAsymmetricAlgorithmAndAuthorizedAudiences()
    {
        using var provider = new ProtocolFixture(SecurityAlgorithms.RsaSsaPssSha384, additionalAudience: true);
        var tokens = await provider.Client.ExchangeCodeAsync(provider.Request, TestContext.Current.CancellationToken);
        tokens.ValidatedPrincipal!.FindAll("aud").Select(claim => claim.Value).Should().BeEquivalentTo("test-client", "profile-api");
        tokens.ValidatedPrincipal.FindFirst("azp")!.Value.Should().Be("test-client");
    }

    [Theory]
    [InlineData("nonce")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("signature")]
    [InlineData("expiry")]
    [InlineData("issued-in-future")]
    [InlineData("additional-audience")]
    public async Task CodeExchange_RejectsInvalidIdToken(string fault)
    {
        using var provider = new ProtocolFixture();
        provider.Handler.Token = provider.Token(fault);
        var exchange = () => provider.Client.ExchangeCodeAsync(provider.Request, TestContext.Current.CancellationToken);
        await exchange.Should().ThrowAsync<SecurityTokenValidationException>();
    }

    [Fact]
    public async Task Grants_RefreshChecksSubject_AndMachineGrantUsesClientCredentials()
    {
        using var provider = new ProtocolFixture();
        var refresh = await provider.Client.RefreshAsync("refresh-value", "user-a", TestContext.Current.CancellationToken);
        refresh.ValidatedPrincipal.Should().NotBeNull();
        provider.Handler.LastForm.Should().Contain("grant_type=refresh_token").And.Contain("refresh_token=refresh-value");
        var wrongSubject = () => provider.Client.RefreshAsync("refresh-value", "other-user", TestContext.Current.CancellationToken);
        await wrongSubject.Should().ThrowAsync<SecurityTokenValidationException>();
        provider.Handler.Token = null;
        (await provider.Client.ClientCredentialsAsync("catalog.read", TestContext.Current.CancellationToken)).AccessToken.Should().Be("access-value");
        provider.Handler.LastForm.Should().Contain("grant_type=client_credentials").And.Contain("scope=catalog.read");
    }

    [Fact]
    public async Task Discovery_RefreshesAfterConfiguredInterval()
    {
        using var provider = new ProtocolFixture();
        (await provider.Client.DiscoverAsync(TestContext.Current.CancellationToken)).TokenEndpoint.Should().EndWith("/token");
        provider.Handler.TokenPath = "/next-token";
        (await provider.Client.DiscoverAsync(TestContext.Current.CancellationToken)).TokenEndpoint.Should().EndWith("/token");
        provider.Clock.Advance(TimeSpan.FromMinutes(6));
        (await provider.Client.DiscoverAsync(TestContext.Current.CancellationToken)).TokenEndpoint.Should().EndWith("/next-token");
        provider.Handler.DiscoveryCalls.Should().Be(2);
    }

    [Fact]
    public async Task Logout_RequiresSignedLogoutEventAndForbidsNonce()
    {
        using var provider = new ProtocolFixture();
        var logout = await provider.Client.ValidateLogoutTokenAsync(provider.Token(logout: true), TestContext.Current.CancellationToken);
        logout.Issuer.Should().Be(ProtocolFixture.Authority); logout.Subject.Should().Be("user-a"); logout.SessionId.Should().Be("provider-session");
        var idAsLogout = () => provider.Client.ValidateLogoutTokenAsync(provider.Token(), TestContext.Current.CancellationToken);
        await idAsLogout.Should().ThrowAsync<SecurityTokenValidationException>();
        var nonceInLogout = () => provider.Client.ValidateLogoutTokenAsync(provider.Token("nonce", logout: true), TestContext.Current.CancellationToken);
        await nonceInLogout.Should().ThrowAsync<SecurityTokenValidationException>();
    }

    [Fact]
    public async Task LegacyExchange_RejectsFlowWithoutPkceAndNonce()
    {
        using var provider = new ProtocolFixture();
#pragma warning disable KUNOIDC001 // Compatibility gate: old signature remains callable but fails closed.
        var exchange = () => provider.Client.ExchangeCodeAsync("code", "https://app.test/callback", TestContext.Current.CancellationToken);
#pragma warning restore KUNOIDC001
        await exchange.Should().ThrowAsync<NotSupportedException>();
    }

    private sealed class ProtocolFixture : IDisposable
    {
        internal const string Authority = "https://idp.test";
        private readonly RSA _key = RSA.Create(2048);
        internal FakeTimeProvider Clock { get; } = new();
        internal ProtocolHandler Handler { get; }
        internal KunOidcClient Client { get; }
        internal OidcCodeExchange Request { get; } = new("auth-code", "https://app.test/callback", new string('a', 64), "nonce-value");
        private readonly string _algorithm;
        private readonly bool _additionalAudience;

        public ProtocolFixture(string algorithm = SecurityAlgorithms.RsaSha256, bool additionalAudience = false)
        {
            _algorithm = algorithm;
            _additionalAudience = additionalAudience;
            Clock.SetUtcNow(DateTimeOffset.UtcNow);
            var key = _key.ExportParameters(false);
            var jwks = JsonSerializer.Serialize(new { keys = new[] { new { kty = "RSA", kid = "test-key", alg = algorithm, use = "sig", n = Base64UrlEncoder.Encode(key.Modulus!), e = Base64UrlEncoder.Encode(key.Exponent!) } } });
            Handler = new ProtocolHandler(jwks) { Token = Token() };
            Client = new KunOidcClient(new KunOidcClientOptions { Name = "test", Authority = Authority, ClientId = "test-client", ClientSecret = "s:e c", AllowedSigningAlgorithms = [algorithm], AllowAdditionalAudiences = additionalAudience }, new Factory(Handler), Clock);
        }
        internal string Token(string? fault = null, bool logout = false)
        {
            var now = Clock.GetUtcNow().UtcDateTime;
            var claims = new Dictionary<string, object> { ["sub"] = "user-a", ["sid"] = "provider-session", ["jti"] = Guid.NewGuid().ToString("N") };
            // additional-audience：客户端不允许额外受众时，带本客户端 azp 的多受众令牌也要被拒。
            var multipleAudiences = _additionalAudience || fault == "additional-audience";
            if (multipleAudiences)
            {
                claims["aud"] = new[] { "test-client", "profile-api" };
                claims["azp"] = "test-client";
            }
            if (!logout || fault == "nonce") claims["nonce"] = fault == "nonce" ? "wrong-nonce" : "nonce-value";
            if (logout) claims["events"] = new Dictionary<string, object> { ["http://schemas.openid.net/event/backchannel-logout"] = new Dictionary<string, object>() };
            using var other = RSA.Create(2048);
            return new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Issuer = fault == "issuer" ? "https://other.test" : Authority,
                Audience = multipleAudiences ? null : fault == "audience" ? "other-client" : "test-client",
                // issued-in-future：签发时间比当前时间晚 3 分钟，超出缺省的 2 分钟时钟容差。
                IssuedAt = fault == "issued-in-future" ? now.AddMinutes(3) : now.AddMinutes(-5),
                NotBefore = now.AddMinutes(-5),
                Expires = fault switch { "expiry" => now.AddMinutes(-3), "issued-in-future" => now.AddMinutes(10), _ => now.AddMinutes(2) },
                Claims = claims,
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(fault == "signature" ? other : _key) { KeyId = "test-key" }, _algorithm)
            });
        }
        public void Dispose() { Client.Dispose(); Handler.Dispose(); _key.Dispose(); }
    }

    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }
    private sealed class ProtocolHandler(string jwks) : HttpMessageHandler
    {
        internal string? Token { get; set; }
        internal string TokenPath { get; set; } = "/token";
        internal string? LastForm { get; private set; }
        internal string? LastAuthorization { get; private set; }
        internal int DiscoveryCalls { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body;
            if (request.RequestUri!.AbsolutePath == "/.well-known/openid-configuration")
            {
                DiscoveryCalls++;
                body = JsonSerializer.Serialize(new { issuer = ProtocolFixture.Authority, authorization_endpoint = ProtocolFixture.Authority + "/auth", token_endpoint = ProtocolFixture.Authority + TokenPath, jwks_uri = ProtocolFixture.Authority + "/jwks" });
            }
            else if (request.RequestUri.AbsolutePath == "/jwks") body = jwks;
            else
            {
                LastForm = await request.Content!.ReadAsStringAsync(cancellationToken);
                LastAuthorization = request.Headers.Authorization?.ToString();
                body = JsonSerializer.Serialize(new { access_token = "access-value", id_token = Token, refresh_token = "refresh-value", token_type = "Bearer", expires_in = 300 });
            }
            return new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }
}
