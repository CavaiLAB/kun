using Kun.Configuration;
using Kun.Configuration.Apollo;
using Kun.Errors;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Kun.IntegrationTests.Configuration;

public sealed partial class ApolloReloadTests
{
    [Fact]
    public void ComposedSources_ApplyHostAndApolloValuesInOrder()
    {
        using var host = new BootstrapHost();
        File.WriteAllText(Path.Combine(host.Directory, "appsettings.json"), """{"Service":{"Retries":"1"}}""");
        var secrets = System.IO.Directory.CreateDirectory(Path.Combine(host.Directory, "secrets"));
        File.WriteAllText(Path.Combine(secrets.FullName, "Service__Retries"), "4");
        Environment.SetEnvironmentVariable(host.Prefix + "Service__Retries", "5");
        try
        {
            using var root = host.Build(secrets.FullName, "--Service:Retries=6");
            var layers = root.Providers.Select(provider => provider.TryGet("Service:Retries", out var value) ? value : null)
                .Where(value => value is not null).ToArray();
            Assert.Equal(["1", "3", "4", "5", "6"], layers);
            Assert.Equal("6", root["Service:Retries"]);
            var info = Assert.Single(root.Providers.Select(ApolloConfigurationDescription.From).OfType<ApolloConfigurationDescription>());
            Assert.Equal("sample", info.AppId);
            Assert.Equal("UAT", info.Environment);
            Assert.Equal(["application"], info.Namespaces);
        }
        finally { Environment.SetEnvironmentVariable(host.Prefix + "Service__Retries", null); }
    }

    [Fact]
    public void InvalidBootstrap_ReportsTheEnvironmentBeforeOpeningTheSource()
    {
        using var host = new BootstrapHost();
        var error = Assert.Throws<ErrorException>(() => host.Build(null, "--Apollo:Env=staging"));
        Assert.Equal(ConfigErrors.BootstrapInvalid, error.Error.Code);
        Assert.Equal(0, host.Handler.SignedRequests);
    }

    [Fact]
    public async Task AccessKeyReference_IsResolvedAndThePublishedResultContainsMetadata()
    {
        using var host = new BootstrapHost();
        using var root = host.Build();
        var info = Assert.Single(root.Providers.Select(ApolloConfigurationDescription.From).OfType<ApolloConfigurationDescription>());
        var release = await info.ReadReleaseAsync(TestContext.Current.CancellationToken);
        Assert.Equal("application", release.Namespace);
        Assert.False(release.Rejected);
        Assert.Equal(1, release.ChangedKeys);
        Assert.Empty(release.Reasons);
        Assert.True(host.Resolutions > 0);
        Assert.True(host.Handler.SignedRequests > 0);
    }

    [Fact]
    public void ComposedSource_UsesTheSameRuleParserForValidAndRejectedSnapshots()
    {
        using var host = new BootstrapHost();
        host.Handler.FlagRule = """{"default":true,"percentage":20}""";
        using (var root = host.Build())
        {
            var rule = FeatureFlagRules.Read(root.AsEnumerable())["beta"];
            Assert.True(rule.Default);
            Assert.Equal(20, rule.Percentage);
            Assert.Empty(rule.Tenants);
        }
        host.Handler.FlagRule = """{"default":true,"percentage":101}""";
        Assert.False(FeatureFlagRules.Validate(new Dictionary<string, string?> { ["flag.beta"] = host.Handler.FlagRule }));
        Assert.Throws<InvalidOperationException>(() => host.Build());
    }

    private sealed class BootstrapHost : IDisposable
    {
        private const string AccessKeyReference = "secretRef://dev/sample/apollo";
        private readonly HttpClient _client;
        internal BootstrapHost()
        {
            _client = new HttpClient(Handler);
            Directory = System.IO.Directory.CreateTempSubdirectory("kun-apollo-host-").FullName;
        }
        internal ApolloHandler Handler { get; } = new();
        internal string Directory { get; }
        internal string Prefix { get; } = "KUN_APOLLO_" + Guid.NewGuid().ToString("N") + "_";
        internal int Resolutions;
        internal ConfigurationRoot Build(string? secrets = null, params string[] overrides)
        {
            var arguments = new List<string>
            {
                "--Apollo:AppId=sample", "--Apollo:Env=UAT", "--Apollo:ConfigServer:0=https://apollo.test",
                "--Apollo:AccessKey=" + AccessKeyReference
            };
            arguments.AddRange(overrides);
            return (ConfigurationRoot)new ConfigurationBuilder().AddKunConfigurationSources(new KunConfigurationOptions
            {
                BasePath = Directory,
                Arguments = arguments,
                EnvironmentPrefix = Prefix,
                SecretDirectory = secrets,
                HttpClient = _client,
                ResolveAccessKey = reference =>
                {
                    Assert.Equal(AccessKeyReference, reference);
                    Interlocked.Increment(ref Resolutions);
                    return "test-access-key";
                }
            }).Build();
        }
        public void Dispose()
        {
            _client.Dispose();
            System.IO.Directory.Delete(Directory, true);
        }
    }
}
