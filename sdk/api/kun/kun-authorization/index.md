> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Authorization

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Authorization` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Authorization`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Authorization)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[AuthorizationSubject](/sdk/api/kun/kun-authorization/authorizationsubject.md)

统一授权主体；权限集合不从令牌直接推导。

[FieldDecision](/sdk/api/kun/kun-authorization/fielddecision.md)

字段可读、可写与需要脱敏的集合。

[PermissionDecision](/sdk/api/kun/kun-authorization/permissiondecision.md)

可解释判定；Allow=false 是缺省语义。

[PermissionRequest](/sdk/api/kun/kun-authorization/permissionrequest.md)

权限点与可选资源实例；Permission 使用稳定的资源动作编码。

[RequiresPermissionAttribute](/sdk/api/kun/kun-authorization/requirespermissionattribute.md)

HTTP/gRPC 端点的静态权限元数据。

### Interfaces

[IDataPermissionFilter](/sdk/api/kun/kun-authorization/idatapermissionfilter.md)

本地数据过滤端口，不依赖 EF 包或具体数据库。

[IFieldPolicy](/sdk/api/kun/kun-authorization/ifieldpolicy.md)

字段权限端口；输出给服务端投影与写入白名单。

[IPermissionChecker](/sdk/api/kun/kun-authorization/ipermissionchecker.md)

权限判定端口；框架不提供 RBAC、ABAC 或持久化引擎。

[IPermissionEnforcementInterceptor](/sdk/api/kun/kun-authorization/ipermissionenforcementinterceptor.md)

标识在调用 gRPC 处理器前执行端点权限要求的拦截器。

[IPermissionEnforcementRequirement](/sdk/api/kun/kun-authorization/ipermissionenforcementrequirement.md)

标识端点授权策略里执行某个权限点的要求。宿主的权限回调给端点加上这样的要求，构造端点时据此核对权限点确实接上了执行点；
自定义执行点让自己的授权要求实现本接口。

[IPermissionProtectedRequest](/sdk/api/kun/kun-authorization/ipermissionprotectedrequest.md)

命令在编译期声明全部所需权限，避免从用户提交的 JSON 改写权限点。
