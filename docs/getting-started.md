# Getting Started with Kun Framework

This tutorial takes about 10 minutes. By the end you will have the full sample stack running
locally, have sent an order through the Order service's command flow, and have a mental model of
how the pieces fit together.

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| .NET SDK | 10.0.100 or later | Pinned in `global.json` |
| Docker Desktop | Any recent stable | Runs PostgreSQL, RabbitMQ, Redis containers |
| Aspire workload | Installed via `dotnet workload` | See step below |
| Visual Studio Build Tools (Windows only) | Optional | Required only for AOT native publish |

Verify your SDK version:

```bash
dotnet --version
# must print 10.0.100 or higher
```

Verify Docker is running:

```bash
docker info
# must succeed — no "Cannot connect" errors
```

---

## Install the Aspire workload

The AppHost project uses .NET Aspire to orchestrate all containers and services. Install the
workload once per machine:

```bash
dotnet workload install aspire
```

If the workload was already installed and you want to update it:

```bash
dotnet workload update
```

---

## Clone and build

```bash
git clone https://github.com/CavaiLAB/kun.git
cd kun
dotnet restore
dotnet build -c Release
```

A successful build produces zero errors. Warnings about AOT trimming in `Kun.EntityFrameworkCore`
are expected and acceptable — that module is deliberately marked trim-safe rather than AOT.

Run the test suite to confirm everything is green:

```bash
dotnet test -c Release --no-build
```

---

## Run the samples

The AppHost project starts every service and all required infrastructure with a single command:

```bash
dotnet run --project samples/Kun.Sample.AppHost
```

Aspire prints a dashboard URL to the console, typically:

```
Login to the dashboard at: http://localhost:15888
```

Open that URL in a browser. You will see the **Aspire Dashboard** with:

| Panel | What it shows |
|---|---|
| Resources | Live health of catalog, order, gateway, postgres, rabbitmq, redis |
| Logs | Structured log streams per service, filterable by level |
| Traces | Distributed OpenTelemetry traces across all service boundaries |
| Metrics | Request rates, latency histograms, CPU and memory |

Wait until all resources in the Resources panel show a green status indicator before sending
requests. PostgreSQL runs schema creation (`EnsureCreated`) at startup, which takes a few seconds.

### Find the service ports

The Order service port is dynamic — Aspire assigns it at startup. In the Dashboard:

1. Click **Resources**.
2. Find the `order` row.
3. Copy the URL from the **Endpoints** column, for example `http://localhost:54321`.

Use that base URL in the curl commands below. The examples use `$ORDER` as a placeholder:

```bash
# Set this to the URL shown in the Aspire Dashboard for the "order" resource
ORDER=http://localhost:54321
```

---

## Place an order

Send a `POST` request to place a two-line demo order:

```bash
curl -s -X POST "$ORDER/demo/orders" | jq .
```

`PlaceOrderCommandHandler` asks the Catalog service over gRPC to reserve stock for each line, then
adds the `Order` and publishes `order.completed.v1`. After the handler returns, the
`UnitOfWorkPipelineBehavior` commits the order row and the Outbox row in one `SaveChangesAsync`
call; the background `OutboxPublisherBackgroundService` publishes the event a moment later.

The demo order uses freshly generated product ids and the sample Catalog starts with no products,
so against the running stack Catalog rejects the reservation: the call returns `422` with
`"code": "BUSINESS_RULE_VIOLATED"`, and neither the order nor an Outbox row is written. When every
reservation succeeds — `samples/Kun.Sample.Tests/PlaceOrderTests.cs` swaps in a fake Catalog
client that accepts them — the response carries the new order id:

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

and the order reads back as `Completed`:

```bash
curl -s "$ORDER/demo/orders/3fa85f64-5717-4562-b3fc-2c963f66afa6" | jq .
```

Open **Traces** in the Aspire Dashboard and filter by `order`. You will see the HTTP request, the
`Mediator.Send PlaceOrderCommand` span, and the gRPC call to Catalog under the same trace.

---

## What you just saw

The order flow exercised these building blocks:

**Outbox transactional publishing** — the order row and its integration event were written in a
single EF Core `SaveChangesAsync` call. If the database transaction rolled back,
no message was produced. The `OutboxPublisherBackgroundService` drains `Pending` rows to the
broker asynchronously, so publish latency is measured in seconds but consistency is guaranteed.

**W3C Trace Context propagation** — every `OutboxMessage` row stores the `traceparent` and
`tracestate` headers captured at enqueue time. The background publisher resumes the trace when it
publishes, so a single `TraceId` threads through HTTP ingress → CQRS handler → Outbox → broker
→ consumer. This is visible in the Aspire Dashboard Traces panel.

---

## Next steps

- **Simpler CQRS story** — `samples/Kun.Sample.Catalog/` demonstrates DDD aggregates, write/read
  split with EF Core and Dapper, and Outbox event publishing. Its
  [README](../samples/Kun.Sample.Catalog/README.md) walks through the file structure.

- **Architecture essays** — understand the design trade-offs:
  - [Why CQRS?](design/why-cqrs.md)
  - [Why Outbox?](design/why-outbox.md)

- **Module map** — [`docs/design/module-map.md`](design/module-map.md) shows the
  full dependency graph and which modules are AOT candidates.

- **Root [AGENTS.md](../AGENTS.md#kun-框架的构建测试与代码约定)**, section Kun 框架的构建、测试与代码约定 — the binding
  architectural rules that every code contribution must satisfy, including the four-layer dependency
  constraint and AOT restrictions.
