> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PgBouncerPostgresFixture

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

Spins up a Postgres 16 container fronted by `edoburu/pgbouncer` in
`POOL_MODE=transaction`. Exposes both direct and bouncer-fronted connection strings so
tests can assert that `SET LOCAL` / `set_config(..., true)` semantics survive
PgBouncer's backend-connection recycling between transactions.

```csharp
public sealed class PgBouncerPostgresFixture : IAsyncLifetime, IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PgBouncerPostgresFixture](/sdk/api/kun/kun-testing-containers/pgbouncerpostgresfixture.md)

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

## Remarks

Shared between P1-4 (`TenantRlsInterceptorPgBouncerTests`, M2) and the later P1-5
PgBouncer compatibility suite. Skips automatically when Docker is not available.

## Constructors

### PgBouncerPostgresFixture() \{#Kun_Testing_Containers_PgBouncerPostgresFixture__ctor}

```csharp
public PgBouncerPostgresFixture()
```

## Properties

### BouncerConnectionString \{#Kun_Testing_Containers_PgBouncerPostgresFixture_BouncerConnectionString}

Connection string routed through PgBouncer (transaction pool mode).

```csharp
public string BouncerConnectionString { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### DirectConnectionString \{#Kun_Testing_Containers_PgBouncerPostgresFixture_DirectConnectionString}

Connection string that talks to Postgres directly (bypasses PgBouncer).

```csharp
public string DirectConnectionString { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### DisposeAsync() \{#Kun_Testing_Containers_PgBouncerPostgresFixture_DisposeAsync}

```csharp
public ValueTask DisposeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### InitializeAsync() \{#Kun_Testing_Containers_PgBouncerPostgresFixture_InitializeAsync}

Called immediately after the class has been created, before it is used.

```csharp
public ValueTask InitializeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
