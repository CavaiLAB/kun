# 贡献指南

> 本指南写的是仓库根上的 Kun 框架。开发环境、分层原则、AOT 约定与提交规范照旧适用；分支策略和 Pull Request 流程两节保留此前的做法，在本仓库以根 [AGENTS.md](AGENTS.md) 与[迁移执行计划](docs/migration-plan.md)为准：PR 一律用合并提交合入，合并由产品负责人发话；发布规范见下文的发布工作流。

感谢您对 **Kun Framework** 的关注！

## 📋 行为准则

本项目采用 [Contributor Covenant](https://www.contributor-covenant.org/) 行为准则。请保持专业、尊重与建设性。

## 🛠️ 开发环境

### 必需工具

- **.NET 10 SDK** (`10.0.100` 或更高) —— 版本在 `global.json` 中锁定
- **Docker** —— 运行集成测试（Testcontainers）
- **Git**

### 推荐工具

- **Visual Studio 2026** / **JetBrains Rider 2026.x** / **VS Code + C# Dev Kit**
- **编码代理**（Claude Code、Codex 等）—— 先读根 [AGENTS.md](AGENTS.md)，Kun 框架的构建、测试与代码约定在其中一节

### 初始构建

```bash
dotnet restore
dotnet build
dotnet test
```

## 🏗️ 分层原则（非常重要）

Kun Framework 严格遵守分层依赖，**任何 PR 违反这些规则都会被拒绝**：

```
Kun.Core
  └─ Kun.Mediator / Kun.EventBus / Kun.Security / Kun.Validation
       └─ Kun.EntityFrameworkCore / Kun.EventBus.RabbitMQ
            └─ Kun.AspNetCore / Kun.Grpc / Kun.Gateway
```

- **`Kun.Core` 零依赖**：不引用任何第三方包（连 EF Core、Dapper 都不行）
- **领域层零基础设施**：`AggregateRoot` 等基类绝不携带 ORM / JSON / 验证框架的任何属性
- **持久化映射下沉**：`IEntityTypeConfiguration<T>` 位于 `Kun.EntityFrameworkCore` 或业务项目的 Infrastructure 层

## 🎯 AOT 兼容性约定

AOT 兼容的模块在 csproj 里写 `<IsAotCompatible>true</IsAotCompatible>`，不兼容的不写（做法见根 [AGENTS.md](AGENTS.md)「项目文件与包」）。**AOT 兼容的模块中**：

- ❌ 禁止使用运行时反射（`Activator.CreateInstance`、`Type.GetMethod`、`MakeGenericType` 等）
- ❌ 禁止在运行时生成 IL（`System.Reflection.Emit`）
- ✅ 必须使用 `JsonSerializerContext` 源生成而非默认 `JsonSerializer.Serialize`
- ❌ 不引用 Dapper：它的经典 API 靠运行期反射生成读取代码
- ✅ 普通构建就开着裁剪与 AOT 分析器；CI 再原生发布并运行 AOT 冒烟宿主 `tests/Kun.AotSmoke`（引用全部 AOT 兼容的模块并整体扎根），ILC 零警告才允许合入

## 🤖 编码代理

`.claude/skills/` 下的技能包已按合并蓝图删除（D23.13 第 6 点）。编码代理（Claude Code 与 Codex 共用）改照根 [AGENTS.md](AGENTS.md) 的「Kun 框架的构建、测试与代码约定」一节做；提交 PR 前照那一节自查分层、AOT、命名与测试。

## 📝 提交规范

本仓库使用 [Conventional Commits](https://www.conventionalcommits.org/)：

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

**Type** 取值：

- `feat`: 新功能
- `fix`: Bug 修复
- `docs`: 文档变更
- `refactor`: 重构（非功能变更）
- `perf`: 性能优化
- `test`: 测试相关
- `build`: 构建系统 / 依赖变更
- `ci`: CI 配置变更
- `chore`: 其他杂项

**Scope** 通常是模块名去掉 `Kun.` 前缀：`mediator`、`eventbus`、`security`、`gateway` 等。

**示例**：

```
feat(eventbus): retry failed outbox messages on a backoff schedule

Adds NextAttemptAt to OutboxMessage so the publisher skips a failed row
until its backoff delay has passed instead of retrying it every poll.

Closes #42
```

## 🔀 分支策略（Hard rule）

> 这一节记的是原仓库的做法。本仓库以根 AGENTS.md 为准：一批一个 PR，PR 一律用合并提交合入（下表的 Squash / rebase merge 在本仓库不用），合并由产品负责人发话。

**`main` 不是开发分支。任何人、任何时候、任何理由都不在 `main` 上直接 `commit` 或 `push`**——包括小范围文档修改、紧急修复、"就一条 commit" 都不行。

> **2026-06-11 起**：原仓库与下游同步采用 **trunk-based** 模型，原 `develop` 分支已删除，git-flow 作废。`main` 是唯一长期分支。

所有改动走短命分支 + PR：

| 角色 | 动作 |
|------|------|
| 开发者 | 从 `main` 切出 `feat/xxx` / `fix/xxx` / `docs/xxx` / `chore/xxx` 短命分支 |
| 开发者 | 在特性分支上提交、push |
| 开发者 | 向 **`main`** 提 PR，链接相关 Issue，填写 PR 模板 |
| CI | `build` / `test` / `dotnet format` / AOT publish / layering 全部绿 |
| 维护者 | 至少一位 Approve |
| 维护者 | Squash / rebase merge 到 `main`，随手删除短命分支 |
| 维护者（发版时） | 在 `main` 上运行发布工作流，见下节 |

**为什么**：下游服务及其 CI 经 `ProjectReference` 直接消费 `main`，`main` 必须始终可构建。该策略同时是将来要开启的 GitHub 分支保护（PR-required + status-checks-required）的编码契约：**规则先于工具落地**，工具开启前也不得违反。

**AI Agent 特别提醒**：会话启动前先跑 `git branch --show-current`；如果是 `main`，第一步必须是 `git pull && git checkout -b <type>/<task>`；从未 commit 过主分支。

## 🏷️ 发布与版本规范

在 GitHub Actions 的 `release` 工作流从 `main` 选择版本。默认的 `dry_run=true` 校验该提交的主干 CI、全量包与符号包、发布版本的 HTTP 契约及独立消费者，并保存制品 30 天。本地可用 `bash scripts/release.sh <版本>` 做构建和测试预检。

首个合并版包含 42 个内核包和 43 个平台包。工作流在打包前核对两个档位的数量。以后新增、停发或调整包的档位时，在同一个 PR 更新发布工作流的 `list --expect`。

开始实际发布时填写演练的 `resume_run_id` 和相同版本，选 `dry_run=false`。发布环境 `release` 只允许主干并要求审批。nuget.org 的可信发布策略使用此仓库、`release.yml` 与 `release` 环境，`NUGET_USER` 填已获包发布权的用户名。正式版和同日修复版先发布公开的 GitHub Packages 平台包，再使用 OIDC 发布 nuget.org 内核包。beta 的全部包发往 GitHub Packages。GitHub Packages 中的包均关联本仓库。

首个稳定版既含已有包的新版本，也含首次注册的内核包 ID。在 nuget.org 建立可信发布策略时，选择有权发布已有内核包的所有者，启用发布新包和发布现有包新版本两种权限。包名匹配范围须覆盖该版本 `release-packages.json` 中的内核档 ID。策略绑定 GitHub 的 `CavaiLAB/kun`、工作流文件 `release.yml` 和环境 `release`，分支限制填写 `main`。[NuGet 可信发布的作用域与归属说明](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing#select-scopes)

组织首次推送的 NuGet 包默认为私有。首次推送后，包管理员打开各包的 **Package settings**，核对已关联 `CavaiLAB/kun`，在 **Change visibility** 选择 **Public** 并确认包名。GitHub 的公开设置不可撤销。若工作流在核验可见性时停止，完成这些设置后，用同一份演练制品的 `resume_run_id` 重新运行。已推送的包会逐项核对内容。[GitHub Packages 的组织包可见性说明](https://docs.github.com/en/packages/learn-github-packages/configuring-a-packages-access-control-and-visibility#configuring-visibility-of-packages-for-an-organization)

补推前，工作流先从本版本涉及的全部包源下载已经可见的同版本包，逐个比对已保存制品的内容。任何一个包出现差异都会终止本次推送。包源核对与干净消费者验收通过后，工作流创建标签和 Release。正式版的运行摘要给出版本日志分支的比较链接，建立 PR 并通过 CI 合入，文档站随后从标签和已发布的内核包生成参考页。若推送途中失败，使用首次保存制品的运行号恢复。

正式版与修复版发布后，另开一个 PR 归档公共 API 清单，以发布标签为界：标签上各可打包项目 `PublicAPI.Unshipped.txt` 的条目移进同目录的 `PublicAPI.Shipped.txt`，按序数排序，首行仍是 `#nullable enable`；其中的 `*REMOVED*` 行连同 `PublicAPI.Shipped.txt` 里对应的条目一起去掉；标签之后才加的条目留在 `PublicAPI.Unshipped.txt`。此后删改已发布的 API，在 `PublicAPI.Unshipped.txt` 里写 `*REMOVED*` 行，不直接删 `PublicAPI.Shipped.txt` 的条目。

**使用 CalVer 日期，tag 一律在 `main` 上打；同日补丁使用 NuGet 兼容的 `hotfix.N` 后缀**。CLI 与 CI 共用 `scripts/release_version.py` 校验（需要 Python 3.9+，见 `.github/workflows/release.yml`）：

| Tag 形式 | 语义 | 示例 |
| :--- | :--- | :--- |
| `vYYYY.M.D` | 正式版本 | `v2026.6.12` |
| `vYYYY.M.D-hotfix.N` | 同日正式修复 / 旧版本热修 | `v2026.6.12-hotfix.2` |
| `vYYYY.M.D-beta.N` | 预发布（GitHub Release 标 prerelease） | `v2026.6.12-beta.1` |

规则：

- 月、日**不补零**，日期必须有效，后缀序号为无前导零的正整数。
- 所有 tag 必须指向 `main` 上可达的 commit；其它形式（含旧的 `vX.Y.Z`、`-alpha`、`-rc`）一律被 Release workflow 拒绝。
- 旧版本紧急修复：在独立分支完成修复与 CI，经 PR 合入 `main` 后，从已验证提交打补丁 tag，随后清理分支。
- **NuGet 排序注意**：`-hotfix.N` 是正式修复版，内核与平台沿用正式版各自的包源。按 NuGet 版本优先级，它低于同日无后缀版本。NuGet 消费端必须显式钉定修复版，不能依赖最新版排序；日期递进的版本继续使用无后缀格式。
- **修复版之后补发正式版**：发布 `-hotfix.N` 之后，下一个工作日补发无后缀正式版。若期间没有其他业务变更，在 `CHANGELOG.md` 的 `[Unreleased]` 中写明此次正式版纳入的修复及其版本号，再运行发布演练。钉在修复版上的消费端可升级到该无后缀版本。首例：`2026.10.12-hotfix.3` 之后补发 `2026.10.13`。
- `v2026.10.12-2` 的纯数字后缀已被 nuget.org 包清单校验拒绝。该失败标签保留历史，禁止重写或继续发布同类标签。字母加点分段后缀遵循 [NuGet 版本规则](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning#semantic-versioning-200)。
- 推送前校验全部发行包和符号包的实际版本、Kun 内部依赖版本及发行物闭包；缺包或混入其它版本即停止。beta 仅推送 GitHub Packages，GitHub Release 标记为 prerelease。
- 历史 SemVer tag（`v0.1.0` 等）保留不动，仅作存档；2026-06-11 前的「`main`→稳定 / `develop`→`-beta`」策略作废。

## 🔀 Pull Request 流程

> 这一节记的是原仓库的做法。在本仓库：PR 一律用合并提交合入，合并由产品负责人发话（根 AGENTS.md）。

1. **Fork 仓库**（或用内部分支），从最新 `main` 切出特性分支：`feat/xxx` / `fix/xxx` / `docs/xxx` / `chore/xxx` — **绝不直接在 `main` 上提交**
2. **编写代码 + 测试**。任何新功能必须附带测试。
3. **本地验证**：
   ```bash
   dotnet build -c Release
   dotnet test
   # AOT 服务额外验证
   dotnet publish hosts/Kun.Gateway.Standalone -c Release
   ```
4. **提交 PR**：填写 PR 模板，链接相关 Issue，说明影响范围与测试情况
5. **CI 全绿**：构建、测试、AOT 发布零警告、格式检查通过
6. **代码审查**：至少一位维护者 Approve 后方可合并

## 🧪 测试要求

参见根 [AGENTS.md 的「Kun 框架的构建、测试与代码约定」](AGENTS.md#kun-框架的构建测试与代码约定)。简而言之：

- **领域变更** → 领域单元测试（纯内存、无容器）
- **跨层变更**（仓储、Outbox）→ Testcontainers 集成测试
- **gRPC 服务变更** → 契约测试验证 Proto 不破坏兼容性

## 🐛 报告问题

提交 Issue 时请包含：

- 使用的 `.NET` 版本与 `Kun.*` NuGet 版本
- 最小可复现仓库 / 代码片段
- 预期行为与实际行为
- 如涉及运行时错误：完整堆栈与日志

## 📄 许可证

通过提交 PR，您同意将您的贡献以 [MIT 许可证](LICENSE) 发布。
