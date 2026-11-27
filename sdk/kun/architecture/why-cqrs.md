> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Why Kun Splits Writes from Reads

:::info 发布版本来源
本页由本仓库的发布标签生成，源文件为 [`docs/design/why-cqrs.md`](https://github.com/CavaiLAB/kun/blob/v2026.11.24/docs/design/why-cqrs.md)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Kun enforces a hard boundary between the write side and the read side of every bounded context.
This is not a stylistic preference — it is enforced by module dependencies and verified by CI.
Understanding the reasoning helps you work within the model rather than against it.

***

## The write side: consistency first

Command handlers use `KunDbContext` directly or inject a repository port defined by their domain. The write side loads an aggregate, calls domain methods,
and delegates to `IUnitOfWork.SaveChangesAsync`. That single call does several things atomically:

1. Applies EF Core change tracking to produce the correct SQL.
2. Collects domain events from every modified `IAggregateRoot` via `ChangeTracker`.
3. Dispatches domain events to in-process handlers via `IDomainEventDispatcher`.
4. Converts any resulting integration events into `OutboxMessage` rows.
5. Commits the aggregate changes and the Outbox rows in one database transaction.

This sequence exists because consistency is the write side's job. A command either fully
succeeds — business state updated, domain events dispatched, integration events enqueued — or
it fully fails with no observable side-effects. The Specification pattern keeps query logic out of
handler code while still letting the repository translate rich criteria into SQL without leaking
EF Core types up into the domain layer.

The cost is performance. Loading aggregates, tracking changes, and running the full pipeline
takes more work per request than a raw SQL insert. For the write side this is the correct
trade-off: correctness matters more than throughput.

***

## The read side: performance first

Query handlers open connections from a dedicated read-side `NpgsqlDataSource` (potentially
pointing at a read replica) and run raw SQL with Dapper; Kun does not wrap either library. No EF
Core context is involved; there is no change tracker, no aggregate loading, no domain logic.

The result is flat DTOs returned directly to the caller. Because there is no object graph to
navigate and no invariants to enforce, the read side can denormalize freely: a single query can
join five tables and return a DTO shaped exactly for the API response, avoiding the N+1 problems
that appear when EF Core navigation properties are misused.

The read side can also be published as a native AOT binary, which the EF Core write side cannot.
Dapper's classic `Query<T>` builds its row readers with reflection, so an AOT-published service
adds Dapper's source generator (`Dapper.AOT`, `[DapperAot]`) or reads rows with
`NpgsqlDataReader` directly; see [read-side patterns](/sdk/kun/guides/read-side-patterns.md#aot-note).

***

## How the Command/Query divide maps to `Kun.Mediator`

`KunMediator` dispatches every `IRequest<TResponse>` to its registered handler through a
`FrozenDictionary<Type, RequestHandlerBase>` — O(1) lookup with zero runtime reflection. Handlers
are registered explicitly at startup via `KunMediatorBuilder`. The mediator does not scan
assemblies; there is no runtime discovery.

Pipeline behaviors wrap each dispatch. The standard write-side stack is:

```
ValidationPipelineBehavior   ← rejects invalid commands before touching the database
UnitOfWorkPipelineBehavior   ← calls SaveChangesAsync after the handler returns
LoggingBehavior              ← structured log at entry and exit
TracingBehavior              ← opens an OpenTelemetry span per dispatch
```

Query handlers carry only `LoggingBehavior` and `TracingBehavior` by default — there is nothing
to validate or commit. Adding `UnitOfWorkPipelineBehavior` to a query handler is a configuration
mistake that the type system cannot prevent, but CI code review does. The distinction is
explicit: every registration in `Program.cs` names the request type, the response type, and the
handler type. There is no magic.

***

## Trade-offs to be aware of

**Two models to maintain.** Every write-side aggregate and its corresponding read-side DTO are
separate representations. When you add a field to an aggregate you must also update any queries
and DTOs that need to surface it. The consistency guarantee you get from the write side does not
automatically flow to the read side.

**Denormalization can drift.** Read-side DTOs are shaped for API consumers, not normalized from
aggregate state. If a consumer's requirements change and the DTO is updated but the underlying
data is not re-projected, the read side and the write side can express different views of the
same entity.

**No EF Core in query handlers.** Using EF Core in a query handler breaks the `UoW` pipeline
behavior's transaction boundary assumptions and gives up the option of publishing the read side
as a native AOT binary. Neither the compiler nor the project references enforce this — command
and query handlers usually live in the same project — so reviewers check that query handlers
take no EF Core types.

The split is explicit and by design. It makes each side independently optimizable and keeps the
invariants of each clearly scoped.
