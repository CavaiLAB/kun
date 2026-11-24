# 测试与门禁

> 对应任务 S1-QA-001。依据：[PRD §14 质量与评估](../docs/prd/14-quality.md)（测试金字塔、C1 PublicAPI、C10 测试门禁、C19 依赖图断言）、[ADR-019](../docs/adr/ADR-019-library-first-packages.md) D19.7 / D19.8、[组件模型规格](../docs/prd/specs/component-model.md) §6.1 与 §10.3。

## 1 目录

`tests/` 收录 .NET 测试、测试宿主、原生发布验证及分析器样例。`tests/Directory.Build.props` 提供项目级 TRX 与覆盖率文件名。`src/Kun.Testing.Containers/` 提供 Testcontainers 夹具。浏览器目录验收在 `tests/directory-e2e/`，界面包门禁在 `tests/gates/ui/`。

```text
src/Kun.Testing.Containers/                  通用容器夹具 NuGet 包
  ApolloMock/apollo_mock.py                  容器内运行的 Apollo 协议替身
tests/
  Kun.Testing.PlatformFixtures/               真实 Apollo 服务与数据库种子
  kun-platform.slnf                          从根目录参与 a/b/c 分片的测试项目
  Kun.Audit.EntityFrameworkCore.IntegrationTests/命令审计的 EF 与 PostgreSQL 契约
  Kun.Audit.Postgres.AotTests/               严格原生审计探针与真实数据库
  Kun.Audit.Postgres.Tests/                  原生 PostgreSQL 审计存储
  Kun.Authorization.Rbac.Tests/              授权：权限目录与默认角色、RBAC 判定与快照缓存（真实 PostgreSQL + Redis，两宿主授予 / 撤销传播）、平台超管旁路（专用平台登录 + 真实 Audit 插件）
  Kun.Configuration.Platform.Tests/          配置与真实 Apollo 的宿主集成
  Kun.Data.AspNetCore.Tests/                 数据端点：基准类型目录（无数据库）与数据连接档案（真实 PostgreSQL、挂载式密钥、连通测试）
  Kun.Data.Native.Postgres.AotTests/         独立原生持续门：运行严格发布的 Linux 二进制，复用共享 fixture 验证真实数据库；不在常规解决方案测试中跳过
  Kun.Data.Native.Postgres.Tests/            NativeData：受限登录、精确 RLS 策略、租户/平台原始 SQL、事务与可撤销会话生命周期（真实 PostgreSQL）
  Kun.Data.Tests/                            数据会话、事务与 Outbox
  Kun.EventBus.IntegrationTests/             事件信封、Outbox 和消息代理
  Kun.Files.Tests/                           文件服务与 HTTP 端点
  Kun.Identity.Client.Standalone.Tests/      独立宿主 E2E：只引用 Kun.Identity.Client，对接 Dex（DexFixture）完成登录与登出，并断言依赖闭包
  Kun.Identity.Client.Tests/                 依赖方：真实 qian-identity（内置账号模型、强制 PKCE）上的服务端换码与 qian.sid 会话、篡改 nonce / iss / state 重放、刷新单飞与轮换（真实 Redis）、登出防伪、卸载、后通道登出；会话主体经 Kun 网关签发器铸 S2S 后下游接受；S2S 契约逐条违约与透传拒绝
  Kun.Identity.Server.Tests/                 真实 OpenIddict + PostgreSQL + Kun 客户端协议验收、跨宿主吊销、后通道与密钥轮换；账号模型（Argon2id、标识与绑定守卫、sub 不碰撞、配额）、统一登录流水线（审计、限流与并发、强制改密、租户选择与切换、MFA 不降级）与账号 HTTP 面端到端（可控时钟 + 可控租户目录）、会话中心（吊销即时与旁路、并发上限与单端、租户冻结、真实 Redis 索引与宕机降级、管理面、多副本竞争、日志卫生）
  Kun.Tenancy.Tests/                         租户目录与宿主契约
  Kun.Testing.Containers.Tests/              fixture 自身的集成测试与单元测试
  Kun.Gateway.Host.AotTests/                生产网关的 Linux NativeAOT 验收
  Kun.Gateway.IntegrationTests/                        网关路由、限流和外壳宿主
  Kun.Capabilities.Tests/
  Kun.Common.Tests/                          src/Common 共享内部源文件的测试，直接编译源文件；并生成 CLI 移植比对用的 tck/profiles/key-vectors.json 与等价类表
  Kun.Docs.Tests/
  Kun.Lifecycle.Tests/                       生命周期：验签与信任根轮换（真实 PostgreSQL）、GitOps 执行器（本机裸仓库 + K3s 单节点集群）、Local 执行器
  Kun.Profiles.Tests/
  Kun.Registry.Tests/
  Kun.Scheduler.Tests/
  Kun.Surfaces.Tests/
```

## 2 本地运行

```bash
dotnet build Kun.slnx -c Release
python3 scripts/check_deps.py --no-restore
python3 -m unittest discover -s tests/gates -v
dotnet test --solution tests/kun-unit.slnf -c Release --no-build
python3 scripts/dotnet_ci.py shard a --out /tmp/kun-tests-a.slnf
dotnet test --solution /tmp/kun-tests-a.slnf -c Release --no-build
```

根 `Kun.slnx` 描述全部 .NET 项目。`tests/kun-platform.slnf` 登记分片 a、b、c 的测试。`tests/kun-unit.slnf` 登记分片 k 的单元测试，集成测试与专属作业由 `python3 scripts/dotnet_ci.py registration` 核对。

