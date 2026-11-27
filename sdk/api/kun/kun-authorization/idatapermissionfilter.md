> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IDataPermissionFilter

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Authorization` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Authorization`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Authorization)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Authorization](/sdk/api/kun/kun-authorization/index.md)\
Assembly: Kun.Authorization.dll

本地数据过滤端口，不依赖 EF 包或具体数据库。

```csharp
public interface IDataPermissionFilter
```

## Methods

### GetExpressionAsync\<T>(PermissionRequest, CancellationToken) \{#Kun_Authorization_IDataPermissionFilter_GetExpressionAsync__1_Kun_Authorization_PermissionRequest_System_Threading_CancellationToken\_}

获取实体谓词；Provider 负责维持租户隔离与行级语义。

```csharp
ValueTask<Expression<Func<T, bool>>> GetExpressionAsync<T>(PermissionRequest request, CancellationToken cancellationToken = default)
```

#### Parameters

`request` [PermissionRequest](/sdk/api/kun/kun-authorization/permissionrequest.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[Expression](https://learn.microsoft.com/dotnet/api/system.linq.expressions.expression-1)\<[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<T, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)>>>

#### Type Parameters

`T`
