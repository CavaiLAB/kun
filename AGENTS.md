# AGENTS.md：给编码代理的入口说明

这个仓库是 **Kun**：匠心智能体平台的企业级后端框架（.NET 10）。**两个原仓库的代码已带历史迁入本仓库，原仓库已归档为只读。Kun 框架的代码在仓库根，文档在 `docs/`；平台代码与工具仍在过渡目录 `_import/qian/`，正按迁移执行计划分批并轨，再按域归并到 `Kun.*` 名下。**

## 要改代码

迁移已于 2026-11-17 启动，分批的做法见[迁移执行计划](docs/migration-plan.md)。历史导入（计划里的 B1）把两个原仓库的 `main` 带历史导入了两个过渡目录，原提交的 SHA 原样保留；B2 的整理随后把 Kun 框架由 `_import/kun/` 提到了仓库根、把它的文档整理进了 `docs/`（git 把这些搬动记成文件重命名，看导入前的历史用 `git log --follow -m -- <路径>`）：

| 位置 | 内容 | 怎么构建与测试 |
|---|---|---|
| 仓库根 | Kun 框架：`src/`、`tests/`、`samples/`、`benchmarks/`、`tools/`、`scripts/`、`Kun.slnx`（`_import/qian/` 下的 .NET 项目也在里面）与构建基线；它的文档在 `docs/`，与本仓库的框架级文档放在一起（入门、`guides/`、`security/`、`design/`，决策记录在 `adr/kun/`） | 见下面「Kun 框架的构建、测试与代码约定」，命令在仓库根运行 |
| `_import/qian/` | 平台代码与工具：.NET 包、前端工作区、文档站、任务看板 | 按 `_import/qian/AGENTS.md`，命令在 `_import/qian/` 下运行 |

构建基线正在并轨（B3，S1-KUN-020）。仓库根上的 `Directory.Build.props`（目标框架、质量门槛、确定性构建）、`Directory.Build.targets`（档位声明检查、公共 API 门禁、AOT 守卫，以及禁用符号与平台分析器的作用域）、`Directory.Packages.props`（中央包版本）、`NuGet.config`（只用 nuget.org）、`BannedSymbols.txt`（禁用符号清单）与 `global.json` 两边共用；`src/` 下 Kun 包的版本与包元数据在 `src/Directory.Build.props`。解决方案只有一个：仓库根的 `Kun.slnx`，含仓库根与 `_import/qian/` 下的全部项目。`_import/qian/` 下的项目改为项目引用 `src/` 下的 Kun 源码，不再按包引用 Kun。`_import/qian/Qian.slnf` 是它们的筛选文件，列出 `_import/qian/` 下的项目，那里的脚本与测试以它所在的目录为根；在 `_import/qian/` 下新加项目，要同时加进 `Kun.slnx` 与 `Qian.slnf`，门禁自测会核对。`_import/qian/` 下的 `Directory.Build.props`、`Directory.Build.targets` 与 `Directory.Packages.props` 先导入根上的：第一个只留那里的打包设置，第二个只留发版脚本读的包判定与网关引用的处理，第三个只剩这一句导入（原来给文档站读的 `KunVersion` 已删，文档站改读仓库根 `CHANGELOG.md` 里 Kun 最新的发布版，门禁自测保证没有 MSBuild 文件再定义或引用它）。SDK 版本只钉在仓库根的 `global.json`，`_import/qian/` 下没有另一份；那里另有带 `root = true` 的 `.editorconfig` 与 pnpm 工作区。在仓库根上不带参数跑 `dotnet`，构建的是整个 `Kun.slnx`；只构建 `_import/qian/` 下的项目时，在那里用 `Qian.slnf`，它引用的 Kun 项目会跟着构建。`_import/qian/` 的其余命令一律在那里跑，不要在仓库根上跑 `pnpm`。仓库根的 `.gitignore` 与 `.gitattributes` 对 `_import/qian/` 同样生效。

