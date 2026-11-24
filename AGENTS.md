# AGENTS.md

Kun 是匠心工业智能体平台的企业级后端框架。命令在仓库根运行。`Kun.slnx` 包含内核、平台组件和宿主；前端工作区使用根 `pnpm-workspace.yaml`，站点位于 `website/`。组件清单在 `plugins/`，约定在 `contracts/` 和 `schemas/`，开发工作按 [PM 说明](pm/README.md)跟踪。

## 仓库检查与合并

变更由根工作流 `.github/workflows/ci.yml` 按 `scripts/ci_changes.py` 的区域触发。
发布工作流 `release.yml` 由维护者手动触发。修改它会在 PR 上运行 .NET 构建和发布工具自测。
PR 的必需检查是「CI 汇总」。相关作业全部通过后，以合并提交合入主干并保留完整历史。

- 「CI 汇总」这个检查名不改。分支保护按名字认必需检查，改了名，PR 会一直等一个不会出现的检查。
- 提交说明里不写跳过 CI 的标记（`[skip ci]`、`[ci skip]`、`[no ci]`、`[skip actions]`、`[actions skip]`），PR 标题也不写，合并提交会带上它。带了标记，整次运行不触发，「CI 汇总」没有结果，PR 合不进去，`main` 上也不跑。
- 加文件、挪目录时，让它落进区域表里合适的区域（要构建 .NET 的区域都含仓库根的构建基线、`Kun.slnx` 与 `src/`），或者明列进不跑检查的名单。区域表的自测与 `ci.yml` 的结构自测在 `scripts/tests/`，变更检测作业每次先跑，本机跑法 `python3 -m unittest discover -s scripts/tests -p 'test_ci_*.py' -v`；查一个区域管哪些文件用 `python3 scripts/ci_changes.py files <区域>`。

## 任务流程（必须遵守）

- 任务在 `pm/tasks/<id>.md`，一个任务一个文件。开始前认领：
  `python3 scripts/pm.py set <id> --status doing --owner <你的代理名> --by <你的代理名> --note "开始"`
- 日志必须署名：`pm.py set` / `pm.py add` 带 `--by <你的代理名>`（或设环境变量 `PM_ACTOR`），缺省时命令报错；`pm.py validate` 拒绝署名为 someone 的日志行（S1-DEC-019 R71）。
- 先建档再交付：任务以 `todo` / `doing` 建档并认领后再提交实现，不要在创建任务的提交里直接写 `done` 并勾满验收。
- 结束时把「验收」里完成的项改为 `[x]`，改状态（`review` 或 `done`），运行 `python3 scripts/pm.py validate`，只提交任务文件。
- 提交信息末尾带任务 id，如 `feat(runtime): ctx.Effect 逆序释放 [S1-RT-001]`。
- 遇到需要人拍板的问题：不要自作主张改设计。用
  `python3 scripts/pm.py add --stage S1 --area DEC --type decision --wave 0 --by <你的代理名> --title "..."`
  新建决策任务并把当前任务标 `blocked`。
- `pm/index.html`、`pm/tasks.json`、`pm/STATUS.md` 是生成物，不进 git（S1-PM-004），不要提交，也不要手改：
  - 本地想看看板，运行 `python3 scripts/pm.py build`，结果已被 `.gitignore` 忽略；
  - 集群看板由看板 Pod 按 `CavaiLAB/kun` 主干上 `pm/tasks/` 的任务文件自行生成（访问方式见 `deploy/pm-board/README.md`；应用由 Argo CD 管，Application 在部署仓库）；

## 写文档与代码的硬规则

