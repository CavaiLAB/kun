---
title: 在 .NET 里接权限
description: 给命令与 HTTP 端点声明权限点，在业务代码之前判定：内核包接命令入口，平台档的包接 HTTP 入口；再看权限引擎的平台模式与还没有的部分。
part: start
project: platform
audience: [developer, agent]
updated: 2026-11-26
---

# 在 .NET 里接权限

平台上不论做什么系统，权限都走 Kun 这一套（已定，2026-11-26 的平台级裁定，Kun 一侧登记在 [S1-DEC-043](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-043.md) D7）。这一页讲代码用 .NET 写、跑在自己的 ASP.NET Core 宿主或 Kun 的宿主里时怎么接。Kun 宿主以外的系统怎么问权限，现在还没有现成的接口，见文末[还没有的](#planned)。

第 1 至 5 步在 2026-11-26 用 .NET SDK 10.0.404、Kun 2026.11.24 照着跑过一遍；第 6 节讲的平台模式，对照的是仓库里客户参考应用的测试。

第 2 步的代码在 Kun 仓库的 [samples/docs/authorization-dotnet](https://github.com/CavaiLAB/kun/tree/main/samples/docs/authorization-dotnet) 里有一份。仓库的 CI 照第 1 至 5 步做一遍：平时用当前源码打的包，每次发版用刚发布的包，页面上贴的状态码、响应体、错误码和第 5 步的表都要对得上。

## 先认几个词

- **权限点**：`<组件 id>:<资源>:<动作>`，例如 `com.example.notes:note:read`。组件 id 里有点号，按冒号分三段。
- **执行点**：在入口上、业务代码之前做判定的那一层。Kun 有三种：HTTP 端点、命令管道、gRPC 方法。三种都在处理器之前判定，缺了判定器就拒绝。
- **判定器**：`IPermissionChecker`，回答「这个主体能不能做这件事」。Kun 在内核包 `Kun.Authorization` 里只定义这个端口；平台的权限引擎（平台档的 `Kun.Authorization.Rbac`）是它的一个实现。
- **主体**：`AuthorizationSubject`，由主体编号、租户、主体类型组成，从已认证的身份里读：`sub`、`tenant_id`、`principal_kind`（`user`、`service` 或 `agent`，不写按 `user`）。权限不写进令牌，也不从令牌里的角色推导，每次都问判定器。

## 两种模式

| 模式 | 判定器 | 用在哪 | 现状 |
|---|---|---|---|
| 库模式 | 宿主自己登记的 `IPermissionChecker` | 自己的 ASP.NET Core 服务，只要执行点 | 已有，第 1 至 5 步 |
| 平台模式 | 权限组件 `qian.authz`（`Kun.Authorization.Rbac`）：权限点目录、租户角色、默认授权、决策审计、身份修订比对 | 平台宿主，以及装了权限组件的宿主 | 已有，要 PostgreSQL，第 6 节 |

## 1 建项目，装包

```bash
dotnet new web -n Notes.Authz
cd Notes.Authz
dotnet add package Kun.Mediator --version 2026.11.24
dotnet add package Kun.Errors.AspNetCore --version 2026.11.24
dotnet add package Kun.Security --version 2026.11.24
```

三个都是内核档的包，在 nuget.org 上。`Kun.Security` 提供从已认证身份读主体的 `ICurrentUser`，并带上了 ASP.NET Core 的 JWT 认证包。

## 2 写命令与判定器

把 `Program.cs` 整个换成：

```csharp
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
```

- 命令实现 `IPermissionProtectedRequest`，用静态的 `RequiredPermissions` 声明全部所需权限，全部放行才执行处理器。
- `AddPermissionBehavior` 把判定接在这个命令的处理器前面；拒绝时用你给的工厂（这里是 `Result<ExportResult>.Fail`）返回失败：未认证是 `UNAUTHENTICATED`（401），其余拒绝一律是 `AUTHZ_DENIED`（403）。

## 3 签本机测试令牌

```bash
dotnet user-jwts create --name alice -o token
dotnet user-jwts create --name bob -o token
dotnet user-jwts create --name carol -o token
dotnet build
```

每条命令打印一个令牌，`sub` 就是 `--name` 的值；把三个令牌分别记下来。`dotnet user-jwts` 是 .NET SDK 自带的本机开发工具：签名密钥放进你的用户机密，受众取项目 `Properties/launchSettings.json` 里的地址，只在 Development 环境下生效。它往项目文件里加了 `UserSecretsId`，所以要重新构建一次；不重新构建，令牌会因为找不到签名密钥而全部被拒。

生产里换成真实的令牌校验。平台里是网关校验外部令牌、再签发短期的服务间令牌，下游服务用 `AddKunServiceAuthentication` 校验，见[服务间 JWT 认证（英文）](/sdk/kun/security/s2s-jwt)。

## 4 跑起来

```bash
dotnet run --launch-profile http
```

终端会打印 `Now listening on: http://localhost:<端口>`，端口是建项目时随机分配的、写在 `launchSettings.json` 里，令牌的受众就是这个地址。另开一个终端，把地址和三个令牌放进环境变量：

```bash
BASE=http://localhost:5013
ALICE=<alice 的令牌>
BOB=<bob 的令牌>
CAROL=<carol 的令牌>
```

```bash
curl -s -X POST "${BASE}/notes/export" -w '\nHTTP %{http_code}\n'
curl -s -X POST "${BASE}/notes/export" -H "Authorization: Bearer ${BOB}" -w '\nHTTP %{http_code}\n'
curl -s -X POST "${BASE}/notes/export" -H "Authorization: Bearer ${ALICE}" -w '\nHTTP %{http_code}\n'
```

不带令牌是 401，bob 只有读权限是 403，alice 有导出权限是 200：

```json
{"type":"https://qian.dev/problems/protocol/UNAUTHENTICATED","title":"未认证","status":401,"detail":"缺少有效凭据或令牌已失效，请重新认证。","instance":"/notes/export","code":"UNAUTHENTICATED","message":"缺少有效凭据或令牌已失效，请重新认证。","trace_id":"1bc697f1545c1b9604d230334544d66a"}
```

```json
{"type":"https://qian.dev/problems/protocol/AUTHZ_DENIED","title":"无权执行","status":403,"detail":"当前主体无权执行该操作。可调用权限解释接口查看缺少的权限点。","instance":"/notes/export","code":"AUTHZ_DENIED","message":"当前主体无权执行该操作。可调用权限解释接口查看缺少的权限点。","trace_id":"2811c7a2f3b2c71476f97ad6aa7a6571"}
```

```json
{"count":3}
```

**忘了接执行点会怎样。** 把 `AddPermissionBehavior` 那一行删掉再请求，命令不会执行：第一次 `Send` 就抛出 `InvalidOperationException`（「受保护请求未装配权限行为：ExportNotesCommand」），响应是 500 的 `INTERNAL`。实现了 `IPermissionProtectedRequest` 的命令不会因为漏装而被放行。

## 5 再接 HTTP 端点

HTTP 端点的执行点 `RequireKunPermission` 在平台档的包 `Kun.Authorization.AspNetCore` 里，从 CavaiLAB 的 GitHub Packages 装，要一个带 `read:packages` 权限的 GitHub 令牌。

先停掉服务，在项目目录里放上发布页附带的包源映射，设好凭据再加包：

```bash
curl -fsSL -o NuGet.config https://github.com/CavaiLAB/kun/releases/download/v2026.11.24/NuGet.config
export KUN_PLATFORM_FEED_USER=<你的 GitHub 用户名>
export KUN_PLATFORM_FEED_TOKEN=<带 read:packages 权限的令牌>
dotnet add package Kun.Authorization.AspNetCore --version 2026.11.24
```

这份 `NuGet.config` 写好了两个包源与逐包的映射：内核档从 nuget.org，平台档从 GitHub Packages，凭据从上面两个环境变量读，令牌不落进文件。

在 `Program.cs` 里加三处：

```csharp
using Kun.Authorization.AspNetCore;
```

```csharp
// 放在登记 DemoPermissionChecker 的下一行
// HTTP 入口的执行点（平台档的包）：普通宿主用上面登记的判定器。
builder.Services.AddKunAuthorization();
```

```csharp
// 放在 app.MapPost("/notes/export", …) 之前
app.MapGet("/notes", () => Results.Ok(new[] { "周报", "月报" }))
    .RequireKunPermission("com.example.notes:note:read");
```

重新构建、启动（`dotnet run --launch-profile http`），启动日志里有一行「授权已使用库模式，只检查 IPermissionChecker，不写决策审计、不比对身份修订。」。四种身份的结果：

| 调用方 | `GET /notes`（要 read） | `POST /notes/export`（要 export） |
|---|---|---|
| 不带令牌 | 401 | 401 |
| bob（只有 read） | 200 | 403 |
| carol（没有授权） | 403 | 403 |
| alice（read 与 export） | 200 | 200 |

- `RequireKunPermission` 可以调多次，表示全部权限都要放行；权限点不是 `<组件 id>:<资源>:<动作>` 的全名时，映射端点时就抛 `ArgumentException`。
- 它同时给端点加上 401、403 的 ErrorDetail 响应声明，并把权限点写进 OpenAPI 的 `x-qian-permission`。
- 库模式对 `agent` 主体与带委托链的请求一律拒绝。

## gRPC 方法

gRPC 方法上标 `[RequiresPermission]`，再装授权拦截器（`AddKunGrpc` 在内核档的 `Kun.Grpc` 里，`AddKunAuthorizationInterceptor` 在 `Kun.Authorization.AspNetCore` 里）：

```csharp
builder.Services.AddKunGrpc(options => options.AddKunAuthorizationInterceptor());
```

```csharp
[RequiresPermission("com.example.notes:note:read")]
public override Task<NoteReply> GetNote(NoteRequest request, ServerCallContext context) { /* … */ }
```

宿主启动时校验方法上的权限点，不是全名就报错；没装拦截器时，标了权限的方法一律拒绝执行。本页没有单独演示 gRPC；仓库里客户参考应用的测试覆盖了 gRPC 与 HTTP 对同一权限点判定一致、带委托链的令牌在两种入口都被拒绝。

## 6 平台模式：权限引擎

平台宿主不让每个服务自己写判定器，而是装权限组件 `qian.authz`（`Kun.Authorization.Rbac`）。下面是现状与入口，没有做成一步步的教程。

**组件声明自己的权限点与默认角色。** 组件文件夹里两份文件，以客户参考应用为例：

```json
[
  { "resource": "customer", "actions": ["read"] },
  { "resource": "customer", "actions": ["write"] },
  { "resource": "customer", "actions": ["export"], "risk": { "export": "high" }, "stepUp": ["export"], "terminals": { "export": ["web"] } }
]
```

上面是 `metadata/permissions.json` 的写法（节选）；权限点的全名由组件 id 拼上资源与动作，如 `qian.sample.crm:customer:read`。也可以在 .NET 代码里用 `[Permission("customer", "export", Risk = PermissionRisk.High, RequiresStepUp = true, Terminals = ["web"])]` 声明（特性在平台档的 `Kun.Authorization.Rbac.Abstractions` 里），再由[清单工具](/start/guides/write-a-component)派生出这份文件。默认角色写在 `metadata/roles.json`：

```json
[
  { "role": "tenant.owner", "grant": ["qian.sample.crm:customer:*"] },
  { "role": "tenant.user", "grant": ["qian.sample.crm:customer:read"] }
]
```

**平台模式做的事。** 权限点持久化成目录；租户开通时建四个内置角色（`tenant.owner`、`tenant.admin`、`tenant.developer`、`tenant.user`），按各组件的 `roles.json` 写默认授权；判定查真实的角色与授权，带主体快照缓存，授予或撤销后不等缓存过期；判定写决策审计（放行的判定可以按比例采样，缺省全写）；令牌带的身份修订旧了，返回 401。谁成为租户的管理员，由开通流程的初始授权决定，不从令牌里的角色推出来。

**宿主怎么装。** 平台模式用另一个重载登记：`services.AddKunAuthorization(http => …)`，回调返回当前请求可注入权限组件的插件上下文；`app.UseKunAuthorization()` 放在 `app.UseAuthorization()` 之后。身份、可信的租户边界与权限组件要先就绪。规则见 [Kun.Authorization.AspNetCore 的说明](https://github.com/CavaiLAB/kun/blob/main/src/Kun.Authorization.AspNetCore/README.md)与[权限组件的说明](https://github.com/CavaiLAB/kun/blob/main/src/Kun.Authorization.Rbac/README.md)。

**能照着看的。** 客户参考应用 [samples/Kun.Sample.Crm](https://github.com/CavaiLAB/kun/tree/main/samples/Kun.Sample.Crm) 装的就是真实的权限组件，它的测试 `tests/Kun.Sample.Crm.Tests` 里的 `IdentityAuthzTests` 起真实的 PostgreSQL 容器，验了开通后 Owner 有读、写、导出，使用者只有读；gRPC 与 HTTP 判定一致；带委托链的令牌被拒；撤销授予后旧令牌失效。跑这组测试要 Docker，前置条件见 [tests/README.md](https://github.com/CavaiLAB/kun/blob/main/tests/README.md)。

**两个只读的 HTTP 端点**（平台模式，`MapKunAuthz`）：`GET /api/authz/me/permissions` 列出当前主体在本租户持有的权限点，带风险、step-up、终端限制，界面可以据此决定显示什么，判定仍在后端入口上做；`GET /api/authz/meta` 发布数据权限的元数据（有哪些数据范围、过滤条件、系统变量），不返回实际的授予。

## 还没有的 {#planned}

| 想做的 | 现状 |
|---|---|
| Kun 宿主以外的系统来问权限：管理端、工作区服务、页面插件、别的语言写的服务 | 远程的授权接口与客户端包是规划，[S1-AZ-005](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-AZ-005.md) 没有开始。现在 HTTP 上只有上面两个只读端点 |
| 数据权限与字段权限，比如按部门、按人过滤数据 | 端口已有（`IDataPermissionFilter`、`IFieldPolicy`），求值没有做；平台模式下现在一律拒绝：数据过滤给恒假条件，字段一个也不给 |
| Agent 主体与委托 | 没有启用：带委托链的请求在三种入口都被拒绝 |
| 角色管理接口，组织、岗位、组的继承 | 规划（S1-AZ-005） |
| 用 `qian add authz` 单独安装权限组件 | 没有收口（S1-RT-040） |

## 再往后

- 组件怎么写、清单怎么派生：[写一个组件](/start/guides/write-a-component)。
- 权限的设计与取舍：[ADR-006 鉴权模型](/design/adr/adr-006-authorization-model)；权限组件在组件目录里的条目：[qian.authz](/components/platform/qian.authz)。
- 判定端口与三种入口的完整规则：[Kun.Authorization 的说明](https://github.com/CavaiLAB/kun/blob/main/src/Kun.Authorization/README.md)。