检查只有一个入口：仓库根的 `.github/workflows/ci.yml`（S1-KUN-020）。它先跑变更检测，按区域表 `scripts/ci_changes.py` 把改动的文件归到各个区域（.NET 的构建、门禁、测试与原生发布，镜像自检，文档与看板，文档站，各项前端验收等），只跑涉及的区域的作业；推 `main` 与手动触发时全部区域都跑。前端与文档的检查是 `.github/workflows/` 下的 `qian-*.yml`，只由 `ci.yml` 调用，在 `_import/qian/` 下运行；其余作业直接写在 `ci.yml` 里，仓库根上的 Kun 框架与 `_import/qian/` 下的 .NET 项目都在其中。两边导入时带来的 `.github/` 原件，Kun 框架的在 `.github/originals/kun/`，平台的在 `_import/qian/.github/`，GitHub 都不读；`ci.yml` 与 `qian-*.yml` 的开头写着对应哪个原件、相对原件改了什么，Kun 框架没接的工作流与模板见 `.github/originals/kun/README.md`。

最后一个作业「CI 汇总」汇总全部作业的结果，必需检查只设它一个：变更检测没有成功、或任一作业失败或取消，即不通过；没涉及而跳过的作业算通过。几条约定：

- 「CI 汇总」这个检查名不改。分支保护按名字认必需检查，改了名，PR 会一直等一个不会出现的检查。
- 提交说明里不写跳过 CI 的标记（`[skip ci]`、`[ci skip]`、`[no ci]`、`[skip actions]`、`[actions skip]`），PR 标题也不写，合并提交会带上它。带了标记，整次运行不触发，「CI 汇总」没有结果，PR 合不进去，`main` 上也不跑。
- 加文件、挪目录时，让它落进区域表里合适的区域（要构建 .NET 的区域都含仓库根的构建基线、`Kun.slnx` 与 `src/`），或者明列进不跑检查的名单。区域表的自测与 `ci.yml` 的结构自测在 `scripts/tests/`，变更检测作业每次先跑，本机跑法 `python3 -m unittest discover -s scripts/tests -p 'test_ci_*.py' -v`；查一个区域管哪些文件用 `python3 scripts/ci_changes.py files <区域>`。

原仓库归档之后的规矩：

- 两个原仓库 2026-11-17 冻结，同日归档为只读。最后一次同步的来源头是归档时的最终头：Kun 框架那一支的 `51b480775c` 与平台那一支的 `5954ec3b86`，两个提交都在本仓库的历史里；同步时 `_import/kun/`、`_import/qian/` 与这两个头的树逐字节相同。两边的源码、文档与任务文件从此以本仓库为准，原仓库不再接受改动。
- 两边的内容从此在本仓库改，在本仓库开 PR。`_import/qian/` 里改的是文件内容；它的搬动与拆分属于后面的批次（见最后一条），不要顺手做。
- Kun 框架的约定已并进本文件（下一节），`_import/qian/AGENTS.md` 里构建、测试与代码约定的部分照用。任务文件照 `_import/qian/pm/README.md` 改：任务文件在 `_import/qian/pm/tasks/` 下，`pm.py` 在 `_import/qian/` 下运行（如 `cd _import/qian && python3 scripts/pm.py validate`），`set` 与 `add` 要带 `--by`，生成物不进 git。迁移任务（S1-KUN-018–022）的文件也在这里改（迁移执行计划第 3 节第 7 条）。`_import/qian/AGENTS.md` 与仓库根的 `CONTRIBUTING.md` 里讲提 PR、分支、发版、发布文档站的部分说的是原仓库，在本仓库以本文件与迁移执行计划为准。
- 两个原仓库冻结时写的去处说明随最后一次同步带了进来，在本仓库不适用：`_import/qian/` 的 `README.md` 开头那一段与 `AGENTS.md` 的「本仓库已冻结」一节（Kun 框架那一份 `README.md`、`CLAUDE.md` 原件已在整理的第二段删除）。Kun 框架自管的任务队列（含 `docs/roadmap/BACKLOG.md`）与第三方生成的代理配置已按合并蓝图删除（D23.13 第 6 点），任务管理统一到 `_import/qian/pm/`；各层工具给 Kun 框架生成的 `AGENTS.md` 也一并删了，模块目录里留下的 `AGENTS.md` 只有生成之后写进去的模块规则。
- 前端框架 qian 在 `CavaiLAB/qian`，改前端框架去那边改。`_import/qian/` 下归它的部分（框架包与演练场、TypeScript 运行时、前端插件开发包、对应的门禁与视觉回归脚本、`.changeset/`，以及四个工作流和它们在仓库根上的两份接入）已在 B11 由前端计划的执行会话删除（见迁移执行计划的 B11）。外壳应用与外壳工具包、三个界面包、参考应用的远程模块、文档站、命令行、声明式表面的校验库留在本仓库，装 `CavaiLAB/qian` 发布在 GitHub Packages 上的 `@cavailab/qian-*`：版本钉在 `_import/qian/pnpm-workspace.yaml` 的版本目录，装依赖要令牌（见 `_import/qian/README.md`「前端依赖与包源」），外壳与界面包的门禁在 `qian-ui.yml`。
- 一批一个 PR。本仓库的 PR 一律用合并提交合入，不压缩、不变基，否则导入的历史会丢。除迁移批次的 PR 之外，不要新建顶层代码目录，也不要把代码搬出 `_import/qian/`。
- 不要抢跑：整理的第二段（树内的历史 PR 编号改写成带仓库名的形式、指向原仓库的地址改指本仓库、`_import/qian/` 下文档站的「Kun 框架」小节改为同仓来源、导入的文档里写给特定下游的内容怎么处理）由计划指定的执行会话做，Kun 框架一侧已经做完（文档整理进 `docs/`，`_import/kun/` 已搬空）；构建并轨是 B3；包名、命名空间与类型名统一到 Kun 下是 B4–B10。