- 文档一律中文。文档里不得出现参考平台的品牌名（统一写「参考平台」），也不要写对标项目的名字（写「对标项目」）。`python3 scripts/check_docs.py` 会检查禁用词、表格列数与链接，提交前跑一遍。
- Markdown 表格单元格里的反引号内若有竖线，写成 `\|`。
- 组件设计先回答四个问题：文件夹里有哪些表面、装进哪个 profile、属于哪个 seam 的哪个角色、能否 `qian add`。凡是把能力绑死在特权内核、DI 扩展方法或数据库开关上的写法都是倒退。
- 术语：平台插件（不是「系统插件」）、运行时 `Kun.Runtime`（不是「内核」）、profile（不是「可部署单元」）、`.qian-plugin/plugin.json`（不是 `qian-plugin.yaml`）、租户 scope patch（不是「租户开关」）。
- Kun 的 .NET 包分内核档和平台档；Qian 为前端框架。站点的 `website/docs/sdk/kun/` 根据根 `CHANGELOG.md` 的正式发布版生成，手写说明为 `with-qian.mdx`。
- 仓库内 .NET 项目使用项目引用。包版本集中在 `Directory.Packages.props`，包源在根 `NuGet.config`。
- 组件的公共端口与提供者按[组件包依赖约定](docs/adr/ADR-019-library-first-packages.md)分包。组件声明使用宿主提供的能力端口。
- 三道 .NET 门禁会自动拦截（细节见 `tests/README.md`，都在仓库根的 `Directory.Build.targets` 与 `scripts/` 下，不看包名）：
  - 可打包的项目（写了 `<IsPackable>true</IsPackable>`）都必须带 `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`，新公共 API 未登记即编译失败；
  - 可打包的项目都要在项目文件里写档位与角色（`KunTier`、`KunRole`），仓库根的 `scripts/check_deps.py` 按它们检查依赖，不看包名：内核档不依赖平台档，平台档只依赖内核档与平台档的契约包，契约包直接依赖的内核档包只能是契约。写法、例外与本机跑法见仓库根 `AGENTS.md`「档位与依赖」；
  - `BannedSymbols.txt` 和 `tools/Kun.Build.Analyzers` 检查平台档的包和宿主。平台宿主在项目中声明 `<KunHost>true</KunHost>`。
- 错误响应一律是 `contracts/v0/error-detail.schema.json` 的 ErrorDetail：用 `Kun.Errors` 定义与登记错误码（大写下划线、全局唯一、在本组件 Definition 包里定义），HTTP 宿主装 `AddKunErrors()` + `UseKunErrors()`；不要自己拼 ProblemDetails 或 `{ error: ... }`。
- 改了 `contracts/v0/` 的 Schema 必须同时改 fixture 与 `Kun.Errors`，`dotnet test` 会校验 Schema、fixture 与内置错误码目录 `error-codes.json` 是否一致。
- 集成测试使用 `src/Kun.Testing.Containers` 中的共享夹具，测试分片与预拉取镜像按[测试说明](tests/README.md)登记。
- 组件、运行时与数据的变更按[已接受的决策](docs/adr/decisions.md)与对应任务的验收项核对。

## Kun 框架的构建、测试与代码约定

### 构建与测试

命令在仓库根运行。要 .NET SDK 10.0.100 以上（`global.json` 钉的版本，允许滚到 10.0 里更新的功能版本）；集成测试要 Docker。测试运行器是 Microsoft.Testing.Platform（`global.json` 的 `test.runner`，xunit.v3 4.x 不再支持 VSTest）：`dotnet test` 用 `--project` 或 `--solution` 指定目标，旧的 `--filter "FullyQualifiedName…"` 与 `--collect` 不再可用。

```bash
dotnet restore
dotnet build -c Release
dotnet test --solution tests/kun-unit.slnf -c Release --no-build
dotnet format Kun.slnx --verify-no-changes
```

