using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Kun.Identity.Implementations;

public sealed partial class KunOidcClient
{
    private const int MaxResponseBytes = 1 << 20;
    private readonly SemaphoreSlim _keysGate = new(1, 1);
    private KeySnapshot? _keys;

    internal static IReadOnlyList<string> SigningAlgorithms { get; } = Array.AsReadOnly<string>(
        [
            SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha512,
            SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512,
            SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512
        ]);

    internal static bool AreValidAlgorithms(IReadOnlyList<string>? values) =>
        values is { Count: > 0 } && values.All(SigningAlgorithms.Contains) &&
        values.Distinct(StringComparer.Ordinal).Count() == values.Count;

    private sealed record KeySnapshot(string Uri, IReadOnlyList<SecurityKey> Values, DateTimeOffset LoadedAt);

    private async Task<KeySnapshot> KeysAsync(string uri, bool refresh, CancellationToken ct)
    {
        bool IsFresh(KeySnapshot? snapshot) => snapshot is not null && snapshot.Uri == uri &&
            _clock.GetUtcNow() - snapshot.LoadedAt < (refresh ? TimeSpan.FromSeconds(30) : TimeSpan.FromHours(1));

        var snapshot = Volatile.Read(ref _keys);
        if (IsFresh(snapshot)) return snapshot!;
        await _keysGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (IsFresh(_keys)) return _keys!;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var document = await ReadResponseAsync(request, ct).ConfigureAwait(false);
            var set = new JsonWebKeySet(document.RootElement.GetRawText()) { SkipUnresolvedJsonWebKeys = true };
            for (var i = set.Keys.Count - 1; i >= 0; i--)
            {
                var key = set.Keys[i];
                if (key.Kty is not ("RSA" or "EC") || key.HasPrivateKey ||
                    key.Use is not (null or "" or "sig") || (key.KeyOps.Count > 0 && !key.KeyOps.Contains("verify")))
                    set.Keys.RemoveAt(i);
            }
            var values = set.GetSigningKeys().Where(key => key is RsaSecurityKey or ECDsaSecurityKey or X509SecurityKey).ToArray();
            if (values.Length == 0) throw new SecurityTokenValidationException("OIDC provider has no usable public signing key.");
            var loaded = new KeySnapshot(uri, values, _clock.GetUtcNow());
            Volatile.Write(ref _keys, loaded);
            return loaded;
        }
        finally { _keysGate.Release(); }
    }

    private async Task<JsonDocument> ReadResponseAsync(HttpRequestMessage request, CancellationToken ct)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var client = _clientFactory();
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException("OIDC provider did not return HTTP 200.", null, response.StatusCode);
        if (response.Content.Headers.ContentLength is > MaxResponseBytes)
            throw new InvalidOperationException("OIDC response exceeds the maximum size.");
        await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await stream.ReadAsync(chunk, deadline.Token).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > MaxResponseBytes) throw new InvalidOperationException("OIDC response exceeds the maximum size.");
            buffer.Write(chunk, 0, read);
        }
        buffer.Position = 0;
        return JsonDocument.Parse(buffer, new JsonDocumentOptions { MaxDepth = 32 });
    }
}

internal sealed class OidcCorrelationException(string message) : SecurityTokenValidationException(message);
