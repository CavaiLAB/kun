using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kun.Plugin.Sdk;
using Kun.Runtime;

namespace ReleaseProbe;

[KunPlugin("qian.release-probe")]
public sealed class ProbePlugin : IKunPlugin
{
    public static readonly ServiceKey Probe = new("ReleaseProbe");
    public IReadOnlyList<ServiceKey> Inject => [];
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        ctx.Provide(Probe, new object());
        return ValueTask.CompletedTask;
    }
}

[Capability("probe.read", typeof(ProbeResponse), typeof(ProbeJson), Description = "读取发布核对组件的一条记录，不修改任何数据。", RequiredScopes = ["qian.release-probe:read"])]
public sealed partial record ProbeRequest(Guid Id);
public sealed record ProbeResponse(Guid Id);

[JsonSerializable(typeof(ProbeRequest))]
[JsonSerializable(typeof(ProbeResponse))]
public sealed partial class ProbeJson : JsonSerializerContext;

public static class Program
{
    public static void Main()
    {
        var probe = Read(typeof(ProbePlugin).Assembly);
        var data = Read(typeof(Kun.Data.DataPlugin).Assembly);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            probe = new { probe.ComponentId, probe.RuntimeEntry, probe.Provides, Capabilities = probe.Capabilities.Select(c => new { c.Id, Metadata = c.Metadata }) },
            data = new { data.ComponentId, data.RuntimeEntry, data.Provides },
        }));
    }

    private static ComponentCodeMetadata Read(Assembly assembly)
    {
        var declaration = assembly.GetCustomAttribute<KunCodeMetadataAttribute>() ?? throw new InvalidOperationException(assembly.GetName().Name + " 没有生成元数据");
        return ((IComponentCodeMetadataFactory)Activator.CreateInstance(declaration.FactoryType)!).Create();
    }
}
