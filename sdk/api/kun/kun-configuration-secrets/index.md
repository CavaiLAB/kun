> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Configuration.Secrets

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[KubernetesSecretLocation](/sdk/api/kun/kun-configuration-secrets/kubernetessecretlocation.md)

可信引导中的 Kubernetes 文件定位，不作为公开组件引用。

[MountedSecretStore](/sdk/api/kun/kun-configuration-secrets/mountedsecretstore.md)

读取受信任根目录中的 K8s 投射文件，支持根内的 ..data 原子轮换。

[SecretAccessPolicy](/sdk/api/kun/kun-configuration-secrets/secretaccesspolicy.md)

调用方的声明与审批策略，两者都必须允许访问。

[SecretBinding](/sdk/api/kun/kun-configuration-secrets/secretbinding.md)

公开引用到 Kubernetes 挂载文件的可信绑定。

[SecretResolverFactory](/sdk/api/kun/kun-configuration-secrets/secretresolverfactory.md)

不可变策略与按需文件读取的组合；不缓存已解析值。

[SecretResolverOptions](/sdk/api/kun/kun-configuration-secrets/secretresolveroptions.md)

只来自可信宿主引导，不从请求或模型输入建立授权。

[SecretServiceExtensions](/sdk/api/kun/kun-configuration-secrets/secretserviceextensions.md)

独立宿主与平台共用的安全解析装配。
