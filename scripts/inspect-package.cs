#:property IsPackable=false
#:property PublishAot=false
// 仅读取 NuGet 制品中的程序集元数据。使用仓库选定的 .NET SDK 执行。
// 读不出的包或程序集逐个输出带 error 的条目，由调用方记为核对问题，其余条目照常读取。
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

var packages = JsonSerializer.Deserialize<string[]>(Console.In.ReadToEnd())!;
var results = new List<object>();
foreach (var path in packages)
{
    var file = Path.GetFileName(path);
    try
    {
        using var package = ZipFile.OpenRead(path);
        foreach (var entry in package.Entries)
        {
            var name = entry.FullName.Replace('\\', '/');
            if (!IsAssembly(name))
                continue;
            try
            {
                results.Add(Inspect(file, name, entry));
            }
            catch (Exception error) when (IsReadError(error))
            {
                results.Add(new { package = file, entry = name, error = Describe(error) });
            }
        }
    }
    catch (Exception error) when (IsReadError(error))
    {
        results.Add(new { package = file, entry = "", error = Describe(error) });
    }
}
Console.WriteLine(JsonSerializer.Serialize(results));

static bool IsAssembly(string name) =>
    name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
    && (name.StartsWith("lib/", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("runtimes/", StringComparison.OrdinalIgnoreCase)
           && name.Contains("/lib/", StringComparison.OrdinalIgnoreCase));

// 文件不是合法的压缩包或 PE、没有托管元数据、属性数据不完整时抛出的几类异常。
static bool IsReadError(Exception error) =>
    error is BadImageFormatException or InvalidOperationException or InvalidDataException or IOException;

static string Describe(Exception error) => $"{error.GetType().Name}: {error.Message}";

static object Inspect(string file, string name, ZipArchiveEntry entry)
{
    using var bytes = new MemoryStream();
    using (var source = entry.Open())
        source.CopyTo(bytes);
    bytes.Position = 0;
    using var pe = new PEReader(bytes);
    var metadata = pe.GetMetadataReader();
    var assembly = metadata.GetAssemblyDefinition();
    var friends = new List<string>();
    var trimmable = false;
    foreach (var handle in assembly.GetCustomAttributes())
    {
        var attribute = metadata.GetCustomAttribute(handle);
        if (attribute.Constructor.Kind != HandleKind.MemberReference)
            continue;
        var parent = metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent;
        if (parent.Kind != HandleKind.TypeReference)
            continue;
        var type = metadata.GetTypeReference((TypeReferenceHandle)parent);
        var blob = metadata.GetBlobReader(attribute.Value);
        var ns = metadata.GetString(type.Namespace);
        var typeName = metadata.GetString(type.Name);
        if (ns == "System.Runtime.CompilerServices" && typeName == "InternalsVisibleToAttribute")
        {
            if (blob.ReadUInt16() != 1)
                throw new BadImageFormatException(name);
            friends.Add(blob.ReadSerializedString() ?? throw new BadImageFormatException(name));
        }
        if (ns == "System.Reflection" && typeName == "AssemblyMetadataAttribute")
        {
            if (blob.ReadUInt16() != 1)
                throw new BadImageFormatException(name);
            if (blob.ReadSerializedString() == "IsTrimmable"
                && string.Equals(blob.ReadSerializedString(), "True", StringComparison.OrdinalIgnoreCase))
                trimmable = true;
        }
    }
    return new
    {
        package = file,
        entry = name,
        assembly = metadata.GetString(assembly.Name),
        version = assembly.Version.ToString(),
        friends,
        trimmable
    };
}
