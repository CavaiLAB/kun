> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Getting Started with Kun Framework

:::info 发布版本来源
本页由本仓库的发布标签生成，源文件为 [`docs/getting-started.md`](https://github.com/CavaiLAB/kun/blob/v2026.11.24/docs/getting-started.md)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

This tutorial takes about 10 minutes. By the end you will have the full sample stack running
locally, have sent an order through the Order service's command flow, and have a mental model of
how the pieces fit together.

***

## Prerequisites

| Requirement                              | Version           | Notes                                                  |
| ---------------------------------------- | ----------------- | ------------------------------------------------------ |
| .NET SDK                                 | 10.0.404          | Verified with the roll-forward policy in `global.json` |
| Docker Desktop                           | Any recent stable | Runs PostgreSQL, RabbitMQ, Redis containers            |
| curl and jq                              | curl 7.82+        | Send JSON requests and read the returned IDs           |
| Visual Studio Build Tools (Windows only) | Optional          | Required only for AOT native publish                   |

Verify your SDK version:

```bash
dotnet --version
# the walkthrough was tested with 10.0.404
```

Verify Docker is running:

```bash
docker info
# must succeed — no "Cannot connect" errors
```

***

## Clone and build

```bash
git clone https://github.com/CavaiLAB/kun.git
cd kun
dotnet restore Kun.slnx
dotnet build Kun.slnx -c Release --no-restore
```

Run the framework unit suite using the solution filter from CI:

```bash
dotnet test --solution tests/kun-unit.slnf -c Release --no-build
```

***

## Run the samples

The AppHost starts the services and their infrastructure. Trust the ASP.NET Core localhost
development certificate so Order can call Catalog over HTTPS. Create a private directory outside
the repository for the local issuer's keys and Order's short-lived service token:

```bash
dotnet dev-certs https --trust
umask 077
export SampleCredentials=$(mktemp -d)
printf 'SampleCredentials=%s\n' "$SampleCredentials"
dotnet run --file samples/local/catalog-credentials.cs -- "$SampleCredentials"
dotnet run --project samples/Kun.Sample.AppHost
```

Aspire prints a dashboard URL to the console, typically:

```
Login to the dashboard at: http://localhost:15888
```

Open that URL in a browser. You will see the **Aspire Dashboard** with:

| Panel     | What it shows                                                     |
| --------- | ----------------------------------------------------------------- |
| Resources | Live health of catalog, order, gateway, postgres, rabbitmq, redis |
| Logs      | Structured log streams per service, filterable by level           |
| Traces    | Distributed OpenTelemetry traces across all service boundaries    |
| Metrics   | Request rates, latency histograms, CPU and memory                 |

Wait for Catalog and Order to finish applying their EF Core migrations before sending requests.

### Find the service ports

Copy the HTTP URLs for `catalog` and `order` from the **Endpoints** column in **Resources**.
Set these variables to the two URLs shown in your dashboard:

```bash
CATALOG=http://localhost:54242
ORDER=http://localhost:54243
```

***

## Place an order

Run these requests in a second terminal. Set `SampleCredentials` to the path printed during
startup. The local token lasts five minutes. To renew it, run
`dotnet run --file samples/local/catalog-credentials.cs -- "$SampleCredentials"` with the same
directory as the first terminal. Order reads the updated token on its next HTTPS call.

Create a Catalog product with stock, then order two units using its returned identifier:

```bash
PRODUCT_ID=$(curl -fsS "$CATALOG/products" --json '{
  "name":"Demo notebook","description":"Grid paper","price":12.50,
  "categoryId":"f4743227-31ea-4312-b9ca-e7183786bb01","initialStock":5
}' | jq -er .)
ORDER_ID=$(curl -fsS "$ORDER/demo/orders" --json "{
  \"productId\":\"$PRODUCT_ID\",\"quantity\":2,\"unitPrice\":12.50
}" | jq -er .orderId)
curl -fsS "$ORDER/demo/orders/$ORDER_ID" | jq '{id,status,totalAmount}'
```

The order reads back as `Completed` with a total of `25.00`. The handler reserved two units over
gRPC and committed the order and its `order.completed.v1` envelope in one transaction. To see
the event, open the Order resource's console logs and look for the Outbox publisher's
`Would publish event 'order.completed.v1'` message. In this sample the publisher logs delivery.

Without a request body, `POST /demo/orders` tries two random product IDs. Catalog rejects the
reservation. Order returns `422 BUSINESS_RULE_VIOLATED` and writes no order or Outbox row.

Open **Traces** in the Aspire Dashboard and filter by `order`. You will see the HTTP request, the
`Mediator.Send PlaceOrderCommand` span, and the gRPC call to Catalog under the same trace. After the
walkthrough, stop the AppHost and remove the temporary credentials directory.

***

## What you just saw

The order flow exercised these building blocks:

**Outbox transactional publishing** — the order row and its integration event were written in a
single EF Core `SaveChangesAsync` call. If the database transaction rolled back,
no message was produced. The `KunOutboxRelay` drains `Pending` rows to the
configured publisher asynchronously. The sample publisher logs each event.

**W3C Trace Context propagation** — every `OutboxMessage` row stores the `traceparent` and
`tracestate` headers captured at enqueue time. The background publisher resumes the trace when it
publishes. The request and the corresponding Outbox publish span share a `TraceId` in the
Aspire Dashboard Traces panel.

***

## Next steps

- **Simpler CQRS story** — `samples/Kun.Sample.Catalog/` demonstrates DDD aggregates, write/read
  split with EF Core and Dapper, and Outbox event publishing. Its
  [README](https://github.com/CavaiLAB/kun/blob/v2026.11.24/samples/Kun.Sample.Catalog/README.md) walks through the file structure.

- **Architecture essays** — understand the design trade-offs:
  - [Why CQRS?](/sdk/kun/architecture/why-cqrs.md)
  - [Why Outbox?](/sdk/kun/architecture/why-outbox.md)

- **Module map** — [`docs/design/module-map.md`](/sdk/kun/modules.md) shows the
  full dependency graph and which modules are AOT candidates.

- **Root [AGENTS.md](https://github.com/CavaiLAB/kun/blob/v2026.11.24/AGENTS.md#kun-框架的构建测试与代码约定)**, section Kun 框架的构建、测试与代码约定 — the binding
  architectural rules that every code contribution must satisfy, including the four-layer dependency
  constraint and AOT restrictions.
