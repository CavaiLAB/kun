> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Storage.Local

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Storage.Local` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Storage.Local`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Storage.Local)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Storage.Local.DependencyInjection](/sdk/api/kun/kun-storage-local-dependencyinjection/index.md)

### Classes

[LocalFileStorageOptions](/sdk/api/kun/kun-storage-local/localfilestorageoptions.md)

Configures [LocalFileSystemStorage](/sdk/api/kun/kun-storage-local/localfilesystemstorage.md).

[LocalFileSystemStorage](/sdk/api/kun/kun-storage-local/localfilesystemstorage.md)

[IFileStorage](/sdk/api/kun/kun-storage/ifilestorage.md) implementation that stores objects on the local filesystem.
Intended for development, tests, and single-node deployments. Presigned URLs are
developer-visible query-string-token URLs — validation is host responsibility.

[LocalSignedTransfers](/sdk/api/kun/kun-storage-local/localsignedtransfers.md)

为本地存储签发和执行限时上传、下载请求。键的用途由宿主指定。

[LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)

直下内容：响应头取自签名参数与存储时的嗅探类型。

[LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)

一次直传 / 直下请求：对象键取自路径，查询参数与请求头原样传入，由网关校验签名。

### Enums

[LocalTransferWriteResult](/sdk/api/kun/kun-storage-local/localtransferwriteresult.md)

直传结果，宿主映射为 HTTP 状态码。
