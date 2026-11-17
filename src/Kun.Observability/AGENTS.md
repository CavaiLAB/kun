# Kun.Observability

## Key Types

| Type | Purpose |
|------|---------|
| `Enrichment.KunBaggageKeys` | Public constants (`tenant.id`, `user.id`, `outbox.message_id`) — canonical baggage keys the framework mirrors onto Kun-sourced span tags when the enricher is enabled. |
| `Enrichment.KunBaggageActivityProcessor` | Internal `BaseProcessor<Activity>` that runs at `OnEnd` for Kun-owned sources and mirrors whitelisted baggage entries onto span tags. Opt-in via `KunObservabilityOptions.EnrichActivitiesFromBaggage`. See [ADR-013](../../docs/adr/kun/ADR-013-Kun.Observability-enricher.md). |
| `Enrichment.KunBaggageLogRecordProcessor` | Internal `BaseProcessor<LogRecord>` that runs at `OnEnd`, reads `Activity.Current?.Baggage`, and mirrors whitelisted keys onto `LogRecord.Attributes` *if not already set by the log statement*. Opt-in via `KunObservabilityOptions.EnrichLogsFromBaggage`. See [ADR-013 §F1 resolution](../../docs/adr/kun/ADR-013-Kun.Observability-enricher.md). |

## Default Toggles

| Option | Default | Flip to disable |
|--------|---------|-----------------|
| `IncludeEfCore` | `true` | Drop EF Core SQL traces; must be false for Native AOT |
| `EnrichActivitiesFromBaggage` | `false` | Flip to `true` to mirror whitelisted baggage entries onto Kun-sourced span tags (ADR-013) |
| `EnrichLogsFromBaggage` | `false` | Flip to `true` to mirror the same baggage keys onto `LogRecord.Attributes` — closes the spans↔logs correlation loop (ADR-013 §F1) |

Native AOT must explicitly set `IncludeEfCore=false`: the runtime-feature guard removes the reflection-based EF instrumentation path from the native binary. Requesting EF instrumentation under Native AOT throws instead of silently omitting it. Other tracing/metrics/logging remain enabled, and the standalone gateway exercises this configuration in its executable AOT gate.
