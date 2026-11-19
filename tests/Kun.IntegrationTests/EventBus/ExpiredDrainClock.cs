namespace Kun.IntegrationTests.EventBus;

internal sealed class ExpiredDrainClock : TimeProvider
{
    private long _elapsed;
    public override long TimestampFrequency => 1;
    public override long GetTimestamp() => Interlocked.Add(ref _elapsed, 31);
}
