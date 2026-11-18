using System.Xml.Linq;
using Kun.Core.Domain;
using Kun.Core.Results;
using Kun.Errors;

namespace Qian.Runtime.Tests;

public sealed class KunErrorBridgeTests
{
    [Theory]
    [InlineData(ErrorType.Validation, "INPUT_INVALID", 400)]
    [InlineData(ErrorType.NotFound, "NOT_FOUND", 404)]
    [InlineData(ErrorType.Conflict, "CONFLICT", 409)]
    [InlineData(ErrorType.Unauthorized, "UNAUTHENTICATED", 401)]
    [InlineData(ErrorType.Forbidden, "AUTHZ_DENIED", 403)]
    [InlineData(ErrorType.Unavailable, "UNAVAILABLE", 503)]
    [InlineData(ErrorType.Failure, "INTERNAL", 500)]
    public void Maps_Kun_types_without_exposing_raw_code_or_message(ErrorType type, string code, int status)
    {
        var error = new Error("order.privateCode", "private message", type).ToQianError();
        Assert.Equal(code, error.Code.Code);
        Assert.Equal(status, error.Code.Status);
        Assert.Empty(error.Args);
        Assert.Empty(error.Errors);
        Assert.True(ErrorCatalog.CreateDefault().Contains(error.Code));
    }

    [Fact]
    public void Success_is_not_an_error_and_domain_failures_use_a_business_code()
    {
        Assert.Throws<ArgumentException>(() => Error.None.ToQianError());
        Assert.Same(PlatformErrors.BusinessRuleViolated, new SampleDomainException().ToQianError().Code);
    }

    [Fact]
    public void Runtime_depends_on_Kun_only_through_Core_and_Mediator_projects()
    {
        // 原 Qian 改为项目引用 Kun 源码（S1-KUN-020）：Qian.Runtime 对 Kun 只依赖仓库 src/ 下的 Kun.Core 与 Kun.Mediator，
        // 资产文件里的 Kun.* 全是 project 类型。不再按包引用 Kun，也就不再用 KunVersion 钉版（见 tests/gates/test_build_merge.py）。
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Qian.slnf"))) root = root.Parent;
        Assert.NotNull(root);
        var repository = Path.GetFullPath(Path.Combine(root.FullName, "..", ".."));
        var directory = Path.Combine(root.FullName, "packages/Qian.Runtime");
        var project = XDocument.Load(Path.Combine(directory, "Qian.Runtime.csproj"));
        Assert.Equal(["Microsoft.Extensions.DependencyInjection"],
            project.Descendants("PackageReference").Select(e => (string)e.Attribute("Include")!));
        Assert.Equal(["src/Kun.Errors/Kun.Errors.csproj", "src/Kun.Core/Kun.Core.csproj", "src/Kun.Mediator/Kun.Mediator.csproj"],
            project.Descendants("ProjectReference")
                .Select(e => Path.GetRelativePath(repository, Path.GetFullPath(Path.Combine(directory, (string)e.Attribute("Include")!))).Replace('\\', '/'))
                .Order(StringComparer.Ordinal));
        using var assets = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "obj/project.assets.json")));
        var kun = assets.RootElement.GetProperty("libraries").EnumerateObject().Where(p => p.Name.StartsWith("Kun.", StringComparison.Ordinal)).ToArray();
        Assert.Contains(kun, p => p.Name.StartsWith("Kun.Core/", StringComparison.Ordinal));
        Assert.Contains(kun, p => p.Name.StartsWith("Kun.Mediator/", StringComparison.Ordinal));
        Assert.All(kun, library => Assert.Equal("project", library.Value.GetProperty("type").GetString()));
        Assert.DoesNotContain(kun, p => p.Name.StartsWith("Kun.Validation/", StringComparison.Ordinal));
    }

    private sealed class SampleDomainException() : DomainException("private.domain", "private message");
}
