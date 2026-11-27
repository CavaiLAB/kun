> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class K3sFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

K3s 单节点集群（特权容器）。[K3sFixture.Kubeconfig](/sdk/api/kun/kun-testing-containers/k3sfixture.md#Kun_Testing_Containers_K3sFixture_Kubeconfig) 里的 API Server 已映射到宿主端口；容器内自带 kubectl（含 kustomize），
可用 [K3sFixture.KubectlAsync](/sdk/api/kun/kun-testing-containers/k3sfixture.md#Kun_Testing_Containers_K3sFixture_KubectlAsync_System_Threading_CancellationToken_System_String___) 代替 Argo 应用部署仓库里的片段。

```csharp
public sealed class K3sFixture : ContainerFixture<K3sContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<K3sContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[K3sFixture](/sdk/api/kun/kun-testing-containers/k3sfixture.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[ContainerFixture\<K3sContainer>.InitializeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync),
[ContainerFixture\<K3sContainer>.InitializeAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync_System_Threading_CancellationToken_),
[ContainerFixture\<K3sContainer>.DisposeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_DisposeAsync),
[ContainerFixture\<K3sContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### K3sFixture() \{#Kun_Testing_Containers_K3sFixture__ctor}

```csharp
public K3sFixture()
```

## Properties

### Kubeconfig \{#Kun_Testing_Containers_K3sFixture_Kubeconfig}

管理员 kubeconfig（YAML）。

```csharp
public string Kubeconfig { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### StartAttempts \{#Kun_Testing_Containers_K3sFixture_StartAttempts}

容器（含 [ContainerFixture\<TContainer>.OnStartingAsync](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_OnStartingAsync_System_Threading_CancellationToken_) 启动的依赖容器）在就绪之前自行退出时，整个启动最多尝试的次数，默认 1（不重试）。
环境类瞬时失败另按 [ContainerFixture\<TContainer>.TransientAttempts](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_TransientAttempts) 重试，不受此值影响。

```csharp
protected override int StartAttempts { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Remarks

K3s 启动时短暂占满全部 CPU；宿主繁忙时 API Server 的启动后钩子可能超时、进程退出（退出码 2），此时重建容器，最多三次。

## Methods

### CopyAsync(string, string, CancellationToken) \{#Kun_Testing_Containers_K3sFixture_CopyAsync_System_String_System_String_System_Threading_CancellationToken\_}

把文本写进容器内的文件（如待 kubectl apply 的清单）。

```csharp
public Task CopyAsync(string content, string path, CancellationToken cancellationToken)
```

#### Parameters

`content` [string](https://learn.microsoft.com/dotnet/api/system.string)

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### CreateContainer() \{#Kun_Testing_Containers_K3sFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override K3sContainer CreateContainer()
```

#### Returns

K3sContainer

### KubectlAsync(CancellationToken, params string\[]) \{#Kun_Testing_Containers_K3sFixture_KubectlAsync_System_Threading_CancellationToken_System_String\_\_\_}

在容器内执行 kubectl，返回标准输出；退出码非零时抛出并带上标准错误。

```csharp
public Task<string> KubectlAsync(CancellationToken cancellationToken, params string[] arguments)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

`arguments` [string](https://learn.microsoft.com/dotnet/api/system.string)\[]

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### OnStartedAsync(K3sContainer, CancellationToken) \{#Kun_Testing_Containers_K3sFixture_OnStartedAsync_Testcontainers_K3s_K3sContainer_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected override ValueTask OnStartedAsync(K3sContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` K3sContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
