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
