using Kun.Errors;

namespace Kun.EntityFrameworkCore;

/// <summary>在构造上下文或启动宿主时拒绝不安全的数据层配置。</summary>
public sealed class KunDataMisconfiguredException : InvalidOperationException
{
    /// <summary>
    /// 这个异常对应的稳定错误码（<c>DATA_MISCONFIGURED</c>，归属 <c>qian.data</c>）。
    /// 由异常类型自己公开，不依赖平台的错误码类；装配数据组件的宿主（<c>AddKunData</c>）把它登记进错误码目录。
    /// </summary>
    public static readonly ErrorCode Code = ErrorCode.Protocol(
        "DATA_MISCONFIGURED", 500, "qian.data",
        new Dictionary<string, string> { ["zh-CN"] = "数据组件装配错误" },
        new Dictionary<string, string> { ["zh-CN"] = "数据组件未满足安全装配要求：{reason}" });

    /// <summary>指定缺失或被覆盖的安全配置。</summary>
    public KunDataMisconfiguredException(string reason) : base($"数据组件未满足安全装配要求：{reason}")
    {
        Error = new Error(Code, new Dictionary<string, string> { ["reason"] = reason });
    }

    /// <summary>供宿主记录的统一错误详情。</summary>
    public Error Error { get; }
}
