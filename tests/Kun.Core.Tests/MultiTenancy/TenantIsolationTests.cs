using System.Text.Json;
using FluentAssertions;
using Kun.Core.MultiTenancy;
using Xunit;

namespace Kun.Core.Tests.MultiTenancy;

/// <summary>
/// Pins the <see cref="TenantIsolation"/> enum contract: exactly three values in the
/// documented order (Row=0, Schema=1, Database=2). Downstream code and
/// docs depend on the numeric values (e.g. appsettings JSON bindings) and the value count
/// (enum-exhaustive switch expressions).
/// </summary>
public sealed class TenantIsolationTests
{
    [Theory]
    [InlineData(TenantIsolation.Row, "row")]
    [InlineData(TenantIsolation.Schema, "schema")]
    [InlineData(TenantIsolation.Database, "database")]
    public void Json_RoundTripsPublishedNames(TenantIsolation value, string name)
    {
        var json = JsonSerializer.Serialize(value);
        Assert.Equal($"\"{name}\"", json);
        Assert.Equal(value, JsonSerializer.Deserialize<TenantIsolation>(json));
    }

    [Fact]
    public void Enum_HasExactlyThreeValues()
    {
        var values = Enum.GetValues<TenantIsolation>();

        values.Should().HaveCount(3);
    }

    [Fact]
    public void Enum_Values_MatchDocumentedNumericOrder()
    {
        ((int)TenantIsolation.Row).Should().Be(0);
        ((int)TenantIsolation.Schema).Should().Be(1);
        ((int)TenantIsolation.Database).Should().Be(2);
    }
}
