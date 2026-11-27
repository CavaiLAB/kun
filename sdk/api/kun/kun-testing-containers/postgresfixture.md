> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PostgresFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

PostgreSQL 16 容器。提供两条连接串：
[PostgresFixture.ConnectionString](/sdk/api/kun/kun-testing-containers/postgresfixture.md#Kun_Testing_Containers_PostgresFixture_ConnectionString) 是超级用户，用于建表、迁移与造数；
[PostgresFixture.AppConnectionString](/sdk/api/kun/kun-testing-containers/postgresfixture.md#Kun_Testing_Containers_PostgresFixture_AppConnectionString) 是 `NOSUPERUSER NOBYPASSRLS` 的应用角色，验证 RLS 的测试必须用它——
超级用户与 `BYPASSRLS` 角色会绕过 `FORCE ROW LEVEL SECURITY`，策略对它们不可见（ADR-004）。

```csharp
public sealed class PostgresFixture : ContainerFixture<PostgreSqlContainer>, IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerFixture\<PostgreSqlContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md) ←
[PostgresFixture](/sdk/api/kun/kun-testing-containers/postgresfixture.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[ContainerFixture\<PostgreSqlContainer>.InitializeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync),
[ContainerFixture\<PostgreSqlContainer>.InitializeAsync(CancellationToken)](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_InitializeAsync_System_Threading_CancellationToken_),
[ContainerFixture\<PostgreSqlContainer>.DisposeAsync()](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_DisposeAsync),
[ContainerFixture\<PostgreSqlContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### PostgresFixture() \{#Kun_Testing_Containers_PostgresFixture__ctor}

```csharp
public PostgresFixture()
```

## Fields

### AppRoleName \{#Kun_Testing_Containers_PostgresFixture_AppRoleName}

应用角色名：只有 DML 权限，不能建表，受 RLS 约束。

```csharp
public const string AppRoleName = "qian_app"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### DatabaseName \{#Kun_Testing_Containers_PostgresFixture_DatabaseName}

测试库名。

```csharp
public const string DatabaseName = "qian_test"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### AppConnectionString \{#Kun_Testing_Containers_PostgresFixture_AppConnectionString}

应用角色连接串（受 RLS 约束的读写）。

```csharp
public string AppConnectionString { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ConnectionString \{#Kun_Testing_Containers_PostgresFixture_ConnectionString}

超级用户连接串（DDL、迁移、造数）。

```csharp
public string ConnectionString { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### CreateContainer() \{#Kun_Testing_Containers_PostgresFixture_CreateContainer}

构造容器（尚未启动）；启动重试时每次重新构造。

```csharp
protected override PostgreSqlContainer CreateContainer()
```

#### Returns

PostgreSqlContainer

### OnStartedAsync(PostgreSqlContainer, CancellationToken) \{#Kun_Testing_Containers_PostgresFixture_OnStartedAsync_Testcontainers_PostgreSql_PostgreSqlContainer_System_Threading_CancellationToken\_}

容器就绪后的初始化，例如读出连接串、建应用角色。

```csharp
protected override ValueTask OnStartedAsync(PostgreSqlContainer container, CancellationToken cancellationToken)
```

#### Parameters

`container` PostgreSqlContainer

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
