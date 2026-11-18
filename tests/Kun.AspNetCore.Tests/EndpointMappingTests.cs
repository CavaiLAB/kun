using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using FluentAssertions;
using Kun.AspNetCore.Endpoints;
using Kun.Core.Cqrs;
using Kun.Mediator.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Kun.AspNetCore.Tests;

/// <summary>
/// <see cref="KunEndpointRouteBuilderExtensions.MapCommand"/> and
/// <see cref="KunEndpointRouteBuilderExtensions.MapQuery"/> bind the request and dispatch it through the mediator.
/// </summary>
public sealed class EndpointMappingTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task MapCommand_DispatchesJsonBodyThroughMediator()
    {
        await using var app = await StartAsync(endpoints =>
            endpoints.MapCommand<RenameItem, string>("/items/rename", EndpointJson.Default.RenameItem, EndpointJson.Default.String));

        var response = await app.GetTestClient().PostAsJsonAsync("/items/rename", new RenameItem("kun"), EndpointJson.Default.RenameItem, Ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync(EndpointJson.Default.String, Ct)).Should().Be("renamed:kun");
    }

    [Fact]
    public async Task MapCommand_NullBody_ReturnsBadRequest()
    {
        await using var app = await StartAsync(endpoints =>
            endpoints.MapCommand<RenameItem, string>("/items/rename", EndpointJson.Default.RenameItem, EndpointJson.Default.String));

        using var body = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await app.GetTestClient().PostAsync("/items/rename", body, Ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MapQuery_BindsQueryStringAndDispatchesThroughMediator()
    {
        await using var app = await StartAsync(endpoints =>
            endpoints.MapQuery<FindItem, string>("/items", EndpointJson.Default.String));

        var response = await app.GetTestClient().GetAsync("/items?name=kun", Ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync(EndpointJson.Default.String, Ct)).Should().Be("found:kun");
    }

    private static async Task<WebApplication> StartAsync(Action<IEndpointRouteBuilder> map)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddKunMediator(mediator => mediator
            .AddRequestHandler<RenameItem, string, RenameItemHandler>()
            .AddRequestHandler<FindItem, string, FindItemHandler>());
        var app = builder.Build();
        map(app);
        await app.StartAsync(Ct);
        return app;
    }
}

internal sealed record RenameItem(string Name) : ICommand<string>;

internal sealed class FindItem : IQuery<string>
{
    public string Name { get; set; } = "";
}

internal sealed class RenameItemHandler : IRequestHandler<RenameItem, string>
{
    public ValueTask<string> Handle(RenameItem request, CancellationToken cancellationToken) => ValueTask.FromResult("renamed:" + request.Name);
}

internal sealed class FindItemHandler : IRequestHandler<FindItem, string>
{
    public ValueTask<string> Handle(FindItem request, CancellationToken cancellationToken) => ValueTask.FromResult("found:" + request.Name);
}

[JsonSerializable(typeof(RenameItem))]
[JsonSerializable(typeof(string))]
internal sealed partial class EndpointJson : JsonSerializerContext;
