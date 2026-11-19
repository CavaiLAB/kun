using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kun.Data;

/// <summary>安装不可由派生上下文遗漏的 Qian 模型约定。</summary>
public static class KunDataModelExtensions
{
    /// <summary>在业务映射之后应用 Kun 数据约定与 Qian schema 安全检查。</summary>
    public static DbContextOptionsBuilder UseKunDataModel(this DbContextOptionsBuilder options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ReplaceService<IModelCustomizer, KunStrictModelCustomizer>();
    }
}

internal sealed class KunStrictModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        if (context is not KunStrictDbContext qian)
            throw new KunDataMisconfiguredException("Qian 模型约定只能用于 KunStrictDbContext。");
        qian.CompleteDataModel(modelBuilder);
    }
}
