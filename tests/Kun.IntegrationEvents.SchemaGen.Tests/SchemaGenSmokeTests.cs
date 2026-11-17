using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Json.Schema;
using Kun.EventBus;
using Xunit;

namespace Kun.IntegrationEvents.SchemaGen.Tests;

/// <summary>
/// End-to-end smoke test for the SchemaGen console tool: runs its entry point against a
/// registry defined in this assembly, then checks the exported schema against the
/// registered sample and against valid / invalid payloads. The tool had no test before,
/// so a JsonSchema.Net / JsonSchema.Net.Generation mismatch (a runtime MissingMethodException)
/// went unnoticed; any future change in the generated text also shows up here.
/// </summary>
public sealed class SchemaGenSmokeTests
{
    [Fact]
    public void Export_WritesOneSchemaPerRegisteredEvent_MatchingTheRegisteredSample()
    {
        var output = Directory.CreateTempSubdirectory("kun-schemagen-");
        try
        {
            var tool = Assembly.Load("Kun.IntegrationEvents.SchemaGen");
            string[] args =
            [
                "--assembly", typeof(SmokeEventRegistry).Assembly.Location,
                "--builder", $"{typeof(SmokeEventRegistry).FullName}::{nameof(SmokeEventRegistry.Build)}",
                "--out", output.FullName,
                "--indent",
            ];

            var exitCode = (int)tool.EntryPoint!.Invoke(null, [args])!;

            Assert.Equal(0, exitCode);
            Assert.Equal(["order.placed.v1.schema.json"], output.GetFiles().Select(file => file.Name));
            var exportedPath = Path.Combine(output.FullName, "order.placed.v1.schema.json");
            var exported = JsonNode.Parse(File.ReadAllText(exportedPath));
            var sample = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "order.placed.v1.schema.json")));
            Assert.True(JsonNode.DeepEquals(sample, exported), exported?.ToJsonString());

            var schema = JsonSchema.FromFile(exportedPath);
            using var valid = JsonDocument.Parse("""{"OrderId":"3f2b8c1e-9d4a-4f6b-8a2e-1c5d7e9f0a3b","Sku":"SKU-1","Quantity":2,"Amount":9.5,"At":{},"Note":null}""");
            using var invalid = JsonDocument.Parse("""{"OrderId":"3f2b8c1e-9d4a-4f6b-8a2e-1c5d7e9f0a3b","Sku":"SKU-1","Quantity":"two","Amount":9.5,"At":{},"Note":null}""");
            Assert.True(schema.Evaluate(valid.RootElement).IsValid);
            Assert.False(schema.Evaluate(invalid.RootElement).IsValid);
        }
        finally
        {
            output.Delete(recursive: true);
        }
    }
}

/// <summary>Sample integration event exported by the smoke test.</summary>
public sealed record OrderPlaced(Guid OrderId, string Sku, int Quantity, decimal Amount, DateTimeOffset At, string? Note);

/// <summary>Registry builder in the shape SchemaGen expects (public static, zero-argument).</summary>
public static class SmokeEventRegistry
{
    /// <summary>Returns a registry holding a single event.</summary>
    public static IEventTypeRegistry Build() => new Registry();

    private sealed class Registry : IEventTypeRegistry
    {
        private const string EventId = "order.placed.v1";

        public Type? Resolve(string eventType) => eventType == EventId ? typeof(OrderPlaced) : null;

        public string? GetEventType(Type clrType) => clrType == typeof(OrderPlaced) ? EventId : null;

        public IReadOnlyCollection<(string EventId, Type ClrType)> GetAll() => [(EventId, typeof(OrderPlaced))];

        public JsonTypeInfo? GetJsonTypeInfo(Type clrType) => null;
    }
}
