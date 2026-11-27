> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Observability.Abstractions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Observability` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Observability`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Observability)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[KunObservabilityOptions](/sdk/api/kun/kun-observability-abstractions/kunobservabilityoptions.md)

Configuration for `AddKunObservability`. Every option has a safe
default; the zero-arg call yields a trace + metric + log pipeline with
ASP.NET Core + HttpClient + EF Core instrumentation enabled and OTLP
export pointed at `OTEL_EXPORTER_OTLP_ENDPOINT`.
