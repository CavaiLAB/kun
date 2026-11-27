> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum PluginState

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

某次 Apply 的状态；失败保留原异常供管理面查询。

```csharp
public enum PluginState
```

## Fields

`Applying = 1`

正在执行 Apply，注册尚未对其它插件公开。

`Failed = 3`

Apply 失败且已尝试撤销全部效果。

`Pending = 0`

等待全部必需服务。

`Ready = 2`

Apply 成功且依赖保持就绪。