外壳与界面包使用仓库根的 pnpm 工作区。运行 `pnpm ui:gates` 检查界面规则，运行 `pnpm e2e:directory` 验收目录选择器。

- 本机跑全量 `dotnet test` 前先补齐 CI 的两个前置，否则 `Kun.Capabilities.Tests` 与 `Kun.Sample.Crm.Tests` 会出现 19 个环境性失败（S1-DEC-019）：
  - 在仓库根 运行 `pnpm install --frozen-lockfile && pnpm --filter "qian..." build`（装依赖要 GitHub Packages 的令牌，见 `README.md`「前端依赖与包源」）：doctor 用例要调用 `packages/qian-cli/dist/main.js`；
  - `python3 scripts/prepare_contract_tools.py --destination <目录>`，再导出 `QIAN_BUF_PATH=<目录>/buf` 与 `QIAN_OASDIFF_PATH=<目录>/oasdiff`：能力契约的真实兼容检查器，缺失时相关用例按设计拒绝跳过。
- `dotnet test` 走 Microsoft Testing Platform（仓库根 [global.json](../global.json) 的 `test.runner`）；xUnit v3 测试项目是可执行程序，csproj 里写 `<OutputType>Exe</OutputType>`。
- 单跑一个测试项目用 `dotnet run --project tests/<X>.Tests -c Release`；`dotnet test <项目路径>` 或 `dotnet test --project …` 在部分机器上会报「运行了零个测试、退出码 5」，以 `--solution` 或 `dotnet run` 的结果为准。
- 已发布组件的 CLI 安装测试单独执行 `pnpm --filter qian test:feed`。设置环境变量 `KUN_PLATFORM_FEED_USER`（GitHub 用户名）与 `KUN_PLATFORM_FEED_TOKEN`（有 `read:packages` 权限的令牌）。测试从 nuget.org 还原稳定版内核、从 GitHub Packages 还原平台包，并检查每个包的实际来源。网络较慢时可另设 `QIAN_PACKAGE_TIMEOUT`（整数秒，缺省 600）。
- 集成测试需要 Docker。本机没有 Docker 时集成测试**跳过**；在 CI（`CI=true`）或设置了 `QIAN_REQUIRE_DOCKER=1` 时**直接失败**，不会悄悄跳过（C10：集成测试必须跑）。
- 首次运行要拉镜像，之后整套测试十秒左右。

## 3 写一个集成测试

整组测试共享一套容器：在测试项目里声明一次 collection，测试类挂上它。

```csharp
[CollectionDefinition(Name)]
public sealed class SharedInfrastructure : ICollectionFixture<InfrastructureFixture>
{
    public const string Name = "shared-infrastructure";
}

[Collection(SharedInfrastructure.Name)]
public sealed class OrderVisibilityTests(InfrastructureFixture infra)
{
    [Fact]
    public async Task Tenant_a_cannot_read_tenant_b()
    {
        // 建表、迁移、造数用超级用户；被测代码一律用受 RLS 约束的应用角色。
        var admin = infra.Postgres.ConnectionString;
        var app = infra.Postgres.AppConnectionString;
        // ...
    }
}
```

只用一个依赖时，也可以把单个 fixture 直接当 class fixture 用。

| fixture | 默认镜像（覆盖用的环境变量） | 提供什么 |
|---|---|---|
| `PostgresFixture` | `postgres:16-alpine`（`QIAN_TEST_IMAGE_POSTGRES`） | `ConnectionString`：超级用户，做 DDL 与造数；`AppConnectionString`：应用角色 `qian_app`（NOSUPERUSER NOBYPASSRLS，只有 DML 权限）。验证 RLS 必须用应用角色——超级用户会绕过 `FORCE ROW LEVEL SECURITY` |
| `RedisFixture` | `redis:7-alpine`（`QIAN_TEST_IMAGE_REDIS`） | `ConnectionString` |
| `RabbitMqFixture` | `rabbitmq:4.3-alpine`（`QIAN_TEST_IMAGE_RABBITMQ`） | `ConnectionString`（AMQP URI）；`Container` 可停启 broker，做「重启后消息不丢」这类韧性测试 |
| `ApolloFixture` | `python:3.13-alpine` 运行 `apollo_mock.py`（`QIAN_TEST_IMAGE_APOLLO_MOCK`） | `ConfigServiceUri`（即客户端的 `Apollo__ConfigServer__0`）；`PublishAsync`、`DeleteAsync`、`SetAccessKeyAsync`、`SetHoldAsync`、`ResetAsync` |
| `InfrastructureFixture` | 以上四个并行启动 | `Postgres`、`Redis`、`RabbitMq`、`Apollo` |
| `K3sFixture` | `rancher/k3s:v1.31.5-k3s1`（`QIAN_TEST_IMAGE_K3S`），特权容器 | `Kubeconfig`（API Server 已映射到宿主端口）；`KubectlAsync` 在容器内执行 kubectl（含 kustomize），`CopyAsync` 写入待 apply 的清单。K3s 启动时会短暂占满 CPU，容器在就绪前自行退出时重建，最多三次（只重建容器，不重试断言）；用例里的工作负载镜像由集群自行拉取，需要出网 |
| `DexFixture` | `ghcr.io/dexidp/dex:v2.41.1`（`QIAN_TEST_IMAGE_DEX`） | 独立实现的外部 OIDC 发行方（S1-IDP-005 的独立宿主 E2E）：`Issuer`、`RedirectUri`（登记给公开 PKCE 客户端 `ClientId` 的唯一回调，依赖方须在该端口以 HTTPS 监听）、`Email` / `Password`（本地密码库用户，口令每次随机、配置里只有 bcrypt 哈希）。issuer 要写进配置，宿主端口在启动前选定，不用随机映射 |

