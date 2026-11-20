using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Kun.Storage.Local;

/// <summary>为本地存储签发和执行限时上传、下载请求。键的用途由宿主指定。</summary>
public sealed class LocalSignedTransfers
{
    private readonly LocalFileSystemStorage _storage;
    private readonly byte[] _key;
    private readonly TimeProvider _clock;
    private readonly Func<string, bool> _acceptUploadKey;
    private readonly Func<string, bool> _acceptDownloadKey;

    /// <summary>使用指定存储、签名密钥、时钟及对象键策略。</summary>
    public LocalSignedTransfers(LocalFileSystemStorage storage, ReadOnlyMemory<byte> signingKey, TimeProvider clock,
        Func<string, bool> acceptUploadKey, Func<string, bool> acceptDownloadKey)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(acceptUploadKey);
        ArgumentNullException.ThrowIfNull(acceptDownloadKey);
        if (signingKey.Length < 32) throw new ArgumentException("签名密钥至少需要 32 字节。", nameof(signingKey));
        _storage = storage;
        _clock = clock;
        _key = signingKey.ToArray();
        _acceptUploadKey = acceptUploadKey;
        _acceptDownloadKey = acceptDownloadKey;
    }

    private const string SignatureParameter = "signature";
    private const string MaxBytesHeader = "X-Kun-Max-Bytes";

    /// <summary>签发限制类型与大小的单次写入地址。</summary>
    public ValueTask<PresignedUploadUrl> CreateUploadAsync(Uri endpoint, string objectKey, string contentType, long maxBytes, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateEndpoint(endpoint);
        ArgumentOutOfRangeException.ThrowIfNegative(maxBytes);
        if (!_acceptUploadKey(objectKey)) throw new ArgumentException("直传只允许写上传暂存键。", nameof(objectKey));
        var presigned = LocalFileSystemStorage.CreateUploadUrl(endpoint, objectKey, contentType, new UploadConstraints(maxBytes), _clock.GetUtcNow().Add(lifetime));
        var query = Query(presigned.Url);
        var signature = Sign("PUT", objectKey, query["expires"], query["contentType"], presigned.RequiredHeaders[MaxBytesHeader]);
        return ValueTask.FromResult(presigned with { Url = Append(presigned.Url, signature) });
    }

    // 恒定 ForceDownload：不论存储类型，下载地址都带 attachment，且没有任何开关能改。
    /// <summary>签发始终以附件形式下载的地址。</summary>
    public ValueTask<PresignedDownloadUrl> CreateDownloadAsync(Uri endpoint, string objectKey, string fileName, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateEndpoint(endpoint);
        if (!_acceptDownloadKey(objectKey)) throw new ArgumentException("暂存键不能签发下载。", nameof(objectKey));
        var presigned = _storage.CreateDownloadUrl(endpoint, objectKey, new DownloadOptions(ForceDownload: true, DownloadFileName: fileName), _clock.GetUtcNow().Add(lifetime));
        var query = Query(presigned.Url);
        if (!query.TryGetValue("disposition", out var disposition) || !disposition.StartsWith("attachment;", StringComparison.Ordinal))
            throw new InvalidOperationException("Kun 本地存储未返回 attachment 下载参数。");
        var signature = Sign("GET", objectKey, query["expires"], disposition);
        return ValueTask.FromResult(presigned with { Url = Append(presigned.Url, signature) });
    }

    /// <summary>校验签名、有效期、类型和长度，通过后原子创建对象。</summary>
    public async ValueTask<LocalTransferWriteResult> ReceiveAsync(LocalTransferRequest request, Stream content, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request); ArgumentNullException.ThrowIfNull(content);
        if (!_acceptUploadKey(request.ObjectKey) ||
            !request.Query.TryGetValue("expires", out var expires) || !request.Query.TryGetValue("contentType", out var contentType) ||
            !request.Headers.TryGetValue(MaxBytesHeader, out var maxBytesText) ||
            !Verify(request, Sign("PUT", request.ObjectKey, expires, contentType, maxBytesText)) || Expired(expires) ||
            !long.TryParse(maxBytesText, NumberStyles.None, CultureInfo.InvariantCulture, out var maxBytes) ||
            !request.Headers.TryGetValue("Content-Type", out var actualType) ||
            !string.Equals(actualType.Split(';', 2)[0].Trim(), contentType, StringComparison.OrdinalIgnoreCase))
            return LocalTransferWriteResult.Rejected;
        return await _storage.StoreNewAsync(request.ObjectKey, content, contentType, maxBytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>读取有效下载地址指向的对象及附件响应元数据。</summary>
    public async ValueTask<LocalTransferRead?> OpenAsync(LocalTransferRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        if (!_acceptDownloadKey(request.ObjectKey) ||
            !request.Query.TryGetValue("expires", out var expires) || !request.Query.TryGetValue("disposition", out var disposition) ||
            !disposition.StartsWith("attachment;", StringComparison.Ordinal) ||
            !Verify(request, Sign("GET", request.ObjectKey, expires, disposition)) || Expired(expires))
            return null;
        var metadata = await _storage.GetMetadataAsync(request.ObjectKey, cancellationToken).ConfigureAwait(false);
        if (metadata is null) return null;
        try
        {
            var stream = await _storage.OpenReadAsync(request.ObjectKey, cancellationToken).ConfigureAwait(false);
            return new LocalTransferRead(stream, metadata.Size, metadata.ContentType ?? "application/octet-stream", disposition);
        }
        catch (FileNotFoundException) { return null; }
    }

    private static void ValidateEndpoint(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri || endpoint.Scheme is not ("http" or "https") || endpoint.Query.Length != 0 ||
            endpoint.Fragment.Length != 0 || endpoint.UserInfo.Length != 0)
            throw new ArgumentException("需要不带查询或片段的 HTTP(S) 绝对地址。", nameof(endpoint));
    }

    private bool Expired(string expires) =>
        !long.TryParse(expires, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) || _clock.GetUtcNow().ToUnixTimeSeconds() > seconds;

    private static bool Verify(LocalTransferRequest request, string expected) =>
        request.Query.TryGetValue(SignatureParameter, out var actual) &&
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(actual), Encoding.ASCII.GetBytes(expected));

    // 签名覆盖方法、对象键与 Kun 生成的全部查询字段（上传另含 MaxBytes 请求头），字段以换行分隔，任一改动都使签名失效。
    private string Sign(params string[] parts)
    {
        var mac = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(string.Join('\n', parts)));
        return Convert.ToBase64String(mac).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static Dictionary<string, string> Query(Uri url)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            values.Add(Uri.UnescapeDataString(parts[0]), parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : "");
        }
        return values;
    }

    private static Uri Append(Uri url, string signature) =>
        new(url.AbsoluteUri + (url.Query.Length == 0 ? "?" : "&") + SignatureParameter + "=" + Uri.EscapeDataString(signature), UriKind.Absolute);
}
