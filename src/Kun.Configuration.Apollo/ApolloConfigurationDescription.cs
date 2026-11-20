using Microsoft.Extensions.Configuration;

namespace Kun.Configuration.Apollo;

/// <summary>已装配的 Apollo 配置源与其脱敏的发布记录。</summary>
public sealed class ApolloConfigurationDescription
{
    private readonly ApolloReleaseTracker _releases;

    internal ApolloConfigurationDescription(ApolloOptions options, string environment, ApolloReleaseTracker releases)
    {
        AppId = options.AppId;
        Environment = environment;
        Servers = Array.AsReadOnly(options.ConfigServers.ToArray());
        Namespaces = Array.AsReadOnly(options.Namespaces.ToArray());
        _releases = releases;
    }

    /// <summary>返回指定提供程序的来源信息；其他配置源返回空。</summary>
    public static ApolloConfigurationDescription? From(IConfigurationProvider provider) =>
        (provider as KunApolloProvider)?.Description;

    /// <summary>应用标识。</summary>
    public string AppId { get; }
    /// <summary>引导阶段选定的环境。</summary>
    public string Environment { get; }
    /// <summary>按引导顺序排列的配置服务器。</summary>
    public IReadOnlyList<Uri> Servers { get; }
    /// <summary>按覆盖顺序排列的命名空间。</summary>
    public IReadOnlyList<string> Namespaces { get; }

    /// <summary>从本实例的有界队列消费下一条发布记录，宿主为每个源指定一个读取方。</summary>
    public ValueTask<ApolloReleaseRecord> ReadReleaseAsync(CancellationToken cancellationToken) =>
        _releases.Records.ReadAsync(cancellationToken);
}
