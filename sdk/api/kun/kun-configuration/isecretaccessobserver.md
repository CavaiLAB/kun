> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface ISecretAccessObserver

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

可替换的审计接收端；写入失败不得返回密钥。

```csharp
public interface ISecretAccessObserver
```

## Methods

### RecordAsync(SecretAccessEvent, CancellationToken) \{#Kun_Configuration_ISecretAccessObserver_RecordAsync_Kun_Configuration_SecretAccessEvent_System_Threading_CancellationToken\_}

记录一次访问结果。

```csharp
ValueTask RecordAsync(SecretAccessEvent access, CancellationToken cancellationToken = default)
```

#### Parameters

`access` [SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
