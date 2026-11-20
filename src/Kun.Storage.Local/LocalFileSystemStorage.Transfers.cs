using System.Text;

namespace Kun.Storage.Local;

public sealed partial class LocalFileSystemStorage
{
    internal async ValueTask<LocalTransferWriteResult> StoreNewAsync(string objectKey, Stream content, string contentType, long maxBytes,
        CancellationToken cancellationToken)
    {
        ValidateKey(objectKey);
        var target = ResolvePath(objectKey);
        if (File.Exists(target)) return LocalTransferWriteResult.Conflict;
        if (content.CanSeek && content.Length > maxBytes) return LocalTransferWriteResult.TooLarge;

        var directory = Path.GetDirectoryName(target)!;
        Directory.CreateDirectory(directory);
        // The reserved sidecar suffix keeps work in progress out of object listings.
        var temporary = Path.Combine(directory, ".kun-" + Guid.NewGuid().ToString("N") + ".contenttype");
        var staged = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        try
        {
            long? size;
            await using (staged.ConfigureAwait(false))
                size = await CopyBoundedAsync(content, staged, maxBytes, cancellationToken).ConfigureAwait(false);
            if (!size.HasValue) return LocalTransferWriteResult.TooLarge;
            cancellationToken.ThrowIfCancellationRequested();
            var result = PublishNew(temporary, target, contentType);
            if (result == LocalTransferWriteResult.Stored) Log.Uploaded(_logger, objectKey, size.Value, contentType);
            return result;
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    private static bool IsSharingConflict(IOException error)
    {
        // Windows reports a sharing/lock violation; flock reports EWOULDBLOCK on Unix.
        var code = error.HResult & 0xffff;
        return OperatingSystem.IsWindows() ? code is 32 or 33 : code is 11 or 35;
    }

    private static LocalTransferWriteResult PublishNew(string temporary, string target, string contentType)
    {
        var metadataPath = target + ".contenttype";
        FileStream metadata;
        try { metadata = new FileStream(metadataPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None); }
        catch (IOException error) when (IsSharingConflict(error))
        {
            return LocalTransferWriteResult.Conflict;
        }
        using (metadata)
        {
            if (File.Exists(target)) return LocalTransferWriteResult.Conflict;
            // Publish only after the complete body and its content type have reached disk.
            metadata.SetLength(0);
            metadata.Write(Encoding.UTF8.GetBytes(contentType));
            metadata.Flush();
            try { File.Move(temporary, target, overwrite: false); }
            catch (IOException) when (File.Exists(target)) { return LocalTransferWriteResult.Conflict; }
            return LocalTransferWriteResult.Stored;
        }
    }
}
