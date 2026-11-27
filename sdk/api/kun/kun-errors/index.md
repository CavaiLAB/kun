> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Errors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Errors.AspNetCore](/sdk/api/kun/kun-errors-aspnetcore/index.md)

### Classes

[AllowedValues](/sdk/api/kun/kun-errors/allowedvalues.md)

构造 `allowed`（允许的取值，1–100 个 JSON 值）。

[Error](/sdk/api/kun/kun-errors/error.md)

一次出错：哪个错误码、文案参数、出错位置与允许值。进程内由 `Kun.Core.Results.Result` 携带或随
[ErrorException](/sdk/api/kun/kun-errors/errorexception.md) 抛出；对外由 [ErrorCatalog.CreateDetail](/sdk/api/kun/kun-errors/errorcatalog.md#Kun_Errors_ErrorCatalog_CreateDetail_Kun_Errors_Error_System_String_System_Globalization_CultureInfo_System_String_) 渲染为 [ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)。

[ErrorCatalog](/sdk/api/kun/kun-errors/errorcatalog.md)

错误码目录：全局唯一的错误码登记处，渲染 [ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)，并导出
contracts/v0/error-catalog.schema.json 格式的 JSON（错误码与多语言文案）。

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

一个登记在错误码目录里的稳定错误码：类别、状态码、归属组件与多语言文案。
组件在自己的 Definition 包里以 `static readonly` 字段定义，挂载时登记到 [ErrorCatalog](/sdk/api/kun/kun-errors/errorcatalog.md)。

[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)

错误响应体（contracts/v0/error-detail.schema.json）：RFC 9457 problem+json 的 Kun 配置文件。
由 [ErrorCatalog.CreateDetail](/sdk/api/kun/kun-errors/errorcatalog.md#Kun_Errors_ErrorCatalog_CreateDetail_Kun_Errors_Error_System_String_System_Globalization_CultureInfo_System_String_) 生成；序列化用 [ErrorDetailJson](/sdk/api/kun/kun-errors/errordetailjson.md)。
v0 已冻结，只接受向后兼容增量；冻结标记是 Schema 元数据，不属于错误响应体。

[ErrorDetailJson](/sdk/api/kun/kun-errors/errordetailjson.md)

[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md) 的 JSON 序列化（源生成，AOT 可用；空成员不输出；中文不转义）。

[ErrorException](/sdk/api/kun/kun-errors/errorexception.md)

携带 [ErrorException.Error](/sdk/api/kun/kun-errors/errorexception.md#Kun_Errors_ErrorException_Error) 的异常，Kun 只有这一个带错误码的异常：业务代码直接抛出，或由
`Result<T>.EnsureSuccess()` 在失败时抛出；HTTP 宿主由 Kun.Errors.AspNetCore 写成 problem+json，
gRPC 宿主由 Kun.Grpc 的拦截器转成对应的状态码。

[ErrorPointer](/sdk/api/kun/kun-errors/errorpointer.md)

构造 `pointer`：RFC 6901 JSON Pointer 的 URI 片段形式，可带文档前缀。
请求输入写 `#/items/0/qty`；文件写 `.qian-plugin/plugin.json#/runtime/nuget/0`。

[FieldError](/sdk/api/kun/kun-errors/fielderror.md)

`errors[]` 中的一项：字段级或业务错误码 + 出错位置。

[FieldErrorDetail](/sdk/api/kun/kun-errors/fielderrordetail.md)

[ErrorDetail.Errors](/sdk/api/kun/kun-errors/errordetail.md#Kun_Errors_ErrorDetail_Errors) 中的一项。

[PlatformErrors](/sdk/api/kun/kun-errors/platformerrors.md)

平台内置错误码（归属 `qian.runtime`），冻结在 contracts/v0/error-codes.json。
协议错误覆盖宿主框架会产生的全部状态码；业务错误只有两个通用兜底，具体业务错误码由各组件在 Definition 包登记。

### Enums

[ErrorKind](/sdk/api/kun/kun-errors/errorkind.md)

错误码类别（contracts/v0/error-catalog.schema.json 的 `kind`）。
