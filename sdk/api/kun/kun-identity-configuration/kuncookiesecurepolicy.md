> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum KunCookieSecurePolicy

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Identity` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Identity`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.Identity)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Identity.Configuration](/sdk/api/kun/kun-identity-configuration/index.md)\
Assembly: Kun.Identity.dll

Secure-policy for [KunCookieAuthOptions.Secure](/sdk/api/kun/kun-identity-configuration/kuncookieauthoptions.md#Kun_Identity_Configuration_KunCookieAuthOptions_Secure). Mirrors
ASP.NET Core's `CookieSecurePolicy` shape without taking a
dependency on that package from the core abstractions.

```csharp
public enum KunCookieSecurePolicy
```

## Fields

`Always = 0`

Emit the cookie only over HTTPS (default in production).

`None = 1`

Emit the cookie over either scheme (development only).

`SameAsRequest = 2`

Match the scheme of the current request.