目标仓库已定为本仓库（产品负责人，2026-11-17）。合并蓝图里写的目标仓库以[迁移与归并](docs/migration.md)第 6 节与迁移执行计划为准。

## Kun 框架的构建、测试与代码约定

本节并自 Kun 框架的 `CLAUDE.md` 与根上那份 `AGENTS.md`（两份都已删除；原文见 git 历史，Kun 框架那一支的最终头 `51b480775c` 上的同名文件，如 `git show 51b480775c:CLAUDE.md`）。Kun 框架的文档已整理进 `docs/`：入门在 `docs/getting-started.md`，指南、安全与设计说明在 `docs/guides/`、`docs/security/`、`docs/design/`，决策记录在 `docs/adr/kun/`（引用时写「Kun ADR-0xx」）；AOT 审计、基准结果、路线图、发版手册与发布说明等历史文档已删除，原文同样在 `51b480775c`。代码与构建文件注释里写的 `docs/…` 是整理之前的路径，没有改：`docs/architecture/` 下的 `ADR-*`、`adr-*` 现在在 `docs/adr/kun/`，其余的在 `docs/design/`；`docs/observability/`、`docs/schema-per-service.md`、`docs/openapi-template.md` 在 `docs/guides/`；`docs/whitepaper.md` 在 `docs/design/`；别的路径不变。原件里讲原仓库流程（分支、合并、发版、派单）的部分以本文件为准。原件里的分层规则已由档位与依赖门禁取代（D23.7、D23.14，见下面「档位与依赖」）；包名与命名空间在 B4–B10 改，在那之前照下面的做。

### 构建与测试

命令在仓库根运行。要 .NET SDK 10.0.100 以上（`global.json` 钉的版本，允许滚到 10.0 里更新的功能版本）；集成测试要 Docker。测试运行器是 Microsoft.Testing.Platform（`global.json` 的 `test.runner`，xunit.v3 4.x 不再支持 VSTest）：`dotnet test` 用 `--project` 或 `--solution` 指定目标，旧的 `--filter "FullyQualifiedName…"` 与 `--collect` 不再可用。

```bash
dotnet restore
dotnet build -c Release
dotnet test --solution tests/kun-unit.slnf -c Release --no-build
dotnet format --verify-no-changes --exclude _import/qian
```

