# Kun.Sandbox

## Purpose

A .NET sandbox abstraction for Kun consumers (C# business services in
any Kun-based application).

```csharp
var sandbox = await provider.CreateAsync(new SandboxCreateOptions
{
    TemplateId = "kun-default",
    CpuCount = 2,
    MemoryMB = 2048,
    TimeoutMs = 300_000,
    Envs = new Dictionary<string, string> { ["DEBUG"] = "1" },
    Metadata = new Dictionary<string, string> { ["tenantId"] = tenantId },
});
```

## Scope and non-scope

- **Out of scope**: *running a sandbox*. No Docker, no gVisor, no
  process supervision. The runtime lives in
  the configured external supervisor. Spec v1.1 §14 pins the
  Supervisor to Python — this module is only the .NET client.

## Backend choice

| Provider | When to use | Requires |
|----------|-------------|----------|
| `SupervisorSandboxProvider` | Phase 1 production path. Calls `/v2/sandboxes/*` on a configured supervisor. | Supervisor URL, mTLS client cert, S2S JWT issuer (via delegating handler). |

## For AI Agents

- **Do** use `InMemorySandboxProvider` for business-service unit
  tests. Integration tests that want a real Supervisor belong under
  `Kun.IntegrationTests`, not here.

S1-KUN-004: ProviderExtensions carries explicitly named provider-owned JSON members; core fields cannot be overridden. The old named extension/property remains obsolete compatibility only and is omitted by default. Do not silently discard a caller's explicitly configured legacy policy. Use a set accessor for JsonExtensionData: source-generated constructor binding does not support an init-only extension property.
