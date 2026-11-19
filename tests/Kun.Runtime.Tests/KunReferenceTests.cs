using System.Xml.Linq;

namespace Kun.Runtime.Tests;

public sealed class KunReferenceTests
{
    [Fact]
    public void Runtime_depends_on_Kun_only_through_the_Errors_project()
    {
        // Kun.Runtime 对 Kun 只用项目引用，且只引用零依赖的 Kun.Errors：资产文件里的 Kun.* 只有它、是 project 类型，
        // Kun.Core、Kun.Mediator 与 Kun.Validation 都不在依赖闭包里。
        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Kun.slnx"))) repository = repository.Parent;
        Assert.NotNull(repository);
        var directory = Path.Combine(repository.FullName, "src", "Kun.Runtime");
        var project = XDocument.Load(Path.Combine(directory, "Kun.Runtime.csproj"));
        Assert.Equal(["src/Kun.Errors/Kun.Errors.csproj"],
            project.Descendants("ProjectReference")
                .Select(e => Path.GetRelativePath(repository.FullName, Path.GetFullPath(Path.Combine(directory, (string)e.Attribute("Include")!))).Replace('\\', '/')));
        Assert.All(project.Descendants("PackageReference"),
            reference => Assert.StartsWith("Microsoft.Extensions.", (string)reference.Attribute("Include")!, StringComparison.Ordinal));
        using var assets = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "obj/project.assets.json")));
        var kun = assets.RootElement.GetProperty("libraries").EnumerateObject().Where(p => p.Name.StartsWith("Kun.", StringComparison.Ordinal)).ToArray();
        Assert.Equal(["Kun.Errors"], kun.Select(p => p.Name.Split('/')[0]));
        Assert.All(kun, library => Assert.Equal("project", library.Value.GetProperty("type").GetString()));
    }
}
