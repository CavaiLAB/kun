# Why Kun Uses the Outbox Pattern

When a service saves a business entity and publishes an integration event, those are two separate
I/O operations. The Outbox pattern exists to make them behave as one. This document explains the
problem, Kun's solution, and the trade-offs you accept.

---

## The dual-write problem

Consider the naive approach to event publishing:

```csharp
await dbContext.SaveChangesAsync();          // (1) aggregate persisted
await broker.PublishAsync(integrationEvent); // (2) event published to RabbitMQ
```

These two operations are not atomic. Any failure between (1) and (2) leaves the system
inconsistent: the database reflects the new state but consumers never learn about it. Retrying
(1) is unsafe — it may double-write. Putting (2) before (1) has the mirror problem: the event
fires but the database write fails.

Worse, the broker itself is a network dependency. A transient RabbitMQ outage at step (2) means
lost events with no recovery path — unless you build one yourself. That recovery logic is exactly
what the Outbox pattern provides.

---

## How Kun atomizes the publish

Kun's write-side pipeline centers on `EfUnitOfWork.SaveChangesAsync`. The sequence inside that
method is:

1. Domain events are collected from `ChangeTracker.Entries<IAggregateRoot>()` and dispatched
   in-process via `IDomainEventDispatcher`.
2. Each domain event handler calls `IIntegrationEventBus.Publish` inside an
   `EventEnvelopeScope`. The writer resolves the stable type identifier and the registered
   source-generated JSON metadata, then serializes the payload inside a v1 envelope.
3. The writer captures the origin, tenant and trace context and stages the complete
   `OutboxMessage` in the request's `IntegrationEventBuffer`.
4. The unit of work attaches the staged rows to the same context and calls
   `DbContext.SaveChangesAsync`. Aggregate changes and Outbox rows commit in one transaction.

---

## How the background publisher drains the Outbox

Hosts call `AddKunOutboxRelay<TContext>()` for regular Kun contexts or `AddKunOutboxRelay()`
for registered strict contexts. The relay claims pending PostgreSQL rows with a committed
`FOR UPDATE SKIP LOCKED` lease, restores each row’s stored trace parent and publishes the
envelope. Each broker confirmation updates that row immediately. The Order sample uses a
logging publisher. Production hosts can register the RabbitMQ publisher with `AddKunEventsBroker`.

Transient failures persist the attempt count and next attempt time. Invalid envelopes and rows
that reach the retry limit move to `Failed`. On shutdown the relay finishes its current row and
returns unissued rows. If the process exits, another replica can claim them after lease expiry.

---

## Consumer idempotency via `IProcessedMessageStore`

Because the publisher uses at-least-once delivery — the broker acknowledgement and the database
`MarkPublished` update are still two operations — a consumer may receive the same message more
than once. Before dispatch, the subscriber checks `IProcessedMessageStore` for this consumer
and event ID. It writes the success mark after the handler completes, then acknowledges the message.

The Redis key is `idemp:{consumerName}:{messageId}`. The default retention is seven days.
A failure between the business commit and the success mark can cause another delivery.
Handlers that write business data should use the message ID as an idempotency key within
their own transaction.

---

## Trade-offs to be aware of

**Latency is measured in seconds, not milliseconds.** The background publisher runs on a polling
interval. A consumer will see an event seconds after the originating transaction commits, not
sub-millisecond. If your design requires synchronous cross-service consistency, the Outbox is not
the right tool — gRPC calls with `Result`-based error handling are.

**The Outbox store requires a reliable database.** The pattern works because the database is the
source of truth for pending messages. If the database is unavailable, no messages are published —
but this is safer than losing messages silently. Recovery is automatic once the database is
reachable again.

**`Failed` messages need a recovery workflow.** Messages that exhaust retries end up in the
`Failed` state. There is no automatic dead-letter re-queue for failed Outbox messages; you need
an operational process to inspect, correct, and republish them.

The Outbox pattern adds operational surface area in exchange for a guarantee that the dual-write
problem cannot silently corrupt your system state. That trade is almost always worth making in a
distributed system where correctness matters.

---

## See also

- [Kun ADR-015 Outbox pattern design](../adr/kun/ADR-015-Outbox-pattern.md) — full design walk-through: type
  surface, atomicity boundary, publisher pump, consumer idempotency, rejected alternatives.
- [AGENTS.md](../../AGENTS.md#运行时约定) §运行时约定 (the Outbox rule) — normative rules (atomicity,
  stable event IDs, idempotent consumers); how the W3C trace context crosses the outbox is shown in the
  [tracing tour](../guides/tracing-tour.md). The rules constrain; this primer explains the problem; the
  ADR explains the design.
