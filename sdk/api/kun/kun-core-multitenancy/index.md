> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Core.MultiTenancy

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Core` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Core`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Core)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[NullTenantContext](/sdk/api/kun/kun-core-multitenancy/nulltenantcontext.md)

[ITenantContext](/sdk/api/kun/kun-core-multitenancy/itenantcontext.md) implementation for non-multi-tenant hosts or background jobs
that operate without a tenant scope.

[NullTenantProvisioner](/sdk/api/kun/kun-core-multitenancy/nulltenantprovisioner.md)

[ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md) 的 no-op 默认实现，用于 Shared 策略或不需要租户生命周期
钩子的宿主。两个方法都立即返回 [`CompletedTask`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.completedtask)。

[TenantInfo](/sdk/api/kun/kun-core-multitenancy/tenantinfo.md)

不可变的租户身份元信息，作为 [ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md) 等生命周期 API 的入参。

### Interfaces

[ITenantContext](/sdk/api/kun/kun-core-multitenancy/itenantcontext.md)

租户上下文，从当前请求的认证信息（S2S JWT 的 tenant_id claim）中解析。

框架内所有需要按租户隔离的组件（EF Core 全局过滤器、缓存 Key、消息路由）
都应依赖此接口而非具体实现，以便在不同宿主（HTTP、gRPC、后台任务）中替换。

[ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md)

租户生命周期钩子：开户（[ITenantProvisioner.ProvisionAsync](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md#Kun_Core_MultiTenancy_ITenantProvisioner_ProvisionAsync_Kun_Core_MultiTenancy_TenantInfo_System_Threading_CancellationToken_)）与注销（[ITenantProvisioner.DeprovisionAsync](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md#Kun_Core_MultiTenancy_ITenantProvisioner_DeprovisionAsync_System_Guid_System_Threading_CancellationToken_)）。

### Enums

[TenantIsolation](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md)

多租户数据隔离策略。数值用于宿主配置，小写 JSON 值用于租户契约。
