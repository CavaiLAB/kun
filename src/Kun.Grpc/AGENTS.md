# Kun.Grpc

## Purpose

Host-layer integration with gRPC (Grpc.AspNetCore). Registers server interceptors for tracing, authentication, tenant scope, permissions and error mapping:

- **`KunResultToRpcInterceptor`** — catches `ErrorException` (thrown by business code or by `Result<T>.EnsureSuccess()`) and translates it into an `RpcException` whose status code follows the HTTP status of the error code; the error code goes into `kun-error-code` and the UTF-8 HTTP error detail goes into `qian-error-bin`.
- **`KunServerAuthInterceptor`** — enforces that every request has an authenticated primary identity with a subject before entering a handler or reading a stream frame. Unary, client streaming, server streaming and duplex all use this boundary.
- **`KunTracingInterceptor`** — starts a diagnostic `Activity("grpc.server")` per RPC call, covering the full awaited lifetime of all four RPC shapes, tagged with `rpc.system=grpc`, `rpc.method`, `rpc.type` and `rpc.grpc.status_code`.

- `RequiredPermissionInterceptorGuard` checks endpoints with permission metadata. Each service must install an interceptor implementing `IPermissionEnforcementInterceptor`, globally or in its own gRPC options.

## Key Files

| File | Description |
|------|-------------|
| `Kun.Grpc.csproj` | `IsAotCompatible=true`; packable, so the public API gate applies. References `Kun.Core`, `Kun.Security`, `Grpc.AspNetCore`, `Microsoft.AspNetCore.App` framework ref. |
| `Interceptors/KunResultToRpcInterceptor.cs` | `internal sealed class`. Static helpers `MapStatus(int?)` and `ToRpcException(Error)` are testable via `InternalsVisibleTo`. |
| `Interceptors/KunServerAuthInterceptor.cs` | `internal sealed class`. Reads the subject from the authenticated primary identity. |
| `Interceptors/KunTracingInterceptor.cs` | `internal sealed class`. Uses `ActivitySource("Kun.Grpc")`. |
| `DependencyInjection/KunGrpcServiceCollectionExtensions.cs` | Public API: `AddKunGrpc(IServiceCollection, Action<GrpcServiceOptions>?)`. Registers tracing, authentication, tenant scope, the required-permission guard and result mapping in call order. |
| `PublicAPI.Unshipped.txt` | Tracks public surface: `KunGrpcServiceCollectionExtensions` + `AddKunGrpc`. |

## For AI Agents

### Working In This Directory

- The built-in interceptors are internal. Tests in `Kun.Grpc.Tests` access them via `InternalsVisibleTo`.
- `Kun.Security.DependencyInjection.KunSecurityServiceCollectionExtensions.AddKunSecurity()` must be called separately to wire up `JwtCurrentUser`.

### Testing Requirements

- Unit tests live in `tests/Kun.Grpc.Tests/ResultMappingTests.cs` — cover the HTTP status → `StatusCode` mapping for every platform error code.
- Integration tests with real gRPC channels live in `tests/Kun.IntegrationTests`.

### Common Patterns

```csharp
// In host startup:
builder.Services.AddKunSecurity();
builder.Services.AddKunGrpc();
```

The interceptors execute in registration order: Tracing → Auth → Tenant scope → Required-permission guard → ResultMapping (innermost).

The required-permission guard runs after authentication. It checks whether the host registered an `IPermissionEnforcementInterceptor` globally or for the specific service. A missing registration on a method marked `RequiresPermission` returns `PermissionDenied` before the handler reads a request frame.

## Dependencies

### Internal

- `Kun.Core` — `Result`; through it `Kun.Errors` — `Error`, `ErrorCode`, `ErrorException`.
- `Kun.MultiTenancy` — request scope and tenant propagation for the tenant interceptors.
- `Kun.Security` — `JwtCurrentUser` (registered separately via `AddKunSecurity()`).

### External

- `Grpc.AspNetCore` (server-side gRPC, `Grpc.Core.Interceptors.Interceptor`)
- `Microsoft.AspNetCore.App` (framework reference — `HttpContext`, `ServerCallContext.GetHttpContext()`)

The tenant interceptor in `AddKunGrpc` establishes a tenant scope from the authenticated claim. An explicit JSON null tenant identifies a platform request and establishes an empty scope. When a client provides tenant metadata, it must match the authenticated tenant. Registering `TenantGrpcServerInterceptor` on its own requires tenant metadata for every call.
