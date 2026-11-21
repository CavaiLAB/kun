> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Identity.Abstractions.Session

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Identity` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Identity`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.Identity)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[SessionData](/sdk/api/kun/kun-identity-abstractions-session/sessiondata.md)

Serialized payload held by [ISessionStore](/sdk/api/kun/kun-identity-abstractions-session/isessionstore.md) implementations.
The session cookie carries only an opaque session id; the record
below is what that id resolves to.

### Interfaces

[ISessionRegistry](/sdk/api/kun/kun-identity-abstractions-session/isessionregistry.md)

Atomic session lifecycle operations. Renewing a revoked session must never recreate it.

[ISessionStore](/sdk/api/kun/kun-identity-abstractions-session/isessionstore.md)

Backend for cookie-based session data. Implementations ship in
`Kun.Identity.Implementations.Session` (in-memory for tests /
single-process dev; Redis-backed via `Kun.Caching` for
production). Implementations must be safe for concurrent use from
multiple request threads.