- 不带参数的 `dotnet restore` 和 `dotnet build` 使用根 `Kun.slnx`。在仓库根运行格式核对。
- 单元测试登记在 `tests/kun-unit.slnf`，与覆盖率一起运行时加 `--coverlet --coverlet-output-format cobertura`。平台测试登记在 `tests/kun-platform.slnf`，分片 a、b、c 的归属由 `scripts/dotnet_ci.py` 核对。
- 单个测试项目：`dotnet test --project tests/Kun.Mediator.Tests -c Release`；单个测试类或方法用 xunit 的筛选参数，如 `--filter-class "*.类名"`、`--filter-method "*.类名.方法名"`。
- 集成测试（`tests/Kun.IntegrationTests`）用 Testcontainers 起容器，对象存储测试用 RustFS 作 S3 兼容服务。本机没有容器环境时跳过；设了 `CI` 时改为失败；拉不到镜像一律失败。跑法：`dotnet test --project tests/Kun.IntegrationTests -c Release`。
- `samples/Kun.Sample.Tests` 要 Aspire 的服务发现环境变量，CI 不跑，写在 `scripts/dotnet_ci.py` 的 `NOT_IN_CI` 里。
- 运行 `python3 scripts/dotnet_ci.py registration` 核对两个筛选文件、`KUN_INTEGRATION`、专属作业及带原因的 `NOT_IN_CI` 名单。每个分片都核对每个模块恰好产生一份 TRX，分片 k 的单元测试另核覆盖率。
- CI（`ci.yml` 的 dotnet 区域）另外：原生发布并运行 AOT 冒烟宿主 `tests/Kun.AotSmoke`，ILC 有任何警告即失败（清单见下面「项目文件与包」）；跑 `tests/Kun.Integration.AotTests`（样例、网关包与网关独立宿主的 NativeAOT 发布并运行）；按档位检查依赖（`scripts/check_deps.py`，见下面「档位与依赖」）与身份模块的日志隐私和产品中立（`scripts/check-identity-boundaries.py`）；用占位版本打全部可打包项目并核对产物（`scripts/release_packages.py`，见下面「项目文件与包」），不推送。单元测试与集成测试在测试分片 k 里跑：每个模块各写一份 TRX 与覆盖率，覆盖率上传前核对 `tests/kun-unit.slnf` 里每个测试项目恰好一份；汇总作业再核对每个模块都跑过、没有失败。提交前至少跑一遍格式检查。
- 基准与对象存储实测不进 CI（S1-DEC-038 D21），在本机跑。基准：`dotnet run -c Release --project benchmarks/Kun.Benchmarks -- --filter '*'`，gRPC 的基准换成 `benchmarks/Kun.Benchmarks.Grpc`，结果写在当前目录的 `BenchmarkDotNet.Artifacts/`。对象存储实测连真实的 OSS 桶，四个 `OSS_SMOKE_*` 环境变量都设了才跑，缺一个就跳过（CI 的分片 k 里没有它们，这组用例跳过）：

  ```bash
  OSS_SMOKE_ENDPOINT=<端点> OSS_SMOKE_BUCKET=<桶> OSS_SMOKE_ACCESS_KEY_ID=<访问键> OSS_SMOKE_ACCESS_KEY_SECRET=<密钥> \
    dotnet test --project tests/Kun.Storage.Oss.Tests -c Release --filter-class "*.OssFileStorageLiveSmokeTests"
  ```

### 档位与依赖

包分内核档与平台档（合并蓝图 D23.7），依赖规则按档位与角色定，写在各项目文件里，门禁不看包名（D23.14 第 1 点）。原件里 Kun 框架的四层方向由它取代。

- 每个可打包的项目都在项目文件里写档位与角色（`<PropertyGroup Label="档位">`）：
  - `KunTier`：`kernel` 是内核档，放通用机制，不知道租户目录、人员、角色、组件清单这些产品概念；`platform` 是平台档，带产品语义，以组件形式装配；
  - `KunRole`：`contract` 是契约，`provider` 是提供者；
  - `Kun.Core` 与 `Kun.Errors` 另写 `<KunZeroDependency>true</KunZeroDependency>`。

  没写或取值不对（区分大小写），构建报 KUNTIER001、KUNTIER002（仓库根的 `Directory.Build.targets`）。测试、工具、宿主、样例不打包，不写。新包的档位按蓝图 D23.9 的归并对照定，各批次开工时按依赖图复核。
- 依赖门禁 `scripts/check_deps.py` 读各项目的 `obj/project.assets.json`，包与项目两类依赖都查：

  | 规则 | 内容 |
  |---|---|
  | K | 内核档包的传递依赖里不得有平台档的包 |
  | P | 平台档包的传递依赖里，平台档的包只能是契约；内核档的包不限 |
  | D | 契约包对内核档包的直接依赖只能是契约 |
  | C | 零依赖的包只能依赖别的零依赖包；除此之外的包引用只许 `Microsoft.Extensions.*` |

  D、C 不计只在构建时用的包引用，即 `PrivateAssets="all"` 而且不带编译、运行时程序集的，比如分析器。`PrivateAssets="all"` 引用运行时库照样算：它不进 nuspec，使用方拿不到它，包会坏。