- 不带参数的 `dotnet restore`、`dotnet build` 用的是 `Kun.slnx`，`_import/qian/` 下的项目也一起构建。格式检查排除 `_import/qian`，与 CI 一致：`_import/qian/` 有自己的 `.editorconfig`，格式门禁不管它。
- `tests/kun-unit.slnf` 列出 Kun 框架除 `Kun.IntegrationTests`、`Kun.Sample.Tests`、`Kun.Integration.AotTests` 之外的全部测试项目，即单元测试；新加单元测试项目时同时加进这个文件。覆盖率加 `--coverlet --coverlet-output-format cobertura`（单元测试项目都引用了 coverlet.MTP）。
- 单个测试项目：`dotnet test --project tests/Kun.Mediator.Tests -c Release`；单个测试类或方法用 xunit 的筛选参数，如 `--filter-class "*.类名"`、`--filter-method "*.类名.方法名"`。
- 集成测试（`tests/Kun.IntegrationTests`）用 Testcontainers 起容器，对象存储测试用 RustFS 作 S3 兼容服务。本机没有容器环境时跳过；设了 `CI` 时改为失败；拉不到镜像一律失败。跑法：`dotnet test --project tests/Kun.IntegrationTests -c Release`。
- `samples/Kun.Sample.Tests` 要 Aspire 的服务发现环境变量，CI 不跑。
- CI（`ci.yml` 的 dotnet 区域）另外：原生发布并运行 AOT 冒烟宿主 `tests/Kun.AotSmoke`，ILC 有任何警告即失败（清单见下面「项目文件与包」）；跑 `tests/Kun.Integration.AotTests`（样例与网关包的 NativeAOT 发布并运行）；按档位检查依赖（`scripts/check_deps.py`，见下面「档位与依赖」）与身份模块的日志隐私和产品中立（`scripts/check-identity-boundaries.py`）；用占位版本打全部可打包项目并核对产物（`scripts/release_packages.py`，见下面「项目文件与包」），不推送。单元测试与集成测试在测试分片 k 里跑：每个模块各写一份 TRX 与覆盖率，覆盖率上传前核对 `tests/kun-unit.slnf` 里每个测试项目恰好一份；汇总作业再核对每个模块都跑过、没有失败。提交前至少跑一遍格式检查。
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
- 禁用符号与平台分析器按档位接入（D23.14 第 4 点，仓库根的 `Directory.Build.targets`）：`BannedSymbols.txt`（RS0030）与 `_import/qian/tools/Qian.Build.Analyzers`（QIAN0002–QIAN0006）接平台档的包与宿主（项目文件里写 `<KunHost>true</KunHost>`，现在是 `_import/qian/hosts/` 下的两个）；过渡期 `_import/qian/` 下的项目全部接入，内核档的包也在内，各域批次把包搬出过渡目录时再按归属收窄。仓库根上 Kun 的项目不接，被禁的类型就定义在 `src/` 下；Kun 旧类型上的 `Obsolete` 诊断号（如 KUNS2S001、KUNLEGACY001）照旧随警告即错误报错。新写的宿主要写 `KunHost`。
- 新的横切机制（认证、审计、多租户、事件总线）要先有决策记录。
- 蓝图的归档名单里有 `Kun.Saga`、`Kun.Sandbox`、`Kun.Dapper`、`Kun.Infrastructure` 与旧线的 `Kun.Storage`（D23.9），各域批次会把它们移出解决方案；`Kun.Dapper`、`Kun.Sandbox` 与旧线的 `Kun.Storage` 已归档。

### 项目文件与包

