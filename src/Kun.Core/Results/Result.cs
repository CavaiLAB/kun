using System.Text;
using Kun.Errors;

namespace Kun.Core.Results;

/// <summary>
/// 无返回值的 Result。Handler 在不需要返回数据时使用。失败时携带 <see cref="Kun.Errors.Error"/>：
/// 一个登记在错误码目录里的错误码，加文案参数与出错位置。
/// </summary>
public readonly record struct Result
{
    private readonly Error? _error;

    /// <summary><see langword="true"/> when the operation succeeded.</summary>
    public bool IsSuccess { get; }
    /// <summary><see langword="true"/> when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 失败结果携带的错误。仅在 <see cref="IsFailure"/> 为 <see langword="true"/> 时可访问，
    /// 成功结果调用此属性会抛异常——应先用 <see cref="IsFailure"/> 判断。
    /// </summary>
    public Error Error => IsFailure
        ? _error ?? throw new InvalidOperationException("Default(Result) carries no error; create results with Ok() or Fail().")
        : throw new InvalidOperationException("Cannot access Error of a success result.");

    private Result(bool success, Error? error)
    {
        IsSuccess = success;
        _error = error;
    }

    /// <summary>Creates a successful result.</summary>
    public static Result Ok() => new(true, null);

    /// <summary>Creates a failure result carrying <paramref name="error"/>.</summary>
    public static Result Fail(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, error);
    }

    /// <summary>Implicit conversion from <see cref="Kun.Errors.Error"/> to a failure result.</summary>
    public static implicit operator Result(Error error) => Fail(error);

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("IsSuccess = ").Append(IsSuccess);
        if (_error is not null)
            builder.Append(", Error = ").Append(_error.Code.Code);
        return true;
    }
}

/// <summary>
/// 带返回值的 Result。Handler 成功时携带 <typeparamref name="T"/>，失败时携带 <see cref="Kun.Errors.Error"/>。
/// </summary>
public readonly record struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    /// <summary><see langword="true"/> when the operation succeeded.</summary>
    public bool IsSuccess { get; }
    /// <summary><see langword="true"/> when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 失败结果携带的错误。仅在 <see cref="IsFailure"/> 为 <see langword="true"/> 时可访问，
    /// 成功结果调用此属性会抛异常——应先用 <see cref="IsFailure"/> 判断。
    /// </summary>
    public Error Error => IsFailure
        ? _error ?? throw new InvalidOperationException("Default(Result<T>) carries no error; create results with Ok() or Fail().")
        : throw new InvalidOperationException("Cannot access Error of a success result.");

    /// <summary>
    /// 仅在 <see cref="IsSuccess"/> 为 <see langword="true"/> 时可访问。
    /// 失败 Result 调用此属性会抛异常——应先用 <see cref="IsSuccess"/> 判断。
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failure result.");

    private Result(bool success, T? value, Error? error)
    {
        IsSuccess = success;
        _value = value;
        _error = error;
    }

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    public static Result<T> Ok(T value) => new(true, value, null);

    /// <summary>Creates a failure result carrying <paramref name="error"/>.</summary>
    public static Result<T> Fail(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, default, error);
    }

    /// <summary>Implicit conversion from <typeparamref name="T"/> to a success result.</summary>
    public static implicit operator Result<T>(T value) => Ok(value);
    /// <summary>Implicit conversion from <see cref="Kun.Errors.Error"/> to a failure result.</summary>
    public static implicit operator Result<T>(Error error) => Fail(error);

    /// <summary>
    /// 在 Handler 直接返回给 gRPC 或 HTTP 端点时使用：失败时抛出携带同一 <see cref="Kun.Errors.Error"/> 的
    /// <see cref="ErrorException"/>，由宿主的错误映射（Kun.Grpc 的拦截器、Kun.Errors.AspNetCore）转成响应。
    /// </summary>
    public T EnsureSuccess() => IsSuccess
        ? _value!
        : throw new ErrorException(Error);

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("IsSuccess = ").Append(IsSuccess);
        if (IsSuccess)
            builder.Append(", Value = ").Append(_value);
        else if (_error is not null)
            builder.Append(", Error = ").Append(_error.Code.Code);
        return true;
    }
}
