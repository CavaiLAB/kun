using System.Text.Json;

namespace Kun.Errors;

/// <summary>
/// 一次出错：哪个错误码、文案参数、出错位置与允许值。进程内由 <c>Kun.Core.Results.Result</c> 携带或随
/// <see cref="ErrorException"/> 抛出；对外由 <see cref="ErrorCatalog.CreateDetail"/> 渲染为 <see cref="ErrorDetail"/>。
/// </summary>
/// <remarks>
/// 字段错误有多个时放进 <see cref="Errors"/>，此时顶层 <see cref="Pointer"/> / <see cref="Allowed"/> 留空，
/// 由渲染取第一项；只有一个出错位置的业务错误直接设 <see cref="Pointer"/>。
/// </remarks>
public sealed class Error
{
    private static readonly IReadOnlyDictionary<string, string> s_noArgs = new Dictionary<string, string>(0);

    private readonly string? _pointer;
    private readonly IReadOnlyList<JsonElement>? _allowed;
    private readonly IReadOnlyList<FieldError> _errors = [];

    /// <summary>创建一次出错。</summary>
    /// <param name="code">协议或业务错误码；字段级错误码只能放进 <see cref="Errors"/>。</param>
    /// <param name="args">文案占位符参数，必须覆盖 <see cref="ErrorCode.Parameters"/>。</param>
    public Error(ErrorCode code, IReadOnlyDictionary<string, string>? args = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (code.Kind == ErrorKind.Field)
            throw new ArgumentException($"{code.Code} 是字段级错误码，只能用于 errors[]。", nameof(code));
        code.EnsureArguments(args);
        Code = code;
        Args = args is null ? s_noArgs : new Dictionary<string, string>(args, StringComparer.Ordinal);
    }

    /// <summary>错误码。</summary>
    public ErrorCode Code { get; }

    /// <summary>文案参数。</summary>
    public IReadOnlyDictionary<string, string> Args { get; }

    /// <summary>
    /// 按当前界面语言（<see cref="System.Globalization.CultureInfo.CurrentUICulture"/>）渲染的文案，找不到时回退 zh-CN。
    /// 供日志、gRPC 状态说明这类进程内用途；HTTP 响应体由错误码目录渲染（<see cref="ErrorCatalog.CreateDetail"/>）。
    /// </summary>
    public string Message => Code.FormatMessage(null, Args);

    /// <summary>出错的输入位置（见 <see cref="ErrorPointer"/>）。</summary>
    public string? Pointer
    {
        get => _pointer;
        init => _pointer = value is null || ErrorPointer.IsValid(value)
            ? value
            : throw new ArgumentException($"'{value}' 不是合法的 pointer（如 #/items/0/qty）。", nameof(Pointer));
    }

    /// <summary>该位置允许的取值（见 <see cref="AllowedValues"/>）。</summary>
    public IReadOnlyList<JsonElement>? Allowed
    {
        get => _allowed;
        init
        {
            AllowedValues.EnsureValid(value, nameof(Allowed));
            _allowed = value;
        }
    }

    /// <summary>逐项的字段错误，最多 100 项。</summary>
    public IReadOnlyList<FieldError> Errors
    {
        get => _errors;
        init
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Errors));
            if (value.Count > 100)
                throw new ArgumentException("errors 最多 100 项。", nameof(Errors));
            _errors = value;
        }
    }
}

/// <summary><c>errors[]</c> 中的一项：字段级或业务错误码 + 出错位置。</summary>
public sealed class FieldError
{
    private static readonly IReadOnlyDictionary<string, string> s_noArgs = new Dictionary<string, string>(0);

    private readonly IReadOnlyList<JsonElement>? _allowed;

    /// <summary>创建一项字段错误。</summary>
    /// <param name="code">字段级（<see cref="ErrorKind.Field"/>）或业务错误码。</param>
    /// <param name="pointer">出错位置（见 <see cref="ErrorPointer"/>）。</param>
    /// <param name="args">文案占位符参数。</param>
    public FieldError(ErrorCode code, string pointer, IReadOnlyDictionary<string, string>? args = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (code.Kind == ErrorKind.Protocol)
            throw new ArgumentException($"{code.Code} 是协议错误码，不能用于 errors[]。", nameof(code));
        if (!ErrorPointer.IsValid(pointer))
            throw new ArgumentException($"'{pointer}' 不是合法的 pointer（如 #/items/0/qty）。", nameof(pointer));
        code.EnsureArguments(args);
        Code = code;
        Pointer = pointer;
        Args = args is null ? s_noArgs : new Dictionary<string, string>(args, StringComparer.Ordinal);
    }

    /// <summary>错误码。</summary>
    public ErrorCode Code { get; }

    /// <summary>出错位置。</summary>
    public string Pointer { get; }

    /// <summary>文案参数。</summary>
    public IReadOnlyDictionary<string, string> Args { get; }

    /// <summary>该位置允许的取值。</summary>
    public IReadOnlyList<JsonElement>? Allowed
    {
        get => _allowed;
        init
        {
            AllowedValues.EnsureValid(value, nameof(Allowed));
            _allowed = value;
        }
    }
}

/// <summary>
/// 携带 <see cref="Error"/> 的异常，Kun 只有这一个带错误码的异常：业务代码直接抛出，或由
/// <c>Result&lt;T&gt;.EnsureSuccess()</c> 在失败时抛出；HTTP 宿主由 Kun.Errors.AspNetCore 写成 problem+json，
/// gRPC 宿主由 Kun.Grpc 的拦截器转成对应的状态码。
/// </summary>
public sealed class ErrorException : Exception
{
    /// <summary>以一次出错创建异常；异常消息为错误码与 zh-CN 文案，只用于日志。</summary>
    public ErrorException(Error error)
        : base(Describe(error))
    {
        Error = error;
    }

    /// <summary>以一次出错与内部异常创建异常。</summary>
    public ErrorException(Error error, Exception? innerException)
        : base(Describe(error), innerException)
    {
        Error = error;
    }

    /// <summary>出错内容。</summary>
    public Error Error { get; }

    private static string Describe(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return $"{error.Code.Code}: {error.Code.FormatMessage(System.Globalization.CultureInfo.InvariantCulture, error.Args)}";
    }
}
