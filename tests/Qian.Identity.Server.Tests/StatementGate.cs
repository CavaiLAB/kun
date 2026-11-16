using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Qian.Identity.Server.Tests;

/// <summary>
/// 测试宿主的语句闸门：身份数据库上经 EF 发出的命令里，第一条命中条件的命令在执行前停住，直到测试放行。
/// 用来把两个并发步骤确定地排成指定的先后（例如在「查到绑定」与「写入连接器票据」之间插入另一笔写入），不靠睡眠或碰运气。
/// 缺省不拦任何命令；一次布置只停一条。原生 SQL（IdentitySql）不经 EF，不受它影响。
/// </summary>
internal sealed class StatementGate : DbCommandInterceptor
{
    private Hold? _hold;

    internal sealed class Hold(Func<DbCommand, bool> match)
    {
        private readonly TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal Func<DbCommand, bool> Match => match;
        /// <summary>命中的命令已停在闸门上。</summary>
        internal Task Reached => _reached.Task;
        internal Task Released => _released.Task;
        internal void Arrive() => _reached.TrySetResult();
        /// <summary>放行停住的命令。</summary>
        internal void Release() => _released.TrySetResult();
    }

    /// <summary>布置一次拦截：此后第一条满足条件的命令停住。</summary>
    internal Hold Arm(Func<DbCommand, bool> match)
    {
        var hold = new Hold(match);
        Volatile.Write(ref _hold, hold);
        return hold;
    }

    /// <summary>连接器票据的写入：INSERT 进 login_tickets、用途为 connector。</summary>
    internal static bool ConnectorTicketInsert(DbCommand command) =>
        command.CommandText.Contains("INSERT INTO", StringComparison.OrdinalIgnoreCase) && command.CommandText.Contains("login_tickets", StringComparison.Ordinal) &&
        command.Parameters.Cast<DbParameter>().Any(parameter => parameter.Value is string value && value == IdentityAccountService.ConnectorPurpose);

    private async ValueTask PauseAsync(DbCommand command, CancellationToken ct)
    {
        if (Volatile.Read(ref _hold) is not { } hold || !hold.Match(command)) return;
        // 只停第一条：抢到闸门的命令清掉布置，其余照常执行。
        if (Interlocked.CompareExchange(ref _hold, null, hold) != hold) return;
        hold.Arrive();
        await hold.Released.WaitAsync(ct).ConfigureAwait(false);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        await PauseAsync(command, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await PauseAsync(command, cancellationToken).ConfigureAwait(false);
        return result;
    }
}
