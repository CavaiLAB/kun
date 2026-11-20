using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;
using Kun.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.Configuration.Tests;

public sealed class ReloadOptions
{
    [Range(1, 10)]
    public int Retries { get; set; }
    public required string Endpoint { get; set; }
}
[JsonSerializable(typeof(ReloadOptions))]
internal sealed partial class ReloadJsonContext : JsonSerializerContext;

[RequiresDynamicCode("验证既有的反射绑定入口。")]
[RequiresUnreferencedCode("验证既有的 DataAnnotations 入口。")]
public sealed class LastGoodOptionsTests
{
    [Fact]
    public void InvalidReload_KeepsOldValue_ReportsMetric_ThenRecovers()
    {
        var rejected = 0L;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meter) =>
        {
            if (instrument.Name == "kun.configuration.reload.rejected") meter.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref rejected, value));
        listener.Start();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Service:Retries"] = "3",
            ["Service:Endpoint"] = "https://config.test"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddKunOptions<ReloadOptions>("Service", ReloadJsonContext.Default.ReloadOptions);
        using var provider = services.BuildServiceProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<ReloadOptions>>();
        var original = monitor.CurrentValue;
        var notifications = 0;
        using var subscription = monitor.OnChange((_, _) => notifications++);
        config["Service:Retries"] = "0";
        config.Reload();
        Assert.Same(original, monitor.CurrentValue);
        config["Service:Retries"] = "not-an-integer";
        config.Reload();
        Assert.Same(original, monitor.CurrentValue);
        Assert.Equal(2, rejected);
        Assert.Equal(0, notifications);
        config["Service:Retries"] = "5";
        config.Reload();
        Assert.Equal(5, monitor.CurrentValue.Retries);
        Assert.Equal(1, notifications);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DelegateAndReflection_ValidateFirstRead_AndNotifyOutsideTheMonitorLock(bool useDelegate)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Service:Retries"] = "0",
            ["Service:Endpoint"] = "https://config.test"
        }).Build();
        using var configOwner = (IDisposable)config;
        var services = new ServiceCollection().AddSingleton<IConfiguration>(config);
        RegisterOptions(services, useDelegate);
        using var provider = services.BuildServiceProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<ReloadOptions>>();
        Assert.Throws<OptionsValidationException>(() => monitor.CurrentValue);

        config["Service:Retries"] = "3";
        config.Reload();
        var accepted = monitor.CurrentValue;
        using var notifications = new ChangeObservations(monitor);
        var failures = new List<OptionsReloadFailure>();
        using var observe = provider.GetRequiredService<IObservable<OptionsReloadFailure>>().Subscribe(
            new FailureObserver(failure =>
            {
                failures.Add(failure);
                notifications.ReadFromAnotherThread();
            }));
        using var subscription = monitor.OnChange((_, _) => notifications.ReadFromAnotherThread());
        config["Service:Retries"] = "0";
        config.Reload();
        Assert.Same(accepted, monitor.CurrentValue);
        var rejected = Assert.Single(failures);
        Assert.Equal((typeof(ReloadOptions).FullName, "", "Service", useDelegate ? "test.options" : null, 1),
            (rejected.OptionsType, rejected.OptionsName, rejected.Section, rejected.Plugin, rejected.Failures));
        config["Service:Retries"] = "5";
        config.Reload();
        Assert.Equal(5, monitor.CurrentValue.Retries);
        Assert.Equal([3, 5], notifications.ReadValues);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DisposedDependency_PropagatesWithoutReportingARejectedCandidate(bool useDelegate)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Service:Retries"] = "3",
            ["Service:Endpoint"] = "https://config.test"
        }).Build();
        using var configOwner = (IDisposable)config;
        var services = new ServiceCollection().AddSingleton<IConfiguration>(config);
        RegisterOptions(services, useDelegate).Validate(o => o.Retries == 8 ? throw new ObjectDisposedException("provider") : true);
        var failures = new List<OptionsReloadFailure>();
        services.AddSingleton<IObserver<OptionsReloadFailure>>(new FailureObserver(failures.Add));
        using var provider = services.BuildServiceProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<ReloadOptions>>();
        var accepted = monitor.CurrentValue;
        config["Service:Retries"] = "8";
        var error = Assert.Throws<AggregateException>(config.Reload);
        Assert.IsType<ObjectDisposedException>(Assert.Single(error.Flatten().InnerExceptions));
        Assert.Same(accepted, monitor.CurrentValue);
        Assert.Empty(failures);
        config["Service:Retries"] = "5";
        config.Reload();
        Assert.Equal(5, monitor.CurrentValue.Retries);
    }

    private static OptionsBuilder<ReloadOptions> RegisterOptions(IServiceCollection services, bool useDelegate)
    {
        if (!useDelegate) return services.AddKunOptions<ReloadOptions>("Service", critical: false);
        return services.AddKunOptions<ReloadOptions>("Service", (section, options) =>
        {
            options.Retries = int.Parse(section["Retries"]!, CultureInfo.InvariantCulture);
            options.Endpoint = section["Endpoint"]!;
        }, critical: false, plugin: "test.options").Validate(options => options.Retries is >= 1 and <= 10);
    }

    private sealed class FailureObserver(Action<OptionsReloadFailure> observe) : IObserver<OptionsReloadFailure>
    {
        public void OnNext(OptionsReloadFailure value) => observe(value);
        public void OnCompleted() { }
        public void OnError(Exception error) => throw error;
    }

    private sealed class ChangeObservations(IOptionsMonitor<ReloadOptions> monitor) : IDisposable
    {
        private readonly List<Thread> _workers = [];
        public List<int?> ReadValues { get; } = [];
        public void ReadFromAnotherThread()
        {
            int? value = null;
            var worker = new Thread(() => value = monitor.CurrentValue.Retries);
            _workers.Add(worker);
            worker.Start();
            ReadValues.Add(worker.Join(TimeSpan.FromSeconds(5)) ? value : null);
        }
        public void Dispose()
        {
            foreach (var worker in _workers) worker.Join();
        }
    }

    [Fact]
    public async Task OptionalUnusedConfiguration_DoesNotPreventStartup_ButStillValidatesWhenRead()
    {
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services =>
            services.AddKunOptions<ReloadOptions>("OptionalMissing", critical: false)).Build();
        await host.StartAsync(TestContext.Current.CancellationToken);
        Assert.Throws<OptionsValidationException>(() => host.Services.GetRequiredService<IOptionsMonitor<ReloadOptions>>().CurrentValue);
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AddKunOptions_Exports202012Schema_FromSourceGeneratedMetadata()
    {
        var services = new ServiceCollection();
        services.AddKunOptions<ReloadOptions>("Service", ReloadJsonContext.Default.ReloadOptions);
        using var provider = services.BuildServiceProvider();
        var entry = provider.GetRequiredService<OptionsSchema>();
        var schema = entry.Export();
        Assert.Equal("Service", entry.Section);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string?)schema["$schema"]);
        // Fresh registry per build: JsonSchema.Net 9 refuses to register the same $id twice.
        var validator = JsonSchema.FromText(schema.ToJsonString(), new BuildOptions { SchemaRegistry = new SchemaRegistry { Fetch = (_, _) => null } });
        using var valid = JsonDocument.Parse("""{"Retries":3,"Endpoint":"https://test"}""");
        using var invalid = JsonDocument.Parse("""{"Retries":"bad","Endpoint":"https://test"}""");
        Assert.True(validator.Evaluate(valid.RootElement).IsValid);
        Assert.False(validator.Evaluate(invalid.RootElement).IsValid);
        schema["title"] = "mutated";
        Assert.Null(entry.Export()["title"]);
    }
}