- 版本：PostgreSQL 16、Redis 7 与部署基线一致（ADR-017 D17.2）；RabbitMQ 取当前 4.x 稳定线。可通过环境变量选择与预发一致的版本或指定镜像源。
- 容器口令每次运行随机生成，仓库里没有写死的凭据。
- 启动失败只在环境原因下重试（S1-QA-011）：
  - Ryuk 初始化失败：Testcontainers 在每个测试进程第一次启动容器时初始化 Ryuk（资源回收容器），等待连接确认的超时写死为 60 秒，宿主繁忙（如多个会话并发跑容器测试）时会耗尽，抛 `ResourceReaperException`。
  - Docker API 请求超时：Docker 守护进程在 Docker 客户端的请求超时内没有响应，抛 `TaskCanceledException`，内层为 `HttpRequestException`。本次启动自己的取消令牌触发的取消不算。
  - 遇到上面两类，所有 fixture 都先释放这次启动拿到的资源，退避后从头再来，最多三次；每次重试记一条诊断消息。
    - 基类只释放主容器：先调用 `OnDisposingAsync`，再释放主容器，最后调用 `OnDisposedAsync`。`OnStartingAsync` 准备的依赖容器与网络、`StartAlongsideAsync` 与主容器并行启动的同级容器，由 fixture 自己在这两个钩子里释放，例如 `RealApolloFixture` 在 `OnDisposingAsync` 里释放 AdminService 与 Portal、在 `OnDisposedAsync` 里释放 MySQL 与网络。新写 fixture 时两个钩子要能处理尚未启动完的状态。
    - 主容器与同级容器都结束才判定成败，失败时不会释放仍在启动的容器；两者都失败时，按先出错的那个判定是否重试（S1-QA-020）。
  - 容器在就绪前自行退出：只在 fixture 声明了 `StartAttempts` 时重试，目前只有 K3s。
  - 其他错误、以及测试已取消时，不重试。
  - 这里只重试启动，不重试断言，与下一条「安全类用例不允许重试通过」不冲突。
- PRD §14.2.1 的约定照旧：多副本用同一套容器起 2–3 个宿主实例，不用「单实例 + 顺序调用」冒充；集成测试默认在两个租户下运行（租户夹具随租户组件补上）；安全类用例不允许重试通过。

### Apollo 协议替身

PRD §14.2 规定集成测试层的 Apollo 用 mock server。[apollo_mock.py](../src/Kun.Testing.Containers/ApolloMock/apollo_mock.py) 只依赖 Python 标准库，实现 Apollo「其它语言客户端接入」HTTP 协议里客户端会用到的部分：

| 接口 | 行为 |
|---|---|
| `GET /configs/{appId}/{cluster}/{namespace}` | 返回 `configurations` 与 `releaseKey`；带相同 `releaseKey` 返回 304；未发布或已撤下返回 404 |
| `GET /configfiles/json/{appId}/{cluster}/{namespace}` | 配置的 JSON 视图 |
| `GET /notifications/v2` | 长轮询：客户端落后时立即返回新通知号；否则挂起（默认 60 秒，与 Apollo 服务端相同）直到有发布，超时返回 304 |
| `GET /services/config` | Meta Server，返回本服务地址 |
| 访问密钥 | `SetAccessKeyAsync` 之后，请求必须带 `Timestamp` 与 `Authorization: Apollo {appId}:{签名}` 头；签名 = Base64(HMAC-SHA1(密钥, 时间戳 + 换行 + 路径与查询串))，时间差超过 60 秒或签名不符返回 401 |

不覆盖：灰度发布与 label 规则、Portal / Admin Service 接口、多实例 Meta Server、增量同步。自研 Apollo provider（S1-CFG-001）若要对真实服务端做契约测试，需另起 Apollo；官方 quick-start 镜像默认要配一个 MySQL 容器（只切换到 H2 配置组仍会去连 MySQL，已实测）。

### CRM 参考宿主验收的用例写法

`Kun.Sample.Crm.Tests` 的 `CustomerCommandTests` 是抽象的公共基类：`Start()` 起完整宿主，另有各主题共用的辅助方法、常量与探针类型，本身不含用例。用例分在八个 sealed 主题类里，各自一个 `PostgresFixture` 容器，主题之间并行、主题内串行（S1-QA-017）；主题类与各自的范围见 [CustomerCommandTests.cs](Kun.Sample.Crm.Tests/CustomerCommandTests.cs) 开头的注释。

