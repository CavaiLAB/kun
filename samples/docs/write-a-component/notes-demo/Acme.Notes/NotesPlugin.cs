using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kun.Plugin.Sdk;
using Kun.Runtime;

namespace Acme.Notes;

/// <summary>组件入口：一个组件 id 对应一个入口类。</summary>
[KunPlugin("com.example.notes")]
public sealed class NotesPlugin : IKunPlugin
{
    /// <summary>本组件提供的服务键。别的组件在自己的 Inject 里写上它，就能拿到这个服务。</summary>
    public static readonly ServiceKey Notes = new("ExampleNotes");

    /// <summary>本组件挂载前要先就绪的服务键；这个组件不依赖别人。</summary>
    public IReadOnlyList<ServiceKey> Inject => [];

    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    {
        // Provide 的登记进当前插件的效果栈，卸载时自动撤销。
        ctx.Provide(Notes, new NoteBook());
        return ValueTask.CompletedTask;
    }
}

/// <summary>本组件对外提供的服务。</summary>
public sealed class NoteBook
{
    private readonly ConcurrentDictionary<Guid, string> _notes = new();

    public Guid Add(string title)
    {
        var id = Guid.CreateVersion7();
        _notes[id] = title;
        return id;
    }

    public int Count => _notes.Count;
}

/// <summary>声明一项能力：输入、输出的 Schema 由类型派生，工具描述由清单工具写进 tools/。</summary>
[Capability("note.create", typeof(NoteCreated), typeof(NotesJson),
    Description = "为当前租户新建一条笔记；修改已有笔记请用其它能力。",
    SideEffect = "write", Idempotent = true, RequiredScopes = ["com.example.notes:write"])]
public sealed partial record CreateNote(Guid IdempotencyKey, string Title);

public sealed record NoteCreated(Guid Id, string Title);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(CreateNote))]
[JsonSerializable(typeof(NoteCreated))]
public sealed partial class NotesJson : JsonSerializerContext;
