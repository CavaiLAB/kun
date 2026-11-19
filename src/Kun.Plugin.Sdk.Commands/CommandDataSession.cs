using Kun.EntityFrameworkCore;

namespace Kun.Plugin.Sdk;

/// <summary>实际 Mediator scope 内共享的业务数据会话；只能由 SDK 事务行为绑定。</summary>
public sealed class CommandDataSession<TContext> where TContext : KunStrictDbContext
{
    private IDataSession<TContext>? _session;
    private bool _readOnly;
    /// <summary>创建尚未绑定的会话端口。</summary>
    public CommandDataSession() { }
    /// <summary>与外层事务一致的真实业务上下文。</summary>
    public TContext Context => Current.Context;
    /// <summary>同一 Data 会话的 Outbox 缓冲。</summary>
    public Kun.EventBus.Outbox.IOutboxWriter Outbox => _readOnly ? throw new InvalidOperationException("查询不能写入集成事件。") : Current.Outbox;
    internal IDataSession<TContext> Current => _session ?? throw new InvalidOperationException("能力数据会话尚未绑定或已经结束。");
    internal void Bind(IDataSession<TContext> session, bool readOnly)
    {
        if (_session is not null) throw new InvalidOperationException("业务数据会话不能重复绑定。");
        _session = session; _readOnly = readOnly;
    }
    internal void Clear() => _session = null;
}
