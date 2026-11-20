namespace Kun.Configuration;

/// <summary>一次被拒绝的候选配置，仅包含登记信息和失败条数。</summary>
public sealed class OptionsReloadFailure
{
    internal OptionsReloadFailure(string optionsType, string optionsName, string section, string? plugin, int failures)
    {
        OptionsType = optionsType;
        OptionsName = optionsName;
        Section = section;
        Plugin = plugin;
        Failures = failures;
    }

    /// <summary>选项类型全名。</summary>
    public string OptionsType { get; }
    /// <summary>命名选项的名称。</summary>
    public string OptionsName { get; }
    /// <summary>配置节路径。</summary>
    public string Section { get; }
    /// <summary>登记选项的组件标识。</summary>
    public string? Plugin { get; }
    /// <summary>未通过的校验条数，绑定失败记一条。</summary>
    public int Failures { get; }
}
