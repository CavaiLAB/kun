> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class DexFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

独立实现的外部 OIDC 发行方（CNCF Dex），用来验证依赖方能对接 Kun 身份服务之外的任意发行方。内存存储、本地密码库里一个用户、一个启用 PKCE 的公开客户端，
跳过授权确认页。Dex 的 issuer 必须在启动前写进配置，所以宿主端口与 RP 回调端口都在启动前选定；口令每次运行随机生成，配置里只有 bcrypt 哈希。

```csharp
public sealed class DexFixture : ContainerFixture<IContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<IContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[DexFixture](/sdk/api/kun/kun-testing-containers/dexfixture.md)

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

### DexFixture() \{#Kun_Testing_Containers_DexFixture__ctor}

```csharp
public DexFixture()
```

## Fields

### ClientId \{#Kun_Testing_Containers_DexFixture_ClientId}

登记在 Dex 的公开客户端标识。

```csharp
public const string ClientId = "qian-standalone"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Email \{#Kun_Testing_Containers_DexFixture_Email}

本地密码库里的用户邮箱（Dex 的登录名）。

```csharp
public const string Email = "alice@example.test"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Issuer \{#Kun_Testing_Containers_DexFixture_Issuer}

Dex 的 issuer（discovery 位于其下的 `/.well-known/openid-configuration`）。

```csharp
public Uri Issuer { get; }
```

#### Property Value

Uri

### Password \{#Kun_Testing_Containers_DexFixture_Password}

用户口令，每次运行随机生成。

```csharp
public string Password { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RedirectUri \{#Kun_Testing_Containers_DexFixture_RedirectUri}

登记给客户端的唯一回调地址：测试里的依赖方须在这个端口以 HTTPS 监听。

```csharp
public Uri RedirectUri { get; }
```

#### Property Value

Uri

## Methods

### CreateContainer() \{#Kun_Testing_Containers_DexFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override IContainer CreateContainer()
```

#### Returns

IContainer

### OnStartingAsync(CancellationToken) \{#Kun_Testing_Containers_DexFixture_OnStartingAsync_System_Threading_CancellationToken\_}

组合 fixture 可先准备受其拥有的依赖与网络；失败时统一清理，瞬时失败重试时会在清理后再次调用。

```csharp
protected override ValueTask OnStartingAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
