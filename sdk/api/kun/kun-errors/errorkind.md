> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum ErrorKind

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

错误码类别（contracts/v0/error-catalog.schema.json 的 `kind`）。

```csharp
public enum ErrorKind
```

## Fields

`Business = 1`

业务错误：请求合法且已授权，业务规则拒绝。状态码只用 409（状态冲突）或 422（规则不满足）。

`Field = 2`

字段级错误码：只出现在 `errors[]` 里，不能作为响应的顶层 `code`。

`Protocol = 0`

协议错误：请求未进入业务语义即被拒——解析、契约校验、认证、鉴权、寻址、限流、服务端故障。
状态码不用 409 / 422。
