# Kun.Saga

## Key Files

| File | Description |
|------|-------------|
| `Kun.Saga.csproj` | `IsAotCandidate=false` (transitive EF Core — see the original Kun [AOT audit (2026-04-23)](https://github.com/CavaiLAB/kun/blob/51b480775cce05bf889ead14a86749e610503628/docs/architecture/AOT-audit-2026-04-23.md); follow-up `P2-KN-Saga-EF-Split` will restore AOT candidacy by extracting `Kun.Saga.EntityFrameworkCore`) |

## For AI Agents

### Working In This Directory

- **Scope and names**: AddSaga registers scoped saga factories; the scoped registry reads stable `ISaga.Name` and step counts after tenant restoration. Never build a temporary DI container. Existing CLR-name rows remain readable via the legacy alias; new rows use `ISaga.Name`.
- **Execution lock**: the default executor requires PostgreSQL and holds a SKIP LOCKED instance row lock through the step, context write-back, queue changes and transaction commit. Timeout detection takes the same lock and rechecks current state. Remote side effects remain at-least-once across crashes; steps must use idempotency keys.
- **Retry**: MaxAttempts defaults to 1 for compatibility. Opt in to bounded retries only for idempotent steps; NextAttemptAt persists exponential backoff (one-minute cap). Add the nullable column through a consumer migration before deployment (Order sample includes one). Failed local writes roll back to a savepoint; partial context persists for retry/compensation.
- **Timeout**: execution passes a linked cancellation token bounded by the current step deadline; steps must honor it. The detector catches expired idle/retry entries. Host shutdown rolls the current transaction back, without marking business failure or enqueuing compensation.
- **Context serialization** uses the JsonTypeInfo passed to AddSaga. Mutable context is serialized after execution/compensation, including partial failure, and persisted with the state transition.
