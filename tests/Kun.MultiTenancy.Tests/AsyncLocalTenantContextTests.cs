using FluentAssertions;
using Xunit;

namespace Kun.MultiTenancy.Tests;

public sealed class AsyncLocalTenantContextTests
{
    [Fact]
    public void EmptyGuidIsRejectedAndPreservesOuterScope()
    {
        var context = new AsyncLocalTenantContext();
        var tenant = Guid.NewGuid();
        using var outer = context.BeginScope(tenant);
        Assert.Throws<ArgumentException>(() => context.BeginScope(Guid.Empty));
        Assert.Equal(tenant, context.TenantId);
    }

    [Fact]
    public void BeginScope_SetsAndRestoresTenantId()
    {
        var ctx = new AsyncLocalTenantContext();
        ctx.TenantId.Should().BeNull();

        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        using (ctx.BeginScope(first))
        {
            ctx.TenantId.Should().Be(first);

            using (ctx.BeginScope(second))
            {
                ctx.TenantId.Should().Be(second);
            }

            ctx.TenantId.Should().Be(first);
        }

        ctx.TenantId.Should().BeNull();
    }

    [Fact]
    public async Task TwoTenants_DoNotLeak_AcrossConcurrentTasks()
    {
        var ctx = new AsyncLocalTenantContext();
        var observed = new System.Collections.Concurrent.ConcurrentBag<(Guid Tenant, Guid? Observed)>();

        async Task ObserveAsync(Guid tenant)
        {
            using (ctx.BeginScope(tenant))
            {
                // Yield to let other tasks run on the same logical thread pool.
                await Task.Yield();
                await Task.Delay(Random.Shared.Next(1, 20), TestContext.Current.CancellationToken);
                observed.Add((tenant, ctx.TenantId));
            }
        }

        var tasks = Enumerable.Range(0, 128)
            .Select(_ => ObserveAsync(Guid.NewGuid()))
            .ToArray();
        await Task.WhenAll(tasks);

        observed.Should().OnlyContain(entry => entry.Tenant == entry.Observed,
            "AsyncLocal isolates tenant scope across concurrent async flows");
    }
}
