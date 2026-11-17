# AGENTS.md：给编码代理的入口说明

这个仓库是 **Kun** 的新家：匠心智能体平台的企业级后端框架（.NET 10）。**两个原仓库的代码已带历史迁入本仓库，原仓库已归档为只读。原 Kun 已提到仓库根，它的文档整理进了 `docs/`；原 Qian 仍在过渡目录 `_import/qian/`，正按迁移执行计划分批并轨、归并与改名。**

## 要改代码

迁移已于 2026-11-17 启动，分批的做法见[迁移执行计划](docs/migration-plan.md)。历史导入（计划里的 B1）把两个原仓库的 `main` 带历史导入了两个过渡目录，原提交的 SHA 原样保留；B2 的整理随后把原 Kun 由 `_import/kun/` 提到了仓库根、把它的文档整理进了 `docs/`（git 识别为改名，看导入前的历史用 `git log --follow -m -- <路径>`）：

| 位置 | 内容 | 怎么构建与测试 |
|---|---|---|
| 仓库根 | 原 Kun 框架（`idadawn/kun`）：`src/`、`tests/`、`samples/`、`benchmarks/`、`tools/`、`scripts/`、`Kun.sln` 与构建基线；它的文档在 `docs/`，与本仓库的框架级文档放在一起（入门、`guides/`、`security/`、`design/`，决策记录在 `adr/kun/`） | 见下面「原 Kun 的构建、测试与代码约定」，命令在仓库根运行 |
| `_import/qian/` | 原 Qian 的整棵树（`idadawn/qian`）：.NET 包、前端工作区、文档站、任务看板 | 按 `_import/qian/AGENTS.md`，命令在 `_import/qian/` 下运行 |

两边各用各的构建基线。仓库根上的 `global.json`、`Directory.Build.props`、`Directory.Packages.props`、`.editorconfig` 是原 Kun 的；`_import/qian/` 有自己的一套（`global.json`、`Directory.*.props`、`NuGet.config`、带 `root = true` 的 `.editorconfig`、pnpm 工作区），它的脚本把 `_import/qian/` 当作根。在仓库根上跑 `dotnet`，构建的是原 Kun 的 `Kun.sln`；原 Qian 的命令一律在 `_import/qian/` 下跑，不要在仓库根上跑 `pnpm`。仓库根的 `.gitignore` 与 `.gitattributes` 也来自原 Kun，末尾各有一段让它们不作用到 `_import/qian/`，`_import/qian/` 搬空之前不要删。

两边的检查接在仓库根的 `.github/workflows/` 里：`kun-ci.yml` 对应原 Kun 的，在仓库根运行；`qian-*.yml` 对应原 Qian 的，在 `_import/qian/` 下运行。原 Kun 的 `.github/` 原件在 `.github/originals/kun/`，原 Qian 的在 `_import/qian/.github/`，GitHub 都不读。每个根工作流的开头写着它对应哪个原件、相对原件改了什么；原 Kun 没接的工作流与模板见 `.github/originals/kun/README.md`。

原仓库归档之后的规矩：