- 新用例写进对应的主题类，即文件里 `public sealed partial class <主题类>` 那一部分；配套的辅助方法、常量与探针类型照旧写进同一文件里基类的 `public abstract partial class CustomerCommandTests` 部分。
- 基类不放 `[Fact]` / `[Theory]`：xUnit 按继承发现测试方法，写在基类的用例会在八个主题类各跑一遍。
- 照拆类前的写法声明 `sealed partial class CustomerCommandTests` 会与抽象基类冲突，编译报 CS0418；漏写 `sealed` 的 `partial class CustomerCommandTests` 却能静默编译通过（`abstract` 写在其他部分上同样生效），里面的用例就进了基类。后一种由守卫用例 `CustomerCommandLayoutTests` 拦下，它断言基类不含测试方法（S1-QA-020）。
- 新增主题类前先看耗时：程序集并发上限 `MaxThreads = 8`，xUnit 按集合分槽位（这里一个类一个集合），类多于 8 个时排在后面的类要等前面的类跑完；各类的耗时分布与重新平衡见 S1-QA-019。

### 调度验收的用例写法

`Kun.Scheduler.Tests` 的用例按主题分在四个 sealed 类里，各自一个 `SchedulerFixture`（独立的 PostgreSQL 容器、角色与 schema），类之间并行、类内串行（S1-QA-022）；各类的范围见 [SchedulerScenario.cs](Kun.Scheduler.Tests/SchedulerScenario.cs) 开头的注释。

- 共用的租户、作业目标与参数是静态类 `SchedulerScenario` 的只读成员，各文件 `using static` 引用；没有公共基类，新用例直接写进对应主题类。
- 分钟验收 `SchedulerMinuteTests` 要等两个真实分钟边界（CI 上 90–143 秒），是程序集的长杆，独占一个类；不往里加用例，其他类的总时长也要留在它之下。100 个 cron 分 10 路并发建：逐个建时 CPU 被挤要 20–60 秒，会跨过分钟边界。
- 同一个类的用例共用一个库：用到库的用例都先 `Reset()` 清表，有的还建删触发器、改授权，所以只能串行。要并行就新开类、配自己的夹具，并同步调高程序集并发上限 `MaxThreads`（现为 4，按容器数定）。
- 等待与观测都按用例传给 worker 的租约（`HostConfig.LeaseSeconds`）计算。真杀、正常停机和续租故障后的接管使用 12 秒租约，核对原租约到期时刻及同一 run 的接管。主动让准备阶段跨越到期点或触发初始续租故障的用例使用 6 秒租约。
  - 等待上限用 `fixture.Leases(n)` 按租约份数给。一份覆盖一次「领取到初始续租」的准备，或一次到期；用例要经过几段就给几份，例如等首个处理器给 2 份，到期后等恢复给 3 份。不写死秒数。
  - 续租后至少还剩半份租约，才算观测到有效续租。
  - 有效准备只留 1.5 秒，小于首个续租周期 2 秒。这个阈值写在注入的数据库触发器与断言里，改租约时要同步重算。
  - 测到期、接管的用例靠杀进程、停 worker 或注入的数据库触发器造出到期，不靠冷路径恰好慢。
  - 不测租约到期的用例（分钟验收、同业务键、冻结租户、misfire、超时重试与取消）用生产缺省的 30 秒租约 `fixture.LongLease`，等待上限传它的配置 `fixture.Leases(n, config)`。租约短了，冷路径恰好慢时第一次执行会在准备阶段丢租约；分钟验收与 misfire 的 worker 第一批还一次领多个、同时准备，CPU 被挤时这一批的准备在预热之后仍可能超过 6 秒租约的一半。取消用例等待半份租约，让处理器在下一个心跳（租约的三分之一）发现取消，再核对终态与副作用。
- worker 进程打出 READY，表示它的第一次执行不再付冷启动。
  - 做法：`ProbeHost` 在挂调度器之前，用调度器的内部入口（`ClaimAsync(0)`，以及对不存在触发器的 `CompleteAsync`）跑过领取与收尾；再开一个最后回滚的事务，造一组定义与触发器，照 `BeginAsync` 的写法把首次建 run 与接管时更新 run 两条准备路径各跑一遍，最后跑一次续租语句。
  - 只建 EF 模型不够：冷进程的准备阶段在 CPU 争用下比热路径慢 5–8 倍，会跨过 6 秒租约，按既定语义多记一次 attempt（S1-QA-023）。
  - 预热不造作业，不留持久状态：写入都在回滚的事务里，触发器行的租约放在过去，续租语句命中 0 行。用例往 run 表注入的 INSERT 触发器会在这个事务里跑一次，新写这类触发器时要让它在租约已过期时无害。
  - 复刻的几步在代码里逐条注明对应 `SchedulerWorker` 的哪个方法；产品改了它们，预热要跟着改。漏改不报错，只会让首次执行变冷。
- `WorkerProcess.Start` 等 READY 的上限（60 秒）只管启动，不进时序判据。等不到也要杀掉子进程，否则它就绪后会接管同类后续用例的 run。
- 作业超时这类产品参数要盖过 CPU 争用下的调度抖动。例如超时重试用例用 2 秒超时、处理器延迟 5 秒，不用几百毫秒。
- 接管、续租类用例失败时都调 `CaptureFailure` 留现场：阶段时刻、触发器与 run 行、处理器进入记录，以及各 worker 的退出码与错误输出。
  - 现场里另有 `firstExecutions`：每个 worker 第一次领取到同一代初始续租的耗时，取自夹具在触发器表上装的租约日志（`public.sch_lease_log`）；`initial_renewal_at` 为空，即那一代在准备阶段丢了租约。再偶发时先看它，将这个耗时与用例实际租约的一半比较。
- 本机复现 CPU 争用的办法：在 `mcr.microsoft.com/dotnet/sdk:10.0 --cpus=1` 容器里跑本程序集，另起 8 个空转进程作邻居。

