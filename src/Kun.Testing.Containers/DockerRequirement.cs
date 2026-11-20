using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Kun.Testing.Containers;

/// <summary>
/// 没有 Docker 时集成测试怎么办：本机跳过；CI 上直接失败，因为 PR 级门禁要求集成测试必须真跑（PRD §14.4 C10）。
/// </summary>
public static class DockerRequirement
{
    /// <summary>设为 <c>1</c> 或 <c>true</c> 时，即使不在 CI 上也要求 Docker（缺失即失败而不是跳过）。</summary>
    public const string RequireVariable = "QIAN_REQUIRE_DOCKER";

    /// <summary>CI 环境或明确设置 <see cref="RequireVariable"/> 时要求 Docker；CI 的空值、0、false 或 no 表示本机。</summary>
    /// <param name="getEnvironmentVariable">读取环境变量的函数，默认 <see cref="Environment.GetEnvironmentVariable(string)"/>；单测可替换。</param>
    public static bool IsRequired(Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;
        var ci = getEnvironmentVariable("CI")?.Trim();
        var inCi = !string.IsNullOrEmpty(ci) && ci != "0" &&
            !ci.Equals("false", StringComparison.OrdinalIgnoreCase) && !ci.Equals("no", StringComparison.OrdinalIgnoreCase);
        return inCi || IsTrue(getEnvironmentVariable(RequireVariable));
    }

    /// <summary>告知 xUnit 此环境缺少 Docker；要求 Docker 的环境抛出异常。</summary>
    [DoesNotReturn]
    public static void ReportUnavailable(string message)
    {
        if (IsRequired()) throw new InvalidOperationException(message);
        Assert.Skip(message);
    }

    private static bool IsTrue(string? value) =>
        value is not null && (value.Trim() == "1" || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase));
}
