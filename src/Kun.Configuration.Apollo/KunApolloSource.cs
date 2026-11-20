using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Kun.Configuration.Apollo;

// 每次 Build 建一份发布记录器，校验委托绑定到它；同一构建器重复 Build 时各自记录。
internal sealed class KunApolloSource(Func<ApolloReleaseTracker, ApolloOptions> options, string environment, string cluster, string appId,
    KunConfigurationOptions owner) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var tracker = new ApolloReleaseTracker(appId, environment, cluster);
        return new KunApolloProvider(options(tracker), environment, owner, tracker);
    }
}

internal sealed class KunApolloProvider : IConfigurationProvider, IDisposable
{
    private readonly ApolloReleaseTracker _releases;
    private readonly ApolloConfigurationProvider _inner;
    private readonly IDisposable _reloads;

    internal KunApolloProvider(ApolloOptions options, string environment, KunConfigurationOptions owner, ApolloReleaseTracker releases)
    {
        _releases = releases;
        Description = new ApolloConfigurationDescription(options, environment, releases);
        _inner = new ApolloConfigurationProvider(options, owner.HttpClient, owner.Logger);
        // 在 Load 之前订阅：首次在线加载的重载回调也要确认为「已接受」。
        _reloads = ChangeToken.OnChange(_inner.GetReloadToken, releases.Reloaded);
    }
    internal ApolloConfigurationDescription Description { get; }
    public bool TryGet(string key, out string? value) => _inner.TryGet(key, out value);
    public void Set(string key, string? value) => _inner.Set(key, value);
    public IChangeToken GetReloadToken() => _inner.GetReloadToken();
    public void Load()
    {
        _inner.Load();
        _releases.Loaded();
    }
    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath) => _inner.GetChildKeys(earlierKeys, parentPath);
    public void Dispose()
    {
        _reloads.Dispose();
        _inner.Dispose();
    }
}
