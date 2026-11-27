> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GrpcErrorTrailers

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Grpc` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Grpc`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Grpc)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Grpc](/sdk/api/kun/kun-grpc/index.md)\
Assembly: Kun.Grpc.dll

以 HTTP 错误契约的同一份详情生成 gRPC 响应尾部。

```csharp
public static class GrpcErrorTrailers
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GrpcErrorTrailers](/sdk/api/kun/kun-grpc/grpcerrortrailers.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Create(ErrorDetail) \{#Kun_Grpc_GrpcErrorTrailers_Create_Kun_Errors_ErrorDetail\_}

返回机器可读错误码与 UTF-8 编码的 ErrorDetail JSON。

```csharp
public static Metadata Create(ErrorDetail detail)
```

#### Parameters

`detail` ErrorDetail

#### Returns

Metadata
