using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kun.Sample.Catalog.Contracts;

/// <summary>JSON contracts used to register Catalog integration events.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(InventoryChangedIntegrationEvent))]
[JsonSerializable(typeof(ProductPublishedIntegrationEvent))]
public sealed partial class CatalogEventJson : JsonSerializerContext;
