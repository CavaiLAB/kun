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

Background: KunOutboxRelay drains outbox_messages
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
| `POST` | `/demo/orders` | Reserves the supplied product and returns `{ "orderId": "…" }` |
| `GET` | `/demo/orders/{id}` | Returns the order's status, total, and creation time |

Pass a JSON body with `productId`, `quantity` and `unitPrice` to order an existing Catalog
product. The [getting-started guide](../../docs/getting-started.md#place-an-order) shows the
commands to create one with stock, place the order and read its status. A request without a
body uses two randomly generated product IDs. Catalog rejects that reservation with HTTP 422
and the order and Outbox tables receive no rows.

---

## Prerequisites

- PostgreSQL (connection string in `appsettings.json` → `ConnectionStrings:OrderDb`, or override
  via environment variable).
- The Catalog gRPC service (`CatalogGrpc:Address` in `appsettings.json` for standalone runs; the
  AppHost injects the address).
- No RabbitMQ required: `NoOpIntegrationEventPublisher` logs to stdout instead.
- The sample uses a single tenant. Catalog validates the Order service token for gRPC stock
  reservations. The getting-started guide creates the local credentials.

---

## How to run

### Via AppHost (recommended)

Follow the [local credentials and startup steps](../../docs/getting-started.md#run-the-samples).

### Standalone

```bash
dotnet run --project samples/Kun.Sample.Order \
  --ConnectionStrings:OrderDb "Host=localhost;Database=order_sample;Username=postgres;Password=postgres"
```

For stock reservations, configure `CatalogGrpc:Address` with the Catalog HTTPS endpoint,
`CatalogGrpc:TokenPath` with the generated `catalog.token` file, and Catalog
with `CatalogAuth:PublicJwksPath` pointing at the corresponding `public.jwks`.

The service applies its EF Core migrations at startup (`MigrateAsync`). No separate migration step
is needed for development.

### Schema isolation

`OrderDbContext` overrides `DefaultSchema => "order"` and the design-time factory chains
`.UseKunSchema("order")`, so orders, order-lines, `outbox_messages`, and
`__ef_migrations_history` all land in the `order` Postgres schema. Catalog uses
`catalog`; both samples can run against a single Postgres instance in the AppHost without
name collisions. See [docs/guides/schema-per-service.md](../../docs/guides/schema-per-service.md) for
the full recipe and the `SchemaConsistencyInterceptor` safety net.
