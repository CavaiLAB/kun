# Kun.Audit

## Purpose

Two complementary halves of the framework audit story live here:

1. **Audit-log abstractions** (`Kun.Audit.Abstractions`): `IAuditWriter`, `IAuditReader`, `AuditableAttribute`, and the `AuditLog` POCO. EF Core integration lives in the sibling `Kun.Audit.EntityFrameworkCore` module; integration-event fan-out is provided by `EventBusAuditWriter` which depends only on `Kun.EventBus`.

2. **Tamper-evident hash-chained operation audit** (`Kun.Audit`): `IAuditSink` + `IAuditChainState` + `AuditContentHasher` + `AuditLogPipelineBehavior<TRequest, TResponse>`. Satisfies SAD v3.0 §15.5 (`audit.operations` + hash chain) for NeuroAgent Phase 1 (P1-CS-019). A Mediator pipeline behaviour captures each command's actor / tenant / timing / outcome, pushes it through `IAuditChainState.AppendAsync`, and writes one `AuditEntry` to `IAuditSink`. Each entry's `ContentHash` is SHA-256 over `prevHash || canonicalFields`, so retroactive tampering with any row invalidates every following row.

## Layer and AOT

- Layer: **Abstraction** (depends on `Kun.Core` + `Kun.Mediator` + `Kun.EventBus` only)

## Key Types

### Hash-chain primitives (tamper-evident operation audit)

| Type | Purpose |
|------|---------|
| `AuditEntry` | Immutable record — Id, TenantId, ActorId, CommandName, OccurredAt, LatencyMs, Outcome, ErrorCode, PrevHash, ContentHash |
| `AuditOutcome` | Success / Exception (Result.Fail mapping is a follow-up) |
| `IAuditSink` | Pluggable write target (`WriteAsync(AuditEntry)`) |
| `IAuditChainState` | Single atomic `AppendAsync(tenantKey, factory)` that serialises read-previous / compute-new / commit-new |
| `AuditContentHasher` | Canonical `pipe \| separated \| fields` + SHA-256, lower-case hex |
| `InMemoryAuditChainState` | Per-tenant SemaphoreSlim + ConcurrentDictionary; dev / single-instance use only |
| `InMemoryAuditSink` | ConcurrentQueue sink for tests |
| `Behaviors/AuditLogPipelineBehavior` | Mediator `IPipelineBehavior<,>` that captures the entry |

### DI / options

| File | Description |
|------|---|
| `DependencyInjection/KunAuditServiceCollectionExtensions.cs` | `AddKunAudit()` + `UseEventBus()` + `AddAuditBehavior<TCommand, TResponse>()` |

## Wiring

```csharp
// Tamper-evident hash-chain opt-in per command:
services.AddKunMediator(m => m.AddAuditBehavior<CreateOrderCommand, Result<Guid>>());
```

## For AI Agents

- New members on `IAuditWriter` / `IAuditReader` / `IAuditSink` / `IAuditChainState` are **public API changes** — update `PublicAPI.Unshipped.txt` when adding.
- Keep `AuditLog` / `AuditEntry` as POCOs with no ORM / JSON attributes. Persistence mapping lives in the EF Core adapter.
- **`TryAdd` every default in `AddKunAudit`**: in-memory chain state/sink + null writer are for tests / single-instance dev only. Production deployments override — never swap the `TryAdd` for `Add`, or you break extensibility.
- **Audit failure must never break a command**: `AuditLogPipelineBehavior` wraps the audit write in its own try/catch that logs at Warning. If you add new sinks / chain states, preserve that contract — the handler's outcome is authoritative.
- **Preserve the original exception**: when the handler throws, the behaviour captures it, writes the audit row with `Outcome=Exception` + `ErrorCode=<ExceptionTypeName>`, then rethrows via `ExceptionDispatchInfo.Capture(ex).Throw()` so the original stack trace is preserved.
- **Per-tenant serialisation is the whole point**: `InMemoryAuditChainState.AppendAsync` uses a per-tenant `SemaphoreSlim`. If you add a durable chain state, it must guarantee the same read-prev / compute-new / commit-new is atomic per tenant — typically via DB-level uniqueness on `(tenant_id, content_hash)` or a serialisable transaction.
- **Opt-in per command**: `AddAuditBehavior<TCommand, TResponse>()` registers exactly one closed generic instance. Queries and events stay unaudited by default — add the behaviour only to commands that belong in the audit trail.
- **Canonical hash is invariant across culture and timezone**: `AuditContentHasher` uses `CultureInfo.InvariantCulture`, round-trip `O` timestamp format, and lower-case hex. Do not switch to binary / base-64 without updating every downstream reader.

## Testing Requirements

- Unit coverage lives in `tests/Kun.Audit.Tests` (xunit.v3, FluentAssertions, NSubstitute):
  - Hash matrix — every field mutation shifts the hash.
  - Chain state — 100 concurrent appends on one tenant observe distinct prevs and form a linked list (no fork).
  - Pipeline behaviour — success / exception paths, sink failure does not propagate, consecutive entries link, per-tenant chains stay independent.
  - Writer paths — `NullAuditWriter` drops silently; `EventBusAuditWriter` publishes as integration event with stable id.
- Integration tests with a durable sink are deferred until a durable implementation ships (out of Kun.Audit's scope — consumers wire their own EF sink for now).

## Dependencies

### Internal

- `Kun.Core` — `Kun.Core.Cqrs.IRequest<>`, `Kun.Core.MultiTenancy.ITenantContext`, `Kun.Core.Security.ICurrentUser`.
- `Kun.Mediator` — `IPipelineBehavior<,>`, `KunMediatorBuilder`.
- `Kun.EventBus` — `IIntegrationEventBus` (for `EventBusAuditWriter`).

### External

- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Options`

## S1-KUN-005 当前入口

实体审计使用 `Kun.Auditing.AddKunAuditing<TContext>()` 同事务写 `AuditLog` + Outbox；旧事后拦截器和 `kun.audit.record.v1` 路径仅保留兼容并标记废弃。命令审计提供 `UsePostgresChain()` 持久化实现；`ActorType` 含 `agent`，使用 `AuditContentHasher.Compute(entry)` 校验新旧条目。具体装配及迁移以 [审计文档](../../_import/kun/docs/security/audit-log.md) 为准。
