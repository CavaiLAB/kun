# Kun.Authorization.AspNetCore

HTTP 与 gRPC 使用同一组权限点和授权执行端口。使用 `RequireKunPermission("sample.crm:customer:read")` 声明端点所需的每个权限。权限点为 `<plugin-id>:<resource>:<action>`，组件 ID 包含点；无效声明在映射时抛出 ArgumentException。

## 普通宿主

宿主登记自己的 `IPermissionChecker`，再调用 `services.AddKunAuthorization()`。库模式从已认证的主身份读取主体、租户和类型，兼容 Cookie。缺少判定器或带委托链时拒绝。启动日志表明库模式不写决策审计、不比对身份修订。

## 组件宿主

调用 `services.AddKunAuthorization(http => …)`，回调返回当前请求的插件上下文。身份、可信租户边界与授权组件须先就绪。HTTP 将 `UseKunAuthorization()` 放在 `UseAuthorization()` 之后。平台模式使用身份快照和授权组件的判定审计，陈旧的租户身份修订会触发 401。每个宿主只能登记一种模式一次。

在 gRPC 服务中用 `services.AddKunGrpc(options => options.AddKunAuthorizationInterceptor())` 安装授权拦截器，也可只安装在给定服务的 GrpcServiceOptions 上。没有装配授权拦截器时，Kun.Grpc 拒绝执行标注了 RequiresPermission 的方法。
