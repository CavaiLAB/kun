using System.Text.Json;
using Json.Schema;
using Kun.Authorization;
using Kun.Authorization.AspNetCore;
using Kun.Data.Dialect.Pg;
using Kun.Files.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kun.Data.AspNetCore.Tests;

/// <summary>端点实际要求的权限点必须在 plugins/qian.data/metadata 声明，元数据符合 authz Schema，默认角色只授予声明过的权限点。</summary>
public sealed class DataPermissionMetadataTests
{
    private static readonly string Root = FindRoot();

    public static TheoryData<string, string> CallbacksWithoutEnforcement()
    {
        var data = new TheoryData<string, string>();
        foreach (var component in new[] { "files", "data" })
        {
            foreach (var callback in new[] { "metadata-only", "metadata-with-generic-policy", "requirement-for-another-permission", "anonymous" })
                data.Add(component, callback);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(CallbacksWithoutEnforcement))]
    public async Task 权限回调没有接上同一权限点的执行要求或端点匿名放行时端点构造失败(string component, string callback)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddKunDataTypes().AddKunPgDialect();
        await using var app = builder.Build();
        Action<RouteHandlerBuilder, string> requirePermission = callback switch
        {
            "metadata-only" => (route, permission) => route.WithMetadata(new RequiresPermissionAttribute(permission)),
            "metadata-with-generic-policy" => (route, permission) => route.WithMetadata(new RequiresPermissionAttribute(permission)).RequireAuthorization(),
            "requirement-for-another-permission" => (route, permission) =>
                route.WithMetadata(new RequiresPermissionAttribute(permission)).RequireKunPermission("sample.other:item:read"),
            _ => (route, permission) => route.RequireKunPermission(permission).AllowAnonymous(),
        };
        Map(app, component, requirePermission);
        var error = Assert.Throws<InvalidOperationException>(() => ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray());
        Assert.Contains("没有接到 PEP", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("files")]
    [InlineData("data")]
    public async Task 外层分组带匿名放行标记时端点构造失败(string component)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddKunDataTypes().AddKunPgDialect();
        await using var app = builder.Build();
        Map(app.MapGroup("/host").AllowAnonymous(), component, (route, permission) => route.RequireKunPermission(permission));
        var error = Assert.Throws<InvalidOperationException>(() => ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray());
        Assert.Contains("匿名放行", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("files")]
    [InlineData("data")]
    public async Task 权限回调以实现执行要求接口的自定义要求接入时端点可以构造(string component)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddKunDataTypes().AddKunPgDialect();
        await using var app = builder.Build();
        Map(app, component, (route, permission) => route.WithMetadata(new RequiresPermissionAttribute(permission))
            .RequireAuthorization(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new HostPermissionRequirement(permission)).Build()));
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>().ToArray();
        Assert.NotEmpty(endpoints);
        Assert.All(endpoints, endpoint => Assert.Contains(endpoint.Metadata.GetOrderedMetadata<AuthorizationPolicy>().SelectMany(policy => policy.Requirements),
            requirement => requirement is HostPermissionRequirement host && host.Permission == Assert.Single(endpoint.Metadata.GetOrderedMetadata<RequiresPermissionAttribute>()).Permission));
    }

    private static void Map(IEndpointRouteBuilder endpoints, string component, Action<RouteHandlerBuilder, string> requirePermission)
    {
        if (component == "files") endpoints.MapKunFiles(_ => throw new InvalidOperationException(), requirePermission);
        else endpoints.MapKunDataTypes(requirePermission);
    }

    private sealed record HostPermissionRequirement(string Permission) : IAuthorizationRequirement, IPermissionEnforcementRequirement;

    [Fact]
    public async Task 每个数据端点都要求已声明的权限点_默认角色只授予租户内权限()
    {
        var permissions = Read("permissions.json");
        var roles = Read("roles.json");
        Assert.True(Schema("authz-permissions.schema.json").Evaluate(permissions).IsValid);
        Assert.True(Schema("authz-roles.schema.json").Evaluate(roles).IsValid);
        var declared = permissions.EnumerateArray().SelectMany(item => item.GetProperty("actions").EnumerateArray()
            .Select(action => "qian.data:" + item.GetProperty("resource").GetString() + ":" + action.GetString())).ToHashSet(StringComparer.Ordinal);
        Assert.Equal([DataPermissions.ConnectionRead, DataPermissions.ConnectionTest, DataPermissions.ConnectionWrite, DataPermissions.TypeDelete, DataPermissions.TypeRead],
            declared.Order(StringComparer.Ordinal));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        TestIdentity.Add(builder.Services);
        builder.Services.AddKunDataTypes().AddKunPgDialect();
        await using var app = builder.Build();
        app.MapKunData((route, permission) => route.RequireKunPermission(permission));
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>().ToArray();
        Assert.Equal(8, endpoints.Length);
        Assert.All(endpoints, endpoint => Assert.Contains(Assert.Single(endpoint.Metadata.GetOrderedMetadata<RequiresPermissionAttribute>()).Permission, declared));

        foreach (var role in roles.EnumerateArray())
        {
            Assert.StartsWith("tenant.", role.GetProperty("role").GetString(), StringComparison.Ordinal);
            foreach (var grant in role.GetProperty("grant").EnumerateArray().Select(item => item.GetString()!))
                Assert.Contains(declared, permission => grant.EndsWith(":*", StringComparison.Ordinal)
                    ? permission.StartsWith(grant[..^1], StringComparison.Ordinal) : permission == grant);
            // 删除基准类型属平台级操作，组件默认模板不授予任何租户角色。
            Assert.DoesNotContain(role.GetProperty("grant").EnumerateArray(), item => item.GetString()!.StartsWith("qian.data:type:", StringComparison.Ordinal)
                && item.GetString() != DataPermissions.TypeRead);
        }
    }

    private static JsonElement Read(string name)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "plugins", "qian.data", "metadata", name)));
        return document.RootElement.Clone();
    }

    private static JsonSchema Schema(string name) =>
        JsonSchema.FromText(File.ReadAllText(Path.Combine(Root, "schemas", "v2", name)), new BuildOptions { SchemaRegistry = new SchemaRegistry() });

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kun.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("找不到仓库根目录。");
    }
}
