# 授权 HTTP 与 gRPC 适配器

- 宿主装配见 [README.md](README.md)。角色、权限目录与审计执行端口见 [权限提供者文档](../Kun.Authorization.Rbac/README.md)。
- 平台模式从已认证主身份和 `ICurrentUser` 快照读取主体，将判定、身份修订与决策审计交给 `IAuthzEnforcement`。组件未就绪时返回 404。权限目录的内容来自 `IAuthzService.ListEffectivePermissionsAsync`。
- 库模式从已认证主身份读取主体并调用宿主提供的 `IPermissionChecker`。宿主启动时记录此模式的审计和修订范围。带组件绑定的端点要求平台模式。两种模式的注册互斥。
- S1 拒绝带委托链和 Agent 的请求。HTTP 端点用 `RequireKunPermission` 声明权限点，gRPC 方法用 `[RequiresPermission]`。按调用者可见性在处理器内判定的平台表面使用 `PermissionEnforcedInHandlerAttribute(reason)` 写明理由。
- HTTP 入口用 ErrorDetail 表达 `AUTHZ_DENIED`（403）、`UNAUTHENTICATED`（401）和 `NOT_FOUND`（404）。gRPC 用对应状态和 `kun-error-code`、`qian-error-bin` 尾部返回同一错误。原因码留在决策审计。
