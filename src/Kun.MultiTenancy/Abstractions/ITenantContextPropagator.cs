using System.Text.Json;

namespace Kun.MultiTenancy.Abstractions;

/// <summary>事件/作业 JSON 与 gRPC 文本元数据共用的租户传播端口；不替代传输层认证。</summary>
public interface ITenantContextPropagator
{
    /// <summary>从当前可信上下文写入 tenant_id；已有不同值、重复字段或无租户时拒绝。</summary>
    JsonElement Write(JsonElement carrier);
    /// <summary>从当前可信上下文生成只包含 tenant_id 的传输头。</summary>
    IReadOnlyDictionary<string, string> WriteHeaders();
    /// <summary>
    /// 消费已认证来源的 JSON；authorizedTenantId 必须来自令牌或受保护调度记录，不能取自同一输入。
    /// 例外是集成事件消费端（R122）：没有独立于消息的令牌，carrier 为信封原文，authorizedTenantId 为已与 AMQP 头核对一致的信封租户，
    /// 这里只做缺失、重复、不一致与租户冲突的 fail-closed 判定，不提供独立授权；信任根是 broker 的连接认证与发布端 Outbox 写入端。
    /// </summary>
    IDisposable BeginScope(JsonElement carrier, Guid authorizedTenantId);
    /// <summary>消费已认证来源的元数据；保留重复项以便拒绝重复 tenant_id。</summary>
    IDisposable BeginScope(IEnumerable<KeyValuePair<string, string>> headers, Guid authorizedTenantId);
}
