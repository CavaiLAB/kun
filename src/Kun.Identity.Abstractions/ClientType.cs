using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kun.Identity.Abstractions;

/// <summary>客户端登记类型，独立于 OIDC grant；Agent 为预留值。</summary>
[JsonConverter(typeof(ClientTypeConverter))]
public enum ClientType
{
    /// <summary>可保管机密的客户端。</summary>
    Confidential,
    /// <summary>公开客户端。</summary>
    Public,
    /// <summary>机器身份客户端。</summary>
    Machine,
    /// <summary>Agent 客户端。</summary>
    Agent
}

internal sealed class ClientTypeConverter : JsonConverter<ClientType>
{
    public override ClientType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType == JsonTokenType.String
        ? reader.GetString() switch { "confidential" => ClientType.Confidential, "public" => ClientType.Public, "machine" => ClientType.Machine, "agent" => ClientType.Agent, _ => throw new JsonException("未知客户端类型。") }
        : throw new JsonException("客户端类型必须为字符串。");
    public override void Write(Utf8JsonWriter writer, ClientType value, JsonSerializerOptions options) => writer.WriteStringValue(value switch { ClientType.Confidential => "confidential", ClientType.Public => "public", ClientType.Machine => "machine", ClientType.Agent => "agent", _ => throw new JsonException("未知客户端类型。") });
}
