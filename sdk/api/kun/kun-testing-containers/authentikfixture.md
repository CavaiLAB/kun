> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuthentikFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

真实 Authentik（server + worker + 专属 PostgreSQL），作为 OIDC 连接器的上游身份源（S1-IDP-007 验收 1）。
启动后用引导令牌经 REST API 开 OAuth2 客户端与用户；[AuthentikFixture.SignInAsync](/sdk/api/kun/kun-testing-containers/authentikfixture.md#Kun_Testing_Containers_AuthentikFixture_SignInAsync_System_Uri_Kun_Testing_Containers_AuthentikFixture_User_System_Uri_System_Threading_CancellationToken_) 像浏览器一样走 Authentik 的登录流程（flow executor），
返回 Authentik 回跳到依赖方回调地址的完整 URL。口令与令牌每次运行随机生成。

```csharp
public sealed class AuthentikFixture : ContainerFixture<IContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<IContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md)

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

### AuthentikFixture() \{#Kun_Testing_Containers_AuthentikFixture__ctor}

```csharp
public AuthentikFixture()
```

## Properties

### BaseUri \{#Kun_Testing_Containers_AuthentikFixture_BaseUri}

Authentik 的根地址（宿主机可达的回环地址）。

```csharp
public Uri BaseUri { get; }
```

#### Property Value

Uri

## Methods

### CreateClientAsync(Uri, CancellationToken) \{#Kun_Testing_Containers_AuthentikFixture_CreateClientAsync_System_Uri_System_Threading_CancellationToken\_}

开一个机密客户端（授权码 + refresh，RS256 签名，sub 为散列后的用户标识），只登记给定的回调地址（严格匹配）。

```csharp
public Task<AuthentikFixture.OAuthClient> CreateClientAsync(Uri redirectUri, CancellationToken cancellationToken = default)
```

#### Parameters

`redirectUri` Uri

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)>

### CreateContainer() \{#Kun_Testing_Containers_AuthentikFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override IContainer CreateContainer()
```

#### Returns

IContainer

### CreateUserAsync(string, CancellationToken) \{#Kun_Testing_Containers_AuthentikFixture_CreateUserAsync_System_String_System_Threading_CancellationToken\_}

建一个本地用户并设置口令。

```csharp
public Task<AuthentikFixture.User> CreateUserAsync(string name, CancellationToken cancellationToken = default)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)>

### OnDisposedAsync() \{#Kun_Testing_Containers_AuthentikFixture_OnDisposedAsync}

主容器释放后，再释放它依赖的数据库与网络；启动重试之间也会调用。

```csharp
protected override ValueTask OnDisposedAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnDisposingAsync() \{#Kun_Testing_Containers_AuthentikFixture_OnDisposingAsync}

释放容器之前释放 fixture 自己持有的资源；启动重试之间也会调用，须能处理尚未启动完的状态。

```csharp
protected override ValueTask OnDisposingAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnStartedAsync(IContainer, CancellationToken) \{#Kun_Testing_Containers_AuthentikFixture_OnStartedAsync_DotNet_Testcontainers_Containers_IContainer_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected override ValueTask OnStartedAsync(IContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` IContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### OnStartingAsync(CancellationToken) \{#Kun_Testing_Containers_AuthentikFixture_OnStartingAsync_System_Threading_CancellationToken\_}

组合 fixture 可先准备受其拥有的依赖与网络；失败时统一清理，瞬时失败重试时会在清理后再次调用。

```csharp
protected override ValueTask OnStartingAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### SignInAsync(Uri, User, Uri, CancellationToken) \{#Kun_Testing_Containers_AuthentikFixture_SignInAsync_System_Uri_Kun_Testing_Containers_AuthentikFixture_User_System_Uri_System_Threading_CancellationToken\_}

以一个全新的浏览器会话打开授权地址，经 flow executor 依次完成标识、口令与（隐式）同意阶段，
返回 Authentik 302 到回调地址的完整 URL（含 code、state 与 iss）。

```csharp
public Task<Uri> SignInAsync(Uri authorization, AuthentikFixture.User user, Uri callbackPrefix, CancellationToken cancellationToken = default)
```

#### Parameters

`authorization` Uri

`user` [AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md).[User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)

`callbackPrefix` Uri

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<Uri>