## 4 门禁

### 4.1 PublicAPI（C1 / G4）

- 受约束的是**可打包的项目**：项目里写了 `<IsPackable>true</IsPackable>`，不看包名，仓库根 `src/` 下的 Kun 包同样受约束（S1-KUN-020）。[测试项目的公共属性](Directory.Build.props) 默认不打包，宿主、样例、测试都不受约束。
- 可打包的项目自动接入 `Microsoft.CodeAnalysis.PublicApiAnalyzers`（仓库根的 `Directory.Build.targets`）。项目目录必须有 `PublicAPI.Shipped.txt` 与 `PublicAPI.Unshipped.txt`，首行写 `#nullable enable`，缺了报 KUNAPI001。
- 新增公共 API 没登记报 RS0016，登记了但代码里已删除报 RS0017，都是编译错误。新 API 先登记进 Unshipped，发布时并入 Shipped；破坏性变更要引用 ADR（C1）。IDE 对 RS0016 的代码修复「添加到公共 API」会生成登记行。

一个可打包项目的目录至少有：

```text
<包目录>/
  Kun.Example.Abstractions.csproj    含 <IsPackable>true</IsPackable>
  PublicAPI.Shipped.txt           #nullable enable
  PublicAPI.Unshipped.txt         #nullable enable，之后逐行登记新 API
```

### 4.2 依赖图（C19 / G3）

依赖门禁已移到仓库根（S1-KUN-020）：[scripts/check_deps.py](../scripts/check_deps.py) 检查 `Kun.slnx` 里每个可打包的项目，按项目文件里写的档位（`KunTier`：内核档或平台档）与角色（`KunRole`：契约或提供者）判定，不再按 `Qian.` 前缀与 `.Abstractions` 后缀。规则、例外的写法与本机跑法见仓库根 [AGENTS.md](../AGENTS.md)「档位与依赖」，自测与样例在仓库根的 `tests/gates/`。

- 本目录的项目写了 `<IsPackable>true</IsPackable>`，就要同时写档位与角色，否则构建报 KUNTIER001。
- 原来按名字的几条规则这样对应：组件之间只经契约包引用、不依赖平台插件 `Kun.Registry` / `Kun.Lifecycle` / `Kun.Capabilities`，对应规则 P（平台档包只依赖内核档的包与平台档的契约包）；Definition 的 Kun 直接依赖对应规则 D（契约包直接依赖的内核档包只能是契约）。
- 原来白名单里的四条边：`Kun.Core`、`Kun.Authorization` 记为契约，`Kun.Tenancy.Abstractions → Kun.Core` 与 `Kun.Authorization.Rbac.Abstractions → Kun.Authorization` 不再需要；`Kun.Data.Abstractions` 的两条记成档位例外，写在它的项目文件里。
- 组合包不再整包豁免。现在一个也没有，以后加了同样要写档位。

### 4.3 门禁自测

`tests/gates` 下的样例项目专门证明门禁拦得住违规，CI 每次都跑（`python3 -m unittest discover -s tests/gates -v`）：

| 样例 | 期望 |
|---|---|
| `publicapi/Kun.GateSample.Declared` | 构建通过 |
| `publicapi/Kun.GateSample.Undeclared` | RS0016，构建失败 |
| `publicapi/Kun.GateSample.Stale` | RS0017，构建失败 |
| `publicapi/Kun.GateSample.NoApiFiles` | KUNAPI001，构建失败 |
| `publicapi/GateSample.NotPackable` | 构建通过：不打包的项目不受约束 |
| 仓库根 `tests/gates/test_check_deps.py` 与 `tests/gates/depgraph/` | 依赖门禁的自测（见仓库根 `AGENTS.md`「档位与依赖」）。S1-RT-014（SDK 包族的完整传递依赖里没有平台档的提供者，也没有 `Kun.Registry` / `Kun.Lifecycle` / `Kun.Capabilities`）与 S1-IDP-007（OIDC 连接器不依赖 `Kun.Identity.Server`）两条验收在它的 `RealGraphTests` 里，读真实依赖图 |
| `scripts/tests/test_release_packages.py` | 核对包名单、版本、档位、族内依赖、许可与符号包。 |
| `test_check_docs.py`（不用样例项目，临时目录里搭最小仓库） | 文档门禁对文档站内容生效：站内路由、`/design/` 镜像路由与生成页路由的解析，生成目录跳过，禁用词在 `.mdx` 与站点源码里同样拦截（S1-DOCS-001）；Kun 小节的路由在生成前交给构建时的断链检查，显式给出的生成目录照查，生成目录里的文件进 Git 即失败，禁用词整词匹配（S1-DOCS-005） |
| `test_dotnet_ci.py`（临时目录里搭 git 跟踪的最小仓库） | 触发过滤自检认得 §5.1 列的每类写法：.NET 源码的单个字面量、`Path.Combine` 分段、`../` 相对路径、插值前缀与洞后段、定位了仓库根的文件里的根目录 Markdown，csproj 的 `None` / `Content` / `Update`（含通配），工作流直接运行的脚本、unittest 收集的模块与它们导入的模块；对照样例不拦：注释、夹在变量后的段、不定位仓库根的 `README.md`、临时目录上的路径、被测试换掉仓库根的模块。`verify` 对 `tests/kun-platform.slnf` 外的测试项目按名字告警，门禁样例与专属作业运行的项目除外（S1-QA-021）。匹配式取自仓库根区域表里的 dotnet 区域，只看 dotnet 区域的作业运行的脚本（含在仓库根运行的）；`verify` 另核分片 k，`coverage` 核对每个单元测试项目恰好一份覆盖率（S1-KUN-020）。`images` 列不出镜像即工具出错：夹具目录不在、解析不出缺省镜像、分片里有项目引用了夹具（含经测试宿主项目间接引用）却一个镜像也没解析出来，没有容器测试的分片 0 个镜像不算错；`prefetch` 拉取失败只记录。`registration` 认测试项目（各种测试运行器的包、`IsTestProject`，只引断言库的支撑库不算），专属作业认项目文件与目录、仓库根与本目录两种工作目录、`\|` 块的续行与 `>` 块，别的区域的作业不算；`NOT_IN_CI` 里的路径不在、不是测试项目或又登记在别处即失败（S1-KUN-028） |

