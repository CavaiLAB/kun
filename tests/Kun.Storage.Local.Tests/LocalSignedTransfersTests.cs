using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.Storage.Local.Tests;

public sealed class LocalSignedTransfersTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("kun-signed-upload-");
    private readonly LocalFileSystemStorage _storage;
    private readonly LocalSignedTransfers _transfers;
    private readonly Uri _endpoint = new("https://files.example.test/objects/");

    public LocalSignedTransfersTests()
    {
        _storage = new LocalFileSystemStorage(Options.Create(new LocalFileStorageOptions { RootPath = _directory.FullName }),
            NullLogger<LocalFileSystemStorage>.Instance);
        _transfers = new LocalSignedTransfers(_storage, RandomNumberGenerator.GetBytes(32), TimeProvider.System, key => key.StartsWith("incoming/", StringComparison.Ordinal),
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

    private static LocalTransferRequest Request(string key, Uri url, IReadOnlyDictionary<string, string> headers)
    {
        var parameters = url.Query[1..].Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => WebUtility.UrlDecode(p[0]), p => WebUtility.UrlDecode(p[1]));
        return new LocalTransferRequest(key, parameters, headers);
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
