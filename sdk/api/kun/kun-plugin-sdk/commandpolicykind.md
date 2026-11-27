> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum CommandPolicyKind

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

基础组合包的三个强制执行点。

```csharp
public enum CommandPolicyKind
```

## Fields

`Audit = 2`

审计策略及同事务写入。

`Authorization = 1`

真实权限判定。

`Tenancy = 0`

验证租户与 scope。
