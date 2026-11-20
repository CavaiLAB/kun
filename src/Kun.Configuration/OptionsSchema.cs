using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;

namespace Kun.Configuration;

/// <summary>由源生成 JSON 元数据导出的配置契约，不包含任何运行时配置值。</summary>
public sealed class OptionsSchema(string section, JsonTypeInfo typeInfo, string? plugin = null)
{
    /// <summary>该配置绑定的 section。</summary>
    public string Section { get; } = section;

    /// <summary>配置类型全名。</summary>
    public string OptionsType { get; } = typeInfo.Type.FullName ?? typeInfo.Type.Name;

    /// <summary>登记该契约的组件标识。</summary>
    public string? Plugin { get; } = plugin;

    /// <summary>每次返回独立的 JSON Schema 2020-12 节点，调用方修改不会污染缓存。</summary>
    public JsonObject Export()
    {
        var schema = typeInfo.GetJsonSchemaAsNode(new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true }).AsObject();
        schema["$schema"] = "https://json-schema.org/draft/2020-12/schema";
        return schema;
    }
}
