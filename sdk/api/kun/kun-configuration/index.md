> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Configuration

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Configuration.Apollo](/sdk/api/kun/kun-configuration-apollo/index.md)

[Kun.Configuration.Attributes](/sdk/api/kun/kun-configuration-attributes/index.md)

[Kun.Configuration.Extensions](/sdk/api/kun/kun-configuration-extensions/index.md)

[Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)

### Classes

[ConfigDefinition](/sdk/api/kun/kun-configuration/configdefinition.md)

平台/服务级只读配置 seam；租户设置由 Tenancy 管理。

[ConfigErrors](/sdk/api/kun/kun-configuration/configerrors.md)

配置组件的稳定错误词汇。

[ConfigEvents](/sdk/api/kun/kun-configuration/configevents.md)

配置组件的事件与审计词汇；只含稳定标识，不含任何配置值。

[ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)

重载失败告警（[ConfigEvents.ReloadFailedType](/sdk/api/kun/kun-configuration/configevents.md#Kun_Configuration_ConfigEvents_ReloadFailedType)）：options 类型与名称、配置节、失败条数与错误码；
不含任何配置值，也不含校验消息原文（自定义校验器的消息可能带值）。

[ConfigRuntimeEvents](/sdk/api/kun/kun-configuration/configruntimeevents.md)

Config seam 的进程内扩展事件（组件模型规格 §6.4）。

[FeatureFlagDraft](/sdk/api/kun/kun-configuration/featureflagdraft.md)

草稿操作输入；目标环境、应用、namespace、操作员与秘密引用均由可信宿主固定。

[FeatureFlagDraftResult](/sdk/api/kun/kun-configuration/featureflagdraftresult.md)

草稿写入回执，不含 Token 或规则内容。

[FeatureFlagManagement](/sdk/api/kun/kun-configuration/featureflagmanagement.md)

可选平台管理组件的声明；只保存 Apollo 草稿，发布仍由 Portal 操作员完成。

[FeatureFlagPolicy](/sdk/api/kun/kun-configuration/featureflagpolicy.md)

固定灰度规则；没有表达式、外部地址或任意配置键。

[FeatureFlagRule](/sdk/api/kun/kun-configuration/featureflagrule.md)

从快照读取的一份开关规则。

[FeatureFlagRules](/sdk/api/kun/kun-configuration/featureflagrules.md)

解析并校验配置快照中的特性开关规则。

[OptionsReloadFailure](/sdk/api/kun/kun-configuration/optionsreloadfailure.md)

一次被拒绝的候选配置，仅包含登记信息和失败条数。

[OptionsSchema](/sdk/api/kun/kun-configuration/optionsschema.md)

由源生成 JSON 元数据导出的配置契约，不包含任何运行时配置值。

[SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)

一次解析审计，只含引用名、调用方、租户和结果，不含值。

[SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)

不含原值的配置扫描诊断。

[SecretErrors](/sdk/api/kun/kun-configuration/secreterrors.md)

秘密解析的统一错误，不把输入值或物理路径放进错误文案。

[SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)

与 policy.permissions.secrets 条目对应的权限，不因声明本身而自动获批。

[SecretReference](/sdk/api/kun/kun-configuration/secretreference.md)

公开的规范引用；Kubernetes 定位只在受信任存储绑定中使用。

[SecretSafety](/sdk/api/kun/kun-configuration/secretsafety.md)

配置契约级敏感数据扫描，可供发布门禁与 Apollo 快照验证共用。

[SecretValue](/sdk/api/kun/kun-configuration/secretvalue.md)

密钥的受控内存副本；默认打印和 JSON 序列化不暴露内容。

[SecretValueJsonConverter](/sdk/api/kun/kun-configuration/secretvaluejsonconverter.md)

禁止把已解析密钥当作普通 JSON 数据。

### Interfaces

[IConfigService](/sdk/api/kun/kun-configuration/iconfigservice.md)

供受信任代码读取配置；不提供写入入口，也不自动公开为模型工具。

[IFeatureFlagDrafts](/sdk/api/kun/kun-configuration/ifeatureflagdrafts.md)

平台专用草稿写入；实现每次重新检查已认证主体、确认、权限与审计。

[IFeatureFlags](/sdk/api/kun/kun-configuration/ifeatureflags.md)

只读租户灰度求值；不读取权益，不改变租户 scope 的组件挂载。

[IKunConfigurationValidator\<TOptions>](/sdk/api/kun/kun-configuration/ikunconfigurationvalidator-1.md)

Reflection-free options validator — the AOT-safe alternative to
DataAnnotations. Register one per options type via
`services.AddSingleton<IKunConfigurationValidator<T>, MyValidator>()`
and `AddKunOptions<T>` will merge the results with any
DataAnnotations-driven validation.

[ISecretAccessObserver](/sdk/api/kun/kun-configuration/isecretaccessobserver.md)

可替换的审计接收端；写入失败不得返回密钥。

[ISecretResolver](/sdk/api/kun/kun-configuration/isecretresolver.md)

已绑定调用方权限的按需密钥解析端口。

[ISecretResolverFactory](/sdk/api/kun/kun-configuration/isecretresolverfactory.md)

宿主根据已审批策略创建解析器；平台调用方身份取自真实插件上下文。
