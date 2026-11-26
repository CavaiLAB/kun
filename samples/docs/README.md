# 使用文档的示例

网站「开始」里三篇使用文档的代码收在这里。CI 像读者那样照页面从建项目做到发请求，平时用当前源码打的包，发版时用刚发布的包，核对页面上贴的状态码、响应体和错误码，让「照着能跑」一直成立，页面不随版本走样。

| 目录 | 页面 | 这里的文件 |
|---|---|---|
| `getting-started/` | 上手：[getting-started.md](../../website/docs/start/getting-started.md) | `Hello.Kun/Program.cs`（第 2 步） |
| `write-a-component/` | 写一个组件：[write-a-component.md](../../website/docs/start/guides/write-a-component.md) | `notes-demo/Acme.Notes/NotesPlugin.cs`（第 2 步）、`notes-demo/notes/.qian-plugin/plugin.json`（第 3 步，手写的那份）、`notes-demo/NotesHost/Program.cs`（第 5 步） |
| `authorization-dotnet/` | 在 .NET 里接权限：[authorization-dotnet.md](../../website/docs/start/guides/authorization-dotnet.md) | `Notes.Authz/Program.cs`（第 2 步） |

## 放什么、为什么放在这里

- 只放页面上让读者整段写进文件的代码，路径与读者照页面做出来的目录一致。项目文件、启动配置、`dotnet user-jwts` 写进去的配置、清单工具派生的字段与工具描述，都由页面上的命令现场生成：照页面的命令建出来的项目，才是读者真会拿到的项目。
- 页面上只加几行的片段（上手的「文案的语言」、接权限第 5 步加的三处）不另存文件，跑的时候从页面取，插在页面说的位置。
- 仓库的样例都在 `samples/` 下，所以放在这里。这些文件按读者的新项目构建（`dotnet new` 的缺省设置），不进 `Kun.slnx`，不受仓库的构建基线、中央包版本与格式规则约束；在仓库里直接构建它们没有意义，要用下面的脚本。

## 页面与文件怎么保持一致

用比对，不从文件引入：页面源文件里照旧写完整的代码，`scripts/docs_examples.py check` 逐字比对。理由：

- 页面自己是完整的：在 GitHub 上看源文件、站点给大模型的 llms-full.txt 与每页的 `.md`，都是完整的代码，给大模型读的入口不缺代码；
- 不用改网站的构建；
- 比对能顺带核对页面上的命令、版本号和输出，从文件引入做不到。

`check` 不要 .NET，核对这几项：

- 页面上整段的代码块与这里的文件逐字一致；
- 页面上的每个代码块，要么有示例的步骤用到，要么在脚本里写明不跑的原因；
- 接权限第 6 节节选的权限点与角色，是客户参考应用里 `metadata/permissions.json`、`metadata/roles.json` 的子集；
- 三篇页面里的 Kun 版本号处处相同，程序集版本是它加 `.0`；
- 示例照着断言的说明文字、插入片段要找的行，都还在。

## 什么时候跑、怎么跑

| 命令 | 做什么 |
|---|---|
| `python3 scripts/docs_examples.py check` | 上一节的核对 |
| `python3 scripts/docs_examples.py feed --version 9999.12.31 --out <目录>` | 用当前源码按占位版本打出页面上 `dotnet add package` 的包，连同它们依赖的 Kun 包，放进本地包源 |
| `python3 scripts/docs_examples.py run --feed local --packages <目录> --version 9999.12.31` | 在仓库外的空目录里开一个 bash，照页面的顺序执行命令（版本号换成占位版本），把这里的文件放到页面说的位置，另起进程跑 `dotnet run`，核对输出；Kun 的包都从本地包源装，清单工具用当前检出构建 |
| `python3 scripts/docs_examples.py run --feed published --version <版本>` | 照页面写的装包方式：内核档从 nuget.org；平台档用发布页附带的 `NuGet.config`，读包令牌从环境变量 `KUN_PLATFORM_FEED_USER`、`KUN_PLATFORM_FEED_TOKEN` 取，只在接权限第 5 步 `export` 时交给终端；清单工具从发布标签构建 |

- 平时：CI 的「使用文档示例」作业依次跑脚本自测、`check`、`feed`、`run --feed local`。改了 `src/` 下的代码、清单工具、这三篇页面或这里的文件，它都会跑；接口改了、页面上的代码或输出对不上，它就失败。
- 发版：`release.yml` 的「发布后照使用文档跑示例」作业在包、标签与发布页都发布以后，用刚发布的版本跑 `run --feed published`。它排在发布之后，没有作业依赖它：失败不撤回已经完成的发布，只把这次运行标成失败，摘要里写明哪一步没过。按那一步改页面与这里的文件，或者出补丁版本，不要因为它重跑整个发布。beta 版不发 nuget.org，不跑。
- `run` 把 `HOME` 与 NuGet 的缓存都指到工作目录里：开发令牌的签名密钥、用户机密和包缓存不落进本机的用户目录。工作目录缺省是临时目录，通过后删掉；`--work <目录>` 指定一个仓库外的空目录，跑完留着，里面的 `logs/` 有每个终端和每次 `dotnet run` 的完整输出，可以拿它在编辑器里打开读者会拿到的项目。

## 改页面时

- 改了页面上整段的代码，同时改这里对应的文件；改了页面上贴的输出，本机跑一遍 `run --feed local`，看是否对得上。
- 在这三篇页面上加了代码块（不管在哪一节），要在 `scripts/docs_examples.py` 里给它加步骤，或者把那一节写进不跑的名单并写明原因，否则 `check` 不过。
- 页面上的命令是照字面执行的。只认这几处替换：Kun 版本号；本机端口被占用时换端口；本地包源模式下不克隆发布标签、不下载发布页的包源映射、不 `export` 读包令牌；写成 `<…>` 的占位符只有接权限第 4 步的变量和第 5 步的读包令牌两处，别处出现会报错。

## 版本号怎么跟着发版走

页面上写的是最近一次核对过的发布版本。发版作业通过以后，如果这次的版本比页面上写的新，摘要里会给出一条命令：

```bash
python3 scripts/docs_examples.py bump --version <版本> --sdk <SDK 版本> --date <YYYY-MM-DD>
```

它把三篇页面里的 Kun 版本号、程序集版本、「照着跑过一遍」那句里的日期与 SDK 版本、页面的 `updated` 一起改掉，再跑一遍 `check`。改完提一个 PR，CI 的「使用文档示例」会照新页面再跑一遍。发版作业不替你推送这个改动。

## 不跑的

| 页面里的小节 | 原因 |
|---|---|
| 写一个组件「无代码的组件」 | 命令行 `qian` 没有发布成包，要从仓库构建，装前端依赖要 GitHub Packages 的令牌；skill-only 模板由 `packages/qian-cli` 的测试覆盖（CI 的「声明式表面」） |
| 在 .NET 里接权限「gRPC 方法」 | 页面上写明没有单独演示；客户参考应用的测试覆盖 gRPC 与 HTTP 对同一权限点判定一致 |

另外几处页面上写了、示例没有专门核对的：上手里「下一步」的各篇；写一个组件里清单少了 `provides` 时报 `DEPENDENCY_MISSING`；接权限里权限点不是全名时映射端点就抛 `ArgumentException`、库模式拒绝 `agent` 主体与委托链。
