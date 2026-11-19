using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Qian.Identity.Server.Tests;

/// <summary>
/// 测试宿主的语句闸门：身份数据库上经 EF 发出的命令里，第一条命中条件的命令在执行之前（或执行完之后）停住，直到测试放行。
/// 用来把两个并发步骤确定地排成指定的先后（例如在「查到绑定」与「写入连接器票据」之间插入另一笔写入），不靠睡眠或碰运气。
/// 执行完之后停住时，语句的结果已经定下（查询读的是执行时的快照），调用方还没有拿到它。
/// 缺省不拦任何命令；一次布置只停一条。原生 SQL（IdentitySql）不经 EF，不受它影响。
/// </summary>
internal sealed class StatementGate : DbCommandInterceptor
{
    private Hold? _hold;

    /// <summary>停在命令执行之前，还是执行完之后。</summary>
    internal enum Moment
    {
        BeforeExecution,
        AfterExecution
    }

    internal sealed class Hold(Func<DbCommand, bool> match, Moment moment)
    {
        private readonly TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal Func<DbCommand, bool> Match => match;
        internal Moment Moment => moment;
        /// <summary>命中的命令已停在闸门上。</summary>
        internal Task Reached => _reached.Task;
        internal Task Released => _released.Task;
        internal void Arrive() => _reached.TrySetResult();
        /// <summary>放行停住的命令。</summary>
        internal void Release() => _released.TrySetResult();
    }

    /// <summary>布置一次拦截：此后第一条满足条件的命令在 <paramref name="moment"/> 停住（缺省是执行之前）。</summary>
    internal Hold Arm(Func<DbCommand, bool> match, Moment moment = Moment.BeforeExecution)
    {
        var hold = new Hold(match, moment);
        Volatile.Write(ref _hold, hold);
        return hold;
    }

    /// <summary>连接器票据的写入：INSERT 进 login_tickets、用途为 connector。</summary>
    internal static bool ConnectorTicketInsert(DbCommand command) =>
        command.CommandText.Contains("INSERT INTO", StringComparison.OrdinalIgnoreCase) && command.CommandText.Contains("login_tickets", StringComparison.Ordinal) &&
        command.Parameters.Cast<DbParameter>().Any(parameter => parameter.Value is string value && value == IdentityAccountService.ConnectorPurpose);

    /// <summary>连接器回调查找已绑定的外部身份：external_identities 联查 persons，绑定与 Person 的身份修订在这一条语句里读出。</summary>
    internal static bool BindingLookup(DbCommand command) =>
        command.CommandText.Contains("external_identities", StringComparison.Ordinal) && command.CommandText.Contains(" JOIN " + IdentitySql.Persons + " ", StringComparison.Ordinal);

    /// <summary>建立会话的写入：INSERT 进 sessions（与登录审计、登录事件同一次保存）。</summary>
    internal static bool SessionInsert(DbCommand command) =>
        command.CommandText.Contains("INSERT INTO " + IdentitySql.Sessions + " ", StringComparison.Ordinal);

    private async ValueTask PauseAsync(DbCommand command, Moment moment, CancellationToken ct)
    {
        if (Volatile.Read(ref _hold) is not { } hold || hold.Moment != moment || !hold.Match(command)) return;
        // 只停第一条：抢到闸门的命令清掉布置，其余照常执行。
        if (Interlocked.CompareExchange(ref _hold, null, hold) != hold) return;
        hold.Arrive();
        await hold.Released.WaitAsync(ct).ConfigureAwait(false);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        await PauseAsync(command, Moment.BeforeExecution, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await PauseAsync(command, Moment.BeforeExecution, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        await PauseAsync(command, Moment.AfterExecution, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await PauseAsync(command, Moment.AfterExecution, cancellationToken).ConfigureAwait(false);
        return result;
    }
}
