# Kun.IntegrationTests

## Purpose

Integration tests that require real infrastructure. Runs against Testcontainers PostgreSQL, RabbitMQ, Redis and an S3-compatible server (RustFS, `Fixtures/S3CompatibleServer.cs`) — never mocks. Covers Outbox atomicity (aggregate + outbox commit together), tenant query filter enforcement, idempotent consumer behavior under message redelivery, and gRPC contract tests.

## For AI Agents

### Working In This Directory

- **Skip only when there is no container runtime.** Route that case through `ContainerRuntime.ReportUnavailable` (`Fixtures/ContainerRuntime.cs`): locally it skips (xunit.v3 1.0.0 reports a skip raised inside a class or collection fixture as a fixture failure), and with `CI` set it fails. Never catch image pull or container startup failures and skip — let them fail the test.

## Dependencies

### External

- `Testcontainers.PostgreSql`, `Testcontainers.RabbitMq`, `Testcontainers.Redis`, `Testcontainers` (generic containers: PgBouncer, RustFS)
