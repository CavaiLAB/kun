using FluentAssertions;
using Kun.Identity.DependencyInjection;
using Kun.Identity.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kun.Identity.Tests;

public sealed class DependencyInjectionTests
{
    private static ServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        return services;
    }

    [Fact]
    public void AddJwtBearer_RegistersValidator()
    {
        var services = BuildServices();
        services.AddKunIdentity(id => id.AddJwtBearer(o =>
        {
            o.Issuer = "https://idp.test";
            o.Audience = "kun-api";
            o.IssuerSigningKeys.Add(TestKeys.CreateRsaKey());
        }));

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IJwtValidator>().Should().BeOfType<KunJwtValidator>();
    }

    [Fact]
    public void AddOidcClient_RegistersNamedClient()
    {
        var services = BuildServices();
        services.AddKunIdentity(id => id.AddOidcClient("authentik", o =>
        {
            o.Authority = "https://idp.test";
            o.ClientId = "kun-edge";
        }));

        using var provider = services.BuildServiceProvider();
        var clients = provider.GetServices<IOidcClient>().ToList();
        clients.Should().ContainSingle(c => c.Name == "authentik");
    }
}
