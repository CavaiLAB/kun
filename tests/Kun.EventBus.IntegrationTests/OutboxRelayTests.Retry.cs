using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Globalization;
using Kun.EventBus.Outbox;
using Npgsql;

namespace Kun.EventBus.IntegrationTests;

// 发布持续失败：达到 MaxAttempts 标记失败后不再领取；重试间隔按 2 的幂增长、封顶一分钟，到点之前不领取。
public sealed partial class OutboxRelayTests
{
    [Fact]
    public async Task 发布持续失败达到MaxAttempts后标记失败_不再领取且failed计数加一()
    {
        var schema = await PrepareAsync();
        await SeedAsync(schema, 1);
        var failing = new FailingPublisher();
        using var metrics = new RelayMetrics("give-up");
        using (var replica = Replica(schema, "give-up", new Counter(TimeSpan.Zero), batch: 10, publisher: failing,
            configure: options => { options.MaxAttempts = 3; options.RetryBaseDelay = TimeSpan.FromMilliseconds(100); }))
        {
            await replica.StartAsync(Ct);
            await Until(async () => await ScalarAsync($"SELECT count(*) FROM {schema}.outbox_messages WHERE status = 2") == 1, "达到 MaxAttempts 后没有标记失败");
            // 标记失败的行不再领取：再过十多轮轮询，发布次数不变。
            await Task.Delay(TimeSpan.FromMilliseconds(750), Ct);
            await replica.StopAsync(Ct);
        }
        Assert.Equal(3, failing.Attempts);
        Assert.Equal(3, await ScalarAsync($"SELECT attempt_count FROM {schema}.outbox_messages"));
        Assert.Equal(1, await ScalarAsync($"SELECT count(*) FROM {schema}.outbox_messages WHERE next_attempt_at IS NULL AND last_error LIKE 'TimeoutException:%'"));
        Assert.Equal((2L, 1L, 0L), (metrics.Count("retried"), metrics.Count("failed"), metrics.Count("confirmed")));
    }

    [Fact]
    public async Task 发布连续失败时下次领取按重试间隔翻倍且封顶一分钟_到点前不领取()
    {
        var schema = await PrepareAsync();
        await SeedAsync(schema, 1);
        var failing = new FailingPublisher();
        using var replica = Replica(schema, "backoff", new Counter(TimeSpan.Zero), batch: 10, publisher: failing,
            configure: options => { options.MaxAttempts = 10; options.RetryBaseDelay = TimeSpan.FromSeconds(5); });
        await replica.StartAsync(Ct);
        // 第 n 次失败后，下次领取排在 5 秒 × 2^(n-1) 之后，封顶 60 秒：5、10、20、40、60。
        // 每次核对排定的时刻，确认到点之前没有再领取，再把时刻拨到现在，让下一次失败马上发生。
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            await Until(async () => await ScalarAsync($"SELECT attempt_count FROM {schema}.outbox_messages") == attempt, $"第 {attempt} 次失败没有记录");
            var expected = Math.Min(5 * Math.Pow(2, attempt - 1), 60);
            var remaining = await SecondsAsync($"SELECT EXTRACT(EPOCH FROM next_attempt_at - now()) FROM {schema}.outbox_messages");
            Assert.InRange(remaining, expected - 3, expected);
            await Task.Delay(TimeSpan.FromMilliseconds(500), Ct);
            Assert.Equal(attempt, failing.Attempts);
            if (attempt < 5) await AdminAsync($"UPDATE {schema}.outbox_messages SET next_attempt_at = now();");
        }
        await replica.StopAsync(Ct);
        Assert.Equal(0, await ScalarAsync($"SELECT status FROM {schema}.outbox_messages"));
    }

    private async Task<double> SecondsAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(postgres.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        return Convert.ToDouble(await command.ExecuteScalarAsync(Ct), CultureInfo.InvariantCulture);
    }

    // 每次发布都以超时失败，属于可重试的失败（不是信封错误）。
    private sealed class FailingPublisher : IIntegrationEventPublisher
    {
        private int _attempts;
        public int Attempts => Volatile.Read(ref _attempts);

        public ValueTask PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _attempts);
            return ValueTask.FromException(new TimeoutException("broker 没有在时限内确认。"));
        }
    }

    // 按副本标识统计投递结果：confirmed、retried、failed。
    private sealed class RelayMetrics : IDisposable
    {
        private readonly ConcurrentDictionary<string, long> _counts = new();
        private readonly MeterListener _listener = new();

        public RelayMetrics(string relay)
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "Kun.EventBus.EntityFrameworkCore" && instrument.Name.StartsWith("qian.events.outbox.", StringComparison.Ordinal)) listener.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            {
                foreach (var tag in tags)
                    if (tag.Key == "qian.events.relay" && Equals(tag.Value, relay))
                        _counts.AddOrUpdate(instrument.Name["qian.events.outbox.".Length..], value, (_, current) => current + value);
            });
            _listener.Start();
        }

        public long Count(string outcome) => _counts.GetValueOrDefault(outcome);

        public void Dispose() => _listener.Dispose();
    }
}