## 5 CI

仓库根 [ci.yml](../.github/workflows/ci.yml) 的 dotnet 区域拆成并行的作业（S1-QA-018、S1-KUN-020），构建、测试及分片汇总在仓库根运行。测试分片每个限时 20 分钟（PRD §14.2.1）；构建与门禁要构建整个 `Kun.slnx`，限时 30 分钟：

- 构建与门禁：还原与构建仓库根的 `Kun.slnx`（公共 API 门禁、AOT 守卫与分析器都在构建里）→ 仓库根的几道检查（发版脚本自测、网关打包冒烟、身份边界）→ 入口清单 → 明文密钥扫描 → namespace 骨架 → 仓库根的依赖门禁与它的自测 → 本目录的门禁自测 → 触发过滤自检。
- 单元与集成测试，本目录的项目分三个分片（a、b、c）：各自还原、构建，再用 `scripts/dotnet_ci.py shard` 写出的解决方案筛选文件只跑本分片的模块。作业开头先在前台列出本分片用到的测试镜像（`scripts/dotnet_ci.py images`，列不出即失败），再在后台预拉，测试前有界等待；拉取失败只记录，由 Testcontainers 补拉。分片清单是脚本里的 `SHARDS`，新加的测试项目不用登记，自动落入其余分片。
  - 测试命令带 `-p:KunTrxPerModule=true`，每个模块的 TRX 按项目名命名（`<项目名>.trx`，见 [Directory.Build.props](Directory.Build.props)）。xUnit 缺省文件名只有用户、机器和时间戳，同时启动的模块会撞名，后写完的覆盖先写完的（S1-QA-017）。
- 分片 k 跑仓库根 `tests/` 下的测试：`tests/kun-unit.slnf` 的单元测试收覆盖率，每个模块一份 TRX、一份覆盖率（开关在仓库根的 `tests/Directory.Build.props`），上传前用 `scripts/dotnet_ci.py coverage` 核对每个测试项目恰好一份，通过后文件名去掉时间戳；`tests/Kun.IntegrationTests` 用 Testcontainers 起容器。
- 测试分片汇总核对：`tests/kun-platform.slnf` 里每个测试项目、分片 k 登记的每个测试项目，都恰好在所属分片跑过一次、没有失败。各分片与门禁作业的结果构件名带尝试号（`test-results-<分片>-attempt-<尝试号>`），汇总作业全部下载后用 `scripts/dotnet_ci.py latest-attempts` 给每个分片只留尝试号最大的一份再核对，取好的那份上传为 `test-results` 构件（每个分片一个子目录）。所以碰到偶发失败时点「重跑失败的作业」就行：没重跑的分片用第 1 次的结果，重跑过的用最新一次的，汇总按重跑的结果变绿。不按构件 ID 取最新：同名构件 download-artifact 只留 ID 最大的一份，而重跑后的 ID 不一定更大。本目录下不在 `tests/kun-platform.slnf` 里的测试项目哪个分片都不跑：门禁作业的 `scripts/dotnet_ci.py registration` 即失败（S1-KUN-028），汇总核对也按名字告警；门禁样例 `tests/gates/**` 与 ci.yml 里由 dotnet 区域的专属作业 `dotnet test --project` 运行的项目（目前是三个 `*.AotTests`）除外（S1-QA-021）。仓库根的测试项目由同一条命令核对，口径见仓库根 `AGENTS.md` 的「构建与测试」。
- AOT 冒烟宿主：仓库根的 `tests/Kun.AotSmoke` 引用 AOT 清单里的每一个项目（项目文件里写了 `IsAotCompatible=true` 的，本目录的 8 个包也在里面），对每个程序集整体扎根，原生发布后运行，ILC 有任何警告即失败（S1-KUN-020）。清单与写法见仓库根 `AGENTS.md`「项目文件与包」。本目录的四个原生发布作业照旧。

### 5.1 触发过滤与写法约定

`ci.yml` 的变更检测按仓库根的区域表 [scripts/ci_changes.py](../scripts/ci_changes.py) 决定跑哪些区域。dotnet 区域管本目录的那一组（`q(…)`）不看 `pm/**`、`docs/**` 与本目录根上的 Markdown：只改这些文件的 PR 不跑 dotnet 区域的作业；推 main 时全部区域都跑。作业读到的这类文件在那一组里重新纳入：

