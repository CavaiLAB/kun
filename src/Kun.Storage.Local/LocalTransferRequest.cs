namespace Kun.Storage.Local;

/// <summary>一次直传 / 直下请求：对象键取自路径，查询参数与请求头原样传入，由网关校验签名。</summary>
public sealed record LocalTransferRequest(string ObjectKey, IReadOnlyDictionary<string, string> Query, IReadOnlyDictionary<string, string> Headers);

/// <summary>直传结果，宿主映射为 HTTP 状态码。</summary>
public enum LocalTransferWriteResult
{
    /// <summary>已写入（201）。</summary>
    Stored,
    /// <summary>签名不符、已过期或缺少必需参数（404，不区分原因）。</summary>
    Rejected,
    /// <summary>对象已存在，不覆盖（409）。</summary>
    Conflict,
    /// <summary>请求体超过签名里的上限（413）。</summary>
    TooLarge
}

/// <summary>直下内容：响应头取自签名参数与存储时的嗅探类型。</summary>
public sealed record LocalTransferRead(Stream Content, long Length, string ContentType, string ContentDisposition);
