# 贡献指南

> 本指南写的是仓库根上的 Kun 框架。开发环境、分层原则、AOT 约定与提交规范照旧适用；分支策略、发布与版本规范、Pull Request 流程三节记的是原仓库的做法，在本仓库以根 [AGENTS.md](AGENTS.md) 与[迁移执行计划](docs/migration-plan.md)为准：PR 一律用合并提交合入，合并由产品负责人发话；发版工作流接不接、怎么接在 B12 之前由产品负责人定。

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
| 维护者（发版时） | 在 `main` 上打 CalVer tag（见下节），tag push 触发 Release workflow |

**为什么**：下游服务及其 CI 经 `ProjectReference` 直接消费 `main`，`main` 必须始终可构建。该策略同时是将来要开启的 GitHub 分支保护（PR-required + status-checks-required）的编码契约：**规则先于工具落地**，工具开启前也不得违反。

**AI Agent 特别提醒**：会话启动前先跑 `git branch --show-current`；如果是 `main`，第一步必须是 `git pull && git checkout -b <type>/<task>`；从未 commit 过主分支。

## 🏷️ 发布与版本规范

> 这一节记的是原仓库的发版做法。本仓库的发版工作流还没接，接不接、怎么接在 B12 之前由产品负责人定（迁移执行计划第 4、8 节）。

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
- **NuGet 排序注意**：`-hotfix.N` 在项目语义中是正式修复版、推送 nuget.org，但 NuGet 按带后缀版本排序，低于同日无后缀版本。NuGet 消费端必须显式钉定修复版，不能依赖最新版排序；日期递进的版本继续使用无后缀格式。
- **修复版之后补发正式版**：发布 `-hotfix.N` 之后，下一个工作日补发无后缀正式版，即使没有其他变更，让 nuget.org 的最新稳定版包含该修复；钉在修复版上的消费端在正式版发布后升级回无后缀版本。首例：`2026.10.12-hotfix.3` 之后补发 `2026.10.13`。
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
