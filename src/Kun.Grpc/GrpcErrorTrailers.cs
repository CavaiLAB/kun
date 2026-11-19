using System.Text;
using Grpc.Core;
using Kun.Errors;

namespace Kun.Grpc;

/// <summary>以 HTTP 错误契约的同一份详情生成 gRPC 响应尾部。</summary>
public static class GrpcErrorTrailers
{
    internal const string DetailKey = "qian-error-bin";

    /// <summary>返回机器可读错误码与 UTF-8 编码的 ErrorDetail JSON。</summary>
    public static Metadata Create(ErrorDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        return new Metadata
        {
            { "kun-error-code", detail.Code },
            { DetailKey, Encoding.UTF8.GetBytes(ErrorDetailJson.Serialize(detail)) }
        };
    }
}
