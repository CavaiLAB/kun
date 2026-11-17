# ADR-001: Do Not Introduce a New `Kun.Identity` Module

> 本文是原 Kun 的决策记录，引用时写「Kun ADR-001」。2026-11-18 搬入本仓库时清理了引擎与下游产品的名字：正文照原文，方括号里是整理时替换的说法。

**Status**: Accepted  
**Date**: 2026-04-22  
**Author**: claude-official (Planner)  
**Context**: Phase 1 gap audit (［第一阶段差距盘点］) flagged P1-KN-002 / P1-KN-003 as "already present"; a prior planning round (ralplan v1) incorrectly invented P1-KN-004 / P1-KN-005 as new Kun.Identity work. This ADR records the explicit decision not to create a dedicated `Kun.Identity` module and instead consume existing capabilities.

---

## Decision

**No new `Kun.Identity` module will be created.**

Authentication and identity concerns for ［下游的智能体产品］ Phase 1 will be handled by:
- **Kun.Gateway** — OIDC IdP (Authentik) token exchange → internal S2S JWT
- **Kun.Security** — S2S JWT validation, JWKS endpoint consumption, claim extraction
- **［下游的 C# 服务］ Edge service** — direct `AddJwtBearer` wiring against Kun.Security primitives

---

## Decision Drivers

1. **Capability already exists** — Kun.Gateway (OIDC→S2S conversion) and Kun.Security (S2S validation + JWKS) together cover the full identity lifecycle needed for Phase 1.
2. **No gap in Phase 1** — The gap audit (［第一阶段差距盘点］ §4-5) identified P1-KN-001 (Kun.Storage) as the *only* missing Kun module for Phase 1. Identity needs are satisfied by existing modules.
3. **Avoid framework bloat** — A new module would add maintenance surface (release, versioning, AOT verification, documentation) without adding net-new capability.
4. **Downstream simplicity** — Edge service consuming `AddJwtBearer` + Kun.Security is less indirection than routing through an additional framework module.

---

## Alternatives Considered

| Alternative | Description | Why Rejected |
|-------------|-------------|--------------|
| A. Create `Kun.Identity` | New module with `IIdentityService`, `IUserContext`, `IPermissionChecker` abstractions | Duplicates Kun.Security (S2S validation) and Kun.Gateway (OIDC exchange). Would require ［下游的 C# 服务］ to learn *another* framework module for no new capability. |
| B. Extract identity types into `Kun.Identity` | Move `IUserContext`, claim constants, etc. from Kun.Security into a slimmer `Kun.Identity` | Refactoring churn with zero functional gain. Would break existing consumers of Kun.Security. |
| C. Keep as-is (status quo) | No new module; consume existing Kun.Gateway + Kun.Security | **Selected.** Satisfies all Phase 1 requirements with zero new code. |

---

## Why Chosen

Status quo (Alternative C) is the only option that satisfies all drivers without trade-offs:
- Zero new framework code to maintain
- Zero migration for existing consumers
- Aligns with gap audit finding that Phase 1's only real Kun blocker is P1-KN-001 (Storage)

---

## Consequences

### Positive
- No framework release cycle consumed by a new module
- No AOT verification work for a new assembly
- No documentation / sample / AGENTS.md debt for Kun.Identity

### Negative / Trade-offs
- **［下游的 C# 服务］ Edge must own the `AddJwtBearer` wiring** — this is consumer-specific config (Authentik issuer URL, client ID, etc.) and does not belong in Kun anyway. The wiring lives in ［下游的 C# 服务］ where it is editable by the consumer team.
- **If future phases need cross-cutting identity abstractions** (e.g., `IPermissionChecker` with policy store), we will revisit this ADR. At that point the scope will be broader than Phase 1 and a new module may be justified.

### Risks
| Risk | Mitigation |
|------|------------|
| Future identity needs outgrow Kun.Security + Kun.Gateway | ADR is revisable; gate is "cross-cutting abstraction needed by 2+ consumers" |
| Edge service OIDC wiring diverges from Kun patterns | Review Edge service PRs for Kun.Security consumption; do not allow custom JWT validation logic |

---

## Follow-ups

1. **P1-CS-007** (Edge OIDC wiring) — implement in ［下游的 C# 服务］ using `AddJwtBearer` + Kun.Security primitives. No Kun work required.
2. **P1-CS-010** (Gateway YARP + OIDC exchange) — Kun.Gateway already owns this pattern; verify it is wired correctly in the Gateway service.
3. **Retrospective note** — The v1 planner mistake (inventing P1-KN-004/005 without reading gap audit) is recorded in project memory as a discipline reminder: *read authoritative specs before dispatching framework tasks*.
