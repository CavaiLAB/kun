using System.Text.Json;
using Acme.Notes;
using Kun.Runtime;
using Microsoft.Extensions.DependencyInjection;

// 平台宿主从清单读 provides；这里照做，读同步之后的 plugin.json。
using var manifest = JsonDocument.Parse(File.ReadAllText(args[0]));
var provides = manifest.RootElement.GetProperty("provides").EnumerateArray()
    .Select(item => new ServiceKey(item.GetString()!)).ToArray();

await using var services = new ServiceCollection().BuildServiceProvider();
await using var runtime = new PluginLoader(services);

var entry = new PluginEntry("notes", "com.example.notes", "0.1.0", ScopeKey.Global, 0, JsonDocument.Parse("{}").RootElement);
await runtime.MountAsync([new PluginDefinition(entry, new NotesPlugin(), provides)]);
Console.WriteLine($"挂载后：{runtime.GetState("notes", ScopeKey.Global)}");

var notes = runtime.GetContext("notes", ScopeKey.Global).Inject<NoteBook>(NotesPlugin.Notes);
notes.Add("周报");
Console.WriteLine($"笔记数：{notes.Count}");

await runtime.UnmountAsync("notes", ScopeKey.Global);
Console.WriteLine($"卸载后挂载树里的条目数：{runtime.DumpConfig().Count}");
