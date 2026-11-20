namespace Kun.Gateway;

internal sealed class GatewayRuntime(GatewayConfiguration configuration) : IGatewayService
{
    private int _active;
    internal GatewayConfiguration Configuration { get; } = configuration;
    internal string SerializedConfiguration { get; } = GatewayConfigurationReader.Serialize(configuration);
    public bool IsReady => Volatile.Read(ref _active) == 1;
    public string AudienceFor(string clusterId) => Configuration.Clusters.SingleOrDefault(cluster => cluster.Id == clusterId)?.Id
        ?? throw new InvalidOperationException("目标 cluster 未登记。");
    internal void Activate()
    {
        if (Interlocked.CompareExchange(ref _active, 1, 0) != 0) throw new InvalidOperationException("网关组件已经挂载。");
    }
    internal void Deactivate() => Interlocked.Exchange(ref _active, 0);
}
