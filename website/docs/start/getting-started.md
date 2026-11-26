---
title: 上手：在 ASP.NET Core 里用上 Kun
description: 从一个空的 ASP.NET Core 项目开始，用 nuget.org 上的两个 Kun 内核包写一个命令，出错时返回统一的 ErrorDetail；不需要任何令牌。
part: start
project: platform
audience: [developer, agent]
updated: 2026-11-26
---

# 上手：在 ASP.NET Core 里用上 Kun

这一页从一个空项目开始，用 Kun 的两个内核包写一个能跑的 HTTP 服务：请求经 `Kun.Mediator` 调度到命令处理器，业务失败用 `Result` 返回，出错时响应写成统一的错误格式 ErrorDetail（`application/problem+json`）。

下面的命令与输出在 2026-11-26 用 .NET SDK 10.0.404、Kun 2026.11.24 照着跑过一遍。

## 先准备

- .NET SDK 10：`dotnet --version` 显示 10.0.100 或更新。
- 能访问 nuget.org。Kun 内核档的包都在 nuget.org 上，装它们不需要令牌；平台档的包在 GitHub Packages 上，要令牌，这一页用不到，见文末[包从哪里装](#packages)。
- `curl`，用来发请求。

## 1 建项目，装包

```bash
dotnet new web -n Hello.Kun
cd Hello.Kun
dotnet add package Kun.Mediator --version 2026.11.24
dotnet add package Kun.Errors.AspNetCore --version 2026.11.24
```

版本号是写本文时 Kun 最新的发布版；现在的版本见[版本矩阵](/start/versions)，Kun 的包都用同一个版本。`Kun.Mediator` 会一起带上 `Kun.Core`、`Kun.Errors` 与 `Kun.Authorization`。

## 2 写一个命令

把 `Program.cs` 整个换成：

```csharp
using System.Collections.Concurrent;
using Kun.Core.Cqrs;
using Kun.Core.Results;
using Kun.Errors;
using Kun.Errors.AspNetCore;
using Kun.Mediator;
using Kun.Mediator.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 错误码目录：平台内置的错误码已经在里面，这里再登记本应用自己的一个。
builder.Services.AddKunErrors(catalog => catalog.Register(NoteErrors.TitleTaken));

// 命令调度：一个命令对应一个处理器，显式登记。
builder.Services.AddKunMediator(m =>
    m.AddRequestHandler<CreateNoteCommand, Result<NoteCreated>, CreateNoteHandler>());

builder.Services.AddSingleton<NoteBook>();

var app = builder.Build();

// 未处理的异常、空响应体的 4xx / 5xx 都写成 ErrorDetail（application/problem+json）。
app.UseKunErrors();

app.MapPost("/notes", async (CreateNoteCommand command, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return result.IsSuccess ? Results.Ok(result.Value) : KunResults.Error(result.Error);
});

app.Run();

/// <summary>新建一条笔记。业务失败用 Result 返回，不抛异常。</summary>
public sealed record CreateNoteCommand(string Title) : ICommand<Result<NoteCreated>>;

public sealed record NoteCreated(Guid Id, string Title);

public sealed class CreateNoteHandler(NoteBook book) : IRequestHandler<CreateNoteCommand, Result<NoteCreated>>
{
    public ValueTask<Result<NoteCreated>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        if (!book.Titles.TryAdd(request.Title, id))
        {
            var error = new Error(NoteErrors.TitleTaken, new Dictionary<string, string> { ["title"] = request.Title });
            return ValueTask.FromResult<Result<NoteCreated>>(error);
        }
        return ValueTask.FromResult<Result<NoteCreated>>(new NoteCreated(id, request.Title));
    }
}

public sealed class NoteBook
{
    public ConcurrentDictionary<string, Guid> Titles { get; } = new(StringComparer.Ordinal);
}

/// <summary>本应用的错误码：大写下划线，全局唯一；归属写成反向域名风格的组件 id。</summary>
public static class NoteErrors
{
    public static readonly ErrorCode TitleTaken = ErrorCode.Business(
        "NOTE_TITLE_TAKEN",
        409,
        "com.example.notes",
        new Dictionary<string, string> { ["zh-CN"] = "标题已存在", ["en"] = "Title already exists" },
        new Dictionary<string, string> { ["zh-CN"] = "已有标题为 {title} 的笔记。", ["en"] = "A note titled {title} already exists." });
}
```

## 3 跑起来

```bash
dotnet run --urls http://127.0.0.1:5199
```

另开一个终端，新建一条笔记：

```bash
curl -i http://127.0.0.1:5199/notes --json '{"title":"周报"}'
```

返回 `200`，响应体是新笔记（`id` 每次不同）：

```json
{"id":"01a2159a-31e4-7b73-8913-c2a0189c145a","title":"周报"}
```

同一个标题再发一次，处理器返回业务失败，响应是 `409`、`Content-Type: application/problem+json`（`trace_id` 每次不同，排查时凭它查日志与链路）：

```json
{"type":"https://qian.dev/problems/business/NOTE_TITLE_TAKEN","title":"标题已存在","status":409,"detail":"已有标题为 周报 的笔记。","instance":"/notes","code":"NOTE_TITLE_TAKEN","message":"已有标题为 周报 的笔记。","trace_id":"ab3bbb4b5c1ae95af67a42a7f6a9cdfc"}
```

请求一个不存在的路径，`UseKunErrors()` 把空响应体的 404 也写成同样的格式：

```bash
curl -s http://127.0.0.1:5199/nope
```

```json
{"type":"https://qian.dev/problems/protocol/NOT_FOUND","title":"资源不存在","status":404,"detail":"请求的资源不存在或对当前调用者不可见。","instance":"/nope","code":"NOT_FOUND","message":"请求的资源不存在或对当前调用者不可见。","trace_id":"ff8ca2ccfaab9d907a161fbe8f586d91"}
```

问题类型地址里的 `qian.dev` 是保留下来的历史字面，不是一个能打开的网站。

## 刚才用到了什么

| 用到的 | 在哪个包 | 做什么 |
|---|---|---|
| `ICommand`、`IRequestHandler` | `Kun.Core` | 命令与处理器的接口；只读的请求用 `IQuery` |
| `Result`、`Result<T>` | `Kun.Core` | 业务失败的返回值，带一个 `Error`；异常只留给并发、网络这类系统性故障 |
| `ErrorCode`、`Error` | `Kun.Errors` | 错误码分协议、业务、字段三类；业务错误只能是 409 或 422；文案按语言写，`zh-CN` 必填 |
| `AddKunMediator`、`ISender` | `Kun.Mediator` | 按请求类型找到处理器；处理器外面还可以套行为，比如[权限判定](/start/guides/authorization-dotnet) |
| `AddKunErrors`、`UseKunErrors`、`KunResults.Error` | `Kun.Errors.AspNetCore` | 把错误写成 ErrorDetail；错误码要先登记进目录，没登记的渲染时报错 |

这几条与 Kun 自己的代码约定一致：命令与查询分开，业务失败走 `Result`，对外的错误只有 ErrorDetail 一种格式。约定全文在仓库的 [AGENTS.md](https://github.com/CavaiLAB/kun/blob/main/AGENTS.md)。

## 文案的语言

ErrorDetail 的文案按当前请求的界面语言取，找不到时用 `zh-CN`。要按请求头 `Accept-Language` 切换，宿主自己装 ASP.NET Core 的请求本地化：

```csharp
builder.Services.AddRequestLocalization(o => o
    .SetDefaultCulture("zh-CN")
    .AddSupportedCultures("zh-CN", "en")
    .AddSupportedUICultures("zh-CN", "en"));

// 放在 app.UseKunErrors() 之后
app.UseRequestLocalization();
```

装上以后带 `Accept-Language: en` 再发一次重复的标题，响应头是 `Content-Language: en`，`title` 变成 `Title already exists`。

## 包从哪里装 {#packages}

Kun 的 .NET 包分两档，每个发布版一起发：

| 档位 | 是什么 | 从哪里装 |
|---|---|---|
| 内核档 | 通用机制：错误、命令调度、租户上下文、事件、数据访问约定、令牌校验、网关、存储、配置；不知道租户目录、角色这些产品概念 | nuget.org，不要令牌 |
| 平台档 | 带产品语义的组件：身份服务、权限引擎、租户、文件、能力目录、注册表、生命周期 | CavaiLAB 的 GitHub Packages，要一个带 `read:packages` 权限的 GitHub 令牌 |

每个正式版的 [GitHub Release](https://github.com/CavaiLAB/kun/releases) 附两个文件：`release-packages.json` 列出全部包与档位；`NuGet.config` 已经写好两个包源与逐包的映射，凭据从环境变量 `KUN_PLATFORM_FEED_USER`（GitHub 用户名）与 `KUN_PLATFORM_FEED_TOKEN`（令牌）读。要用平台档的包，把这份 `NuGet.config` 放进项目目录，设好这两个环境变量再还原。升级与旧版本的映射见 [NuGet 包迁移指南](/sdk/kun/guides/package-migration)。

## 下一步

- [写一个组件](/start/guides/write-a-component)：把一项能力做成组件，派生清单，装进宿主。
- [在 .NET 里接权限](/start/guides/authorization-dotnet)：给命令和 HTTP 端点声明权限点，在处理器之前判定。
- Kun 框架的指南：[Outbox 与集成事件](/sdk/kun/guides/outbox-and-integration-events)、[命令校验](/sdk/kun/guides/validating-commands)、[多租户策略](/sdk/kun/guides/saas-multitenancy)。
- 整套样例服务（商品目录与订单，经 Aspire 编排）：[Getting Started（英文）](/sdk/kun/getting-started)。它要 Docker，还要先信任本机的 ASP.NET Core 开发证书；本页没有替你核对过这一篇。
