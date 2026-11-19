using System.Net;
using System.Text.Json;
using Kun.Errors.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kun.Errors.Tests;

/// <summary>
/// 带逐项字段错误的 <see cref="ErrorException"/>（Kun.Validation 的校验失败就是这种形状）：
/// 先于宿主自己的兜底异常处理写成 ErrorDetail，100 项字段错误仍在冻结契约之内。
/// </summary>
public sealed class ErrorExceptionHandlerTests
{
    [Fact]
    public void An_error_with_100_field_errors_stays_within_the_frozen_contract()
    {
        var error = new Error(PlatformErrors.InputInvalid)
        {
            Errors = Enumerable.Range(0, 100).Select(i => new FieldError(PlatformErrors.ValueNotAllowed, ErrorPointer.Input("items", i))).ToArray(),
        };

        var detail = ErrorCatalog.CreateDefault().CreateDetail(error, new string('a', 32));
        var body = JsonSerializer.SerializeToElement(detail, ErrorDetailJson.TypeInfo);

        Assert.Empty(Contracts.Validate("error-detail", body));
        Assert.Empty(Contracts.CheckSemantics("error-detail", body));
        Assert.Equal("#/items/0", body.GetProperty("pointer").GetString());
        Assert.Equal(100, body.GetProperty("errors").GetArrayLength());
    }

    [Fact]
    public void More_than_100_field_errors_are_rejected()
    {
        var errors = Enumerable.Range(0, 101).Select(i => new FieldError(PlatformErrors.Required, ErrorPointer.Input("items", i))).ToArray();

        Assert.Throws<ArgumentException>(() => new Error(PlatformErrors.InputInvalid) { Errors = errors });
    }

    [Fact]
    public async Task Error_exception_response_is_written_before_a_catch_all_handler()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddExceptionHandler<CatchAllHandler>();
        builder.Services.AddKunErrors();
        builder.Services.AddKunErrors();
        await using var app = builder.Build();
        app.UseKunErrors();
        app.MapGet("/validate", string () => throw new ErrorException(new Error(PlatformErrors.InputInvalid)
        {
            Errors =
            [
                new FieldError(PlatformErrors.Required, ErrorPointer.Input("items", 0, "name")),
                new FieldError(PlatformErrors.OutOfRange, ErrorPointer.Input("amount")),
            ],
        }));
        await app.StartAsync(TestContext.Current.CancellationToken);
        using var client = app.GetTestClient();

        using var response = await client.GetAsync(new Uri("/validate", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ErrorDetailJson.MediaType, response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var body = document.RootElement;
        Assert.Empty(Contracts.Validate("error-detail", body));
        Assert.Empty(Contracts.CheckSemantics("error-detail", body));
        Assert.Equal("INPUT_INVALID", body.GetProperty("code").GetString());
        Assert.Equal("#/items/0/name", body.GetProperty("pointer").GetString());
        Assert.Equal("OUT_OF_RANGE", body.GetProperty("errors")[1].GetProperty("code").GetString());
    }

    private sealed class CatchAllHandler : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = 599;
            return ValueTask.FromResult(true);
        }
    }
}
