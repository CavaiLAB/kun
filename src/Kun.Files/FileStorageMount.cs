using System.Security.Cryptography;
using System.Text.Json;
using Kun.Errors;
using Kun.Storage;
using Kun.Storage.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Kun.Files;

internal sealed class FileStorageMount(IFileStorage storage, LocalSignedTransfers transfers, Uri endpoint) : IFileTransferGateway
{
    internal const string Local = "local";
    internal const string UploadSuffix = ".upload";
    private static readonly Lazy<byte[]> DevelopmentKey = new(() => RandomNumberGenerator.GetBytes(32));

    internal IFileStorage Storage { get; } = storage;

    internal static FileStorageMount Create(string kind, JsonElement settings, TimeProvider clock, IServiceProvider services, ReadOnlyMemory<byte> signingKey)
    {
        if (kind != Local)
            throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Pointer = "#/storage/kind", Allowed = AllowedValues.Of([Local]) });
        if (settings.ValueKind != JsonValueKind.Object) throw Invalid("storage");
        string? root = null; Uri? endpoint = null;
        foreach (var property in settings.EnumerateObject())
        {
            switch (property.Name)
            {
                case "kind": break;
                case "root" when property.Value.ValueKind == JsonValueKind.String && Path.IsPathFullyQualified(property.Value.GetString()!):
                    root = Path.GetFullPath(property.Value.GetString()!); break;
                case "endpoint" when property.Value.ValueKind == JsonValueKind.String &&
                    Uri.TryCreate(property.Value.GetString(), UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" &&
                    uri.Query.Length == 0 && uri.Fragment.Length == 0 && uri.UserInfo.Length == 0:
                    endpoint = uri.AbsoluteUri.EndsWith('/') ? uri : new Uri(uri.AbsoluteUri + "/"); break;
                default: throw Invalid(property.Name);
            }
        }
        if (root is null) throw Invalid("root");
        if (endpoint is null) throw Invalid("endpoint");
        if (signingKey.IsEmpty)
        {
            if (services.GetService<IHostEnvironment>()?.IsDevelopment() != true)
                throw new InvalidOperationException("非开发环境须配置 KunFilesOptions.LocalSigningKey。");
            signingKey = DevelopmentKey.Value;
        }
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger<LocalFileSystemStorage>() ?? NullLogger<LocalFileSystemStorage>.Instance;
        var storage = new LocalFileSystemStorage(Options.Create(new LocalFileStorageOptions
        {
            RootPath = root,
            PublicBaseUrl = endpoint,
            DefaultPresignedUrlLifetime = FilesRuntime.UploadLifetime
        }), logger, clock);
        return new FileStorageMount(storage, new LocalSignedTransfers(storage, signingKey, clock,
            key => ValidKey(key) && key.EndsWith(UploadSuffix, StringComparison.Ordinal),
            key => ValidKey(key) && !key.EndsWith(UploadSuffix, StringComparison.Ordinal)), endpoint);
    }

    internal ValueTask<PresignedUploadUrl> CreateUploadAsync(string objectKey, string contentType, long maxBytes, TimeSpan lifetime, CancellationToken cancellationToken) =>
        transfers.CreateUploadAsync(endpoint, objectKey, contentType, maxBytes, lifetime, cancellationToken);

    internal ValueTask<PresignedDownloadUrl> CreateDownloadAsync(string objectKey, string fileName, TimeSpan lifetime, CancellationToken cancellationToken) =>
        transfers.CreateDownloadAsync(endpoint, objectKey, fileName, lifetime, cancellationToken);

    public async ValueTask<FileTransferResult> ReceiveAsync(FileTransferRequest request, Stream content, CancellationToken cancellationToken = default)
    {
        var result = await transfers.ReceiveAsync(new LocalTransferRequest(request.ObjectKey, request.Query, request.Headers), content, cancellationToken).ConfigureAwait(false);
        return result switch
        {
            LocalTransferWriteResult.Stored => FileTransferResult.Stored,
            LocalTransferWriteResult.Rejected => FileTransferResult.Rejected,
            LocalTransferWriteResult.Conflict => FileTransferResult.Conflict,
            LocalTransferWriteResult.TooLarge => FileTransferResult.TooLarge,
            _ => throw new InvalidOperationException("未识别的本地传输结果。")
        };
    }

    public async ValueTask<FileTransferDownload?> OpenAsync(FileTransferRequest request, CancellationToken cancellationToken = default)
    {
        var read = await transfers.OpenAsync(new LocalTransferRequest(request.ObjectKey, request.Query, request.Headers), cancellationToken).ConfigureAwait(false);
        return read is null ? null : new FileTransferDownload(read.Content, read.Length, read.ContentType, read.ContentDisposition);
    }

    private static bool ValidKey(string key) => key.StartsWith("tenants/", StringComparison.Ordinal) &&
        !key.Contains("..", StringComparison.Ordinal) && !key.Contains('\\', StringComparison.Ordinal);

    private static ErrorException Invalid(string member) =>
        new(new Error(PlatformErrors.InputInvalid) { Pointer = "#/storage/" + member });
}
