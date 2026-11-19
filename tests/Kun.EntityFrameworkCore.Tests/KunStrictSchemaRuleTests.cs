using Kun.Core.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests;

// 严格基类的 schema 规则：内核缺省只要求合法的小写标识符；宿主用 RequireKunSchemaPrefix 加前缀（平台的 AddKunData 设 plugin_）。
// 规则在构造时校验，不连库。
public sealed class KunStrictSchemaRuleTests
{
    [Theory]
    [InlineData("catalog")]
    [InlineData("plugin_catalog")]
    [InlineData("a")]
    public void KernelDefault_AcceptsLowercaseIdentifier(string schema)
    {
        Assert.Null(Accept(schema));
    }

    [Theory]
    [InlineData("Catalog")]
    [InlineData("1catalog")]
    [InlineData("_catalog")]
    [InlineData("catalog-x")]
    public void KernelDefault_RejectsIllegalIdentifier(string schema)
    {
        Assert.Throws<KunDataMisconfiguredException>(() => new StrictContext(Options(schema)));
    }

    [Fact]
    public void KernelDefault_AllowsAtMost63Characters()
    {
        Assert.Null(Accept("a" + new string('b', 62)));
        Assert.Throws<KunDataMisconfiguredException>(() => new StrictContext(Options("a" + new string('b', 63))));
    }

    [Fact]
    public void Prefix_RejectsSchemaWithoutPrefix()
    {
        Assert.Null(Accept("plugin_catalog", "plugin_"));
        var error = Assert.Throws<KunDataMisconfiguredException>(() => new StrictContext(Options("catalog", "plugin_")));
        Assert.Contains("plugin_", error.Message, StringComparison.Ordinal);
        Assert.Throws<KunDataMisconfiguredException>(() => new StrictContext(Options("plugin_Catalog", "plugin_")));
    }

    [Fact]
    public void Prefix_KeepsTotalLengthAtMost63Characters()
    {
        // plugin_ 时与原来写死的 ^plugin_[a-z][a-z0-9_]{0,55}$ 边界相同。
        Assert.Null(Accept("plugin_a" + new string('b', 55), "plugin_"));
        Assert.Throws<KunDataMisconfiguredException>(() => new StrictContext(Options("plugin_a" + new string('b', 56), "plugin_")));
    }

    [Fact]
    public void Prefix_CannotBeChangedOnceSet()
    {
        var builder = new DbContextOptionsBuilder().RequireKunSchemaPrefix("plugin_");
        builder.RequireKunSchemaPrefix("plugin_");
        Assert.Throws<KunDataMisconfiguredException>(() => builder.RequireKunSchemaPrefix("app_"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Plugin_")]
    [InlineData("1_")]
    [InlineData("plugin-")]
    public void Prefix_MustBeLowercaseIdentifier(string prefix)
    {
        Assert.Throws<ArgumentException>(() => new DbContextOptionsBuilder().RequireKunSchemaPrefix(prefix));
    }

    private static Exception? Accept(string schema, string? prefix = null) =>
        Record.Exception(() => new StrictContext(Options(schema, prefix)).Dispose());

    private static DbContextOptions Options(string schema, string? prefix = null)
    {
        var builder = StrictContextOptions.Builder(schema);
        if (prefix is not null) builder.RequireKunSchemaPrefix(prefix);
        return builder.Options;
    }

    private sealed class StrictContext(DbContextOptions options) : KunStrictDbContext(options, NullTenantContext.Instance);
}
