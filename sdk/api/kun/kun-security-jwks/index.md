> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Security.Jwks

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[FileKunJwksProvider](/sdk/api/kun/kun-security-jwks/filekunjwksprovider.md)

Loads a JSON Web Key Set from a local file and caches it in memory.
The cache is invalidated when the file's last-write time changes, enabling
key rotation without service restart.

### Interfaces

[IKunJwksProvider](/sdk/api/kun/kun-security-jwks/ikunjwksprovider.md)

Provides the JSON Web Key Set used for S2S token signature verification.
