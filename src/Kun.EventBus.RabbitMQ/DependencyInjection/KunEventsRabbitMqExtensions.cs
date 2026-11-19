using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kun.EventBus.RabbitMQ;

/// <summary>Registers the RabbitMQ envelope publisher and subscribers.</summary>
public static class KunEventsRabbitMqExtensions
{
    /// <summary>
    /// 登记 RabbitMQ 发布端。AMQP URI 在首次连接时经 <paramref name="resolveAmqpUri"/> 读取，值不进入组件清单；
    /// <paramref name="brokerReference"/> 是组件配置 <c>broker</c> 应匹配的 secretRef。
    /// </summary>
    public static IServiceCollection AddKunEventsBroker(this IServiceCollection services, Func<IServiceProvider, string> resolveAmqpUri, string? brokerReference = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(resolveAmqpUri);
        if (brokerReference is not null) EventsBrokerBinding.ValidateReference(brokerReference);
        if (services.Any(service => service.ServiceType == typeof(EventsBrokerBinding)))
            throw new KunEventsException(EventsErrors.Misconfigured, "同一宿主只能登记一个事件 broker。");
        services.AddKunEvents();
        services.AddSingleton(new EventsBrokerBinding(brokerReference));
        var address = new EventsBrokerAddress(resolveAmqpUri);
        services.AddSingleton(address);
        services.AddSingleton(sp => new RabbitMqBroker(() => address.Resolve(sp), sp.GetRequiredService<ILogger<RabbitMqBroker>>()));
        services.Replace(ServiceDescriptor.Singleton<IIntegrationEventPublisher>(sp => new KunEnvelopePublisher(sp.GetRequiredService<RabbitMqBroker>())));
        return services;
    }

    /// <summary>
    /// 登记消费端（S1-EVT-002）：<paramref name="service"/> 是订阅方服务名，<paramref name="subscribe"/> 里逐个 <c>Subscribe&lt;TEvent, THandler&gt;()</c>。
    /// 消费连接与发布端分开（ClientProvidedName 为 qian.events.subscriber），AMQP 地址复用 <see cref="AddKunEventsBroker"/> 登记的引用；
    /// 幂等记录默认写 Redis（宿主登记 IConnectionMultiplexer，或以 Kun 的 UseRedisIdempotency 登记 IProcessedMessageStore）。
    /// 可以多次调用以追加订阅，服务名必须相同；缺少 broker、幂等存储或事件登记时宿主拒绝启动。
    /// </summary>
    public static IServiceCollection AddKunEventsSubscriber(this IServiceCollection services, string service, Action<KunEventsSubscriberBuilder> subscribe,
        Action<KunEventsSubscriberOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(subscribe);
        try { _ = KunEventTopology.SubscriptionQueue(service, "s"); }
        catch (ArgumentException error) { throw new KunEventsException(EventsErrors.Misconfigured, "订阅方服务名不合法：" + error.Message); }
        services.AddKunEvents();
        var options = services.AddOptions<KunEventsSubscriberOptions>();
        if (configure is not null) options.Configure(configure);
        var registry = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(EventSubscriptionRegistry))?.ImplementationInstance as EventSubscriptionRegistry;
        if (registry is null)
        {
            registry = new EventSubscriptionRegistry(service);
            services.AddSingleton(registry);
            services.AddSingleton<IEventSubscriptionCatalog>(registry);
            services.TryAddScoped<EventDeliverySlot>();
            services.TryAddScoped(sp => sp.GetRequiredService<EventDeliverySlot>().Current
                ?? throw new KunEventsException(EventsErrors.Misconfigured, "KunEventDelivery 只能在订阅器派发的消息作用域内解析。"));
            services.TryAddSingleton<OpenConsumerConnection>((factory, token) => factory.CreateConnectionAsync(token));
            services.AddHostedService(sp => new KunEventSubscriber(sp, sp.GetRequiredService<EventSubscriptionRegistry>(),
                sp.GetRequiredService<IOptions<KunEventsSubscriberOptions>>(), sp.GetRequiredService<ILogger<KunEventSubscriber>>(),
                sp.GetRequiredService<TimeProvider>(), sp.GetRequiredService<OpenConsumerConnection>()));
        }
        else if (registry.Service != service)
            throw new KunEventsException(EventsErrors.Misconfigured, "同一宿主的订阅方服务名必须一致：已登记 " + registry.Service + "。");
        subscribe(new KunEventsSubscriberBuilder(services, registry));
        return services;
    }
}
/// <summary>宿主登记的 AMQP 地址解析；发布端与消费端在首次连接时读取，值不进入组件清单。</summary>
internal sealed class EventsBrokerAddress(Func<IServiceProvider, string> resolve)
{
    public string Resolve(IServiceProvider services) => resolve(services) is { Length: > 0 } uri ? uri
        : throw new KunEventsException(EventsErrors.Misconfigured, "broker 引用未解析出 AMQP 地址。");
}

internal delegate Task<global::RabbitMQ.Client.IConnection> OpenConsumerConnection(global::RabbitMQ.Client.ConnectionFactory factory, CancellationToken cancellationToken);
