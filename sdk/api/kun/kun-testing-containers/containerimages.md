> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ContainerImages

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

集成测试使用的镜像。版本与部署基线一致：PostgreSQL 16、Redis 7（ADR-017 D17.2）；
RabbitMQ 取当前 4.x 稳定线（PRD §14.6）。每个镜像都可用环境变量覆盖，便于与 staging 对齐或换内网镜像源。

```csharp
public static class ContainerImages
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ContainerImages](/sdk/api/kun/kun-testing-containers/containerimages.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### ApolloMock \{#Kun_Testing_Containers_ContainerImages_ApolloMock}

运行 Apollo 协议替身的 Python 镜像，覆盖变量 `QIAN_TEST_IMAGE_APOLLO_MOCK`。

```csharp
public static string ApolloMock { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Authentik \{#Kun_Testing_Containers_ContainerImages_Authentik}

Authentik（S1-IDP-007 OIDC 连接器的真实上游），覆盖变量 `QIAN_TEST_IMAGE_AUTHENTIK`。测试容器的版本与运维侧现网的 Authentik（2024.10）解耦，
只验证标准 OIDC 授权码流程；server 与 worker 用同一个镜像。

```csharp
public static string Authentik { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Dex \{#Kun_Testing_Containers_ContainerImages_Dex}

外部 OIDC 发行方 Dex 的镜像，覆盖变量 `QIAN_TEST_IMAGE_DEX`。

```csharp
public static string Dex { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### K3s \{#Kun_Testing_Containers_ContainerImages_K3s}

K3s 单节点集群镜像，覆盖变量 `QIAN_TEST_IMAGE_K3S`。

```csharp
public static string K3s { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PgBouncer \{#Kun_Testing_Containers_ContainerImages_PgBouncer}

PgBouncer 镜像，覆盖变量 QIAN_TEST_IMAGE_PGBOUNCER。

```csharp
public static string PgBouncer { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Postgres \{#Kun_Testing_Containers_ContainerImages_Postgres}

PostgreSQL 镜像，覆盖变量 `QIAN_TEST_IMAGE_POSTGRES`。

```csharp
public static string Postgres { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RabbitMq \{#Kun_Testing_Containers_ContainerImages_RabbitMq}

RabbitMQ 镜像，覆盖变量 `QIAN_TEST_IMAGE_RABBITMQ`。

```csharp
public static string RabbitMq { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RabbitMq313 \{#Kun_Testing_Containers_ContainerImages_RabbitMq313}

RabbitMQ 3.13 兼容性测试镜像，覆盖变量 QIAN_TEST_IMAGE_RABBITMQ_313。

```csharp
public static string RabbitMq313 { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Redis \{#Kun_Testing_Containers_ContainerImages_Redis}

Redis 镜像，覆盖变量 `QIAN_TEST_IMAGE_REDIS`。

```csharp
public static string Redis { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RustFs \{#Kun_Testing_Containers_ContainerImages_RustFs}

S3 兼容服务镜像，覆盖变量 QIAN_TEST_IMAGE_RUSTFS。

```csharp
public static string RustFs { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
