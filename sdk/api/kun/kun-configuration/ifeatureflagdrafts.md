> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IFeatureFlagDrafts

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

平台专用草稿写入；实现每次重新检查已认证主体、确认、权限与审计。

```csharp
public interface IFeatureFlagDrafts
```

## Methods

### SetAsync(FeatureFlagDraft, CancellationToken) \{#Kun_Configuration_IFeatureFlagDrafts_SetAsync_Kun_Configuration_FeatureFlagDraft_System_Threading_CancellationToken\_}

保存受限规则草稿；成功不代表已发布或对读者生效。

```csharp
ValueTask<FeatureFlagDraftResult> SetAsync(FeatureFlagDraft request, CancellationToken cancellationToken = default)
```

#### Parameters

`request` [FeatureFlagDraft](/sdk/api/kun/kun-configuration/featureflagdraft.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[FeatureFlagDraftResult](/sdk/api/kun/kun-configuration/featureflagdraftresult.md)>
