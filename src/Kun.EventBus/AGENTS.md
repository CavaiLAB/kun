# Kun.EventBus

## Purpose

> **For consumers wiring their first integration event**: read
> [`docs/guides/outbox-and-integration-events.md`](../../docs/guides/outbox-and-integration-events.md)
> first. It walks end-to-end: atomicity boundary, stable-event-ID rule,
> publish/consume worked examples, testing via `OutboxAssertions`,
> W3C trace propagation, and four common anti-patterns.

## Key Files

| File | Description |
|------|-------------|
| `Kun.EventBus.csproj` | `IsAotCompatible=true`, references `Kun.EventBus.Abstractions` |

## For AI Agents

### Working In This Directory

- **The Outbox contract** (the shape is defined in `Kun.EventBus.Abstractions` and type-forwarded from this assembly; `Kun.EntityFrameworkCore` writes, `Kun.EventBus.RabbitMQ` publishes): message row carries `Id`, `EventTypeId` (stable string), `Payload` (JSON), `TraceParent`/`TraceState` (W3C), `OccurredAt`, `PublishedAt`, `Status`. Full design walk-through — type surface, atomicity boundary, publisher pump, rejected alternatives — in [Kun ADR-015 Outbox pattern design](../../docs/adr/kun/ADR-015-Outbox-pattern.md).

### Testing Requirements

For tests that assert EF-persisted `OutboxMessage` state (e.g. "did the command handler stage the right integration event inside its transaction?"), use [`Kun.Testing.EntityFrameworkCore`](../Kun.Testing.EntityFrameworkCore/)'s `OutboxAssertions` — `ShouldHaveOutboxMessage` / `ShouldHaveExactlyOneOutboxMessage` / `ShouldNotHaveOutboxMessage` on both `DbContext` and `IEnumerable<OutboxMessage>`. Closes Kun ADR-015 §F3.

## Dependencies

### Internal

- `Kun.EventBus.Abstractions` — 稳定事件标记、注册与查询端口，Outbox 写入端口与集成事件信封；挪过去的类型由本程序集类型转发。

## 信封写入端

- IOutboxWriter 与 IIntegrationEventBus 必须解析到同一个 scoped 信封写入端；Outbox 行的 payload 就是完整 v1 信封，不另存半成品。