- 两个原仓库 2026-11-17 冻结，同日归档为只读。最后一次同步的来源头是归档时的最终头：`idadawn/kun` 的 `51b480775c` 与 `idadawn/qian` 的 `5954ec3b86`，同步时 `_import/kun/`、`_import/qian/` 与这两个头的树逐字节相同。原 Kun 与原 Qian 的源码、文档与任务文件从此以本仓库为准，原仓库不再接受改动。
- 原 Kun 与原 Qian 的内容从此在本仓库改，在本仓库开 PR。`_import/qian/` 里改的是文件内容；它的搬动与拆分属于后面的批次（见最后一条），不要顺手做。
- 原 Kun 的约定已并进本文件（下一节），`_import/qian/AGENTS.md` 里构建、测试与代码约定的部分照用。任务文件照原 Qian 的 `pm/README.md` 改：任务文件在 `_import/qian/pm/tasks/` 下，`pm.py` 在 `_import/qian/` 下运行（如 `cd _import/qian && python3 scripts/pm.py validate`），`set` 与 `add` 要带 `--by`，生成物不进 git。迁移任务（S1-KUN-018–022）的文件也在这里改（迁移执行计划第 3 节第 7 条）。原 Qian 的 `AGENTS.md` 与仓库根上原 Kun 的 `CONTRIBUTING.md` 里讲提 PR、分支、发版、发布文档站的部分说的是原仓库，在本仓库以本文件与迁移执行计划为准。
- 两个原仓库冻结时写的去处说明随最后一次同步带了进来，在本仓库不适用：`_import/qian/` 的 `README.md` 开头那一段与 `AGENTS.md` 的「本仓库已冻结」一节（原 Kun 的 `README.md`、`CLAUDE.md` 原件已在整理的第二段删除）。原 Kun 的自管任务队列（含 `docs/roadmap/BACKLOG.md`）与第三方生成的代理配置已按合并蓝图删除（D23.13 第 6 点），任务管理统一到 `_import/qian/pm/`；原 Kun 各层工具生成的 `AGENTS.md` 也一并删了，模块目录里留下的 `AGENTS.md` 只有生成之后写进去的模块规则。
- 归前端仓库 `CavaiLAB/qian` 的路径在这里只读，改前端去那边改：`_import/qian/packages/yue/`、`_import/qian/apps/yue-playground/`、`_import/qian/packages/qian-plugin-sdk/`、`_import/qian/packages/qian-runtime/`、`_import/qian/tests/gates/yue/`、`_import/qian/scripts/yue-visual.mjs`、`_import/qian/.changeset/`，以及原 Qian 的 yue、runtime-ts、runtime-ts-release、plugin-sdk-release 四个工作流（仓库根上对应 `qian-yue.yml` 与 `qian-runtime-ts.yml`，两个发布的没有接）。它们在 B11 由前端计划的执行会话删除。外壳应用与外壳工具包、三个界面包、参考应用的远程模块、文档站、命令行、声明式表面的校验库留在本仓库。
- 一批一个 PR。本仓库的 PR 一律用合并提交合入，不压缩、不变基，否则导入的历史会丢。除迁移批次的 PR 之外，不要新建顶层代码目录，也不要把代码搬出 `_import/qian/`。
- 不要抢跑：整理的第二段（树内的历史 PR 编号改写成带仓库名的形式、指向原仓库的地址改指本仓库、原 Qian 文档站的「Kun 框架」小节改为同仓来源、导入的文档里写给特定下游的内容怎么处理）由计划指定的执行会话做，原 Kun 一侧已经做完（文档整理进 `docs/`，`_import/kun/` 已搬空）；构建并轨是 B3；包名、命名空间与类型名的改名是 B4–B10。

目标仓库已定为本仓库（产品负责人，2026-11-17）。合并蓝图里写的 `idadawn/kun` 以[迁移与归并](docs/migration.md)第 6 节与迁移执行计划为准。

## 原 Kun 的构建、测试与代码约定

本节并自原 Kun 的 `CLAUDE.md` 与根上那份 `AGENTS.md`（两份都已删除；原文见 git 历史，原 Kun 最终头 `51b480775c` 上的同名文件，如 `git show 51b480775c:CLAUDE.md`）。原 Kun 的文档已整理进 `docs/`：入门在 `docs/getting-started.md`，指南、安全与设计说明在 `docs/guides/`、`docs/security/`、`docs/design/`，决策记录在 `docs/adr/kun/`（引用时写「Kun ADR-0xx」）；AOT 审计、基准结果、路线图、发版手册与发布说明等历史文档已删除，原文同样在 `51b480775c`。代码与构建文件注释里写的 `docs/…` 是原 Kun 的旧路径，没有改：`docs/architecture/` 下的 `ADR-*`、`adr-*` 现在在 `docs/adr/kun/`，其余的在 `docs/design/`；`docs/observability/`、`docs/schema-per-service.md`、`docs/openapi-template.md` 在 `docs/guides/`；`docs/whitepaper.md` 在 `docs/design/`；别的路径不变。`scripts/check-layering.sh` 的注释里提到的 `.claude/skills/` 已经删除，照本节做。原件里讲原仓库流程（分支、合并、发版、派单）的部分以本文件为准。合并蓝图落地之后，分层规则由显式的档位表与门禁取代（D23.7、D23.14，B3 落地），包名与命名空间在 B4–B10 改；在那之前照下面的做。

### 构建与测试

