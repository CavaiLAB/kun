using System.Diagnostics.Metrics;

namespace Kun.EventBus.RabbitMQ;

internal static class EventsMetrics
{
    public const string MeterName = "Kun.EventBus.RabbitMQ";
    private static readonly Meter Meter = new(MeterName);
    public static readonly Counter<long> ConsumerProcessed = Meter.CreateCounter<long>("qian.events.consumer.processed", description: "handler 成功、已写幂等记录并确认的消息条数");
    public static readonly Counter<long> ConsumerRetried = Meter.CreateCounter<long>("qian.events.consumer.retried", description: "瞬时失败并转入重试队列退避的条数");
    public static readonly Counter<long> ConsumerDeadLettered = Meter.CreateCounter<long>("qian.events.consumer.dead_lettered", description: "重试用尽或 handler 确定性失败、已转入死信队列的条数");
    public static readonly Counter<long> ConsumerDuplicate = Meter.CreateCounter<long>("qian.events.consumer.duplicate", description: "幂等记录命中、未再执行 handler 的条数");
    public static readonly Counter<long> ConsumerRejected = Meter.CreateCounter<long>("qian.events.consumer.rejected", description: "信封、租户、类型或载荷不满足消费契约、直接拒绝的条数");
    public static readonly Counter<long> ConsumerForwardFailed = Meter.CreateCounter<long>("qian.events.consumer.forward_failed",
        description: "重试或死信副本连续转发失败、原消息 nack 不放回交 broker 按队列死信参数处理的条数（竞争队列转入死信队列，广播队列丢弃）");
    public static readonly Counter<long> ConsumerDuplicateExecuted = Meter.CreateCounter<long>("qian.events.consumer.duplicate_executed",
        description: "幂等记录已由同一事件的另一次投递先写入、handler 至少执行了两次的条数（并发重复投递）");
}
