using System.Globalization;
using System.Text;
using FluentAssertions;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Kun.Errors;
using Kun.Grpc.DependencyInjection;
using Kun.Grpc.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.Grpc.Tests;

/// <summary>
/// Tests that the HTTP status of each error code maps to the correct gRPC <see cref="StatusCode"/>
/// via <see cref="KunResultToRpcInterceptor.MapStatus"/> and
/// <see cref="KunResultToRpcInterceptor.ToRpcException"/>.
/// </summary>
public sealed class ResultMappingTests
{
    [Fact]
    public void RegistrationKeepsAuthenticationAndTenantScopeBeforePermission()
    {
        var services = new ServiceCollection();
        services.AddKunGrpc();
        using var provider = services.BuildServiceProvider();
        var interceptors = provider.GetRequiredService<IOptions<GrpcServiceOptions>>().Value.Interceptors;
        interceptors.Select(entry => entry.Type).Should().Equal(typeof(KunTracingInterceptor),
            typeof(KunServerAuthInterceptor), typeof(TenantGrpcServerInterceptor),
            typeof(RequiredPermissionInterceptorGuard), typeof(KunResultToRpcInterceptor));
    }

    [Theory]
    [InlineData("BAD_REQUEST", StatusCode.InvalidArgument)]
    [InlineData("INPUT_INVALID", StatusCode.InvalidArgument)]
    [InlineData("UNAUTHENTICATED", StatusCode.Unauthenticated)]
    [InlineData("AUTHZ_DENIED", StatusCode.PermissionDenied)]
    [InlineData("INSUFFICIENT_SCOPE", StatusCode.PermissionDenied)]
    [InlineData("NOT_FOUND", StatusCode.NotFound)]
    [InlineData("REQUEST_TIMEOUT", StatusCode.DeadlineExceeded)]
    [InlineData("GATEWAY_TIMEOUT", StatusCode.DeadlineExceeded)]
    [InlineData("CONFLICT", StatusCode.FailedPrecondition)]
    [InlineData("PRECONDITION_FAILED", StatusCode.FailedPrecondition)]
    [InlineData("BUSINESS_RULE_VIOLATED", StatusCode.FailedPrecondition)]
    [InlineData("PAYLOAD_TOO_LARGE", StatusCode.ResourceExhausted)]
    [InlineData("RATE_LIMITED", StatusCode.ResourceExhausted)]
    [InlineData("BAD_GATEWAY", StatusCode.Unavailable)]
    [InlineData("UNAVAILABLE", StatusCode.Unavailable)]
    [InlineData("INTERNAL", StatusCode.Internal)]
    [InlineData("METHOD_NOT_ALLOWED", StatusCode.Internal)]
    public void ToRpcException_StatusCodeFollowsTheHttpStatusOfTheErrorCode(string code, StatusCode expectedCode)
    {
        // Arrange
        ErrorCatalog.CreateDefault().TryGet(code, out var errorCode).Should().BeTrue();

        // Act
        var ex = KunResultToRpcInterceptor.ToRpcException(new Error(errorCode!));

        // Assert
        ex.StatusCode.Should().Be(expectedCode);
    }

    [Fact]
    public void ErrorTrailersCarryTheHttpErrorDocumentVerbatim()
    {
        var catalog = ErrorCatalog.CreateDefault();
        var detail = catalog.CreateDetail(new Error(PlatformErrors.Conflict), "00000000000000000000000000000001",
            instance: "/customers/one");

        var trailers = GrpcErrorTrailers.Create(detail);

        trailers.GetValue("kun-error-code").Should().Be(detail.Code);
        trailers.GetValueBytes("qian-error-bin").Should().Equal(Encoding.UTF8.GetBytes(ErrorDetailJson.Serialize(detail)));
    }

    [Fact]
    public void ToRpcException_IncludesErrorCodeInTrailers()
    {
        // Act
        var ex = KunResultToRpcInterceptor.ToRpcException(new Error(PlatformErrors.Conflict));

        // Assert
        var codeTrailer = ex.Trailers.FirstOrDefault(e => e.Key == "kun-error-code");
        codeTrailer.Should().NotBeNull();
        codeTrailer!.Value.Should().Be("CONFLICT");
        var payload = ex.Trailers.Single(entry => entry.Key == "qian-error-bin");
        var detail = ErrorDetailJson.Deserialize(Encoding.UTF8.GetString(payload.ValueBytes));
        detail!.Code.Should().Be(codeTrailer.Value);
        detail.Status.Should().Be(409);
        detail.Message.Should().Be(ex.Status.Detail);
    }

    [Fact]
    public void ToRpcException_StatusDetailIsTheMessageInTheCurrentUiLanguage()
    {
        // Arrange
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
        try
        {
            // Act
            var ex = KunResultToRpcInterceptor.ToRpcException(new Error(PlatformErrors.NotFound));

            // Assert
            ex.Status.Detail.Should().Be("The requested resource does not exist or is not visible to the caller.");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void MapStatus_UnknownStatus_MapsToInternal()
    {
        KunResultToRpcInterceptor.MapStatus(418).Should().Be(StatusCode.Internal);
    }
}
