> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ContainerFixture\<TContainer>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

单个容器 fixture 的公共骨架：启动容器（瞬时失败时有界重试）、按 [DockerRequirement](/sdk/api/kun/kun-testing-containers/dockerrequirement.md) 处理 Docker 缺失、随 fixture 一起释放。
直接用作 xUnit 的 class / collection / assembly fixture。

```csharp
public abstract class ContainerFixture<TContainer> : IAsyncLifetime, IAsyncDisposable where TContainer : class, IContainer
```

#### Type Parameters

`TContainer`

Testcontainers 容器类型。

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<TContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ContainerFixture() \{#Kun_Testing_Containers_ContainerFixture_1__ctor}

```csharp
protected ContainerFixture()
```

## Properties

### Container \{#Kun_Testing_Containers_ContainerFixture_1_Container}

已启动的容器，供需要停启容器的韧性测试使用（例如 RabbitMQ 重启后消息不丢）。

```csharp
public TContainer Container { get; }
```

#### Property Value

TContainer

### StartAttempts \{#Kun_Testing_Containers_ContainerFixture_1_StartAttempts}

容器（含 [ContainerFixture\<TContainer>.OnStartingAsync](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnStartingAsync_System_Threading_CancellationToken_) 启动的依赖容器）在就绪之前自行退出时，整个启动最多尝试的次数，默认 1（不重试）。
环境类瞬时失败另按 [ContainerFixture\<TContainer>.TransientAttempts](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_TransientAttempts) 重试，不受此值影响。

```csharp
protected virtual int StartAttempts { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### CreateContainer() \{#Kun_Testing_Containers_ContainerFixture_1_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected abstract TContainer CreateContainer()
```

#### Returns

TContainer

### DisposeAsync() \{#Kun_Testing_Containers_ContainerFixture_1_DisposeAsync}

```csharp
public ValueTask DisposeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

#### Remarks

可重复调用：组合 fixture 启动失败时会先自行释放一次。

### InitializeAsync() \{#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync}

Called immediately after the class has been created, before it is used.

```csharp
public ValueTask InitializeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### InitializeAsync(CancellationToken) \{#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync_System_Threading_CancellationToken\_}

启动夹具，并在调用方取消时释放已启动的容器。

```csharp
public ValueTask InitializeAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### NewSecret() \{#Kun_Testing_Containers_ContainerFixture_1_NewSecret}

每次运行随机生成的口令：测试容器不使用写死在仓库里的凭据。

```csharp
protected static string NewSecret()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OnDisposedAsync() \{#Kun_Testing_Containers_ContainerFixture_1_OnDisposedAsync}

主容器释放后，再释放它依赖的数据库与网络；启动重试之间也会调用。

```csharp
protected virtual ValueTask OnDisposedAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnDisposingAsync() \{#Kun_Testing_Containers_ContainerFixture_1_OnDisposingAsync}

释放容器之前释放 fixture 自己持有的资源；启动重试之间也会调用，须能处理尚未启动完的状态。

```csharp
protected virtual ValueTask OnDisposingAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnStartedAsync(TContainer, CancellationToken) \{#Kun_Testing_Containers_ContainerFixture_1_OnStartedAsync__0_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected virtual ValueTask OnStartedAsync(TContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` TContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnStartingAsync(CancellationToken) \{#Kun_Testing_Containers_ContainerFixture_1_OnStartingAsync_System_Threading_CancellationToken\_}

组合 fixture 可先准备受其拥有的依赖与网络；失败时统一清理，瞬时失败重试时会在清理后再次调用。

```csharp
protected virtual ValueTask OnStartingAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### StartAlongsideAsync(CancellationToken) \{#Kun_Testing_Containers_ContainerFixture_1_StartAlongsideAsync_System_Threading_CancellationToken\_}

与主容器并行启动的同级依赖，例如与主容器一样只依赖 [ContainerFixture\<TContainer>.OnStartingAsync](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnStartingAsync_System_Threading_CancellationToken_) 所备数据库的另一个服务。
与主容器一起等待、按同样的规则重试；缺省没有。基类只释放主容器，同级依赖由 fixture 自己在 [ContainerFixture\<TContainer>.OnDisposingAsync](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnDisposingAsync) 里释放。

```csharp
protected virtual ValueTask StartAlongsideAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
