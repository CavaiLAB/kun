using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kun.Sample.Order.Contracts;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(OrderCompletedIntegrationEvent))]
internal sealed partial class OrderEventJson : JsonSerializerContext;
