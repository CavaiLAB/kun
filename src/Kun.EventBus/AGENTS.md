# Kun.EventBus

## Purpose

> **For consumers wiring their first integration event**: read
> [`docs/guides/outbox-and-integration-events.md`](../../_import/kun/docs/guides/outbox-and-integration-events.md)
> first. It walks end-to-end: atomicity boundary, stable-event-ID rule,
> publish/consume worked examples, testing via `OutboxAssertions`,
> W3C trace propagation, and four common anti-patterns.

## Key Files

| File | Description |
|------|-------------|
| `Kun.EventBus.csproj` | `IsAotCandidate=true`, references `Kun.EventBus.Abstractions` |

## For AI Agents

### Working In This Directory

- **The Outbox contract** (this module defines the shape, `Kun.EntityFrameworkCore` writes, `Kun.EventBus.RabbitMQ` publishes): message row carries `Id`, `EventTypeId` (stable string), `Payload` (JSON), `TraceParent`/`TraceState` (W3C), `OccurredAt`, `PublishedAt`, `Status`. Full design walk-through — type surface, atomicity boundary, publisher pump, rejected alternatives — in [ADR-015 Outbox pattern design](../../_import/kun/docs/architecture/ADR-015-Outbox-pattern.md).

### Testing Requirements

For tests that assert EF-persisted `OutboxMessage` state (e.g. "did the command handler stage the right integration event inside its transaction?"), use [`Kun.Testing.EntityFrameworkCore`](../Kun.Testing.EntityFrameworkCore/)'s `OutboxAssertions` — `ShouldHaveOutboxMessage` / `ShouldHaveExactlyOneOutboxMessage` / `ShouldNotHaveOutboxMessage` on both `DbContext` and `IEnumerable<OutboxMessage>`. Closes ADR-015 §F3.

## Dependencies

### Internal

- `Kun.EventBus.Abstractions` — 稳定事件标记、注册与查询端口；原包以类型转发兼容。
