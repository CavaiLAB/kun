> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class TenantGrpcServerInterceptor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Grpc` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Grpc`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Grpc)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Grpc.Interceptors](/sdk/api/kun/kun-grpc-interceptors/index.md)\
Assembly: Kun.Grpc.dll

租户 gRPC 服务入口；宿主先完成 JWT 认证，再核对传播元数据，覆盖全部四种调用形态。

```csharp
public sealed class TenantGrpcServerInterceptor : Interceptor
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
Interceptor ←
[TenantGrpcServerInterceptor](/sdk/api/kun/kun-grpc-interceptors/tenantgrpcserverinterceptor.md)

#### Inherited Members

Interceptor.BlockingUnaryCall\<TRequest, TResponse>(TRequest, ClientInterceptorContext\<TRequest, TResponse>, Interceptor.BlockingUnaryCallContinuation\<TRequest, TResponse>),
Interceptor.AsyncUnaryCall\<TRequest, TResponse>(TRequest, ClientInterceptorContext\<TRequest, TResponse>, Interceptor.AsyncUnaryCallContinuation\<TRequest, TResponse>),
Interceptor.AsyncServerStreamingCall\<TRequest, TResponse>(TRequest, ClientInterceptorContext\<TRequest, TResponse>, Interceptor.AsyncServerStreamingCallContinuation\<TRequest, TResponse>),
Interceptor.AsyncClientStreamingCall\<TRequest, TResponse>(ClientInterceptorContext\<TRequest, TResponse>, Interceptor.AsyncClientStreamingCallContinuation\<TRequest, TResponse>),
Interceptor.AsyncDuplexStreamingCall\<TRequest, TResponse>(ClientInterceptorContext\<TRequest, TResponse>, Interceptor.AsyncDuplexStreamingCallContinuation\<TRequest, TResponse>),
Interceptor.UnaryServerHandler\<TRequest, TResponse>(TRequest, ServerCallContext, UnaryServerMethod\<TRequest, TResponse>),
Interceptor.ClientStreamingServerHandler\<TRequest, TResponse>(IAsyncStreamReader\<TRequest>, ServerCallContext, ClientStreamingServerMethod\<TRequest, TResponse>),
Interceptor.ServerStreamingServerHandler\<TRequest, TResponse>(TRequest, IServerStreamWriter\<TResponse>, ServerCallContext, ServerStreamingServerMethod\<TRequest, TResponse>),
Interceptor.DuplexStreamingServerHandler\<TRequest, TResponse>(IAsyncStreamReader\<TRequest>, IServerStreamWriter\<TResponse>, ServerCallContext, DuplexStreamingServerMethod\<TRequest, TResponse>),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### TenantGrpcServerInterceptor(ITenantContext, TenantRequestMode) \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor__ctor_Kun_Core_MultiTenancy_ITenantContext_Kun_MultiTenancy_TenantRequestMode\_}

租户 gRPC 服务入口；宿主先完成 JWT 认证，再核对传播元数据，覆盖全部四种调用形态。

```csharp
[ActivatorUtilitiesConstructor]
public TenantGrpcServerInterceptor(ITenantContext tenant, TenantRequestMode mode = TenantRequestMode.TenantRequired)
```

#### Parameters

`tenant` ITenantContext

`mode` TenantRequestMode

### TenantGrpcServerInterceptor(ITenantContext) \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor__ctor_Kun_Core_MultiTenancy_ITenantContext\_}

为严格租户 RPC 要求携带与认证身份匹配的传播元数据。

```csharp
public TenantGrpcServerInterceptor(ITenantContext tenant)
```

#### Parameters

`tenant` ITenantContext

## Fields

### ErrorDetailTrailer \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor_ErrorDetailTrailer}

携带冻结 ErrorDetail JSON 的二进制响应 trailer。

```csharp
public const string ErrorDetailTrailer = "qian-error-bin"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### ClientStreamingServerHandler\<TRequest, TResponse>(IAsyncStreamReader\<TRequest>, ServerCallContext, ClientStreamingServerMethod\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor_ClientStreamingServerHandler__2_Grpc_Core_IAsyncStreamReader___0__Grpc_Core_ServerCallContext_Grpc_Core_ClientStreamingServerMethod___0___1\_\_}

Server-side handler for intercepting client streaming call.

