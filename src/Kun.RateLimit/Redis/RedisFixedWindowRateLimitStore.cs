using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Kun.RateLimit;

/// <summary>Counts a hit and establishes the fixed window atomically in Redis. A preexisting counter without a TTL starts a fresh expiry window.</summary>
public sealed class RedisFixedWindowRateLimitStore : IRedisRateLimitStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisRateLimitStoreOptions _options;

    /// <summary>Creates a new store.</summary>
    public RedisFixedWindowRateLimitStore(
        IConnectionMultiplexer redis,
        IOptions<RedisRateLimitStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(redis);
        ArgumentNullException.ThrowIfNull(options);
        _redis = redis;
        _options = options.Value;
    }

    private const string Hit = """
        local count = redis.call('INCR', KEYS[1])
        local ttl = redis.call('PTTL', KEYS[1])
        if ttl < 0 then
            redis.call('PEXPIRE', KEYS[1], ARGV[1])
            ttl = tonumber(ARGV[1])
        end
        return {count, ttl}
        """;

    /// <inheritdoc />
    public async Task<RateLimitHitResult> HitAsync(string partitionKey, TimeSpan window, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();
        var result = (RedisResult[])(await _redis.GetDatabase().ScriptEvaluateAsync(Hit,
            [(RedisKey)(_options.KeyPrefix + ":" + partitionKey)], [(RedisValue)(long)Math.Ceiling(window.TotalMilliseconds)])
            .WaitAsync(cancellationToken).ConfigureAwait(false))!;
        return new((long)result[0], TimeSpan.FromMilliseconds((long)result[1]));
    }
}
