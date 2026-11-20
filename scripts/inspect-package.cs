#:property IsPackable=false
#:property PublishAot=false
// 仅读取 NuGet 制品中的程序集元数据。使用仓库选定的 .NET SDK 执行。
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

var packages = JsonSerializer.Deserialize<string[]>(Console.In.ReadToEnd())!;
var results = new List<object>();
foreach (var path in packages)
{
    using var package = ZipFile.OpenRead(path);
    foreach (var entry in package.Entries)
    {
        var name = entry.FullName.Replace('\\', '/');
        if (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            || !(name.StartsWith("lib/", StringComparison.OrdinalIgnoreCase)
                 || name.StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase)
                 || name.StartsWith("runtimes/", StringComparison.OrdinalIgnoreCase)
                    && name.Contains("/lib/", StringComparison.OrdinalIgnoreCase)))
            continue;
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
        results.Add(new
        {
            package = Path.GetFileName(path),
            entry = name,
            assembly = metadata.GetString(assembly.Name),
            version = assembly.Version.ToString(),
            friends,
            trimmable
        });
    }
}
Console.WriteLine(JsonSerializer.Serialize(results));
