---
title: 写一个组件
description: 用组件开发包写一个带代码的组件：声明服务与能力，手写清单，用清单工具从代码派生其余字段，再装进宿主；另附无代码组件怎么起步。
part: start
project: platform
audience: [developer, agent]
updated: 2026-11-26
---

# 写一个组件

组件是 Kun 分发与安装的单位：一个文件夹，里面一份清单 `.qian-plugin/plugin.json`，加上声明式的内容（工具描述、权限点、角色、事件、技能等），可以带 .NET 代码，也可以一行代码都没有。租户、身份、权限这些平台能力本身也是这样的组件。概念见[组件](/start/concepts/component)。

这一页做一个带代码的组件 `com.example.notes`：它提供一个笔记服务，声明一项「新建笔记」的能力；清单里能从代码推出来的字段交给清单工具派生；最后写一个小宿主把它装上。末尾另讲[无代码的组件](#no-code)怎么起步。

下面的命令与输出在 2026-11-26 用 .NET SDK 10.0.404、Kun 2026.11.24 照着跑过一遍。

这一页让你写进文件的代码，在 Kun 仓库的 [samples/docs/write-a-component](https://github.com/CavaiLAB/kun/tree/main/samples/docs/write-a-component) 里有一份。仓库的 CI 照第 1 至 5 步做一遍：平时用当前源码打的包和当前源码构建的清单工具，每次发版用刚发布的包和发布标签，清单工具的输出与宿主的输出都要和页面上的对得上。

名字里带 `qian` 的地方（清单目录 `.qian-plugin/`、命令行 `qian`、平台组件 id 的前缀 `qian.`）是保留下来的运行期字面，不表示产品名。

## 先准备

- 照[上手](/start/getting-started)准备好 .NET SDK 10，能访问 nuget.org。
- Git。清单工具还没有发布成包，要从 Kun 仓库的发布标签构建（第 4 步）。

## 1 建类库，装包

```bash
mkdir notes-demo && cd notes-demo
dotnet new classlib -n Acme.Notes
rm Acme.Notes/Class1.cs
cd Acme.Notes
dotnet add package Kun.Runtime --version 2026.11.24
dotnet add package Kun.Plugin.Sdk --version 2026.11.24
dotnet add package Kun.Plugin.Sdk.Generators --version 2026.11.24
cd ..
```

三个都是内核档的包，在 nuget.org 上：`Kun.Runtime` 是组件运行时（上下文、服务键、效果栈、装载器），`Kun.Plugin.Sdk` 放代码声明用的特性，`Kun.Plugin.Sdk.Generators` 是源生成器，`dotnet add package` 会把它标成只在构建时用（`PrivateAssets` 为 `all`）。

## 2 写入口与能力

新建 `Acme.Notes/NotesPlugin.cs`：

```csharp
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
```

- `[KunPlugin("com.example.notes")]` 标出组件入口。组件 id 是反向域名风格的小写名字。
- `Inject` 列出挂载前要先就绪的服务键；`ApplyAsync` 里用 `Provide` 登记本组件提供的服务，登记进效果栈，卸载时按注册的逆序撤销。
- `[Capability]` 标在 `partial` 的请求类型上：输入、输出的 JSON Schema 从源生成的 `JsonTypeInfo` 导出，序列化设置就是契约（上面的写法不接受多余字段、构造参数必填）；描述、副作用、幂等、所需 scope 写进工具描述。写操作缺省记审计；`destructive` 的策略提示缺省是要人确认。

## 3 手写清单

在类库旁边建组件文件夹 `notes/`：

```bash
mkdir -p notes/.qian-plugin notes/tools
```

新建 `notes/.qian-plugin/plugin.json`：

```json
{
  "id": "com.example.notes",
  "version": "0.1.0",
  "description": "示例组件：提供一个笔记服务和一项新建笔记的能力。",
  "author": { "name": "Example" },
  "license": "MIT",
  "interface": { "displayName": "示例笔记", "category": "productivity" },
  "inject": [],
  "provides": [],
  "scopes": ["global", "tenant"],
  "surfaces": { "tools": "./tools" },
  "policy": { "tier": "official", "sandbox": "none", "authentication": "ON_INSTALL" }
}
```

这些字段是清单的必填项。能从代码推出来的（`inject`、`provides`、`runtime`、工具描述）先留空，下一步由工具补上；组件 id、版本、展示、策略、scope 一直是手写的。

信任等级（`policy.tier`）要留意：带进程内代码的组件（派生出来的 `runtime.form` 是 `module`），等级只能是 `system` 或 `official`，写成别的，清单工具的校验就不过（报 `.qian-plugin/plugin.json#/runtime/form`）。平台的规则是只有平台自己的和官方的组件，代码可以进平台的共享进程；别的来源的代码要隔离着跑，那条路是规划，见[组件模型规格](/design/prd/specs/component-model)第 11 节。没有代码的组件，各个等级都可以发布。

## 4 用清单工具派生

先构建组件，再从 Kun 仓库的发布标签构建清单工具：

```bash
dotnet build Acme.Notes -c Release
git clone --depth 1 --branch v2026.11.24 https://github.com/CavaiLAB/kun.git kun-v2026.11.24
dotnet build kun-v2026.11.24/tools/Kun.Plugin.Tool -c Release -p:Version=2026.11.24
```

清单工具与组件要用同一个版本的 Kun。发布的包里程序集版本是 `2026.11.24.0`，从源码构建工具时用 `-p:Version=2026.11.24` 对上；对不上，工具加载不了组件的程序集。

先只读地比对：

```bash
TOOL=kun-v2026.11.24/tools/Kun.Plugin.Tool/bin/Release/net10.0/Kun.Plugin.Tool.dll
DLL=Acme.Notes/bin/Release/net10.0/Acme.Notes.dll
dotnet "${TOOL}" check "${DLL}" notes
```

退出码是 1，列出清单与代码的差异：

```json
{"changed":[{"file":"tools/note.create.json","pointer":""},{"file":".qian-plugin/plugin.json","pointer":"/provides"},{"file":".qian-plugin/plugin.json","pointer":"/runtime"},{"file":".qian-plugin/plugin.json","pointer":"/x-qian-generated"}],"applied":false}
```

同步，再比对一次：

```bash
dotnet "${TOOL}" sync "${DLL}" notes
dotnet "${TOOL}" check "${DLL}" notes
```

`sync` 退出码 0；第二次 `check` 输出 `{"changed":[],"applied":false}`，退出码 0。清单里补上了这几项：

```json
{
  "provides": ["ExampleNotes"],
  "runtime": { "form": "module", "entry": "Acme.Notes.NotesPlugin, Acme.Notes" },
  "x-qian-generated": {
    "$generated": true,
    "fields": ["/inject", "/provides", "/runtime/form", "/runtime/entry"],
    "tools": ["tools/note.create.json"],
    "metadata": {}
  }
}
```

`notes/tools/note.create.json` 是派生出来的工具描述，下面是其中一部分；完整文件还有 `inputSchema`、`outputSchema`（从 `CreateNote`、`NoteCreated` 导出）与 `schemaSha256`：

```json
{
  "name": "note.create",
  "description": "为当前租户新建一条笔记；修改已有笔记请用其它能力。",
  "sideEffect": "write",
  "policyHint": "allow",
  "idempotent": true,
  "requiredScopes": ["com.example.notes:write"],
  "dataClassification": "internal",
  "audit": true,
  "agent": { "expose": false, "requiresConfirmation": false },
  "binding": { "kind": "command", "id": "com.example.notes/note.create" }
}
```

几条规矩：

- `check` 只读：有差异退出 1，输入不对（比如上面的信任等级）退出 2，错误写成一行 ErrorDetail。
- 代码里删掉一项 `[Capability]`、重新构建以后，`check` 会报出旧的 `tools/note.create.json` 与 `/x-qian-generated/tools`，`sync` 时把那个文件撤下。
- 改了代码就重新构建、重新 `sync`；放进持续集成时跑 `check`，有差异就失败。Kun 仓库对自己的全部组件就是这样做的。
- 工具要加载你的程序集来读生成的元数据，只对自己信得过的本地构建用，不能拿它校验来路不明的组件包。

## 5 装进宿主

```bash
dotnet new console -n NotesHost
cd NotesHost
dotnet add reference ../Acme.Notes/Acme.Notes.csproj
```

把 `NotesHost/Program.cs` 换成：

```csharp
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
```

```bash
dotnet run -- ../notes/.qian-plugin/plugin.json
```

```text
挂载后：Ready
笔记数：1
卸载后挂载树里的条目数：0
```

清单里的 `provides` 不是摆设。装载器在调用任何 `ApplyAsync` 之前，按各条目的 `Inject` 与清单的 `provides` 检查依赖、排挂载顺序：另一个组件的 `Inject` 写了 `ExampleNotes` 时，清单写全了，装载器先挂 `notes` 再挂它；清单少了 `provides`，`MountAsync` 直接报 `DEPENDENCY_MISSING`（「请求的服务键 ExampleNotes 无人提供」）。这也是改了代码要重新 `sync` 的原因。

平台宿主不用这样手写：它按 profile 与补丁解析出条目，从清单读入口与 `provides`，再交给同一个装载器，见[profile 与 patch](/start/concepts/profile)。

## 现在还做不到的

这一页是现在照着能跑通的部分。下面几件是现状：

| 想做的 | 现状 |
|---|---|
| 用 `qian add` 把这个组件装进别的宿主 | 命令行没有发布成包，要从仓库构建。安装经目录市场，按信任等级验签，带代码的组件还要把 NuGet 包发到包源；`qian add identity authz files` 这样的整组安装还没有收口。命令见 [CLI 与规格](/cli/commands) |
| 平台的能力目录收录这项能力、经通用入口执行 | 能力目录的代码已有，在平台档的 `Kun.Capabilities` 等包里（装包要包源令牌，见[上手](/start/getting-started#packages)）。平台宿主调用 `AddKunCapabilities()` 装配；单独的宿主没有通用执行入口，要自己装配。规则与接口见[能力与函数](/sdk/dotnet/capabilities)，组件页 [qian.capabilities](/components/platform/qian.capabilities) 还是占位页 |
| 把目录里的能力交给 Agent 当工具 | `[Capability]` 上可以先写 `AgentExpose`、`RequiresConfirmation`，清单工具把它们写进工具描述的 `agent` 字段（上面的 `tools/note.create.json` 里就有）；把能力交给 Agent 的那一端是规划 |
| 代码热装卸 | 没有承诺，更新代码靠重启进程 |
| 插件基类、客户端门面、依赖注入登记的代码生成 | 推迟到 S2，现在用接口加 `ctx.Inject` |
| 第三方的代码隔离运行 | 规划 |

## 无代码的组件 {#no-code}

只有技能、命令、工具描述这类声明式内容、没有 .NET 代码的组件，用命令行生成骨架：

```bash
pnpm exec qian init --template skill-only --id com.example.guide --name 使用指引 --cwd ./guide
```

会生成 `README.md`、`AGENTS.md`、`.qian-plugin/plugin.json`、`.qian/config.yml`、`profiles/minimal/qian.profile.yml`、`profiles/minimal/qian.patch.yml` 与 `skills/guide/SKILL.md`。清单里没有 `runtime`，信任等级是 `private`。README 的「模型体验」「已知限制与延期项」两节要照实填写。

命令行现在要从 Kun 仓库构建：在仓库根运行 `pnpm install`（前端依赖里的 `@cavailab/qian-*` 在 GitHub Packages，要带 `read:packages` 权限的令牌，配置见仓库 [README](https://github.com/CavaiLAB/kun/blob/main/README.md) 的「前端依赖与包源」），再运行 `pnpm --filter "qian..." build`。无代码组件装进 minimal profile、在两个租户之间隔离与卸载的过程，仓库里有一组语言无关的验收场景：[tck/runtime/skill-only.json](https://github.com/CavaiLAB/kun/blob/main/tck/runtime/skill-only.json)。

## 再往后

- 清单的每个字段、各种声明式内容的文件格式：[组件清单 plugin.json](/cli/plugin-json)、[声明式表面的文件格式](/cli/surfaces)；完整规格是[组件模型规格](/design/prd/specs/component-model)。
- 一项能力怎么拆成契约包与提供方包：[seam](/start/concepts/seam)；注册归属哪一层：[scope](/start/concepts/scope)。
- 给组件的能力加权限：[在 .NET 里接权限](/start/guides/authorization-dotnet)。
- 一个完整的参考组件：客户参考应用 [samples/Kun.Sample.Crm](https://github.com/CavaiLAB/kun/tree/main/samples/Kun.Sample.Crm)，带权限点、角色、工具描述、命令、事件与审计；它要 PostgreSQL，跑法见它的说明与测试。
- 组件开发包的完整规则（代码声明、命令管道、清单工具）：[Kun.Plugin.Sdk 的说明](https://github.com/CavaiLAB/kun/blob/main/src/Kun.Plugin.Sdk/README.md)。
