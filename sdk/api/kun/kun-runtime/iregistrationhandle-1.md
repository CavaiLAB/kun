> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IRegistrationHandle\<TService>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

Provide 返回的注册句柄：Dispose 撤销注册，可重复调用；Replace 原子替换同一注册的实例。

```csharp
public interface IRegistrationHandle<TService> : IDisposable where TService : class
```

#### Type Parameters

`TService`

#### Implements

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

## Methods

### Replace(TService) \{#Kun_Runtime_IRegistrationHandle_1_Replace__0\_}

在原注册位原子替换实例：解析方只会看到旧实例或新实例，不会看到注册缺失；scope、注册顺序与独占性不变。
替换等价于在同一临界区内撤销后再登记，以 inject 依赖该键的条目同样回到 Pending 并重新 Apply。
句柄已撤销或上下文已卸载时抛出 ObjectDisposedException。

```csharp
void Replace(TService instance)
```

#### Parameters

`instance` TService
