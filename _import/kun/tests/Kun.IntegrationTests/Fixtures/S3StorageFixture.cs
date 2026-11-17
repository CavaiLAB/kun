using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Xunit;

namespace Kun.IntegrationTests.Fixtures;

/// <summary>
/// Per-class fixture that starts an S3-compatible server container (<see cref="S3CompatibleServer"/>),
/// provisions a test bucket, and exposes an <see cref="IAmazonS3"/> factory configured for the container.
/// Skips only when this host has no container runtime; in CI that fails instead (see <see cref="ContainerRuntime"/>).
/// </summary>
public sealed class S3StorageFixture : IAsyncLifetime
{
    private S3CompatibleServer? _server;

    /// <summary>HTTP endpoint to reach the running S3-compatible server.</summary>
    public string Endpoint { get; private set; } = string.Empty;

    /// <summary>Access key (server root user).</summary>
    public static string AccessKey => S3CompatibleServer.AccessKey;

    /// <summary>Secret key (server root password).</summary>
    public static string SecretKey => S3CompatibleServer.SecretKey;

    /// <summary>Bucket provisioned for the fixture lifetime.</summary>
    public string BucketName { get; } = $"kun-test-{Guid.NewGuid():N}";

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        try
        {
            _server = S3CompatibleServer.Create();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Docker", StringComparison.OrdinalIgnoreCase))
        {
            ContainerRuntime.ReportUnavailable("Docker is not available on this host — S3 storage integration test requires Docker (runs in CI).");
            return;
        }

        await _server.StartAsync(TestContext.Current.CancellationToken);
        Endpoint = _server.Endpoint;

        using var s3 = CreateClient();
        try
        {
            await s3.PutBucketAsync(
                new PutBucketRequest { BucketName = BucketName },
                TestContext.Current.CancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Bucket already exists — benign.
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_server is not null)
        {
            await _server.DisposeAsync();
        }
    }

    /// <summary>Builds a new <see cref="IAmazonS3"/> client aimed at the running S3-compatible container.</summary>
    /// <remarks>
    /// The container listens on plain HTTP. Setting <c>UseHttp = true</c> tells
    /// the AWS SDK that the target endpoint supports HTTP, so presigned URLs are emitted with
    /// the <c>http://</c> scheme (the SDK default is <c>https://</c>, which would make any
    /// downstream HttpClient attempt a TLS handshake against a plain-text server and fail
    /// with "Cannot determine the frame size" on the wire).
    /// </remarks>
    public IAmazonS3 CreateClient() =>
        new AmazonS3Client(
            new BasicAWSCredentials(AccessKey, SecretKey),
            new AmazonS3Config
            {
                ServiceURL = Endpoint,
                ForcePathStyle = true,
                UseHttp = true,
            });
}
