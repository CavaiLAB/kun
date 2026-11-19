using FluentAssertions;
using Kun.Core.MultiTenancy;
using Xunit;

namespace Kun.Core.Tests.MultiTenancy;

/// <summary>
/// Guard-clause coverage for <see cref="TenantInfo"/>. The record is a DTO crossing module
/// boundaries so the ArgumentException contract is load-bearing.
/// </summary>
public sealed class TenantInfoTests
{
    [Fact]
    public void Ctor_AcceptsNonEmptyTenantId()
    {
        var id = Guid.NewGuid();
        var info = new TenantInfo(id);

        info.TenantId.Should().Be(id);
        info.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Ctor_AcceptsOptionalDisplayName()
    {
        var info = new TenantInfo(Guid.NewGuid(), displayName: "Fabrikam Inc.");

        info.DisplayName.Should().Be("Fabrikam Inc.");
    }

    [Fact]
    public void Ctor_RejectsEmptyTenantId()
    {
        var act = () => new TenantInfo(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("tenantId");
    }

}