- `TargetFramework`、`Nullable`、`LangVersion`、`TreatWarningsAsErrors`、分析器只在 `Directory.Build.props` 里设，不写进单个项目文件。
- AOT 按项目选择：AOT 兼容的库在项目文件里写 `<IsAotCompatible>true</IsAotCompatible>`，SDK 据此打开 `IsTrimmable` 与裁剪、AOT、单文件三个分析器，普通构建里就查；不兼容的什么都不写，项目文件里写一句为什么。几条守卫在仓库根的 `Directory.Build.targets`：写了 `IsAotCandidate`（已经不用的开关）报 KUNAOT001；可打包的库在项目文件里写 `PublishAot` 报 KUNAOT002，它只写在要原生发布的可执行项目里；写了 `IsAotCompatible` 却关掉分析器报 KUNAOT003。
- AOT 清单就是写了 `IsAotCompatible=true` 的项目，没有别的副本：`python3 scripts/aot_projects.py` 列出它们。AOT 冒烟宿主 `tests/Kun.AotSmoke` 引用其中每一个并整体扎根，CI 原生发布后运行它；它的项目引用必须正好等于清单，库加上或去掉 `IsAotCompatible` 时同时改它，`python3 scripts/aot_projects.py --check` 与 `tests/gates` 的自测核对。本机原生发布：`dotnet publish tests/Kun.AotSmoke -c Release`（要 clang；在 macOS 上发布的是本机架构，IL 分析与 CI 的 linux-x64 相同）。`scripts/release.sh` 发版前照 CI 的做法核对、原生发布并运行冒烟宿主。
- 发版的包名单就是 `Kun.slnx` 里可打包的项目，不另写：`python3 scripts/release_packages.py list --transitional` 列出并核对档位与角色。CI「构建与门禁」用占位版本打全部包，`verify-packages --transitional` 核对：恰好是名单上的包、版本一致、名单内的依赖都在且写成 `V`、`[V]` 或 `[V,)`、符号包配对（只带分析器的包免符号包、也不许带，两种模式一样：项目求值 `IncludeBuildOutput=false` 且包里 `lib/`、`runtimes/` 下没有 `.dll`、`.exe` 才算，不看包名，这样的项目写 `IncludeSymbols=false`）、MIT、仓库地址与提交号、作者与版权与项目地址（照 `src/Directory.Build.props`）、描述不空且不写过渡目录的名字、内核档不依赖平台档、稳定版不带预发布依赖（项目在 `NoWarn` 里写了 NU5104 的除外）。过渡模式只给项目在过渡目录（`_import/`）下的包放行其中几项（包名前缀、仓库地址、符号包、元数据、描述、预发布依赖），`src/` 下的包两种模式都按严格判，包一搬进 `src/` 当次就查；档位例外不分位置只报数。开关写在脚本开头，B10 之后去掉 `--transitional` 收紧。
- 包版本统一写在 `Directory.Packages.props`（中央包管理），项目里写 `<PackageReference Include="X" />`，不带 `Version`。加新包先查这里有没有；AOT 兼容的模块还要核对新包的 AOT 兼容性。
- 公共 API 要写 XML 文档注释。可打包的项目都要带 `PublicAPI.Shipped.txt` 与 `PublicAPI.Unshipped.txt`，首行写 `#nullable enable`（仓库根 `Directory.Build.targets` 的公共 API 门禁，按 `IsPackable` 判定）：缺文件报 QIAN0001；新增公共 API 没登记报 RS0016，登记了却已删除报 RS0017，都是编译错误。新 API 先登记进 Unshipped，IDE 对 RS0016 的代码修复会生成登记行。
- 改一个模块之前，先读它目录里的说明文件（`AGENTS.md`、`README.md`）与 `docs/` 下对应的设计说明、指南与 Kun 决策记录，也读要引用的模块的。

### AOT 兼容模块里不许用的写法

- 运行期反射：`Activator.CreateInstance`、`Type.GetMethod`、`MakeGenericType`、`Assembly.GetTypes`、`AppDomain.CurrentDomain.GetAssemblies`；`System.Reflection.Emit`。
- 不带 `JsonTypeInfo` 的 `JsonSerializer.Serialize(obj)`：序列化走源生成的 `KunJsonContext`，被序列化的事件类型都要在里面登记。
- Dapper：它的经典 `Query<T>` 靠运行期反射生成读取代码，AOT 兼容的模块不引用它。

### 运行时约定

