> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class IdentitySessionTokens

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Identity.AspNetCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Identity.AspNetCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Identity.AspNetCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Identity.Client](/sdk/api/kun/kun-identity-client/index.md)\
Assembly: Kun.Identity.AspNetCore.dll

当前浏览器会话的访问令牌。到期前用 refresh token 换新：同一进程内同一会话只发一次刷新请求，跨副本用 Kun 会话注册表的一次性标记互斥
（发行方开启了 refresh 轮换与复用检测，并发复用会吊销整族）；刷新结果只续写仍然存活的会话，已登出的会话不会因刷新复活；刷新失败即结束会话。

```csharp
public sealed class IdentitySessionTokens
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[IdentitySessionTokens](/sdk/api/kun/kun-identity-client/identitysessiontokens.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### GetAccessTokenAsync(HttpContext, CancellationToken) \{#Kun_Identity_Client_IdentitySessionTokens_GetAccessTokenAsync_Microsoft_AspNetCore_Http_HttpContext_System_Threading_CancellationToken\_}

返回当前会话有效的访问令牌；没有会话、会话已结束或刷新失败时返回 null。令牌只在服务端使用，不得写入日志、响应或前端存储。

```csharp
public ValueTask<string?> GetAccessTokenAsync(HttpContext context, CancellationToken cancellationToken = default)
```

#### Parameters

`context` [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)?>
