> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class RabbitMqFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

RabbitMQ 容器。韧性测试可经 [ContainerFixture\<TContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container) 停启 broker。

```csharp
public class RabbitMqFixture : ContainerFixture<RabbitMqContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<RabbitMqContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[RabbitMqFixture](/sdk/api/kun/kun-testing-containers/rabbitmqfixture.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[ContainerFixture\<RabbitMqContainer>.InitializeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync),
[ContainerFixture\<RabbitMqContainer>.InitializeAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync_System_Threading_CancellationToken_),
[ContainerFixture\<RabbitMqContainer>.DisposeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_DisposeAsync),
[ContainerFixture\<RabbitMqContainer>.CreateContainer()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_CreateContainer),
[ContainerFixture\<RabbitMqContainer>.OnStartingAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnStartingAsync_System_Threading_CancellationToken_),
[ContainerFixture\<RabbitMqContainer>.StartAlongsideAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_StartAlongsideAsync_System_Threading_CancellationToken_),
[ContainerFixture\<RabbitMqContainer>.OnStartedAsync(RabbitMqContainer, CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnStartedAsync__0_System_Threading_CancellationToken_),
[ContainerFixture\<RabbitMqContainer>.OnDisposingAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnDisposingAsync),
[ContainerFixture\<RabbitMqContainer>.OnDisposedAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnDisposedAsync),
[ContainerFixture\<RabbitMqContainer>.NewSecret()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_NewSecret),
[ContainerFixture\<RabbitMqContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container),
[ContainerFixture\<RabbitMqContainer>.StartAttempts](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_StartAttempts),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### RabbitMqFixture() \{#Kun_Testing_Containers_RabbitMqFixture__ctor}

```csharp
public RabbitMqFixture()
```

## Properties

### ConnectionString \{#Kun_Testing_Containers_RabbitMqFixture_ConnectionString}

AMQP 连接串（`amqp://user:password@host:port/`）。

```csharp
public string ConnectionString { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Image \{#Kun_Testing_Containers_RabbitMqFixture_Image}

本次运行的 RabbitMQ 镜像，默认取 ContainerImages.RabbitMq。

```csharp
public string Image { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PreservePortAcrossRestarts \{#Kun_Testing_Containers_RabbitMqFixture_PreservePortAcrossRestarts}

重启同一实例时保持宿主映射端口。

```csharp
public bool PreservePortAcrossRestarts { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### CreateContainer() \{#Kun_Testing_Containers_RabbitMqFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override RabbitMqContainer CreateContainer()
```

#### Returns

RabbitMqContainer

### OnStartedAsync(RabbitMqContainer, CancellationToken) \{#Kun_Testing_Containers_RabbitMqFixture_OnStartedAsync_Testcontainers_RabbitMq_RabbitMqContainer_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected override ValueTask OnStartedAsync(RabbitMqContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` RabbitMqContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
