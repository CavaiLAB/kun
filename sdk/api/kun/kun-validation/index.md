> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Validation

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Validation` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Validation`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Validation)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Validation.Behaviors](/sdk/api/kun/kun-validation-behaviors/index.md)

[Kun.Validation.DependencyInjection](/sdk/api/kun/kun-validation-dependencyinjection/index.md)

### Structs

[ValidationResult](/sdk/api/kun/kun-validation/validationresult.md)

Result of running a validator against an instance. [ValidationResult.IsValid](/sdk/api/kun/kun-validation/validationresult.md#Kun_Validation_ValidationResult_IsValid) is true iff
[ValidationResult.Errors](/sdk/api/kun/kun-validation/validationresult.md#Kun_Validation_ValidationResult_Errors) is empty. Each error is a [FieldError](/sdk/api/kun/kun-errors/fielderror.md): a field-level error code
registered in the error catalog (such as [PlatformErrors.Required](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_Required)) plus the JSON Pointer
of the offending input ([ErrorPointer.Input](/sdk/api/kun/kun-errors/errorpointer.md#Kun_Errors_ErrorPointer_Input_System_ReadOnlySpan_System_Object__), e.g. `#/items/0/qty`).

### Interfaces

[IValidator\<T>](/sdk/api/kun/kun-validation/ivalidator-1.md)

Validates instances of `T`. Implementations are registered per
request type via `KunMediatorBuilder.AddValidator<T, TValidator>()` (see
[KunValidationServiceCollectionExtensions](/sdk/api/kun/kun-validation-dependencyinjection/kunvalidationservicecollectionextensions.md)).
