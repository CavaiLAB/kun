using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.EntityFrameworkCore;

/// <summary>安装不可由派生上下文遗漏的 Kun 模型约定。</summary>
public static class KunDataModelExtensions
{
    /// <summary>在业务映射之后应用 Kun 数据约定与 schema 安全检查。</summary>
    public static DbContextOptionsBuilder UseKunDataModel(this DbContextOptionsBuilder options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ReplaceService<IModelCustomizer, KunStrictModelCustomizer>();
    }

    /// <summary>
    /// 要求 <c>UseKunSchema</c> 指定的 schema 以 <paramref name="prefix"/> 开头，前缀之后仍须小写字母开头、只含小写字母、数字和下划线，总长不超过 63 个字符。
    /// 不调用时 <see cref="KunStrictDbContext"/> 只要求合法的小写标识符。前缀设定后不能改成别的，再次设定不同的前缀即拒绝。
    /// </summary>
    /// <param name="options">数据上下文的配置。</param>
    /// <param name="prefix">小写字母开头、只含小写字母、数字和下划线的前缀，最长 62 个字符。</param>
    public static DbContextOptionsBuilder RequireKunSchemaPrefix(this DbContextOptionsBuilder options, string prefix)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(prefix);
        if (!KunSchemaPrefixOptionsExtension.IsValidPrefix(prefix))
            throw new ArgumentException("schema 前缀必须小写字母开头，只含小写字母、数字和下划线，最长 62 个字符。", nameof(prefix));
        var existing = options.Options.FindExtension<KunSchemaPrefixOptionsExtension>();
        if (existing is not null && existing.Prefix != prefix)
            throw new KunDataMisconfiguredException($"schema 前缀已设为 {existing.Prefix}，不能改成 {prefix}。");
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(existing ?? new KunSchemaPrefixOptionsExtension(prefix));
        return options;
    }
}

/// <summary>严格上下文的 schema 前缀；只供 <see cref="KunStrictDbContext"/> 校验，不登记服务、不进模型。</summary>
internal sealed class KunSchemaPrefixOptionsExtension(string prefix) : IDbContextOptionsExtension
{
    private const int MaxIdentifierLength = 63;
    private DbContextOptionsExtensionInfo? _info;

    public string Prefix { get; } = prefix;

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) { }

    public void Validate(IDbContextOptions options) { }

    internal static bool IsValidPrefix(string prefix) =>
        prefix.Length is > 0 and < MaxIdentifierLength && prefix[0] is >= 'a' and <= 'z' &&
        prefix.All(c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_');

    /// <summary>没有前缀时是 <c>^[a-z][a-z0-9_]{0,62}$</c>；前缀 <c>plugin_</c> 时与原来写死的 <c>^plugin_[a-z][a-z0-9_]{0,55}$</c> 逐字相同。</summary>
    internal static string SchemaPattern(string? prefix) => string.Create(CultureInfo.InvariantCulture,
        $"^{Regex.Escape(prefix ?? string.Empty)}[a-z][a-z0-9_]{{0,{MaxIdentifierLength - 1 - (prefix?.Length ?? 0)}}}$");

    internal static bool IsValidSchema(string schema, string? prefix) =>
        Regex.IsMatch(schema, SchemaPattern(prefix), RegexOptions.CultureInvariant);

    private sealed class ExtensionInfo(KunSchemaPrefixOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "KunSchemaPrefix=" + ((KunSchemaPrefixOptionsExtension)Extension).Prefix + " ";

        // 前缀只影响构造时的校验，不影响 EF 内部服务，前缀不同的上下文也共用同一个内部服务容器。
        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["Kun:SchemaPrefix"] = ((KunSchemaPrefixOptionsExtension)Extension).Prefix;
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
