using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Kun.IntegrationTests.Fixtures;

/// <summary>
/// 集中决定「没有容器环境」时测试怎么办。
/// </summary>
/// <remarks>
/// <para>
/// 只有 Testcontainers 在构建容器时报告找不到可用的容器环境（<see cref="ArgumentException"/>，
/// 消息里有 Docker），才算「没有容器环境」，调用方据此调用 <see cref="ReportUnavailable"/>：
/// 本机跳过；CI 里（环境变量 <c>CI</c> 为真，GitHub Actions 会设成 <c>true</c>）抛出异常、
/// 算作失败，因为 CI 的执行环境必须提供容器环境。
/// </para>
/// <para>
/// 拉不到镜像、容器起不来、等待就绪超时这类错误不是「没有容器环境」，不要经过这里，
/// 也不要捕获后跳过：让异常照常抛出，测试算作失败。
/// </para>
/// <para>
/// 在类夹具或集合夹具的 <c>InitializeAsync</c> 里调用时，xunit.v3 1.0.0 会把这次跳过报成
/// 「夹具初始化失败」，所以本机没有容器环境时，靠这类夹具起容器的测试显示为失败而不是跳过；
/// 测试类自己在 <c>InitializeAsync</c> 里起容器的，照常显示为跳过。
/// </para>
/// </remarks>
internal static class ContainerRuntime
{
    /// <summary>当前是否在 CI 里运行：环境变量 <c>CI</c> 有值，且不是 <c>false</c>、<c>0</c>、<c>no</c>。</summary>
    public static bool IsCi { get; } = IsTruthy(Environment.GetEnvironmentVariable("CI"));

    /// <summary>报告没有容器环境：本机跳过当前测试；CI 里抛出异常，测试失败。</summary>
    /// <param name="reason">跳过或失败的原因，原样写进测试结果。</param>
    /// <exception cref="InvalidOperationException">在 CI 里运行时总是抛出。</exception>
    [DoesNotReturn]
    public static void ReportUnavailable(string reason)
    {
        if (IsCi)
        {
            throw new InvalidOperationException(
                $"CI 里必须有可用的容器环境，集成测试不允许因此跳过（环境变量 CI 为真）：{reason}");
        }

        Assert.Skip(reason);
    }

    private static bool IsTruthy(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Trim().Equals("false", StringComparison.OrdinalIgnoreCase)
        && !value.Trim().Equals("0", StringComparison.Ordinal)
        && !value.Trim().Equals("no", StringComparison.OrdinalIgnoreCase);
}
