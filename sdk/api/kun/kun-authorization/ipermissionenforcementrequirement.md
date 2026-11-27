> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IPermissionEnforcementRequirement

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Authorization` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Authorization`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Authorization)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Authorization](/sdk/api/kun/kun-authorization/index.md)\
Assembly: Kun.Authorization.dll

标识端点授权策略里执行某个权限点的要求。宿主的权限回调给端点加上这样的要求，构造端点时据此核对权限点确实接上了执行点；
自定义执行点让自己的授权要求实现本接口。

```csharp
public interface IPermissionEnforcementRequirement
```

## Properties

### Permission \{#Kun_Authorization_IPermissionEnforcementRequirement_Permission}

这项要求执行的权限点。

```csharp
string Permission { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
