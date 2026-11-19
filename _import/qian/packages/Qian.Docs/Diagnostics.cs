using Qian.Docs.Abstractions;
using Kun.Errors;
using Qian.Surfaces.Abstractions;

namespace Qian.Docs;

internal sealed class Diagnostics
{
    private readonly ErrorCatalog _catalog = CreateCatalog();
    private readonly string _trace = Guid.NewGuid().ToString("N");
    internal List<SurfaceDiagnostic> Items { get; } = [];
    private static ErrorCatalog CreateCatalog()
    {
        var catalog = ErrorCatalog.CreateDefault();
        foreach (var code in DocumentErrors.All) catalog.Register(code);
        return catalog;
    }
    internal void Add(string file, int line, ErrorCode code, string reason, bool warning = false)
    {
        var pointer = string.Join('/', file.Replace('\\', '/').Split('/').Select(Uri.EscapeDataString)) + "#";
        var error = new Error(PlatformErrors.InputInvalid)
        {
            Errors = [new FieldError(code, pointer, new Dictionary<string, string> { ["reason"] = reason })]
        };
        Items.Add(new SurfaceDiagnostic(file, line, warning ? "warning" : "error", _catalog.CreateDetail(error, _trace)));
    }
}
