# Qian.Profiles.Abstractions

profile 装配的 Definition：输入选项、来源层、结果快照、`IProfileLoader` 与 `PROFILE_INVALID` 错误定义，以及平台承认的 scope 种类 `ScopeKinds`（tenant、app、agent-session；内核的 `ScopeKey` 只查语法）。依赖 Runtime 与表面 Definition，不依赖 Provider。完整规则统一维护在 [Qian.Profiles](../Qian.Profiles/README.md)。
