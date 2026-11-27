> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Validation.Behaviors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Validation` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Validation`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Validation)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[ValidationPipelineBehavior\<TRequest, TResponse>](/sdk/api/kun/kun-validation-behaviors/validationpipelinebehavior-2.md)

Pipeline behavior that runs every registered [IValidator\<T>](/sdk/api/kun/kun-validation/ivalidator-1.md) against the
incoming request and throws [ErrorException](/sdk/api/kun/kun-errors/errorexception.md) if any report errors. The carried
[Error](/sdk/api/kun/kun-errors/error.md) has code [PlatformErrors.InputInvalid](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_InputInvalid) (400) and lists the field errors
(at most 100) in [Error.Errors](/sdk/api/kun/kun-errors/error.md#Kun_Errors_Error_Errors); Kun.Errors.AspNetCore writes it as an ErrorDetail.

For Result-based semantics (returning a failed `Result` carrying the same error instead of
throwing), register [ValidationResultPipelineBehavior\<TRequest, TValue>](/sdk/api/kun/kun-validation-behaviors/validationresultpipelinebehavior-2.md) instead.

[ValidationResultPipelineBehavior\<TRequest, TValue>](/sdk/api/kun/kun-validation-behaviors/validationresultpipelinebehavior-2.md)

Pipeline behavior that runs every registered [IValidator\<T>](/sdk/api/kun/kun-validation/ivalidator-1.md)
against the incoming request and returns a failed [Result\<T>](/sdk/api/kun/kun-core-results/result-1.md)
carrying [PlatformErrors.InputInvalid](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_InputInvalid) with the field errors (at most 100)
instead of throwing when any report errors. Companion to the
exception-throwing [ValidationPipelineBehavior\<TRequest, TResponse>](/sdk/api/kun/kun-validation-behaviors/validationpipelinebehavior-2.md)
for handlers that deliberately compose via [Result\<T>](/sdk/api/kun/kun-core-results/result-1.md)
and want validation failures to surface through the normal
railway pipeline (see `docs/architecture/ADR-011-Result-async-extensions.md`
and `docs/architecture/ADR-014-Validation-Result-behavior.md`).

[ValidationResultPipelineBehavior\<TRequest>](/sdk/api/kun/kun-validation-behaviors/validationresultpipelinebehavior-1.md)

Non-generic variant of
[ValidationResultPipelineBehavior\<TRequest, TValue>](/sdk/api/kun/kun-validation-behaviors/validationresultpipelinebehavior-2.md) for
handlers whose response is the non-generic [Result](/sdk/api/kun/kun-core-results/result.md)
(void commands with no return value). Returns a failed [Result](/sdk/api/kun/kun-core-results/result.md)
carrying [PlatformErrors.InputInvalid](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_InputInvalid) with the field errors
instead of throwing when validators report errors. Closes ADR-014
§F1 follow-up.
