using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Kun.Errors.AspNetCore;

namespace Kun.Errors.Tests;

public sealed class KunErrorsExtensionsTests
{
    [Fact]
    public void Repeated_calls_share_one_catalog_and_one_exception_handler()
    {
        var services = new ServiceCollection();
        services.AddKunErrors(c => c.Register(ErrorCatalogTests.QuotaExceeded));
        services.AddKunErrors();

        using var provider = services.BuildServiceProvider();
        var catalog = Assert.Single(provider.GetServices<ErrorCatalog>());
        Assert.True(catalog.Contains(ErrorCatalogTests.QuotaExceeded));
        Assert.True(catalog.Contains(PlatformErrors.NotFound));
        Assert.Single(services, d => d.ServiceType == typeof(IExceptionHandler));
    }

    [Fact]
    public void A_catalog_registered_by_factory_is_rejected()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_ => new ErrorCatalog());

        Assert.Throws<InvalidOperationException>(() => services.AddKunErrors());
    }
}
