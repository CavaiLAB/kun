using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Kun.Security.Tests;

// 主体类型经枚举上的转换器特性读写（不带源生成上下文的默认读写）：只收 user、service、agent 三个小写字符串。
// 经身份契约源生成上下文的那条路，由身份契约的测试钉住。
public sealed class PrincipalKindJsonTests
{
    [Theory]
    [InlineData(PrincipalKind.User, "\"user\"")]
    [InlineData(PrincipalKind.Service, "\"service\"")]
    [InlineData(PrincipalKind.Agent, "\"agent\"")]
    public void Converter_WritesAndReadsLowercaseStrings(PrincipalKind kind, string json)
    {
        JsonSerializer.Serialize(kind).Should().Be(json);
        JsonSerializer.Deserialize<PrincipalKind>(json).Should().Be(kind);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("\"User\"")]
    [InlineData("\"robot\"")]
    [InlineData("null")]
    public void Converter_RejectsNumbersWrongCaseUnknownValuesAndNull(string json)
    {
        var read = () => JsonSerializer.Deserialize<PrincipalKind>(json);
        read.Should().Throw<JsonException>();
    }

    [Fact]
    public void Converter_RejectsUndefinedEnumValuesOnWrite()
    {
        var write = () => JsonSerializer.Serialize((PrincipalKind)42);
        write.Should().Throw<JsonException>();
    }
}
