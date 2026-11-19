using System.Xml.Linq;

namespace Qian.Runtime.Tests;

public sealed class KunReferenceTests
{
    [Fact]
    public void Runtime_depends_on_Kun_only_through_Errors_Core_and_Mediator_projects()
    {
        // Qian.Runtime 对 Kun 只用项目引用（S1-KUN-020），且只引用仓库 src/ 下的 Kun.Errors、Kun.Core 与 Kun.Mediator：
        // 资产文件里的 Kun.* 都是 project 类型，Kun.Validation 不在依赖闭包里。
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Qian.slnf"))) root = root.Parent;
        Assert.NotNull(root);
        var repository = Path.GetFullPath(Path.Combine(root.FullName, "..", ".."));
        var directory = Path.Combine(root.FullName, "packages/Qian.Runtime");
        var project = XDocument.Load(Path.Combine(directory, "Qian.Runtime.csproj"));
        Assert.Equal(["Microsoft.Extensions.DependencyInjection"],
            project.Descendants("PackageReference").Select(e => (string)e.Attribute("Include")!));
        Assert.Equal(["src/Kun.Core/Kun.Core.csproj", "src/Kun.Errors/Kun.Errors.csproj", "src/Kun.Mediator/Kun.Mediator.csproj"],
            project.Descendants("ProjectReference")
                .Select(e => Path.GetRelativePath(repository, Path.GetFullPath(Path.Combine(directory, (string)e.Attribute("Include")!))).Replace('\\', '/'))
                .Order(StringComparer.Ordinal));
        using var assets = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "obj/project.assets.json")));
        var kun = assets.RootElement.GetProperty("libraries").EnumerateObject().Where(p => p.Name.StartsWith("Kun.", StringComparison.Ordinal)).ToArray();
        Assert.Contains(kun, p => p.Name.StartsWith("Kun.Errors/", StringComparison.Ordinal));
        Assert.Contains(kun, p => p.Name.StartsWith("Kun.Core/", StringComparison.Ordinal));
        Assert.Contains(kun, p => p.Name.StartsWith("Kun.Mediator/", StringComparison.Ordinal));
        Assert.All(kun, library => Assert.Equal("project", library.Value.GetProperty("type").GetString()));
        Assert.DoesNotContain(kun, p => p.Name.StartsWith("Kun.Validation/", StringComparison.Ordinal));
    }
}
