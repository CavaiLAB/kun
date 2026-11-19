using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Kun.Core.MultiTenancy;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.Outbox;
using Kun.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.EventBus.IntegrationTests;

public sealed class EnvelopeContractTests
{
    private const string Tenant = "0190f3a2-7c41-7b3e-9d2a-5c8e1f4a6b20";

    [Fact]
    public void 契约正例经信封类型往返后仍通过Schema_反例与语义反例全部被拒()
    {
        foreach (var (name, data) in EnvelopeSchemas.Fixtures("valid"))
        {
            var envelope = KunEventEnvelope.Parse(data.GetRawText());
            using var written = JsonDocument.Parse(envelope.ToJson());
            Assert.True(EnvelopeSchemas.ValidateQian(written.RootElement) is [], name + "：" + string.Join("\n", EnvelopeSchemas.ValidateQian(written.RootElement)));
            Assert.Equal(data.GetProperty("event_id").GetString(), written.RootElement.GetProperty("event_id").GetString());
            Assert.True(JsonElement.DeepEquals(data.GetProperty("payload"), written.RootElement.GetProperty("payload")), name);
        }
        foreach (var kind in new[] { "invalid", "semantic-invalid" })
            foreach (var (name, data) in EnvelopeSchemas.Fixtures(kind))
            {
                var error = Assert.Throws<KunEventsException>(() => KunEventEnvelope.Parse(data.GetRawText()));
                Assert.Equal(EventsErrors.PublishRejected, error.Error.Code);
                _ = name;
            }
    }

    [Fact]
    public void 写入端生成的租户信封同时通过Qian与iClaw的EventEnvelope校验()
    {
        using var provider = Services();
        using var scope = provider.CreateScope();
        var tenant = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        var origin = new EventOrigin("qian.sample.crm", EventActor.Service("crm-writer")) { CausationId = Guid.CreateVersion7(), CorrelationId = Guid.CreateVersion7() };
        using (tenant.BeginScope(Guid.ParseExact(Tenant, "D")))
        using (EventEnvelopeScope.Begin(origin))
            scope.ServiceProvider.GetRequiredService<IOutboxWriter>().Enqueue(new PingSent(7, "hello"));
        var message = Assert.Single(scope.ServiceProvider.GetRequiredService<IntegrationEventBuffer>().GetPendingMessages());
        using var document = JsonDocument.Parse(message.Payload);
        var root = document.RootElement;
        Assert.Empty(EnvelopeSchemas.ValidateQian(root));
        Assert.Empty(EnvelopeSchemas.ValidateIClaw(root));
        var envelope = KunEventEnvelope.Parse(message.Payload);
        Assert.Equal(message.Id, envelope.EventId);
        Assert.Equal(7, envelope.EventId.Version);
        Assert.Equal("tests.ping.sent.v1", message.EventTypeId);
        Assert.Equal("tests.ping.sent", envelope.Subject);
        Assert.Equal(Tenant, message.TenantId);
        Assert.Equal(Tenant, root.GetProperty("tenant_id").GetString());
        Assert.Equal("qian.sample.crm", envelope.Producer);
        Assert.Equal(origin.CausationId, envelope.CausationId);
        Assert.Equal(origin.CorrelationId, envelope.CorrelationId);
        Assert.Equal(envelope.TraceParent, message.TraceParent);
        Assert.Equal(7, root.GetProperty("payload").GetProperty("sequence").GetInt32());
        Assert.Equal("service", root.GetProperty("actor").GetProperty("kind").GetString());
    }

