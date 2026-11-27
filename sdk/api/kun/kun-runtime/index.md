> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Runtime

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[KunEvent\<TPayload, TResult>](/sdk/api/kun/kun-runtime/kunevent-2.md)

Definition 定义的事件。TResult 是整个分发的结果；扇出与按序模式用 IReadOnlyList\<T>。

[KunEvent](/sdk/api/kun/kun-runtime/kunevent.md)

事件定义工厂。

[KunEventAttribute](/sdk/api/kun/kun-runtime/kuneventattribute.md)

标注事件字段或属性的分发模式；文档生成器校验它与事件值及注册调用一致，不影响运行时求值。

[ParallelDispatchException\<TResult>](/sdk/api/kun/kun-runtime/paralleldispatchexception-1.md)

所有并行监听器结束后报告失败；Partial 仅含成功结果，顺序与监听器一致。

[PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)

装载输入；Provides 来自清单，不通过试运行 Apply 推测。

[PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

已由清单与配置装配器解析的插件条目。

[PluginLoader](/sdk/api/kun/kun-runtime/pluginloader.md)

按服务依赖装载插件。代码实例由宿主提供，不加载动态程序集、不读数据库。

[Registration](/sdk/api/kun/kun-runtime/registration.md)

目录与执行共用的注册描述；Name 为空表示键本身，具名 seam 使用独立名称。

[ServiceNotReadyException](/sdk/api/kun/kun-runtime/servicenotreadyexception.md)

当前服务尚不可见。不存在与被过滤的服务返回相同错误。

### Structs

[ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

两层扁平作用域：[ScopeKey.Global](/sdk/api/kun/kun-runtime/scopekey.md#Kun_Runtime_ScopeKey_Global)，或「种类 + 非空 id」；作用域之间没有继承关系。
运行时只查语法（种类是小写字母开头、由小写字母数字与连字符组成的名字），种类有哪些由平台档定。

[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

Definition 拥有的服务键；具名注册可用 Key:name 声明就绪依赖。

### Interfaces

[IKunPlugin\<TConfig>](/sdk/api/kun/kun-runtime/ikunplugin-1.md)

类型化配置接口；Schema 校验由清单装配器在调用前完成。

[IKunPlugin](/sdk/api/kun/kun-runtime/ikunplugin.md)

插件的最小接口。

[IPluginContext](/sdk/api/kun/kun-runtime/iplugincontext.md)

插件的服务、效果与作用域入口。

[IRegistrationHandle\<TService>](/sdk/api/kun/kun-runtime/iregistrationhandle-1.md)

Provide 返回的注册句柄：Dispose 撤销注册，可重复调用；Replace 原子替换同一注册的实例。

### Enums

[DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

进程内扩展事件的分发模式。

[PluginState](/sdk/api/kun/kun-runtime/pluginstate.md)

某次 Apply 的状态；失败保留原异常供管理面查询。
