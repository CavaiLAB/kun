> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Testing.Domain

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.Testing)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[DomainEventAssertions](/sdk/api/kun/kun-testing-domain/domaineventassertions.md)

Fluent assertion extensions for [IAggregateRoot](/sdk/api/kun/kun-core-domain/iaggregateroot.md) domain-
event collections. On mismatch, throws
[KunAssertionException](/sdk/api/kun/kun-testing/kunassertionexception.md) with a precise message per the
ADR-012 §Error-message contract.
