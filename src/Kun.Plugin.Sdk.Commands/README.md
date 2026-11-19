# Kun.Plugin.Sdk.Commands

插件开发包的平台档一半：能力命令的执行管道。声明、生成契约与命令策略协议在内核档的 [Kun.Plugin.Sdk](../Kun.Plugin.Sdk/README.md)，那里的「实际命令执行」与「开户初始化 hook」两节是这里的主文档。

- `AddKunCapability<TRequest, TResponse, THandler, TContext>`：经权限行为原子登记能力命令。每次 Send 在新的 DI scope 里打开业务数据会话；外层 SERIALIZABLE 事务包住 `command/dispatching` 与保存，审计追加完成后才提交；宿主启动时检查当前主体端口没有落到匿名兜底。
- `CommandDataSession<TContext>`：同一 scope 里共享的业务数据会话与 Outbox，只能由管道绑定，查询不能写集成事件。
- `HookCapabilityRegistration.ExposeCapabilityHook`：把能力命令挂成声明式表面的钩子处理器，开户初始化 hook 用它。

依赖数据、授权、事件与表面的平台档契约，所以记平台档；平台组件都经它登记能力命令，记契约，与 `Kun.Plugin.Sdk.AspNetCore` 同理。
