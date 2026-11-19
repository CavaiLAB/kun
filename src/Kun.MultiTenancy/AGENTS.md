# Kun.MultiTenancy

通过 `AddKunMultiTenancy()` 登记租户上下文。在 HTTP 管道中先执行 `UseAuthentication()`，再执行 `UseKunMultiTenancy()`。租户入口从已认证主身份读取唯一的 `tenant_id`，使用 D 格式的非空 UUID。

平台宿主可显式选择 `TenantRequestMode.PlatformPrincipalAllowed`。平台身份的令牌包含 JSON null 租户声明，匿名路由由宿主的授权策略处理。两者都在空作用域内运行。

后台任务通过 `ITenantContext.BeginScope` 设置工作单元的租户，释放时还原上一个作用域。事件和元数据传播使用 `ITenantContextPropagator`。EF 与 RLS 的登记见 `docs/security/multi-tenancy-rls.md`。
