> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Plugin.Sdk

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[CapabilityAttribute](/sdk/api/kun/kun-plugin-sdk/capabilityattribute.md)

声明命令或查询的契约；源生成器验证并生成强类型能力定义。

[CapabilityBindingRegistration](/sdk/api/kun/kun-plugin-sdk/capabilitybindingregistration.md)

SDK 层的可逆能力注册，不依赖 platform 的 Capabilities Provider。

[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)

代码生成的能力声明；只有元数据，不承担请求认证、授权或安装状态判定。

[CapabilityContractException](/sdk/api/kun/kun-plugin-sdk/capabilitycontractexception.md)

能力契约在登记阶段被拒绝：带组件 ID、能力短名、出错位置与规则。同样的值写入 [`Data`](https://learn.microsoft.com/dotnet/api/system.exception.data)（含原因码 `INPUT_INVALID`），
供 scope 装载失败与管理面诊断读取；消息不含 Schema 或配置原文。

[CapabilitySourceAttribute](/sdk/api/kun/kun-plugin-sdk/capabilitysourceattribute.md)

引用与真实执行绑定共用的静态能力契约；不得通过实例化插件或处理器取得。

[CommandEvents](/sdk/api/kun/kun-plugin-sdk/commandevents.md)

SDK 生产执行端拥有的事件 Definition，供基础组件注册强制监听器。

[CommandInvocation](/sdk/api/kun/kun-plugin-sdk/commandinvocation.md)

一次已经绑定业务事务的调用；输入和身份不得从日志字符串化展开。

[CommandPolicies](/sdk/api/kun/kun-plugin-sdk/commandpolicies.md)

基础 Provider 的监听登记入口；执行器同时核验服务、实际进入与卸载取消。

[ComponentCodeMetadata](/sdk/api/kun/kun-plugin-sdk/componentcodemetadata.md)

代码派生的组件字段；手写版本、展示、策略和路径仍由唯一 plugin.json 保存。

[ComponentMetadataDeclarations](/sdk/api/kun/kun-plugin-sdk/componentmetadatadeclarations.md)

生成代码使用的声明校验；只读取静态声明，不执行插件。

[ComponentOwnerAttribute](/sdk/api/kun/kun-plugin-sdk/componentownerattribute.md)

在多组件程序集内为权限或事件声明指定唯一归属。

[ContributesAttribute](/sdk/api/kun/kun-plugin-sdk/contributesattribute.md)

声明具名扩展点贡献；服务键由指定类型的公开静态只读 ServiceKey 字段取得。

[DelegatePluginAttribute](/sdk/api/kun/kun-plugin-sdk/delegatepluginattribute.md)

稳定组件入口委托给已分类的实现构件；必须同时独立声明其完整服务输出。

[JobBindingRegistration](/sdk/api/kun/kun-plugin-sdk/jobbindingregistration.md)

local 作业的可逆注册；独立宿主与平台目录消费同一能力绑定。

[JobContract](/sdk/api/kun/kun-plugin-sdk/jobcontract.md)

绑定真实能力的不可变作业目录项；能力本身仍负责执行策略。

[JsonSchemaReferences](/sdk/api/kun/kun-plugin-sdk/jsonschemareferences.md)

源生成与实际 HTTP 描述共用的 JSON Schema 引用处理。

[KunCodeMetadataAttribute](/sdk/api/kun/kun-plugin-sdk/kuncodemetadataattribute.md)

编译程序集暴露的生成元数据入口；只用于可信项目的构建期工具。

[KunConfigSchemaAttribute](/sdk/api/kun/kun-plugin-sdk/kunconfigschemaattribute.md)

组件配置的代码声明；工具派生为清单所指的 configSchema 文件。

[KunJobAttribute](/sdk/api/kun/kun-plugin-sdk/kunjobattribute.md)

将已有能力请求声明为可持久调度的 local 作业；code 在组件内唯一。

[KunPluginAttribute](/sdk/api/kun/kun-plugin-sdk/kunpluginattribute.md)

声明组件的稳定 ID；同程序集允许多个不同 ID 的入口。

[ManifestErrors](/sdk/api/kun/kun-plugin-sdk/manifesterrors.md)

可信组件清单构建的错误目录；不改变市场准入或运行权限。

[ProvidesAttribute](/sdk/api/kun/kun-plugin-sdk/providesattribute.md)

委托入口的单项服务输出；直接 Apply 仍由语义分析派生，不使用此声明覆盖分析。

[SensitiveAttribute](/sdk/api/kun/kun-plugin-sdk/sensitiveattribute.md)

将输入属性标为敏感；生成契约在实际 JSON 字段上添加 sensitive，而非把原值写入审计。

[SharedEventsAttribute](/sdk/api/kun/kun-plugin-sdk/sharedeventsattribute.md)

引用另一个组件拥有的共享事件契约；不共享权限或其它元数据。

### Interfaces

[ICapabilityContractCheck](/sdk/api/kun/kun-plugin-sdk/icapabilitycontractcheck.md)

能力登记期的契约检查端口。平台宿主登记实现后，[CapabilityBindingRegistration.ProvideCapability](/sdk/api/kun/kun-plugin-sdk/capabilitybindingregistration.md#Kun_Plugin_Sdk_CapabilityBindingRegistration_ProvideCapability__2_Kun_Runtime_IPluginContext_)
在登记前调用它：生成目录条目并预编译输入校验器，不合格的契约在登记阶段拒绝，不留到首次调用。platform profile 宿主必须登记
（`AddKunCapabilities()`）；未登记时不做此检查，独立宿主没有通用执行入口，不需要它。

[ICapabilityRequest\<TResponse>](/sdk/api/kun/kun-plugin-sdk/icapabilityrequest-1.md)

真实 Mediator 请求与生成契约的类型关联。

[IComponentCodeMetadataFactory](/sdk/api/kun/kun-plugin-sdk/icomponentcodemetadatafactory.md)

生成代码与清单工具之间的构建期协议，不实例化业务插件。

[IExecutableCapability](/sdk/api/kun/kun-plugin-sdk/iexecutablecapability.md)

已挂载代码能力的真实执行端口；目录读取契约，调用仍经实际 Mediator 和基础策略。

[IJobRequest\<TResponse>](/sdk/api/kun/kun-plugin-sdk/ijobrequest-1.md)

源生成器为 KunJob 请求实现的静态声明，不使用反射寻找处理器。

### Enums

[CommandPolicyKind](/sdk/api/kun/kun-plugin-sdk/commandpolicykind.md)

基础组合包的三个强制执行点。
