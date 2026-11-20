using Kun.Core.MultiTenancy;
using Kun.Errors;
using Kun.EventBus.DependencyInjection;
using Kun.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kun.EventBus.Tests;

public sealed class EventsHostLifecycleTests
{
    [Fact]
    public async Task Concurrent_shutdown_releases_only_the_hosts_error_registrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext());
        services.AddKunEventBus();
        services.AddKunEvents();
        using var provider = services.BuildServiceProvider();
        var catalog = provider.GetRequiredService<ErrorCatalog>();
        using var sharedRegistration = catalog.Register(EventsErrors.Misconfigured);
        var hosted = provider.GetRequiredService<IHostedService>();
        var disposable = Assert.IsAssignableFrom<IDisposable>(hosted);

        for (var cycle = 0; cycle < 16; cycle++)
        {
            await hosted.StartAsync(cancellationToken);
            Assert.True(catalog.Contains(EventsErrors.PublishRejected));
            Assert.True(catalog.Contains(EventsErrors.ConsumeRejected));

            await Task.WhenAll(
                Task.Run(() => hosted.StopAsync(cancellationToken), cancellationToken),
                Task.Run(disposable.Dispose, cancellationToken));

            Assert.True(catalog.Contains(EventsErrors.Misconfigured));
            Assert.False(catalog.Contains(EventsErrors.PublishRejected));
            Assert.False(catalog.Contains(EventsErrors.ConsumeRejected));
        }
    }
}
