> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class RedisFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

Redis 7 容器。

```csharp
public sealed class RedisFixture : ContainerFixture<RedisContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<RedisContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[RedisFixture](/sdk/api/kun/kun-testing-containers/redisfixture.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[ContainerFixture\<RedisContainer>.InitializeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync),
[ContainerFixture\<RedisContainer>.InitializeAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync_System_Threading_CancellationToken_),
[ContainerFixture\<RedisContainer>.DisposeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_DisposeAsync),
[ContainerFixture\<RedisContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### RedisFixture() \{#Kun_Testing_Containers_RedisFixture__ctor}

```csharp
public RedisFixture()
```

## Properties

### ConnectionString \{#Kun_Testing_Containers_RedisFixture_ConnectionString}

StackExchange.Redis 格式的连接串（`host:port`）。

```csharp
public string ConnectionString { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### CreateContainer() \{#Kun_Testing_Containers_RedisFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override RedisContainer CreateContainer()
```

#### Returns

RedisContainer

### OnStartedAsync(RedisContainer, CancellationToken) \{#Kun_Testing_Containers_RedisFixture_OnStartedAsync_Testcontainers_Redis_RedisContainer_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected override ValueTask OnStartedAsync(RedisContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` RedisContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