| 重新纳入 | 读取方 |
|---|---|
| `docs/generated/**` | `Kun.Tenancy.Tests` 的 `TenancyConfigurationTests` 读 `docs/generated/config-catalog.md`，核对 tenancy 的实际配置与发布的配置目录一致。`Kun.Docs.Tests` 只用样例项目测生成器，不读这个目录；派生文档与源码是否一致（新鲜度）由仓库根的 [qian-documentation.yml](../.github/workflows/qian-documentation.yml) 重新生成后比对，它所在的 documentation 区域含 `docs/generated/**` |
| `docs/ops/configuration-runbook.md` | `Kun.Sample.Crm.Tests` 的 `CapabilityInvokeTests` 原样执行手册里的过期回执清理 SQL |

测试或门禁新读一个这类文件时，把它加进那一组（写相对本目录的路径）。门禁作业的触发过滤自检（`python3 scripts/dotnet_ci.py paths-check`）读这一组，按各组匹配式与全局触发清单，从下面三处核对引用的仓库文件，对得上 git 跟踪的文件、而只改该文件的 PR 不跑 dotnet 区域的作业即失败：

- .NET 源码：git 跟踪的 `*.cs` 里的字符串字面量（注释不算）。
  - 含 `/` 或 `\` 的字面量，去掉开头的 `./`、`../` 后按仓库相对路径算：`"docs/x.md"`、`"../docs/x.md"`、`@"..\docs\x.md"`。
  - `Path.Combine` / `Path.Join` 从第一个实参或紧跟它起的连续字面量段拼成一条路径：`Path.Combine(root, "docs", "prd", "x.md")`。后面还有变量时按目录前缀算，如 `Path.Combine(root, "pm", "tasks", name)` 算 `pm/tasks/` 下的全部文件；夹在变量之后的段相对未知目录，不算。
  - 插值字符串的常量前缀，如 `$"docs/prd/{name}.md"` 算 `docs/prd/` 起头的全部文件；插值洞后以 `/` 起头的段，如 `$"{root}/docs/x.md"`。`+` 拼接同理：`root + "/docs/x.md"`，字面量后面还接 `+` 时按前缀算。
  - 单写一个根目录文件名（`"AGENTS.md"`、`Path.Combine(root, "README.md")`）只在本文件定位了仓库根时算：写到 `"Kun.slnx"`（向上找仓库根），或用到名为 `RepositoryRoot` / `RepoRoot` 的辅助方法。`README.md` 这类常见名字在别处多半指临时目录或包里的文件，如 `Kun.Lifecycle.Tests` 的临时部署仓库。
- MSBuild 项：`*.csproj`、`*.props`、`*.targets` 里 `Include`、`Update`、`Project` 属性指向的文件，相对本文件所在目录解析，认得通配与 `$(MSBuildThisFileDirectory)`、`$(MSBuildProjectDirectory)`，含其他属性的路径跳过。`.props` / `.targets` 里的相对路径实际按导入它的项目目录求值，这里写路径用 `$(MSBuildThisFileDirectory)` 起头，自检才对得上。
- dotnet 区域的作业运行的 Python：`ci.yml` 里这些作业的 `run:` 里 `python3 <脚本>` 直接运行的脚本、`python3 -m unittest discover` 收集的测试模块，以及它们 import、按路径加载（`importlib`）或当脚本运行的本仓库模块，按语法树判断。
  - 以仓库根为锚的路径才算：`Path(__file__)` 推出的路径与模块顶层赋给名字的这类路径（`ROOT = Path(__file__).resolve().parents[1]`、`SCRIPT = ROOT / "scripts" / "x.py"`）、`Path.cwd()`，在锚上用 `/`、`joinpath`、`os.path.join` 拼的常量段（f-string 取常量前缀）；传给 `Path()`、`os.path.join()`、`open()` 的相对路径字符串按仓库根算（作业在仓库根 下运行这些脚本）。
  - 临时目录等别的锚上拼的路径不算。被测试按名字加载、又被该测试换成临时目录的模块级名字，在函数里不算：`test_check_docs.py` 把 `check_docs.ROOT` 换掉，`check_docs.py` 函数里读的 `docs/` 就是临时目录。

写法约定：读取仓库里的文件，写成仓库根（或上面的锚）后跟常量路径段。自检认不出的写法：路径来自变量、配置、命令行参数或文件内容，目录枚举后按名字筛选（如 `Directory.GetFiles(root)`），以及经外部程序间接读取（测试调用的 qian CLI 是 TypeScript，不在扫描范围）。这些情况改用常量路径，或者手工把读到的文件加进区域表的那一组。各类写法的正反例见 [gates/test_dotnet_ci.py](gates/test_dotnet_ci.py)。

本机照 CI 只跑一个分片：`python3 scripts/dotnet_ci.py shard a --out /tmp/a.slnf && dotnet test --solution /tmp/a.slnf -c Release --no-build`；仓库根测试的跑法见仓库根 `AGENTS.md` 的「构建与测试」。CI 在 GitHub 托管的 ubuntu-24.04 上跑（自带 Docker），不接自托管 runner（S1-DEC-038 D21）。

PR 会打包并按 [制品检查脚本](../scripts/release_packages.py) 验证同版本的发布文件。发布流程和渠道见 [SDK 包族说明](../src/Kun.Plugin.Sdk/README.md#包族发布)。


## 旧 Kun API 门禁（S1-KUN-006、S1-KUN-004）

仓库根的 `BannedSymbols.txt` 与 `Microsoft.CodeAnalysis.BannedApiAnalyzers` 5.6.0 对实际 API 使用产生编译错误 RS0030。作用域按档位（S1-KUN-020，仓库根的 `Directory.Build.targets`）：平台档的包与宿主（项目文件里写 `<KunHost>true</KunHost>`）接入；过渡期本目录下的项目全部接入，去内核档的包也在内，各域批次把包搬出本目录时再按归属收窄；仓库根的 Kun 项目不接，被禁的类型就定义在那里。下面几道分析器门禁（KUNLEGACY002–KUNEVENT001）的作用域相同。`tests/gates/bannedapi` 的样例通过项目引用编译真实接口，覆盖受限制的服务身份 API 和可用的存储 API。

标准分析器覆盖符号使用（例如 typeof、调用、构造）；`tools/Kun.Build.Analyzers` 使用同一清单补齐未使用的参数、字段、返回类型等声明，并发出 KUNLEGACY002。负例包含纯参数声明，不依赖上游新包的 Obsolete 标记。

执行 `python3 -m unittest discover -s tests/gates`，同时验证 PublicAPI 与旧 API 的正反例（依赖门禁的自测在仓库根的 `tests/gates/`）。样例按项目引用 Kun 源码，Kun 的改动合入即进入这些正反例。

编译样例引用仓库根的真实项目。已归档的签发器类型产生 CS0234，工厂方法产生 CS1061，透传入口产生 KUNS2S001。网关签发器正常编译。

S1-IDP-005 再禁用 Kun.Security 的令牌透传：`PropagateS2sTokenHandler` 与挂它的 `AddKunServiceAuth`（把入站 Authorization 原样转给下一跳）。S2S 令牌按跳签发、`aud` 只绑一个下游，透传在编译期就拒绝；负例 `LegacyS2sPropagation` 引用该处理器，产生 KUNS2S001。

## 数据安全编译门禁（S1-DATA-001）

`DataBoundaryAnalyzer` 对非测试项目中直接或间接继承 EF `DbContext`、却不继承 `KunStrictDbContext` 的上下文报 KUNDATA001。测试项目可定义验证用的上下文；编译样例显式设置 `IsTestProject=false` 模拟在线代码，不因位于 tests 目录而豁免。

接入分析器的项目里，对 EF `IgnoreQueryFilters` 的调用和方法委托引用报 KUNDATA002，按语义符号识别，其他类的同名方法不误报。`test_data_boundary.py` 验证六组真实 EF 正反例，调用与方法委托引用各用一组负例，负例即使关闭 TreatWarningsAsErrors 也必须失败。真库隔离测试使用受限应用角色的原生 SQL，不调用过滤器旁路。

### 受保护命令注册门禁（S1-AZ-001）

KUNAUTHZ001 在接入分析器的项目里检查真正的 Kun `AddRequestHandler`：受保护请求或未确定请求类型的泛型封装必须改用 `Kun.Authorization.Rbac.Abstractions.AddAuthorizedHandler`，同一入口将处理器与权限行为登记到同一个 builder。两个重载、方法组引用、生成代码均受检查；关闭警告即错误不会降低规则。

`python3 -m unittest discover -s tests/gates -p test_authorized_handler.py` 编译八组真实样例：直接注册、能力注册、委托引用、泛型包装、应用伪造桥接类必须失败；安全入口、普通请求及无关同名方法必须通过。运行时拒绝/放行顺序由 Kun.Authorization.Rbac.Abstractions.Tests 验证。

### 集成事件发布位置门禁（S1-EVT-001）

KUNEVENT001 落实「集成事件只能在命令 handler 同 scope 发出」：对 Kun `IIntegrationEventBus.Publish` 与 `IOutboxWriter.Enqueue` 的调用和方法组引用，只允许出现在写命令 handler（处理 Kun `ICommand`，或处理 `[Capability]` 且 SideEffect 不为 read 的请求）与领域事件通知 handler 内；直接向 Kun 缓冲器追加消息（`IntegrationEventBuffer.Add`、`IIntegrationEventOutbox.Add`）会绕过 v1 信封，只允许出现在 Outbox 写入端（实现 `IOutboxWriter` 或 `IIntegrationEventBus` 的类型）里。测试项目（引用了 xUnit 的编译）不受检查。运行时由 SDK 命令管道开启的事件来源作用域兜底，作用域外发布被拒绝。

`python3 -m unittest discover -s tests/gates -p test_event_publish_scope.py` 编译 `tests/gates/events` 下八组真实样例：命令 handler、领域事件 handler、写能力和无关同名方法必须通过；查询 handler、只读能力、后台服务和直接写缓冲器必须以 KUNEVENT001 失败。

### 能力契约的真实兼容检查

运行 `Kun.Capabilities.Tests` 和客户目录契约挂载用例前，先执行 `python3 scripts/prepare_contract_tools.py --destination /tmp/qian-contract-tools`，再设置 `QIAN_OASDIFF_PATH=/tmp/qian-contract-tools/oasdiff`（Windows 使用对应 exe）。下载版本与 SHA-256 固定在 `scripts/contract-tools.lock.json`，不会运行远程安装脚本。全部 .NET 测试也需要此环境变量；工具缺失按失败处理。CI 已包含安装步骤。

proto 与反射用例还需设置 `QIAN_BUF_PATH=/tmp/qian-contract-tools/buf`（Windows 使用 buf.exe）；同一安装脚本会下载并核验该二进制。运行全部 .NET 测试时同时提供 QIAN_OASDIFF_PATH 与 QIAN_BUF_PATH。
