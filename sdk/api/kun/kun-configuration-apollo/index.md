> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Configuration.Apollo

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Apollo` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Apollo`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Apollo)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[ApolloConfigurationDescription](/sdk/api/kun/kun-configuration-apollo/apolloconfigurationdescription.md)

已装配的 Apollo 配置源与其脱敏的发布记录。

[ApolloConfigurationExtensions](/sdk/api/kun/kun-configuration-apollo/apolloconfigurationextensions.md)

显式添加 Apollo；源顺序由宿主按 appsettings、Apollo、Secret、环境变量、命令行安排。

[ApolloConfigurationProvider](/sdk/api/kun/kun-configuration-apollo/apolloconfigurationprovider.md)

通过公开协议获取服务级配置；无网络且无缓存时启动失败，不以空配置继续。

[ApolloConfigurationSource](/sdk/api/kun/kun-configuration-apollo/apolloconfigurationsource.md)

Apollo 标准 IConfiguration 源。

[ApolloOptions](/sdk/api/kun/kun-configuration-apollo/apollooptions.md)

平台／服务级 Apollo 引导配置；不承载租户级配置或明文业务密钥。

[ApolloReleaseRecord](/sdk/api/kun/kun-configuration-apollo/apolloreleaserecord.md)

一份快照的发布结果，仅含资源标识、变更数量、原因码与脱敏摘要。

[KunApolloConfigurationExtensions](/sdk/api/kun/kun-configuration-apollo/kunapolloconfigurationextensions.md)

固定配置源顺序，复用 Kun 的协议、缓存、签名与重载实现。

[KunConfigurationOptions](/sdk/api/kun/kun-configuration-apollo/kunconfigurationoptions.md)

配置装配参数；引导值来自宿主源，Apollo 不能改变自己的访问地址或访问密钥引用。
