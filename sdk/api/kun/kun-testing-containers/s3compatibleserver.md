> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class S3CompatibleServer

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

存储集成测试用的 S3 兼容服务容器：RustFS（Apache-2.0）。

```csharp
public sealed class S3CompatibleServer : IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[S3CompatibleServer](/sdk/api/kun/kun-testing-containers/s3compatibleserver.md)

#### Implements

[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

镜像钉到版本与摘要。RustFS 项目自己在 GHCR 与 Docker Hub 发布同一个摘要，两处都能匿名拉取，
这里用 GHCR。原来用的 `minio/minio` 已经从 Docker Hub 删除，Quay 上的同名镜像要登录才能拉。

存储测试用到的 S3 能力它都支持并照规范校验：SigV4 头签名（含带尾部校验和的 aws-chunked 流式上传）、
预签名 PUT 与 GET（AWSSDK 对自定义端点生成的是 SigV2 查询签名；含 `response-content-disposition`
覆盖）、对象的 Content-Type 与 Content-Disposition、ListObjectsV2，以及浏览器表单上传（POST Object）
的 SigV4 签名策略与 `content-length-range` 条件：超限时返回 400（EntityTooLarge），对象不落盘。

## Fields

### AccessKey \{#Kun_Testing_Containers_S3CompatibleServer_AccessKey}

根用户的访问密钥。

```csharp
public const string AccessKey = "kunadmin"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### SecretKey \{#Kun_Testing_Containers_S3CompatibleServer_SecretKey}

根用户的私有密钥。

```csharp
public const string SecretKey = "kunadmin-secret"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Endpoint \{#Kun_Testing_Containers_S3CompatibleServer_Endpoint}

S3 端点，形如 `http://127.0.0.1:端口/`。启动之后才有效。

```csharp
public string Endpoint { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Image \{#Kun_Testing_Containers_S3CompatibleServer_Image}

RustFS 1.0.6，多架构索引的摘要（linux/amd64、linux/arm64）。

```csharp
public static string Image { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Create() \{#Kun_Testing_Containers_S3CompatibleServer_Create}

构建容器，不启动。本机没有容器环境时，Testcontainers 在这里抛出消息含 Docker 的
[`ArgumentException`](https://learn.microsoft.com/dotnet/api/system.argumentexception)，调用方据此交给 [DockerRequirement.ReportUnavailable](/sdk/api/kun/kun-testing-containers/dockerrequirement.md#Kun_Testing_Containers_DockerRequirement_ReportUnavailable_System_String_)。

```csharp
public static S3CompatibleServer Create()
```

#### Returns

[S3CompatibleServer](/sdk/api/kun/kun-testing-containers/s3compatibleserver.md)

### DisposeAsync() \{#Kun_Testing_Containers_S3CompatibleServer_DisposeAsync}

```csharp
public ValueTask DisposeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### StartAsync(CancellationToken) \{#Kun_Testing_Containers_S3CompatibleServer_StartAsync_System_Threading_CancellationToken\_}

启动容器并等到服务就绪。拉不到镜像或起不来时照常抛出，不跳过。

```csharp
public Task StartAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
