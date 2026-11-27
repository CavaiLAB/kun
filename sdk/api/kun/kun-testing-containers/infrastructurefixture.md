> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class InfrastructureFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

一次拉起全部四个依赖（PostgreSQL / Redis / RabbitMQ / Apollo 替身），并行启动。
平台插件的集成测试通常同时需要它们，作为 collection 或 assembly fixture 使用，整组测试共享一套容器。

```csharp
public sealed class InfrastructureFixture : IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[InfrastructureFixture](/sdk/api/kun/kun-testing-containers/infrastructurefixture.md)

#### Implements

IAsyncLifetime,
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### InfrastructureFixture() \{#Kun_Testing_Containers_InfrastructureFixture__ctor}

```csharp
public InfrastructureFixture()
```

## Properties

### Apollo \{#Kun_Testing_Containers_InfrastructureFixture_Apollo}

Apollo 协议替身。

```csharp
public ApolloFixture Apollo { get; }
```

#### Property Value

[ApolloFixture](/sdk/api/kun/kun-testing-containers/apollofixture.md)

### Postgres \{#Kun_Testing_Containers_InfrastructureFixture_Postgres}

PostgreSQL 16。

```csharp
public PostgresFixture Postgres { get; }
```

#### Property Value

[PostgresFixture](/sdk/api/kun/kun-testing-containers/postgresfixture.md)

### RabbitMq \{#Kun_Testing_Containers_InfrastructureFixture_RabbitMq}

RabbitMQ。

```csharp
public RabbitMqFixture RabbitMq { get; }
```

#### Property Value

[RabbitMqFixture](/sdk/api/kun/kun-testing-containers/rabbitmqfixture.md)

### Redis \{#Kun_Testing_Containers_InfrastructureFixture_Redis}

Redis 7。

```csharp
public RedisFixture Redis { get; }
```

#### Property Value

[RedisFixture](/sdk/api/kun/kun-testing-containers/redisfixture.md)

## Methods

### DisposeAsync() \{#Kun_Testing_Containers_InfrastructureFixture_DisposeAsync}

```csharp
public ValueTask DisposeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### InitializeAsync() \{#Kun_Testing_Containers_InfrastructureFixture_InitializeAsync}

Called immediately after the class has been created, before it is used.

```csharp
public ValueTask InitializeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