- 暂时去不掉的违规边记成例外，写在边起点的项目文件里，一条边一项：`<KunTierException Include="被依赖的包" Rule="K" Until="B5" Reason="…" />`。三个属性都必填，`Until` 写最早能去掉的批次。K、P 查传递依赖时跳过记了例外的边，所以只在出问题的那条边的起点记一次。边没了或者不再违规，门禁报例外已失效，要删掉；例外只减不增，现有的用 `git grep KunTierException` 查。
- 本机跑：`python3 scripts/check_deps.py`（先还原 `Kun.slnx`，已还原时加 `--no-restore`）。自测与样例在 `tests/gates/`：`python3 -m unittest discover -s tests/gates -v`。
- `Kun.Core` 只依赖 BCL 与同为零依赖的 `Kun.Errors`，不加第三方包引用（规则 C 把关）。领域类型（聚合根、值对象、领域事件）不带 ORM、JSON、校验特性，持久化映射放在 `Kun.EntityFrameworkCore` 的 `IEntityTypeConfiguration<T>` 里。
- 平台档包（`KunTier=platform`）与宿主（`KunHost=true`，含 `hosts/Kun.Platform.Host` 与 `hosts/Kun.Gateway.Host`）接入根目录的 `BannedSymbols.txt` 和 `tools/Kun.Build.Analyzers`。门禁样例可在项目中用 `KunPlatformAnalyzers=true` 显式接入，产品项目按档位和宿主声明接入。
- 新的横切机制（认证、审计、多租户、事件总线）要先有决策记录。
- 已归档的模块为 `Kun.Saga`、`Kun.Sandbox`、`Kun.Dapper`、`Kun.Infrastructure` 与旧线的 `Kun.Storage`（D23.9）。

### 项目文件与包

- `TargetFramework`、`Nullable`、`LangVersion`、`TreatWarningsAsErrors`、分析器只在 `Directory.Build.props` 里设，不写进单个项目文件。
- AOT 按项目选择：AOT 兼容的库在项目文件里写 `<IsAotCompatible>true</IsAotCompatible>`，SDK 据此打开 `IsTrimmable` 与裁剪、AOT、单文件三个分析器，普通构建里就查；不兼容的什么都不写，项目文件里写一句为什么。几条守卫在仓库根的 `Directory.Build.targets`：写了 `IsAotCandidate`（已经不用的开关）报 KUNAOT001；可打包的库在项目文件里写 `PublishAot` 报 KUNAOT002，它只写在要原生发布的可执行项目里；写了 `IsAotCompatible` 却关掉分析器报 KUNAOT003。
- AOT 清单就是写了 `IsAotCompatible=true` 的项目，没有别的副本：`python3 scripts/aot_projects.py` 列出它们。AOT 冒烟宿主 `tests/Kun.AotSmoke` 引用其中每一个并整体扎根，CI 原生发布后运行它；它的项目引用必须正好等于清单，库加上或去掉 `IsAotCompatible` 时同时改它，`python3 scripts/aot_projects.py --check` 与 `tests/gates` 的自测核对。本机原生发布：`dotnet publish tests/Kun.AotSmoke -c Release`（要 clang；在 macOS 上发布的是本机架构，IL 分析与 CI 的 linux-x64 相同）。`scripts/release.sh` 发版前照 CI 的做法核对、原生发布并运行冒烟宿主。
- `python3 scripts/release_packages.py list` 从 `Kun.slnx` 列出可打包项目和档位。CI 以占位版本打包，再由 `verify-packages` 核对包名、精确依赖、档位、符号包、仓库与提交、许可、作者、项目地址和描述。仅发布分析器且不含运行程序集的项目使用 `IncludeSymbols=false`。
- 包间使用内部成员时，开放方在项目的 `InternalsVisibleTo` 项用 `KunPackageFriendReason` 记录原因；消费方打包后对开放方精确依赖 `[V]`。测试程序集也用项目项声明。制品核对会逐个读取程序集元数据，比对实际授权，并核验 AOT 清单中程序集的 `IsTrimmable=True`。
- 包版本统一写在 `Directory.Packages.props`（中央包管理），项目里写 `<PackageReference Include="X" />`，不带 `Version`。加新包先查这里有没有；AOT 兼容的模块还要核对新包的 AOT 兼容性。
- 公共 API 要写 XML 文档注释。可打包的项目都要带 `PublicAPI.Shipped.txt` 与 `PublicAPI.Unshipped.txt`，首行写 `#nullable enable`（仓库根 `Directory.Build.targets` 的公共 API 门禁，按 `IsPackable` 判定）：缺文件报 KUNAPI001；新增公共 API 没登记报 RS0016，登记了却已删除报 RS0017，都是编译错误。新 API 先登记进 Unshipped，IDE 对 RS0016 的代码修复会生成登记行。
- 改一个模块之前，先读它目录里的说明文件（`AGENTS.md`、`README.md`）与 `docs/` 下对应的设计说明、指南与 Kun 决策记录，也读要引用的模块的。

