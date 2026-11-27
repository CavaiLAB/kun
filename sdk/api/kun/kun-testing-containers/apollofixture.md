> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ApolloFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

Apollo 配置服务的协议替身（PRD §14.2：集成测试层的 Apollo 用 mock server）。
容器内运行 `ApolloMock/apollo_mock.py`：实现客户端用到的 `/configs`、`/configfiles/json`、
`/notifications/v2` 长轮询、Meta Server 与访问密钥签名；测试经本类的方法发布或撤下配置。
覆盖范围见脚本文件头注释与 tests/README.md。

```csharp
public sealed class ApolloFixture : ContainerFixture<IContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<IContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[ApolloFixture](/sdk/api/kun/kun-testing-containers/apollofixture.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[ContainerFixture\<IContainer>.InitializeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync),
[ContainerFixture\<IContainer>.InitializeAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync_System_Threading_CancellationToken_),
[ContainerFixture\<IContainer>.DisposeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_DisposeAsync),
[ContainerFixture\<IContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ApolloFixture() \{#Kun_Testing_Containers_ApolloFixture__ctor}

```csharp
public ApolloFixture()
```

## Fields

### DefaultCluster \{#Kun_Testing_Containers_ApolloFixture_DefaultCluster}

默认集群名。

```csharp
public const string DefaultCluster = "default"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### ConfigServiceUri \{#Kun_Testing_Containers_ApolloFixture_ConfigServiceUri}

Config Service 地址（同时也是 Meta Server 地址），即客户端的 `Apollo__ConfigServer__0`。

```csharp
public Uri ConfigServiceUri { get; }
```

#### Property Value

Uri

## Methods

### ClearAccessKeyAsync(string, CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_ClearAccessKeyAsync_System_String_System_Threading_CancellationToken\_}

关闭 appId 的访问密钥校验。

```csharp
public Task ClearAccessKeyAsync(string appId, CancellationToken cancellationToken = default)
```

#### Parameters

`appId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### CreateContainer() \{#Kun_Testing_Containers_ApolloFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override IContainer CreateContainer()
```

#### Returns

IContainer

### DeleteAsync(string, string, string, CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_DeleteAsync_System_String_System_String_System_String_System_Threading_CancellationToken\_}

撤下一个 namespace：之后拉取返回 404，长轮询的客户端收到变更通知。不存在时返回 false。

```csharp
public Task<bool> DeleteAsync(string appId, string namespaceName, string cluster = "default", CancellationToken cancellationToken = default)
```

#### Parameters

`appId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`namespaceName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cluster` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)>

### OnDisposingAsync() \{#Kun_Testing_Containers_ApolloFixture_OnDisposingAsync}

释放容器之前释放 fixture 自己持有的资源；启动重试之间也会调用，须能处理尚未启动完的状态。

```csharp
protected override ValueTask OnDisposingAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnStartedAsync(IContainer, CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_OnStartedAsync_DotNet_Testcontainers_Containers_IContainer_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected override ValueTask OnStartedAsync(IContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` IContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### PublishAsync(string, string, IReadOnlyDictionary\<string, string>, string, CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_PublishAsync_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_System_Threading_CancellationToken\_}

发布一个 namespace 的新版本：生成新的 releaseKey 并唤醒正在长轮询的客户端。

```csharp
public Task<ApolloRelease> PublishAsync(string appId, string namespaceName, IReadOnlyDictionary<string, string> configurations, string cluster = "default", CancellationToken cancellationToken = default)
```

#### Parameters

`appId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`namespaceName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`configurations` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`cluster` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)>

### ResetAsync(CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_ResetAsync_System_Threading_CancellationToken\_}

清空全部配置、通知与访问密钥，长轮询时长恢复为 60 秒。

```csharp
public Task ResetAsync(CancellationToken cancellationToken = default)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### SetAccessKeyAsync(string, string, CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_SetAccessKeyAsync_System_String_System_String_System_Threading_CancellationToken\_}

为 appId 开启访问密钥校验（Apollo AccessKey，HMAC-SHA1 签名）。

```csharp
public Task SetAccessKeyAsync(string appId, string secret, CancellationToken cancellationToken = default)
```

#### Parameters

`appId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`secret` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### SetHoldAsync(TimeSpan, CancellationToken) \{#Kun_Testing_Containers_ApolloFixture_SetHoldAsync_System_TimeSpan_System_Threading_CancellationToken\_}

调整长轮询挂起时长（Apollo 服务端为 60 秒）。测试「超时返回 304」时可调短。

```csharp
public Task SetHoldAsync(TimeSpan hold, CancellationToken cancellationToken = default)
```

#### Parameters

`hold` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
