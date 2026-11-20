# Tracing Tour: Reading Distributed Traces in the Aspire Dashboard

This document walks through the OpenTelemetry traces you will see when running the Kun sample
stack via `dotnet run --project samples/Kun.Sample.AppHost`. It assumes Docker and the Aspire
workload are installed and the stack is running. For setup instructions see
[docs/getting-started.md](../getting-started.md).

---

## W3C Trace Context and OTLP

Every Kun service propagates traces using the
[W3C Trace Context](https://www.w3.org/TR/trace-context/) standard. Each in-process span records
a `traceparent` header that carries the `TraceId` and `SpanId`. When a request crosses a process
boundary — HTTP, gRPC, or the Outbox — the receiving side extracts the parent context and creates
a child span under the same `TraceId`. The result is a single trace tree that spans every hop
from the initial HTTP request to the final consumer acknowledgement.

Spans are exported via OTLP. When running under .NET Aspire the `OTEL_EXPORTER_OTLP_ENDPOINT`
environment variable is set automatically to the Aspire Dashboard's collector endpoint. The
Dashboard collects and renders all spans without any additional configuration in the services
themselves. Framework modules (`src/Kun.*`) contain no OTel package dependencies — all
`AddKunObservability` wiring lives in the sample composition root
(`samples/Kun.Sample.Shared/OpenTelemetryExtensions.cs`) so the framework remains AOT-clean.

---

## What Instruments What

### Kun.Mediator

**ActivitySource name:** `Kun.Mediator`

`TracingBehavior<TRequest, TResponse>` wraps every `ISender.Send` and `IPublisher.Publish` call
(see `src/Kun.Mediator/Behaviors/TracingBehavior.cs`). It starts an `ActivityKind.Internal` span
named `Mediator.Send {RequestTypeName}` — for example `Mediator.Send CreateProductCommand` or
`Mediator.Send GetProductByIdQuery`.

The span is a child of the ambient activity at dispatch time, which is typically the ASP.NET Core
server span for the incoming HTTP request. Because the Mediator span is internal, it appears as a
nested block directly under the HTTP span in the Dashboard timeline.

Tags set on this span:

| Tag | Value |
|-----|-------|
| (none beyond span name) | The request type name is encoded in the span name |

If you want to add custom tags (e.g. command ID, tenant ID), do so from within the handler by
calling `Activity.Current?.SetTag(...)`.

### Kun.EventBus.EntityFrameworkCore

**ActivitySource name:** `Kun.EventBus.EntityFrameworkCore`

The EF relay creates one `ActivityKind.Producer` span named `outbox.publish` for each delivery
attempt. The envelope writer stores the W3C parent identifier and trace state with the Outbox
row during the command. The relay restores that parent before handing the stored envelope to
the configured publisher. A retry creates another span with the same message ID.

Tags set on `outbox.publish`:

| Tag | Value |
|-----|-------|
| `messaging.system` | `outbox` |
| `messaging.destination` | Stable event type ID, e.g. `product.published.v1` |
| `messaging.message_id` | UUID of the `OutboxMessage` row |

### Kun.Grpc

**ActivitySource name:** `Kun.Grpc`

`KunTracingInterceptor` (see `src/Kun.Grpc/Interceptors/KunTracingInterceptor.cs`) runs first in
the interceptor chain registered by `AddKunGrpc`. It starts an `ActivityKind.Server` span named
`grpc.server` for each unary, client-streaming, server-streaming or duplex RPC call. The span
remains active until the awaited handler completes, including all stream operations.

Tags set:

| Tag | Value |
|-----|-------|
| `rpc.system` | `grpc` |
| `rpc.method` | Full gRPC method path, e.g. `/catalog.CatalogService/GetProduct` |
| `rpc.type` | `unary`, `client_streaming`, `server_streaming` or `duplex_streaming` |
| `rpc.grpc.status_code` | Final gRPC status code; faults also mark the activity as Error |

If a downstream service calls a Kun gRPC service, the caller's `HttpClient` instruments propagate
the `traceparent` header via the standard gRPC client instrumentation wired in
`AddKunObservability`. The server interceptor picks up the parent context automatically from the
incoming metadata — `ActivityKind.Server` causes the OTel SDK to look for an inbound W3C header.

---

## How to Read a Happy-Path Trace

Place an order using the Catalog and Order endpoints in the [getting started walkthrough](../getting-started.md).
Find its `POST /demo/orders` request in the Aspire Dashboard.

1. The mediator span contains the gRPC stock reservation for each order line.
2. The handler stages an `order.completed.v1` envelope in the same transaction as the order.
   The envelope carries the producer, actor, message ID and W3C trace parent.
3. The EF relay claims the row and opens `outbox.publish` with its stored parent. The Order
   sample logs the delivery. Its span tags include the event type and message ID.
4. In a host configured with RabbitMQ, the publisher sends the envelope to `qian.events`
   under the subject `order.completed`. A registered subscriber restores the envelope's parent
   for its `consume order.completed` span and acknowledges after successful processing.
   The publish and consume spans share the trace that the command stored in the envelope.

---

## How to Read Outbox Lag

Outbox lag is the time between when an integration event was enqueued (the producer's
`SaveChangesAsync` commit) and when it was actually published to the broker (the
`outbox.publish` span).

To measure lag in the Dashboard:

1. Find the `Mediator.Send` span for a command that produces an integration event (e.g.
   `CreateProductCommand`).
2. Note its **end timestamp** — this is approximately when `SaveChangesAsync` committed and the
   outbox row was written.
3. Find the `outbox.publish` span for the same message (match by `messaging.message_id`, or by
   `messaging.destination` + approximate time).
4. The **start timestamp** of `outbox.publish` minus the end timestamp of the producer span is
   the outbox lag for that message.

The EF relay immediately polls again after a nonempty batch. The following defaults apply to
`KunEventsOptions` in the publisher host.

| Setting | Default | Effect |
|---------|---------|--------|
| `BatchSize` | 100 | Maximum rows claimed per context per cycle |
| `PollInterval` | 1 second | Delay after an empty cycle |
| `PublishTimeout` | 10 seconds | Confirmation deadline per message |
| `LeaseDuration` | 30 seconds | Other replicas can claim unfinished rows after expiry |
| `MaxAttempts` | 5 | Maximum attempts after transient failures |
| `RetryBaseDelay` | 1 second | Initial exponential backoff |

A message in `OutboxStatus.Failed` will not be retried automatically. Monitor for `Failed` rows
in the `outbox_messages` table and alert on them — they represent messages that could not be
delivered after all attempts.

---

## Pitfalls and Common Gaps

### Background services that do not start an activity

`KunOutboxRelay` is a `BackgroundService` implementation.
Its `ExecuteAsync` loop does not have an ambient `Activity` unless one is explicitly started.
This means:

- Any log message emitted inside the loop but outside a span will have no `TraceId` attached in
  structured logs.
- Any child activity started without an explicit parent context appears as a root span in OTLP.

If you add custom background services to your application and want their spans connected to
a broader trace, you must restore the parent context manually — the same pattern used by
`KunOutboxRelay` with `ActivityContext.TryParse`.

### async void timers and fire-and-forget

Avoid `async void` methods and `_ = Task.Run(...)` inside handlers or background services.
Exceptions thrown on `async void` paths crash the process without ending any open spans, leaving
dangling incomplete spans in the Dashboard. Use `async Task` and `await` throughout.

### Gateway observability

The standalone `Kun.Gateway` host registers `AddKunObservability` with service name
`kun-gateway`, configured through `KunObservability`. EF instrumentation is disabled because
this host does not access EF. Native AOT hosts must set `IncludeEfCore=false`; a runtime-feature
guard removes that reflection-based path at link time and rejects attempts to enable it at runtime. ASP.NET request spans cover proxy and JWKS requests, while
HttpClient instrumentation covers downstream calls; there is no separate custom transform span.
The gateway remains in the Native AOT audit. Consumers that reference the gateway package in
their own host choose their own observability registration.

### Key span names to recognize at a glance

| Span name | Source | Meaning |
|-----------|--------|---------|
| `Mediator.Send {Type}` | `Kun.Mediator` | A command or query was dispatched |
| `outbox.publish` | `Kun.EventBus.EntityFrameworkCore` | An outbox row was published to the broker |
| `consume {subject}` | `Kun.EventBus.RabbitMQ` | The subscriber dispatched an envelope |
| `grpc.server` | `Kun.Grpc` | A unary or streaming gRPC call was handled |
