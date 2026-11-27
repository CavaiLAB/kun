> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class TenantGrpcClientInterceptor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Grpc` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Grpc`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Grpc)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Grpc.Interceptors](/sdk/api/kun/kun-grpc-interceptors/index.md)\
Assembly: Kun.Grpc.dll

从当前可信租户上下文添加 gRPC tenant_id 元数据；认证令牌由宿主的身份客户端另行提供。

```csharp
public sealed class TenantGrpcClientInterceptor : Interceptor
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
Interceptor ←
[TenantGrpcClientInterceptor](/sdk/api/kun/kun-grpc-interceptors/tenantgrpcclientinterceptor.md)

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

### TenantGrpcClientInterceptor(ITenantContext) \{#Kun_Grpc_Interceptors_TenantGrpcClientInterceptor__ctor_Kun_Core_MultiTenancy_ITenantContext\_}

从当前可信租户上下文添加 gRPC tenant_id 元数据；认证令牌由宿主的身份客户端另行提供。

```csharp
public TenantGrpcClientInterceptor(ITenantContext tenant)
```

#### Parameters

`tenant` ITenantContext

## Methods

### AsyncClientStreamingCall\<TRequest, TResponse>(ClientInterceptorContext\<TRequest, TResponse>, AsyncClientStreamingCallContinuation\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcClientInterceptor_AsyncClientStreamingCall__2_Grpc_Core_Interceptors_ClientInterceptorContext___0___1__Grpc_Core_Interceptors_Interceptor_AsyncClientStreamingCallContinuation___0___1\_\_}

Intercepts an asynchronous invocation of a client streaming call.

```csharp
public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`context` ClientInterceptorContext\<TRequest, TResponse>

The `ClientInterceptorContext`
associated with the current invocation.

`continuation` Interceptor.AsyncClientStreamingCallContinuation\<TRequest, TResponse>

The callback that continues the invocation process.
This can be invoked zero or more times by the interceptor.
The interceptor can invoke the continuation passing the given
context argument, or substitute as it sees fit.

#### Returns

AsyncClientStreamingCall\<TRequest, TResponse>

An instance of `AsyncClientStreamingCall`
representing an asynchronous client-streaming invocation.
The interceptor can simply return the return value of the
continuation delegate passed to it intact, or construct its
own substitute as it sees fit.

#### Type Parameters

`TRequest`

`TResponse`

### AsyncDuplexStreamingCall\<TRequest, TResponse>(ClientInterceptorContext\<TRequest, TResponse>, AsyncDuplexStreamingCallContinuation\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcClientInterceptor_AsyncDuplexStreamingCall__2_Grpc_Core_Interceptors_ClientInterceptorContext___0___1__Grpc_Core_Interceptors_Interceptor_AsyncDuplexStreamingCallContinuation___0___1\_\_}

Intercepts an asynchronous invocation of a duplex streaming call.

```csharp
public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`context` ClientInterceptorContext\<TRequest, TResponse>

The `ClientInterceptorContext`
associated with the current invocation.

`continuation` Interceptor.AsyncDuplexStreamingCallContinuation\<TRequest, TResponse>

The callback that continues the invocation process.
This can be invoked zero or more times by the interceptor.
The interceptor can invoke the continuation passing the given
context argument, or substitute as it sees fit.

#### Returns

AsyncDuplexStreamingCall\<TRequest, TResponse>

An instance of `AsyncDuplexStreamingCall`
representing an asynchronous duplex-streaming invocation.
The interceptor can simply return the return value of the
continuation delegate passed to it intact, or construct its
own substitute as it sees fit.

#### Type Parameters

`TRequest`

`TResponse`

### AsyncServerStreamingCall\<TRequest, TResponse>(TRequest, ClientInterceptorContext\<TRequest, TResponse>, AsyncServerStreamingCallContinuation\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcClientInterceptor_AsyncServerStreamingCall__2___0_Grpc_Core_Interceptors_ClientInterceptorContext___0___1__Grpc_Core_Interceptors_Interceptor_AsyncServerStreamingCallContinuation___0___1\_\_}

Intercepts an asynchronous invocation of a streaming remote call.

```csharp
public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`request` TRequest

The request message of the invocation.

`context` ClientInterceptorContext\<TRequest, TResponse>

The `ClientInterceptorContext`
associated with the current invocation.

`continuation` Interceptor.AsyncServerStreamingCallContinuation\<TRequest, TResponse>

The callback that continues the invocation process.
This can be invoked zero or more times by the interceptor.
The interceptor can invoke the continuation passing the given
request value and context arguments, or substitute them as it sees fit.

#### Returns

AsyncServerStreamingCall\<TResponse>

An instance of `AsyncServerStreamingCall`
representing an asynchronous server-streaming invocation.
The interceptor can simply return the return value of the
continuation delegate passed to it intact, or construct its
own substitute as it sees fit.

#### Type Parameters

`TRequest`

`TResponse`

### AsyncUnaryCall\<TRequest, TResponse>(TRequest, ClientInterceptorContext\<TRequest, TResponse>, AsyncUnaryCallContinuation\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcClientInterceptor_AsyncUnaryCall__2___0_Grpc_Core_Interceptors_ClientInterceptorContext___0___1__Grpc_Core_Interceptors_Interceptor_AsyncUnaryCallContinuation___0___1\_\_}

Intercepts an asynchronous invocation of a simple remote call.

```csharp
public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncUnaryCallContinuation<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`request` TRequest

The request message of the invocation.

`context` ClientInterceptorContext\<TRequest, TResponse>

The `ClientInterceptorContext`
associated with the current invocation.

`continuation` Interceptor.AsyncUnaryCallContinuation\<TRequest, TResponse>

The callback that continues the invocation process.
This can be invoked zero or more times by the interceptor.
The interceptor can invoke the continuation passing the given
request value and context arguments, or substitute them as it sees fit.

#### Returns

AsyncUnaryCall\<TResponse>

An instance of `AsyncUnaryCall`
representing an asynchronous unary invocation.
The interceptor can simply return the return value of the
continuation delegate passed to it intact, or construct its
own substitute as it sees fit.

#### Type Parameters

`TRequest`

`TResponse`

### BlockingUnaryCall\<TRequest, TResponse>(TRequest, ClientInterceptorContext\<TRequest, TResponse>, BlockingUnaryCallContinuation\<TRequest, TResponse>) \{#Kun_Grpc_Interceptors_TenantGrpcClientInterceptor_BlockingUnaryCall__2___0_Grpc_Core_Interceptors_ClientInterceptorContext___0___1__Grpc_Core_Interceptors_Interceptor_BlockingUnaryCallContinuation___0___1\_\_}

Intercepts a blocking invocation of a simple remote call.

```csharp
public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.BlockingUnaryCallContinuation<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
```

#### Parameters

`request` TRequest

The request message of the invocation.

`context` ClientInterceptorContext\<TRequest, TResponse>

The `ClientInterceptorContext`
associated with the current invocation.

`continuation` Interceptor.BlockingUnaryCallContinuation\<TRequest, TResponse>

The callback that continues the invocation process.
This can be invoked zero or more times by the interceptor.
The interceptor can invoke the continuation passing the given
request value and context arguments, or substitute them as it sees fit.

#### Returns

TResponse

The response message of the current invocation.
The interceptor can simply return the return value of the
continuation delegate passed to it intact, or an arbitrary
value as it sees fit.

#### Type Parameters

`TRequest`

`TResponse`
