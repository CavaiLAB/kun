namespace Kun.EventBus.Outbox;

/// <summary>
/// 替换 Kun 默认累加器的 Outbox 写入端（如事件组件的信封写入端）声明自己写入的缓冲器；
/// 数据会话只接受写入当前 scope 缓冲器的实现，保证事件随同一次保存落库。
/// </summary>
public interface IOutboxBufferBinding
{
    /// <summary>写入端追加消息的缓冲器，必须是当前 scope 的实例。</summary>
    IntegrationEventBuffer Buffer { get; }
}
