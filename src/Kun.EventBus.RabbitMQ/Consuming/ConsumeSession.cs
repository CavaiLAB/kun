using RabbitMQ.Client;

namespace Kun.EventBus.RabbitMQ.Consuming;

/// <summary>
/// 一次消费会话的通道，以及确认与关通道之间的闸：关通道一旦开始，就不再在这条通道上确认或拒绝。
/// 否则确认帧可能落在关通道之后，RabbitMQ 在已经清掉的通道号上收到帧，按连接级错误
/// （504 CHANNEL_ERROR）关掉整条连接，同一连接上的发布通道一起断。
/// </summary>
internal sealed class ConsumeSession(IChannel channel)
{
    private const int Closing = 1 << 30;

    // 低位是正在写的确认帧个数，Closing 位表示通道已开始关闭；两者在同一个整数上原子地变。
    // 置了 Closing 之后不会再有确认登记进来，计数归零（状态恰为 Closing）时放行关通道。
    private int _state;
    private readonly TaskCompletionSource _drained = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IChannel Channel { get; } = channel;

    /// <summary>确认或拒绝一条投递；通道已开始关闭时什么都不发，返回 false。</summary>
    public async ValueTask<bool> TrySettleAsync(ulong deliveryTag, bool ack, bool requeue)
    {
        var state = Volatile.Read(ref _state);
        while (true)
        {
            if ((state & Closing) != 0)
            {
                return false;
            }

            var seen = Interlocked.CompareExchange(ref _state, state + 1, state);
            if (seen == state)
            {
                break;
            }

            state = seen;
        }

        try
        {
            if (ack)
            {
                await Channel.BasicAckAsync(deliveryTag, multiple: false, CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                await Channel.BasicNackAsync(deliveryTag, multiple: false, requeue, CancellationToken.None).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            if (Interlocked.Decrement(ref _state) == Closing)
            {
                _drained.TrySetResult();
            }
        }
    }

    /// <summary>先置「正在关」，等已经开始写的确认帧写完，再关通道。</summary>
    public async Task CloseAsync()
    {
        if (Interlocked.Or(ref _state, Closing) == 0)
        {
            _drained.TrySetResult();
        }

        // 只等确认帧写完，不等处理器：客户端关通道本身就要等正在执行的处理器返回，
        // 那些处理器之后的确认都会看到「正在关」而不发。
        await _drained.Task.ConfigureAwait(false);
        await Channel.CloseAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