### AOT 兼容模块里不许用的写法

- 运行期反射：`Activator.CreateInstance`、`Type.GetMethod`、`MakeGenericType`、`Assembly.GetTypes`、`AppDomain.CurrentDomain.GetAssemblies`；`System.Reflection.Emit`。
- 不带 `JsonTypeInfo` 的 `JsonSerializer.Serialize(obj)`：序列化走源生成的 `KunJsonContext`，被序列化的事件类型都要在里面登记。
- Dapper：它的经典 `Query<T>` 靠运行期反射生成读取代码，AOT 兼容的模块不引用它。

### 运行时约定

- 命令与查询分离。`Kun.Mediator` 的处理器在编译期由源生成器发现，不要改回反射解析。写侧（命令处理器）用 `Kun.EntityFrameworkCore`：仓储、规约、变更跟踪，在 `SaveChangesAsync` 里登记 Outbox；读侧（查询处理器）直接用 Npgsql 的数据源与 Dapper：原生 SQL、DTO 投影、不加载聚合，写法见 `docs/guides/read-side-patterns.md`。查询处理器里不用 EF Core，命令处理器里不用 Dapper。
- Outbox：集成事件与业务数据在同一事务里落库，事件类型用稳定的字符串 id（如 `order.placed.v1`），不用程序集限定名；优先用 `[IntegrationEventId("order.placed.v1")]` 特性加 `Register<TEvent>()`。消费端先查成功标记，再执行处理器、写成功标记、确认消息；业务副作用、标记与确认之间仍有重放窗口，写类处理器必须幂等。设计说明见 Kun ADR-015。
- 服务间零信任。网关核验外部 Bearer 或读取认证会话，选定目标服务后用网关私钥对短期 S2S 令牌签名一次（RS256）。下游通过 Kun.Security 检查签名、issuer、该服务的 audience、有效期及主体契约。主体和租户来自已验证的请求身份。
- HTTP 与 gRPC 端点通过已认证主身份建立租户范围，后台任务显式调用 `BeginScope`。租户范围使用非空 `Guid`，显式平台范围使用 `null`。实体租户列与 PostgreSQL RLS 会话变量使用小写、带连字符的 UUID 文本。平台组件使用 `KunStrictDbContext`，独立业务可在 `KunDbContext` 上配置租户过滤。
- 业务失败返回 `Result` / `Result<T>`，异常只留给系统性故障（并发、网络、基础设施）。失败带的是 `Kun.Errors` 的 `Error`：错误码先定义成 `ErrorCode` 并登记进错误码目录（平台内置的在 `PlatformErrors`），需要抛的时候只抛 `ErrorException`。HTTP 宿主用 `Kun.Errors.AspNetCore` 的 `AddKunErrors()` 与 `UseKunErrors()` 把错误写成 problem+json（`ErrorDetail`）；gRPC 端点可以 `result.EnsureSuccess()`，由 `Kun.Grpc` 的拦截器按错误码的 HTTP 状态转成 gRPC 状态码。
- 审计按需开启：命令用 `AddKunMediator(b => b.AddAuditBehavior<TCommand, TResponse>())` 登记，查询默认不审计。

