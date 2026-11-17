using System.Text;
using Amazon.S3;
using FluentAssertions;
using Kun.IntegrationTests.Fixtures;
using Kun.Storage.S3;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.IntegrationTests.Storage;

/// <summary>
/// End-to-end integration test for <see cref="S3FileStorage"/> against an S3-compatible server
/// container (<see cref="S3CompatibleServer"/>; the class name predates the switch away from MinIO).
/// Covers the M1 invariant: all four canonical active web content types must round-trip
/// and their presigned download URL must carry <c>response-content-disposition=attachment</c>.
/// </summary>
public sealed class S3FileStorageMinioTests : IAsyncLifetime
{
    private S3CompatibleServer? _server;
    private AmazonS3Client? _s3;
    private S3FileStorage? _storage;
    private const string BucketName = "kun-storage-tests";

    public async ValueTask InitializeAsync()
    {
        try
        {
            _server = S3CompatibleServer.Create();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Docker", StringComparison.OrdinalIgnoreCase))
        {
            ContainerRuntime.ReportUnavailable("Docker is not available on this host — integration test requires Docker.");
            return;
        }

        // Image pull or startup failures propagate and fail the test; they are not "Docker unavailable".
        await _server.StartAsync(TestContext.Current.CancellationToken);

        var options = new S3StorageOptions
        {
            BucketName = BucketName,
            ServiceUrl = new Uri(_server.Endpoint, UriKind.Absolute),
            ForcePathStyle = true,
            AccessKey = S3CompatibleServer.AccessKey,
            SecretKey = S3CompatibleServer.SecretKey,
            Region = "us-east-1",
        };

        var cfg = new AmazonS3Config
        {
            ServiceURL = options.ServiceUrl.AbsoluteUri,
            ForcePathStyle = true,
            AuthenticationRegion = options.Region,
        };

        _s3 = new AmazonS3Client(
            new Amazon.Runtime.BasicAWSCredentials(options.AccessKey, options.SecretKey),
            cfg);

        await _s3.PutBucketAsync(
            new Amazon.S3.Model.PutBucketRequest { BucketName = BucketName },
            TestContext.Current.CancellationToken);

        _storage = new S3FileStorage(
            _s3,
            Options.Create(options),
            NullLogger<S3FileStorage>.Instance,
            TimeProvider.System);
    }

    public async ValueTask DisposeAsync()
    {
        _s3?.Dispose();
        if (_server is not null)
        {
            await _server.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("text/html", "<html><body>hi</body></html>")]
    [InlineData("image/svg+xml", "<svg xmlns='http://www.w3.org/2000/svg'/>")]
    [InlineData("application/xhtml+xml", "<html xmlns='http://www.w3.org/1999/xhtml'></html>")]
    [InlineData("application/xml", "<root/>")]
    public async Task RoundTrip_ActiveWebContentType_ForcesAttachmentDisposition(string contentType, string payload)
    {
        if (_storage is null)
        {
            ContainerRuntime.ReportUnavailable("Container was not started (Docker unavailable).");
            return;
        }

        var key = $"active/{Guid.NewGuid():N}.blob";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        var meta = await _storage.UploadAsync(
            key, stream, contentType,
            cancellationToken: TestContext.Current.CancellationToken);
        meta.Size.Should().Be(payload.Length);

        var presigned = await _storage.GetPresignedDownloadAsync(
            key, cancellationToken: TestContext.Current.CancellationToken);

        presigned.ContentDispositionForced.Should().BeTrue();
        presigned.ResponseHeaders.Should().ContainKey("Content-Disposition");

        // Presigned URL itself carries response-content-disposition=… as a signed query param
        // (AWS SDK maps ResponseHeaderOverrides.ContentDisposition to this parameter).
        presigned.Url.Query.Should().Contain("response-content-disposition", because:
            "S3 presigned URLs encode the Content-Disposition override as a query parameter.");

        (await _storage.DeleteAsync(key, TestContext.Current.CancellationToken)).Should().BeTrue();
    }
}