- 命令与查询分离。`Kun.Mediator` 的处理器在编译期由源生成器发现，不要改回反射解析。写侧（命令处理器）用 `Kun.EntityFrameworkCore`：仓储、规约、变更跟踪，在 `SaveChangesAsync` 里登记 Outbox；读侧（查询处理器）直接用 Npgsql 的数据源与 Dapper：原生 SQL、DTO 投影、不加载聚合，写法见 `docs/guides/read-side-patterns.md`。查询处理器里不用 EF Core，命令处理器里不用 Dapper。
- Outbox：集成事件与业务数据在同一事务里落库，事件类型用稳定的字符串 id（如 `order.placed.v1`），不用程序集限定名；优先用 `[IntegrationEventId("order.placed.v1")]` 特性加 `Register<TEvent>()`。消费端先查成功标记，再执行处理器、写成功标记、确认消息；业务副作用、标记与确认之间仍有重放窗口，写类处理器必须幂等。设计说明见 Kun ADR-015。
- Saga 只做编排：一个 Saga 是有序的步骤，每步有执行与补偿；补偿从失败的那一步起逆序执行，补偿本身要幂等，没执行过的步骤也要能安全补偿。
- 服务间零信任：外部令牌只在 `Kun.Gateway` 校验；网关用自己的私钥签发内部短期令牌（RS256 或 ES256，不用 HS256），下游经 `Kun.Security` 校验。身份（`sub`、`tenant_id`）只从内部令牌的声明里取，不信 `X-User-Id` 这类请求头。
- 多租户：`Kun.Core` 的 `ITenantContext` 贯穿全栈；实现 `IMultiTenant` 的实体自动带 `TenantId` 查询过滤器，除明确的管理场景外不要用 `IgnoreQueryFilters()`；可以再加 PostgreSQL 行级安全作纵深防御。租户号改为只收 Guid 是 B5 的事（D23.5）。
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
| Saga 与步骤 | `{名词}Saga` / `{动词}Step` | `OrderCreationSaga` / `ReserveInventoryStep` |
| Outbox 的稳定事件 id | `{上下文}.{名字}.v{n}` | `order.placed.v1` |

提交信息用 Conventional Commits：`<type>(<scope>): <subject>`，Kun 框架模块的 scope 是模块名去掉 `Kun.` 前缀（如 `feat(saga): …`），末尾带任务 id（任务看板的约定，如 `[S1-KUN-019]`）。

## 要改文档

1. 先读 [README.md](README.md) 与平台总览仓库的[术语表](https://github.com/CavaiLAB/jiangxin/blob/main/docs/glossary.md)。「Qian」只作前端框架的名字，`_import/qian/` 下的平台后端并入 Kun。组件 id 前缀 `qian.`、清单文件名这些运行期字面当作历史字面保留；`qian` 命令行与 `@qian/*` 导入名本次不改。
2. 合并蓝图是本仓库里的 `_import/qian/docs/adr/ADR-023-kun-qian-merge-blueprint.md`，2026-11-17 接受，裁定记在 `_import/qian/pm/tasks/S1-DEC-036.md`（D1–D14）。写到归并的细节时以它为准，蓝图里的条目按已定写。被之后的裁定改写的几项（目标仓库、仓库划分、包的作用域、总名）见[迁移与归并](docs/migration.md)第 6 节。
3. 文档用中文，不写参考平台和对标项目的名字。
4. 分清「已定」「提议」「待定」。说现状的时候写清是「已有」（在原仓库里已经实现）还是「规划」。
5. 平台级的口径（名称、三个框架的分工、Agent 协同）在 [jiangxin](https://github.com/CavaiLAB/jiangxin) 里改，这里只引用。
6. 在 `docs/` 下新增文档，要在 `docs/_meta.json` 里加上它的文件名（不带扩展名）。这个文件定这些文档在平台文档站侧栏里的顺序；漏列的话，文档站升级到这个版本时构建会报错。子目录在 `docs/_meta.json` 里写成对象，如 `{ "type": "dir", "name": "guides", "label": "指南" }`，label 用中文；在子目录里加文档不用改顶层这一份。子目录自己的 `_meta.json` 可以不写，不写就按文件名排；写了就要列全，漏列的页面会从侧栏里消失，构建不报错。
7. 改动走分支和 PR，不直接推 `main`。建仓期间的初始文档除外，见平台总览的[协作约定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/07-governance.md)第 2 节。