    [Fact]
    public void 平台级事件的tenant_id显式为null_只与iClaw已登记的不兼容项冲突()
    {
        using var provider = Services();
        using var scope = provider.CreateScope();
        using (EventEnvelopeScope.Begin(new EventOrigin("qian.lifecycle", EventActor.Service("qian-platform"))))
            scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>().Publish(new PingSent(1, "platform"));
        var message = Assert.Single(scope.ServiceProvider.GetRequiredService<IntegrationEventBuffer>().GetPendingMessages());
        using var document = JsonDocument.Parse(message.Payload);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("tenant_id").ValueKind);
        Assert.Null(message.TenantId);
        Assert.Empty(EnvelopeSchemas.ValidateQian(document.RootElement));
        // iclaw-alignment.md 第 3 节：平台级事件没有租户，是两边已登记的不兼容项；只把 tenant_id 换成 UUID 后整封通过 iClaw 校验。
        Assert.Contains(EnvelopeSchemas.ValidateIClaw(document.RootElement), error => error.StartsWith("/tenant_id", StringComparison.Ordinal));
        var patched = JsonNode.Parse(message.Payload)!.AsObject();
        patched["tenant_id"] = Tenant;
        Assert.Empty(EnvelopeSchemas.ValidateIClaw(JsonSerializer.SerializeToElement(patched)));
    }

    [Fact]
    public void actor_kind支持agent与委托人()
    {
        using var provider = Services();
        using var scope = provider.CreateScope();
        var tenant = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        var person = "0190f3a2-7c41-7b3e-9d2a-5c8e1f4a6b21";
        using (tenant.BeginScope(Guid.ParseExact(Tenant, "D")))
        using (EventEnvelopeScope.Begin(new EventOrigin("qian.sample.crm", EventActor.Agent("agent-client-1", person))))
            scope.ServiceProvider.GetRequiredService<IOutboxWriter>().Enqueue(new PingSent(2, "agent"));
        var message = Assert.Single(scope.ServiceProvider.GetRequiredService<IntegrationEventBuffer>().GetPendingMessages());
        using var document = JsonDocument.Parse(message.Payload);
        var actor = document.RootElement.GetProperty("actor");
        Assert.Equal("agent", actor.GetProperty("kind").GetString());
        Assert.Equal("agent-client-1", actor.GetProperty("id").GetString());
        Assert.Equal(person, actor.GetProperty("on_behalf_of").GetString());
        Assert.Empty(EnvelopeSchemas.ValidateQian(document.RootElement));
        Assert.Equal(EventActorKind.Agent, KunEventEnvelope.Parse(message.Payload).Actor.Kind);
    }

    [Fact]
    public void 不在事件作用域_未登记或无JsonTypeInfo时拒绝发布且不写缓冲()
    {
        using var provider = Services();
        using var scope = provider.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();
        var buffer = scope.ServiceProvider.GetRequiredService<IntegrationEventBuffer>();
        Assert.Equal(EventsErrors.PublishRejected, Assert.Throws<KunEventsException>(() => writer.Enqueue(new PingSent(1, "no scope"))).Error.Code);
        using (EventEnvelopeScope.Begin(new EventOrigin("qian.sample.crm", EventActor.User("person-1"))))
        {
            Assert.Equal(EventsErrors.PublishRejected, Assert.Throws<KunEventsException>(() => writer.Enqueue(new Unregistered(1))).Error.Code);
            Assert.Equal(EventsErrors.PublishRejected, Assert.Throws<KunEventsException>(() => writer.Enqueue(new Reflected(1))).Error.Code);
        }
        Assert.Empty(buffer.GetPendingMessages());
    }

    [Theory]
    [InlineData("tenant-a")]
    [InlineData("0190F3A2-7C41-7B3E-9D2A-5C8E1F4A6B20")]
    public void 信封拒绝非规范租户文本(string tenant)
    {
        var (_, fixture) = EnvelopeSchemas.Fixtures("valid").First(item =>
            item.Item2.GetProperty("tenant_id").ValueKind == JsonValueKind.String);
        var envelope = JsonNode.Parse(fixture.GetRawText())!.AsObject();
        envelope["tenant_id"] = tenant;
        Assert.Equal(EventsErrors.PublishRejected, Assert.Throws<KunEventsException>(
            () => KunEventEnvelope.Parse(envelope.ToJsonString())).Error.Code);
    }

    [Fact]
    public void 事件来源校验字段并按打开的逆序恢复外层()
    {
        Assert.Null(EventEnvelopeScope.Current);
        Assert.Throws<KunEventsException>(() => EventEnvelopeScope.Begin(new EventOrigin("Qian.Sample", EventActor.User("p"))));
        Assert.Throws<KunEventsException>(() => EventEnvelopeScope.Begin(new EventOrigin("qian.sample", EventActor.User(""))));
        Assert.Throws<KunEventsException>(() => EventEnvelopeScope.Begin(new EventOrigin("qian.sample", EventActor.User("p")) { TraceParent = "00-abc" }));
        Assert.Throws<KunEventsException>(() => EventEnvelopeScope.Begin(new EventOrigin("qian.sample", EventActor.User("p")) { CausationId = Guid.Empty }));
        var outer = new EventOrigin("qian.outer", EventActor.Service("outer"));
        var inner = new EventOrigin("qian.inner", EventActor.Service("inner"));
        using (EventEnvelopeScope.Begin(outer))
        {
            var nested = EventEnvelopeScope.Begin(inner);
            Assert.Same(inner, EventEnvelopeScope.Current);
            nested.Dispose();
            nested.Dispose();
            Assert.Same(outer, EventEnvelopeScope.Current);
        }
        Assert.Null(EventEnvelopeScope.Current);
    }

    [Fact]
    public void 拓扑命名与路由键()
    {
        Assert.Equal("qian.events", KunEventTopology.Exchange);
        Assert.Equal("qian.dlx", KunEventTopology.DeadLetterExchange);
        Assert.Equal("crm.customer.created", KunEventTopology.RoutingKey("crm.customer.created.v1"));
        Assert.Equal("identity.session_revoked", KunEventTopology.RoutingKey("identity.session_revoked.v1"));
        Assert.Equal("q.qian.sample.crm.customer-sync", KunEventTopology.SubscriptionQueue("qian.sample.crm", "customer-sync"));
        Assert.Equal("q.qian.gateway.gw-7f9c", KunEventTopology.BroadcastQueue("qian.gateway", "gw-7f9c"));
        Assert.Throws<ArgumentException>(() => KunEventTopology.SubscriptionQueue("Qian.Crm", "sync"));
        Assert.Throws<ArgumentException>(() => KunEventTopology.SubscriptionQueue("qian.crm", "sync.all"));
        Assert.Throws<ArgumentException>(() => KunEventTopology.BroadcastQueue("qian.crm", new string('a', 300)));
        foreach (var type in new[] { "crm.v1", "crm.customer.created", "crm.customer.created.v0", "CRM.customer.created.v1", "crm..created.v1" })
        {
            Assert.False(IntegrationEventTypes.TryParse(type, out _, out _), type);
            Assert.Throws<KunEventsException>(() => KunEventTopology.RoutingKey(type));
        }
        Assert.True(IntegrationEventTypes.TryParse("lifecycle.tenant-plugin.upgraded.v12", out var subject, out var version));
        Assert.Equal(("lifecycle.tenant-plugin.upgraded", 12), (subject, version));
    }

    private static ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext());
        services.AddKunEventBus(events => events
            .Register(PingSent.Type, TestEventJson.Default.PingSent)
            .Register<Reflected>("tests.reflected.created.v1"));
        services.AddKunEvents();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}

[IntegrationEventId(Type)]
internal sealed record PingSent(int Sequence, string Note)
{
    public const string Type = "tests.ping.sent.v1";
}
internal sealed record Unregistered(int Id);
internal sealed record Reflected(int Id);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PingSent))]
internal sealed partial class TestEventJson : JsonSerializerContext;
