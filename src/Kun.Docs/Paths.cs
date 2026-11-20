namespace Kun.Docs;

internal static class Paths
{
    internal static string Within(string root, string path, bool allowRoot = false)
    {
        root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path, root));
        var prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
        if (full == root ? !allowRoot : !full.StartsWith(prefix, StringComparison.Ordinal)) throw new IOException("路径超出目录。");
        var current = root;
        foreach (var segment in Path.GetRelativePath(root, full).Split(Path.DirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            if (Path.Exists(current) && File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint)) throw new IOException("路径不能经过符号链接。");
        }
        return full;
    }
}
