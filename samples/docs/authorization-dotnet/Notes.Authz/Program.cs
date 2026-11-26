using Kun.Authorization;
using Kun.Core.Cqrs;
using Kun.Core.Results;
using Kun.Errors.AspNetCore;
using Kun.Mediator;
using Kun.Mediator.Behaviors;
using Kun.Mediator.DependencyInjection;
using Kun.Security.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 认证用宿主自己的方案。这里是本机演示：令牌由 dotnet user-jwts 签发。
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddKunErrors();
// 当前主体：从已认证的主身份读 sub、tenant_id、principal_kind（缺省按 user）。
builder.Services.AddKunSecurity();
// 权限判定器：Kun 只定义端口，判定由它回答。这里用内存里的授权表演示。
builder.Services.AddSingleton<IPermissionChecker, DemoPermissionChecker>();

builder.Services.AddKunMediator(m =>
{
    m.AddRequestHandler<ExportNotesCommand, Result<ExportResult>, ExportNotesHandler>();
    // 命令入口的执行点：在处理器之前判定，拒绝时返回 Result 失败，不抛异常。
    m.AddPermissionBehavior<ExportNotesCommand, Result<ExportResult>>(Result<ExportResult>.Fail);
});

var app = builder.Build();
app.UseKunErrors();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/notes/export", async (ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(new ExportNotesCommand(), ct);
    return result.IsSuccess ? Results.Ok(result.Value) : KunResults.Error(result.Error);
});

app.Run();

/// <summary>导出笔记。所需权限在编译期写死，请求体改不了它。</summary>
public sealed record ExportNotesCommand : ICommand<Result<ExportResult>>, IPermissionProtectedRequest
{
    public static IReadOnlyList<string> RequiredPermissions { get; } = ["com.example.notes:note:export"];
}

public sealed record ExportResult(int Count);

public sealed class ExportNotesHandler : IRequestHandler<ExportNotesCommand, Result<ExportResult>>
{
    public ValueTask<Result<ExportResult>> Handle(ExportNotesCommand request, CancellationToken cancellationToken)
        => ValueTask.FromResult<Result<ExportResult>>(new ExportResult(3));
}

/// <summary>演示用的判定器：按主体编号查一张写死的授权表。真实系统里换成权限引擎。</summary>
public sealed class DemoPermissionChecker : IPermissionChecker
{
    private static readonly Dictionary<string, string[]> Grants = new(StringComparer.Ordinal)
    {
        ["alice"] = ["com.example.notes:note:read", "com.example.notes:note:export"],
        ["bob"] = ["com.example.notes:note:read"],
    };

    public ValueTask<PermissionDecision> CheckAsync(PermissionRequest request, CancellationToken cancellationToken = default)
    {
        var allow = request.Subject.Kind == "user"
            && Grants.TryGetValue(request.Subject.Id, out var granted) && granted.Contains(request.Permission);
        return ValueTask.FromResult(new PermissionDecision(allow, allow ? "granted" : "not_granted", allow ? ["demo"] : []));
    }

    public async ValueTask<IReadOnlyList<PermissionDecision>> BatchCheckAsync(IReadOnlyList<PermissionRequest> requests, CancellationToken cancellationToken = default)
    {
        var decisions = new List<PermissionDecision>(requests.Count);
        foreach (var request in requests)
            decisions.Add(await CheckAsync(request, cancellationToken));
        return decisions;
    }

    public ValueTask<IReadOnlyList<string>> ListPermissionsAsync(AuthorizationSubject subject, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IReadOnlyList<string>>(Grants.TryGetValue(subject.Id, out var granted) ? granted : []);

    public ValueTask<IReadOnlyList<string>> ListAllowedActionsAsync(AuthorizationSubject subject, string resourceType, string? resourceId = null, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IReadOnlyList<string>>([]);

    public ValueTask<PermissionDecision> ExplainAsync(PermissionRequest request, CancellationToken cancellationToken = default)
        => CheckAsync(request, cancellationToken);
}
