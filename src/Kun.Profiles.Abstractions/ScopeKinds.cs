namespace Kun.Profiles.Abstractions;

/// <summary>平台承认的非全局 scope 种类。内核的 <c>ScopeKey</c> 只查语法，种类的取值在平台档定。</summary>
public static class ScopeKinds
{
    /// <summary>租户。</summary>
    public const string Tenant = "tenant";

    /// <summary>应用。</summary>
    public const string App = "app";

    /// <summary>Agent 会话。</summary>
    public const string AgentSession = "agent-session";

    /// <summary><paramref name="kind"/> 是不是平台承认的非全局种类。</summary>
    public static bool IsKnown(string? kind) => kind is Tenant or App or AgentSession;
}
