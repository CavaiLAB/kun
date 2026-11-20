using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Kun.Auditing;

// One interceptor may serve multiple contexts in a scope. Each save owns only the rows it added.
internal sealed class StagedAuditRows
{
    private readonly ConditionalWeakTable<DbContext, List<object>> _rows = new();

    internal List<object> Begin(DbContext context)
    {
        Rollback(context);
        return _rows.GetOrCreateValue(context);
    }

    internal void Complete(DbContext? context)
    {
        if (context is not null) _rows.Remove(context);
    }

    internal void Rollback(DbContext? context)
    {
        if (context is null || !_rows.TryGetValue(context, out var rows)) return;
        foreach (var row in rows)
        {
            var entry = context.Entry(row);
            if (entry.State == EntityState.Added) entry.State = EntityState.Detached;
        }
        _rows.Remove(context);
    }
}
