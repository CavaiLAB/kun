using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.Storage.Local.Tests;

public sealed class LocalSignedTransfersTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("kun-signed-upload-");
    private readonly ManualClock _clock = new(DateTimeOffset.UtcNow);
    private readonly byte[] _signingKey = RandomNumberGenerator.GetBytes(32);
    private readonly LocalFileSystemStorage _storage;
    private readonly LocalSignedTransfers _transfers;
    private readonly Uri _endpoint = new("https://files.example.test/objects/");

    public LocalSignedTransfersTests()
    {
        _storage = new LocalFileSystemStorage(Options.Create(new LocalFileStorageOptions { RootPath = _directory.FullName }),
            NullLogger<LocalFileSystemStorage>.Instance);
        _transfers = new LocalSignedTransfers(_storage, _signingKey, _clock, key => key.StartsWith("incoming/", StringComparison.Ordinal),
            key => key.StartsWith("final/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ConcurrentUploads_PublishOneCompleteObjectAndPreserveItsContent()
    {
        const int attempts = 4;
        const string key = "incoming/sample";
        var ct = TestContext.Current.CancellationToken;
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 1, TimeSpan.FromMinutes(1), ct);
        var request = Request(key, upload.Url, upload.RequiredHeaders);
        var allStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var count = 0;
        async Task UploadReady()
        {
            if (Interlocked.Increment(ref count) == attempts) allStarted.TrySetResult();
            await allStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        }
        async Task<LocalTransferWriteResult> Write(byte index)
        {
            using var content = new CoordinatedStream([index], UploadReady);
            return await _transfers.ReceiveAsync(request, content, ct);
        }

        var results = await Task.WhenAll(Enumerable.Range(0, attempts).Select(i => Write((byte)i)));
        var winner = Assert.Single(Enumerable.Range(0, attempts), i => results[i] == LocalTransferWriteResult.Stored);
        Assert.Equal(attempts - 1, results.Count(r => r == LocalTransferWriteResult.Conflict));
        Assert.Equal([(byte)winner], await File.ReadAllBytesAsync(Path.Combine(_directory.FullName, key), ct));
        var metadata = await _storage.GetMetadataAsync(key, ct);
        Assert.Equal((1L, "text/plain"), (metadata!.Size, metadata.ContentType));
        Assert.Empty(_directory.EnumerateFiles(".kun-*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task InterruptedOrOversizedBody_LeavesTheAddressAvailableForRetry()
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "incoming/retry";
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 2, TimeSpan.FromMinutes(1), ct);
        Assert.Equal("https://files.example.test/objects/incoming/retry", upload.Url.GetLeftPart(UriPartial.Path));
        var request = Request(key, upload.Url, upload.RequiredHeaders);
        using var interrupted = new CoordinatedStream([1], () => throw new IOException("Input stream closed."));
        await Assert.ThrowsAsync<IOException>(async () => await _transfers.ReceiveAsync(request, interrupted, ct));
        using var oversized = new MemoryStream([1, 2, 3]);
        Assert.Equal(LocalTransferWriteResult.TooLarge, await _transfers.ReceiveAsync(request, oversized, ct));
        Assert.False(await _storage.ExistsAsync(key, ct));
        Assert.Empty(_directory.EnumerateFiles("*", SearchOption.AllDirectories));
        using var normal = new MemoryStream([1, 2]);
        Assert.Equal(LocalTransferWriteResult.Stored, await _transfers.ReceiveAsync(request, normal, ct));
    }

    [Fact]
    public async Task UploadWithAlteredSignatureOrSignedField_IsRejectedAndWritesNothing()
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "incoming/signed";
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        var request = Request(key, upload.Url, upload.RequiredHeaders);
        var foreign = await new LocalSignedTransfers(_storage, RandomNumberGenerator.GetBytes(32), _clock, _ => true, _ => true)
            .CreateUploadAsync(_endpoint, key, "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        var later = (_clock.GetUtcNow().ToUnixTimeSeconds() + 3600).ToString(CultureInfo.InvariantCulture);
        LocalTransferRequest[] altered =
        [
            request with { Query = With(request.Query, "signature", Flip(request.Query["signature"])) },
            request with { Query = Without(request.Query, "signature") },
            request with { ObjectKey = "incoming/other" },
            request with { Query = With(request.Query, "expires", later) },
            request with { Query = With(request.Query, "contentType", "text/html"), Headers = With(request.Headers, "Content-Type", "text/html") },
            request with { Headers = With(request.Headers, "X-Kun-Max-Bytes", "4096") },
            Request(key, foreign.Url, foreign.RequiredHeaders),
        ];
        foreach (var candidate in altered)
        {
            using var body = new MemoryStream("abc"u8.ToArray());
            Assert.Equal(LocalTransferWriteResult.Rejected, await _transfers.ReceiveAsync(candidate, body, ct));
        }
        Assert.Empty(_directory.EnumerateFiles("*", SearchOption.AllDirectories));
        using var original = new MemoryStream("abc"u8.ToArray());
        Assert.Equal(LocalTransferWriteResult.Stored, await _transfers.ReceiveAsync(request, original, ct));
    }

    [Fact]
    public async Task DownloadWithAlteredSignatureOrSignedField_IsNotOpened()
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "final/report.txt";
        await Store(key, ct);
        await Store("final/other.txt", ct);
        var download = await _transfers.CreateDownloadAsync(_endpoint, key, "report.txt", TimeSpan.FromMinutes(1), ct);
        var request = Request(key, download.Url, new Dictionary<string, string>());
        Assert.StartsWith("attachment;", request.Query["disposition"], StringComparison.Ordinal);
        var later = (_clock.GetUtcNow().ToUnixTimeSeconds() + 3600).ToString(CultureInfo.InvariantCulture);
        LocalTransferRequest[] altered =
        [
            request with { Query = With(request.Query, "signature", Flip(request.Query["signature"])) },
            request with { Query = Without(request.Query, "signature") },
            request with { ObjectKey = "final/other.txt" },
            request with { Query = With(request.Query, "expires", later) },
            request with { Query = With(request.Query, "disposition", "attachment; filename=\"other.html\"") },
        ];
        foreach (var candidate in altered) Assert.Null(await _transfers.OpenAsync(candidate, ct));
        var opened = await _transfers.OpenAsync(request, ct);
        Assert.NotNull(opened);
        await using (opened.Content) Assert.Equal((3L, "text/plain", request.Query["disposition"]), (opened.Length, opened.ContentType, opened.ContentDisposition));
    }

    [Fact]
    public async Task ExpiredUploadAndDownloadAddresses_AreRefused()
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "incoming/late";
        await Store("final/report.txt", ct);
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        var download = await _transfers.CreateDownloadAsync(_endpoint, "final/report.txt", "report.txt", TimeSpan.FromMinutes(1), ct);
        var downloadRequest = Request("final/report.txt", download.Url, new Dictionary<string, string>());
        var opened = await _transfers.OpenAsync(downloadRequest, ct);
        Assert.NotNull(opened);
        await opened.Content.DisposeAsync();

        _clock.Advance(TimeSpan.FromSeconds(61));
        Assert.Null(await _transfers.OpenAsync(downloadRequest, ct));
        using (var body = new MemoryStream("abc"u8.ToArray()))
            Assert.Equal(LocalTransferWriteResult.Rejected, await _transfers.ReceiveAsync(Request(key, upload.Url, upload.RequiredHeaders), body, ct));
        Assert.False(await _storage.ExistsAsync(key, ct));
        var renewed = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        using (var body = new MemoryStream("abc"u8.ToArray()))
            Assert.Equal(LocalTransferWriteResult.Stored, await _transfers.ReceiveAsync(Request(key, renewed.Url, renewed.RequiredHeaders), body, ct));
    }

    [Theory]
    [InlineData("text/html", false)]
    [InlineData(null, false)]
    [InlineData("TEXT/PLAIN; charset=utf-8", true)]
    public async Task UploadContentType_MustMatchTheSignedType(string? sent, bool accepted)
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "incoming/typed";
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        var headers = sent is null ? Without(upload.RequiredHeaders, "Content-Type") : With(upload.RequiredHeaders, "Content-Type", sent);
        using var body = new MemoryStream("abc"u8.ToArray());
        Assert.Equal(accepted ? LocalTransferWriteResult.Stored : LocalTransferWriteResult.Rejected,
            await _transfers.ReceiveAsync(Request(key, upload.Url, headers), body, ct));
        Assert.Equal(accepted, await _storage.ExistsAsync(key, ct));
    }

    [Fact]
    public async Task StreamedBodyOverTheSignedLimit_IsTooLargeAndLeavesNothing()
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "incoming/limit";
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 4, TimeSpan.FromMinutes(1), ct);
        var request = Request(key, upload.Url, upload.RequiredHeaders);
        using (var oversized = new UnseekableStream([1, 2, 3, 4, 5]))
            Assert.Equal(LocalTransferWriteResult.TooLarge, await _transfers.ReceiveAsync(request, oversized, ct));
        Assert.Empty(_directory.EnumerateFiles("*", SearchOption.AllDirectories));
        using (var exact = new UnseekableStream([1, 2, 3, 4]))
            Assert.Equal(LocalTransferWriteResult.Stored, await _transfers.ReceiveAsync(request, exact, ct));
        byte[] expected = [1, 2, 3, 4];
        Assert.Equal(expected, await File.ReadAllBytesAsync(Path.Combine(_directory.FullName, key), ct));
    }

    [Fact]
    public async Task SecondWriteToAStoredAddress_IsAConflictAndKeepsTheFirstObject()
    {
        var ct = TestContext.Current.CancellationToken;
        const string key = "incoming/once";
        var upload = await _transfers.CreateUploadAsync(_endpoint, key, "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        var request = Request(key, upload.Url, upload.RequiredHeaders);
        using (var first = new MemoryStream("one"u8.ToArray()))
            Assert.Equal(LocalTransferWriteResult.Stored, await _transfers.ReceiveAsync(request, first, ct));
        using (var second = new MemoryStream("two"u8.ToArray()))
            Assert.Equal(LocalTransferWriteResult.Conflict, await _transfers.ReceiveAsync(request, second, ct));
        Assert.Equal("one"u8.ToArray(), await File.ReadAllBytesAsync(Path.Combine(_directory.FullName, key), ct));
        Assert.Empty(_directory.EnumerateFiles(".kun-*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task KeysOutsideTheHostPolicy_AreNeitherSignedNorServed()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentException>(async () => await _transfers.CreateUploadAsync(_endpoint, "final/direct", "text/plain", 3, TimeSpan.FromMinutes(1), ct));
        await Assert.ThrowsAsync<ArgumentException>(async () => await _transfers.CreateDownloadAsync(_endpoint, "incoming/staged", "staged.txt", TimeSpan.FromMinutes(1), ct));

        // 同一密钥、不限键的实例签出的地址有效，本实例仍按自己的键策略拒绝。
        var permissive = new LocalSignedTransfers(_storage, _signingKey, _clock, _ => true, _ => true);
        var upload = await permissive.CreateUploadAsync(_endpoint, "final/direct", "text/plain", 3, TimeSpan.FromMinutes(1), ct);
        var uploadRequest = Request("final/direct", upload.Url, upload.RequiredHeaders);
        using (var body = new MemoryStream("abc"u8.ToArray()))
            Assert.Equal(LocalTransferWriteResult.Rejected, await _transfers.ReceiveAsync(uploadRequest, body, ct));
        Assert.False(await _storage.ExistsAsync("final/direct", ct));
        using (var body = new MemoryStream("abc"u8.ToArray()))
            Assert.Equal(LocalTransferWriteResult.Stored, await permissive.ReceiveAsync(uploadRequest, body, ct));

        await Store("incoming/staged", ct);
        var download = await permissive.CreateDownloadAsync(_endpoint, "incoming/staged", "staged.txt", TimeSpan.FromMinutes(1), ct);
        var downloadRequest = Request("incoming/staged", download.Url, new Dictionary<string, string>());
        Assert.Null(await _transfers.OpenAsync(downloadRequest, ct));
        var opened = await permissive.OpenAsync(downloadRequest, ct);
        Assert.NotNull(opened);
        await opened.Content.DisposeAsync();
    }

    private static LocalTransferRequest Request(string key, Uri url, IReadOnlyDictionary<string, string> headers)
    {
        var parameters = url.Query[1..].Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => WebUtility.UrlDecode(p[0]), p => WebUtility.UrlDecode(p[1]));
        return new LocalTransferRequest(key, parameters, headers);
    }

    private async Task Store(string key, CancellationToken cancellationToken)
    {
        using var content = new MemoryStream("abc"u8.ToArray());
        await _storage.UploadAsync(key, content, "text/plain", cancellationToken: cancellationToken);
    }

    private static Dictionary<string, string> With(IReadOnlyDictionary<string, string> source, string name, string value) =>
        new(source, StringComparer.OrdinalIgnoreCase) { [name] = value };

    private static Dictionary<string, string> Without(IReadOnlyDictionary<string, string> source, string name)
    {
        var copy = new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);
        copy.Remove(name);
        return copy;
    }

    // 换掉首字符，得到格式合法但不同的签名。
    private static string Flip(string signature) => (signature[0] == 'A' ? "B" : "A") + signature[1..];

    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class UnseekableStream(byte[] body) : MemoryStream(body)
    {
        public override bool CanSeek => false;
    }

    private sealed class CoordinatedStream(byte[] body, Func<Task> start) : MemoryStream(body)
    {
        private bool _started;
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                _started = true;
                await start();
            }
            return await base.ReadAsync(buffer, cancellationToken);
        }
    }

    public void Dispose() => _directory.Delete(recursive: true);
}