```csharp
public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`requestStream` IAsyncStreamReader\<TRequest>

The request stream of the incoming invocation.

`context` ServerCallContext

An instance of `ServerCallContext` representing
the context of the invocation.

`continuation` ClientStreamingServerMethod\<TRequest, TResponse>

A delegate that asynchronously proceeds with the invocation, calling
the next interceptor in the chain, or the service request handler,
in case of the last interceptor and return the response value of
the RPC. The interceptor can choose to call it zero or more times
at its discretion.

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<TResponse>

A future representing the response value of the RPC. The interceptor
can simply return the return value from the continuation intact,
or an arbitrary response value as it sees fit. The interceptor has
the ability to wrap or substitute the request stream when calling
the continuation.

#### Type Parameters

`TRequest`

Request message type for this method.

`TResponse`

Response message type for this method.

### DuplexStreamingServerHandler\<TRequest, TResponse>(IAsyncStreamReader\<TRequest>, IServerStreamWriter\<TResponse>, ServerCallContext, DuplexStreamingServerMethod\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor_DuplexStreamingServerHandler__2_Grpc_Core_IAsyncStreamReader___0__Grpc_Core_IServerStreamWriter___1__Grpc_Core_ServerCallContext_Grpc_Core_DuplexStreamingServerMethod___0___1\_\_}

Server-side handler for intercepting bidirectional streaming calls.

```csharp
public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`requestStream` IAsyncStreamReader\<TRequest>

The request stream of the incoming invocation.

`responseStream` IServerStreamWriter\<TResponse>

The response stream of the incoming invocation.

`context` ServerCallContext

An instance of `ServerCallContext` representing
the context of the invocation.

`continuation` DuplexStreamingServerMethod\<TRequest, TResponse>

A delegate that asynchronously proceeds with the invocation, calling
the next interceptor in the chain, or the service request handler,
in case of the last interceptor and the interceptor can choose to
call it zero or more times at its discretion. The interceptor has
the ability to wrap or substitute the request and response streams
when calling the continuation.

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

#### Type Parameters

`TRequest`

Request message type for this method.

`TResponse`

Response message type for this method.

### ServerStreamingServerHandler\<TRequest, TResponse>(TRequest, IServerStreamWriter\<TResponse>, ServerCallContext, ServerStreamingServerMethod\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor_ServerStreamingServerHandler__2___0_Grpc_Core_IServerStreamWriter___1__Grpc_Core_ServerCallContext_Grpc_Core_ServerStreamingServerMethod___0___1\_\_}

Server-side handler for intercepting server streaming call.

```csharp
public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`request` TRequest

The request value of the incoming invocation.

`responseStream` IServerStreamWriter\<TResponse>

The response stream of the incoming invocation.

`context` ServerCallContext

An instance of `ServerCallContext` representing
the context of the invocation.

`continuation` ServerStreamingServerMethod\<TRequest, TResponse>

A delegate that asynchronously proceeds with the invocation, calling
the next interceptor in the chain, or the service request handler,
in case of the last interceptor and the interceptor can choose to
call it zero or more times at its discretion. The interceptor has
the ability to wrap or substitute the request value and the response stream
when calling the continuation.

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

#### Type Parameters

`TRequest`

Request message type for this method.

`TResponse`

Response message type for this method.

### UnaryServerHandler\<TRequest, TResponse>(TRequest, ServerCallContext, UnaryServerMethod\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcServerInterceptor_UnaryServerHandler__2___0_Grpc_Core_ServerCallContext_Grpc_Core_UnaryServerMethod___0___1\_\_}

Server-side handler for intercepting and incoming unary call.

```csharp
public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`request` TRequest

The request value of the incoming invocation.

`context` ServerCallContext

An instance of `ServerCallContext` representing
the context of the invocation.

`continuation` UnaryServerMethod\<TRequest, TResponse>

A delegate that asynchronously proceeds with the invocation, calling
the next interceptor in the chain, or the service request handler,
in case of the last interceptor and return the response value of
the RPC. The interceptor can choose to call it zero or more times
at its discretion.

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<TResponse>

A future representing the response value of the RPC. The interceptor
can simply return the return value from the continuation intact,
or an arbitrary response value as it sees fit.

#### Type Parameters

`TRequest`

Request message type for this method.

`TResponse`

Response message type for this method.
