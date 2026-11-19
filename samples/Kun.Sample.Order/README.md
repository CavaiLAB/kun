# Kun.Sample.Order

A self-contained sample of a command flow. `PlaceOrderCommand` reserves stock in the Catalog
service over gRPC, then stores the order and its `order.completed.v1` integration event in one
unit of work through the Outbox.

---

## Flow

```
POST /demo/orders
        │
        ▼
PlaceOrderCommandHandler
  ├─ Create Order (Pending)
  ├─ For each line: CatalogService.ReserveStock (gRPC)
  │     rejected → throws ErrorException (422 BUSINESS_RULE_VIOLATED); nothing is committed
  ├─ Order → Reserved → Completed
  └─ IIntegrationEventBus.Publish(OrderCompletedIntegrationEvent)   (buffered, not yet written)
        │
        ▼
UnitOfWorkPipelineBehavior → EfUnitOfWork.SaveChangesAsync
  └─ order row + outbox_messages row committed in one transaction

Background: OutboxPublisherBackgroundService drains outbox_messages
  └─ NoOpIntegrationEventPublisher logs the event (no RabbitMQ in this sample)
```

---

## What it demonstrates

| Concept | Where |
|---|---|
| Command handler calling another service over gRPC | `Application/Commands/PlaceOrderCommandHandler.cs` |
| Unit of work committed by a pipeline behavior | `Program.cs` — `UnitOfWorkPipelineBehavior<PlaceOrderCommand, Guid>` |
| Integration event written to the Outbox in the same transaction | `PlaceOrderCommandHandler` publishes `order.completed.v1` |
| Business error written as problem+json | `ErrorException` with `PlatformErrors.BusinessRuleViolated` → 422 |
| Demo endpoints | `Api/Endpoints/DemoEndpoints.cs` |

---

## Demo endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/demo/orders` | Places a two-line demo order; returns `{ "orderId": "…" }` |
| `GET` | `/demo/orders/{id}` | Returns the order's status, total, and creation time |

The demo order uses freshly generated product ids. The sample Catalog starts with no products, so
against a running Catalog the reservation is rejected and `POST /demo/orders` returns 422 with
nothing written. `samples/Kun.Sample.Tests/PlaceOrderTests.cs` replaces the Catalog client with a
fake that accepts every reservation to exercise the success path.

### Example curl commands

```bash
# Set ORDER to the URL from the Aspire Dashboard (or http://localhost:5000 standalone)
ORDER=http://localhost:<port>

curl -s -X POST "$ORDER/demo/orders" | jq .

# Read the order back — use the orderId returned above
curl -s "$ORDER/demo/orders/<orderId>" | jq .
```

---

## Prerequisites

- PostgreSQL (connection string in `appsettings.json` → `ConnectionStrings:OrderDb`, or override
  via environment variable).
- The Catalog gRPC service (`CatalogGrpc:Address` in `appsettings.json` for standalone runs; the
  AppHost injects the address).
- No RabbitMQ required: `NoOpIntegrationEventPublisher` logs to stdout instead.
- No JWT required: `NullTenantContext` is used (single-tenant sample).

---

## How to run

### Via AppHost (recommended)

```bash
dotnet run --project samples/Kun.Sample.AppHost
```

### Standalone

```bash
dotnet run --project samples/Kun.Sample.Order \
  --ConnectionStrings:OrderDb "Host=localhost;Database=order_sample;Username=postgres;Password=postgres"
```

The service applies its EF Core migrations at startup (`MigrateAsync`). No separate migration step
is needed for development.

### Schema isolation

`OrderDbContext` overrides `DefaultSchema => "order"` and the design-time factory chains
`.UseKunSchema("order")`, so orders, order-lines, `outbox_messages`, and
`__ef_migrations_history` all land in the `order` Postgres schema. Catalog uses
`catalog`; both samples can run against a single Postgres instance in the AppHost without
name collisions. See [docs/guides/schema-per-service.md](../../docs/guides/schema-per-service.md) for
the full recipe and the `SchemaConsistencyInterceptor` safety net.