命令在仓库根运行。要 .NET SDK 10.0.100 以上（`global.json` 钉的版本，允许滚到 10.0 里更新的功能版本）；集成测试要 Docker。

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
dotnet format --verify-no-changes
```

- 单个测试项目：`dotnet test tests/Kun.Mediator.Tests -c Release`；单个测试加 `--filter "FullyQualifiedName~类名.方法名"`。
- 集成测试（`tests/Kun.IntegrationTests`）用 Testcontainers 起容器，对象存储测试用 RustFS 作 S3 兼容服务。本机没有容器环境时跳过；设了 `CI` 时改为失败；拉不到镜像一律失败。本机迭代可以加 `--filter "FullyQualifiedName!~IntegrationTests"` 排除。
- `samples/Kun.Sample.Tests` 要 Aspire 的服务发现环境变量，CI 不跑。
- CI（`kun-ci.yml`）另外：逐个 AOT 候选模块做 NativeAOT 发布，警告即错误（模块清单见工作流里的矩阵）；跑 `tests/Kun.Integration.AotTests`（样例与网关包的 NativeAOT 发布并运行）；检查分层（`scripts/check-layering.sh`）与身份模块的日志隐私和产品中立（`scripts/check-identity-boundaries.py`）。提交前至少跑一遍格式检查。

### 分层与依赖

- 依赖只能向下（原 Kun 的四层，列的是最初的模块）：宿主层（`Kun.AspNetCore`、`Kun.Grpc`、`Kun.Gateway`）→ 基础设施层（`Kun.EntityFrameworkCore`、`Kun.Dapper`、`Kun.EventBus.RabbitMQ`、`Kun.Saga`）→ 抽象层（`Kun.Mediator`、`Kun.Infrastructure`、`Kun.EventBus`、`Kun.Security`、`Kun.Validation`）→ 核心层（`Kun.Core`）。
- `Kun.Core` 只依赖 BCL，不加第三方包引用（分层检查脚本把关）。领域类型（聚合根、值对象、领域事件）不带 ORM、JSON、校验特性，持久化映射放在 `Kun.EntityFrameworkCore` 的 `IEntityTypeConfiguration<T>` 里。
- 新的横切机制（认证、审计、多租户、事件总线）要先有决策记录。
- 蓝图的归档名单里有 `Kun.Saga`、`Kun.Sandbox`、`Kun.Dapper`、`Kun.Infrastructure` 与旧线的 `Kun.Storage`（D23.9），各域批次会把它们移出解决方案。

### 项目文件与包

- `TargetFramework`、`Nullable`、`LangVersion`、`TreatWarningsAsErrors`、分析器只在 `Directory.Build.props` 里设，不写进单个项目文件。
- AOT 按项目选择：项目文件里只写 `<IsAotCandidate>true|false</IsAotCandidate>`，`Directory.Build.props` 据此打开或关闭 `IsAotCompatible`、`IsTrimmable` 与裁剪、AOT、单文件分析器；不要在项目文件里重复写这些属性，也不要在库上设 `PublishAot`。
- 包版本统一写在 `Directory.Packages.props`（中央包管理），项目里写 `<PackageReference Include="X" />`，不带 `Version`。加新包先查这里有没有；AOT 候选模块还要核对新包的 AOT 兼容性。
- 公共 API 要写 XML 文档注释；带 `PublicAPI.Shipped.txt`、`PublicAPI.Unshipped.txt` 的项目，改公共 API 要同步这两份清单。
- 改一个模块之前，先读它目录里的说明文件（`AGENTS.md`、`README.md`）与 `docs/` 下对应的设计说明、指南与 Kun 决策记录，也读要引用的模块的。

### AOT 候选模块里不许用的写法

- 运行期反射：`Activator.CreateInstance`、`Type.GetMethod`、`MakeGenericType`、`Assembly.GetTypes`、`AppDomain.CurrentDomain.GetAssemblies`；`System.Reflection.Emit`。
- 不带 `JsonTypeInfo` 的 `JsonSerializer.Serialize(obj)`：序列化走源生成的 `KunJsonContext`，被序列化的事件类型都要在里面登记。
- Dapper 的经典 `Query<T>`：用标了 `[DapperAot]` 的查询，由源生成器生成读取代码。

### 运行时约定

- 命令与查询分离。`Kun.Mediator` 的处理器在编译期由源生成器发现，不要改回反射解析。写侧（命令处理器）用 `Kun.EntityFrameworkCore`：仓储、规约、变更跟踪，在 `SaveChangesAsync` 里登记 Outbox；读侧（查询处理器）用 `Kun.Dapper`：原生 SQL、DTO 投影、不加载聚合。查询处理器里不用 EF Core，命令处理器里不用 Dapper。
- Outbox：集成事件与业务数据在同一事务里落库，事件类型用稳定的字符串 id（如 `order.placed.v1`），不用程序集限定名；优先用 `[IntegrationEventId("order.placed.v1")]` 特性加 `Register<TEvent>()`。消费端先查成功标记，再执行处理器、写成功标记、确认消息；业务副作用、标记与确认之间仍有重放窗口，写类处理器必须幂等。设计说明见原 Kun 的 ADR-015。
- Saga 只做编排：一个 Saga 是有序的步骤，每步有执行与补偿；补偿从失败的那一步起逆序执行，补偿本身要幂等，没执行过的步骤也要能安全补偿。
- 服务间零信任：外部令牌只在 `Kun.Gateway` 校验；网关用自己的私钥签发内部短期令牌（RS256 或 ES256，不用 HS256），下游经 `Kun.Security` 校验。身份（`sub`、`tenant_id`）只从内部令牌的声明里取，不信 `X-User-Id` 这类请求头。
- 多租户：`Kun.Core` 的 `ITenantContext` 贯穿全栈；实现 `IMultiTenant` 的实体自动带 `TenantId` 查询过滤器，除明确的管理场景外不要用 `IgnoreQueryFilters()`；可以再加 PostgreSQL 行级安全作纵深防御。租户号改为只收 Guid 是 B5 的事（D23.5）。
- 业务失败返回 `Result` / `Result<T>`，异常只留给系统性故障（并发、网络、基础设施）。gRPC 端点可以 `result.EnsureSuccess()`，由全局拦截器把失败转成对应的状态码。
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
| Saga 与步骤 | `{名词}Saga` / `{动词}Step` | `OrderCreationSaga` / `ReserveInventoryStep` |
| Outbox 的稳定事件 id | `{上下文}.{名字}.v{n}` | `order.placed.v1` |

提交信息用 Conventional Commits：`<type>(<scope>): <subject>`，原 Kun 模块的 scope 是模块名去掉 `Kun.` 前缀（如 `feat(saga): …`），末尾带任务 id（原 Qian 的约定，如 `[S1-KUN-019]`）。

## 要改文档

1. 先读 [README.md](README.md) 与平台总览仓库的[名称对照](https://github.com/CavaiLAB/jiangxin/blob/main/docs/glossary.md)。「Qian」换了所指：以前指基于 Kun 的平台层，现在只作前端框架的名字；原来叫 Qian 的后端并入 Kun。组件 id 前缀 `qian.`、清单文件名这些运行期字面当作历史字面保留；`qian` 命令行与 `@qian/*` 导入名本次不改。
2. 合并蓝图是本仓库里的 `_import/qian/docs/adr/ADR-023-kun-qian-merge-blueprint.md`，2026-11-17 接受，裁定记在 `_import/qian/pm/tasks/S1-DEC-036.md`（D1–D14）；两者与已归档的 `idadawn/qian` 里的同名文件内容相同。写到归并的细节时以它为准，蓝图里的条目按已定写。被之后的裁定改写的几项（目标仓库、仓库划分、包的作用域、总名）见[迁移与归并](docs/migration.md)第 6 节。
3. 文档用中文，不写参考平台和对标项目的名字。
4. 分清「已定」「提议」「待定」。说现状的时候写清是「已有」（在原仓库里已经实现）还是「规划」。
5. 平台级的口径（名称、三个框架的分工、Agent 协同）在 [jiangxin](https://github.com/CavaiLAB/jiangxin) 里改，这里只引用。
6. 在 `docs/` 下新增文档，要在 `docs/_meta.json` 里加上它的文件名（不带扩展名）。这个文件定这些文档在平台文档站侧栏里的顺序；漏列的话，文档站升级到这个版本时构建会报错。子目录在 `docs/_meta.json` 里写成对象，如 `{ "type": "dir", "name": "guides", "label": "指南" }`，label 用中文；在子目录里加文档不用改顶层这一份。子目录自己的 `_meta.json` 可以不写，不写就按文件名排；写了就要列全，漏列的页面会从侧栏里消失，构建不报错。
7. 改动走分支和 PR，不直接推 `main`。建仓期间的初始文档除外，见平台总览的[协作约定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/07-governance.md)第 2 节。