### 命名与提交

| 种类 | 规则 | 例子 |
|---|---|---|
| 程序集与根命名空间 | `Kun.{模块}` | `Kun.Mediator` |
| 命令 | 动词 + `Command` | `CreateOrderCommand` |
| 查询 | `Get`/`List`/`Search` + `Query` | `GetOrderByIdQuery` |
| 处理器 | `{请求}Handler` | `CreateOrderCommandHandler` |
| 领域事件 | 过去时 + `Event` | `OrderPlacedEvent` |
| 集成事件 | 过去时 + `IntegrationEvent` | `OrderPlacedIntegrationEvent` |
| Outbox 的稳定事件 id | `{上下文}.{名字}.v{n}` | `order.placed.v1` |

提交信息用 Conventional Commits：`<type>(<scope>): <subject>`，Kun 框架模块的 scope 是模块名去掉 `Kun.` 前缀（如 `feat(eventbus): …`），末尾带任务 id（任务看板的约定，如 `[S1-KUN-019]`）。

## 要改文档

1. 先读 [README.md](README.md) 与平台总览仓库的[术语表](https://github.com/CavaiLAB/jiangxin/blob/main/docs/glossary.md)。「Qian」只作前端框架的名字，Kun 提供平台组件。组件 id 前缀 `qian.`、清单文件名这些运行期字面当作历史字面保留；`qian` 命令行与 `@qian/*` 导入名本次不改。
2. 合并蓝图是本仓库里的 `docs/adr/ADR-023-kun-qian-merge-blueprint.md`，2026-11-17 接受，裁定记在 `pm/tasks/S1-DEC-036.md`（D1–D14）。写到归并的细节时以它为准，蓝图里的条目按已定写。被之后的裁定改写的几项（目标仓库、仓库划分、包的作用域、总名）见[迁移与归并](docs/migration.md)第 6 节。
3. 文档用中文，不写参考平台和对标项目的名字。
4. 分清「已定」「提议」「待定」。说现状的时候写清是「已有」（在原仓库里已经实现）还是「规划」。
5. 平台级的口径（名称、三个框架的分工、Agent 协同）在 [jiangxin](https://github.com/CavaiLAB/jiangxin) 里改，这里只引用。
6. 在 `docs/` 下新增文档，要在 `docs/_meta.json` 里加上它的文件名（不带扩展名）。这个文件定这些文档在平台文档站侧栏里的顺序；漏列的话，文档站升级到这个版本时构建会报错。子目录在 `docs/_meta.json` 里写成对象，如 `{ "type": "dir", "name": "guides", "label": "指南" }`，label 用中文；在子目录里加文档不用改顶层这一份。子目录自己的 `_meta.json` 可以不写，不写就按文件名排；写了就要列全，漏列的页面会从侧栏里消失，构建不报错。
7. 改动走分支和 PR，不直接推 `main`。建仓期间的初始文档除外，见平台总览的[协作约定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/07-governance.md)第 2 节。

<!-- BEGIN:turborepo-agent-rules -->

# This is NOT the Turborepo you know

Turborepo configuration, task behavior, and CLI commands can vary between installed versions and may differ from your training data. Resolve the `turbo` package from this file's directory or relevant workspace; in monorepos, it may not be visible from the repository root. For example, run `node -p "require.resolve('turbo/package.json')"` from a workspace that depends on `turbo`.

Read `docs/README.md` inside that installed package first, then read the relevant pages from its `docs/` directory before changing Turborepo configuration or commands. Heed deprecation notices. These bundled docs match the installed package version and are available without network access.

This block is written and re-added by `turbo` before repository-scoped commands when an AI agent is detected. In the Turborepo source repository, its template is defined in `crates/turborepo-cli/src/cli/agent_guidance.rs`. Removing the managed block while updates are enabled means a later qualifying invocation will add it again. Set `"agentGuidance": false` in the root `turbo.json` or `turbo.jsonc` to opt out; this does not remove an existing block. Keep the block committed with your work to avoid an uncommitted change on the next agent invocation.
<!-- END:turborepo-agent-rules -->
