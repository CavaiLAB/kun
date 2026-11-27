# Kun 的包、文档、示例与站点导航的去向

状态：讲现状的清单，2026-11-27 起草，随整理改写。去向里标「提议」的是整理时的提议，没有问过产品负责人；动它会改变对外承诺的几项列在第 10 节，等他定。登记在 [S1-DEC-051](../pm/tasks/S1-DEC-051.md)。

产品负责人 2026-11-27 的新方向里有这几句：「Kun 保持现有基础框架和接口，继续负责认证、会话和权限。登录、工作区和管理端等产品页面统一放到 jiangxin 重新设计，旧布局和样式可以淘汰。」「请一并整理 Kun 的 packages、已发布的包、docs、示例和站点导航，更新或归档过时的 Qian 内容，保留仍在使用的命令、包名和组件 ID 的兼容性。」「先列清各个包的去向，修订文档，再用登录页跑通前后端流程。」本文做第一步：把本仓库里每一个前端包、外壳、示例、已发布的包、文档、站点导航项与相关的工作流逐项列清；合并以前在别的包源位置发过的一批旧包不逐个列，见 5.3 节。后两步要做的活建成了任务，见第 11 节。

## 1 怎么读

每一项写：它是什么；现在谁在用；去向；仍在使用、要保持兼容的名字；去向的依据。

「谁在用」是对着 2026-11-27 的主干用 `git grep` 核的本仓库里的引用（不算 `pnpm-lock.yaml`、任务文件、决策记录、需求文档与 `CHANGELOG.md` 里的提及），括号里是那个目录里引用它的文件数。对着本机检出的匠心主干核过：匠心没有依赖本仓库的前端包，也不装 `@cavailab/qian-*`，这个名字只在几处讲「从哪里迁来」的说明与注释里出现。

去向只用五种：

| 去向 | 意思 |
|---|---|
| 留在 Kun 照旧维护 | 照旧改、照旧发、照旧进检查 |
| 留在 Kun 冻结 | 代码或文字留在仓库里，不删，不再改；要不要进镜像、进检查，写在那一行 |
| 归档 | 标成已归档，留着可查；不再出现在给人看的主路径上 |
| 页面去匠心重新设计 | 产品页面在匠心重做，本仓库只出接口；本仓库现在这一版怎么办写在那一行 |
| 文字更新 | 内容还要，过时的说法照现行裁定改写 |

依据的写法：

| 写法 | 指的是 |
|---|---|
| 一表 29、一表 30 | 第一张《决策表》第 29、30 条，产品负责人 2026-11-27 答：其余逐条采用原表建议（[S1-DEC-049](../pm/tasks/S1-DEC-049.md)） |
| 二表 2、二表 3、二表 5 | 第二张表第 2、3、5 条，产品负责人 2026-11-27 答：都按建议（[S1-DEC-050](../pm/tasks/S1-DEC-050.md)） |
| 新方向 | 产品负责人 2026-11-27 发来的新方向（[S1-DEC-051](../pm/tasks/S1-DEC-051.md)） |
| 归档 D1 | [S1-DEC-043](../pm/tasks/S1-DEC-043.md) D1：前端框架 Qian 的仓库归档，已发布的包留着、能装，不再发新版 |
| 字面保留 | 组件 id 前缀 `qian.`、清单文件名这些运行期字面当作历史字面保留，`qian` 命令行与 `@qian/*` 导入名不改（`AGENTS.md`「要改文档」第 1 条；ADR-023 D23.18；第一张表成批的丙批里「`qian.` 开头的运行期字面量不改」，按提议推进） |
| 丙批删 | 第一张表成批的丙批里「Kun 那六份同类文档照同一口径删掉」，按提议推进，没有做（S1-DEC-049「后续」第 5 项） |
| 站 D2 | [S1-DEC-047](../pm/tasks/S1-DEC-047.md) D2：前端框架的组件页从站里拿掉 |
| 提议、读法 | 整理时的提议与读法，产品负责人没有逐句确认 |

## 2 前端包、外壳与工作区里的前端项目

pnpm 工作区（`pnpm-workspace.yaml`）收的是下面前 11 项。都标了 `private`，一个也没有发布到 npm。

| 项 | 是什么 | 谁在用 | 去向 | 要保持兼容的名字 | 依据 |
|---|---|---|---|---|---|
| `packages/qian-cli`（包名 `qian`） | 组件的命令行：`init`、`add`、`list`、`remove`、`doctor`、`docs generate` | 工作流 `qian-documentation.yml`、`qian-surfaces.yml`；.NET 测试经它装组件（`tests/Kun.Sample.Crm.Tests`（4）等）；`samples/docs`；根 `package.json` 的开发依赖；连同 `src/` 下几个组件的说明共 48 个文件 | 留在 Kun 照旧维护 | 命令名 `qian` 与六个子命令；`.qian-plugin/plugin.json`、`qian.profile.yml`、`qian.patch.yml`、`.qian/marketplace.json` | 新方向（保留仍在使用的命令）；字面保留；留在本仓库是 `docs/architecture.md` §10 的提议 |
| `packages/qian-surfaces`（`@qian/surfaces`） | 声明式表面的校验库（TypeScript），与 .NET 一侧对同一套样例 | `packages/qian-cli`（24）、`src/Common`（3）、`tests/Kun.Common.Tests`（2）、`tck/surfaces`、`tck/profiles`、工作流 `qian-surfaces.yml` | 留在 Kun 照旧维护 | 导入名 `@qian/surfaces` | 新方向；字面保留；留在本仓库是提议 |
| `packages/qian-identity-ui`（`@qian/identity-ui`） | 认证页：登录、选择租户、强制改密、连接器落点；构建产物由身份服务在 `/login` 下同源托管 | 平台宿主经 `UseKunIdentityPages` 托管（`src/Kun.Identity.Server`（2）、`hosts/Kun.Platform.Host`）；平台镜像的 `/app/web/identity/`（`deploy/images/README.md`）；`packages/qian-shell-kit`、`apps/qian-shell` 的登录验收；`tools/Kun.Dev.Shell`（2）；工作流 `qian-ui.yml`、`qian-shell-auth.yml` | 页面去匠心重新设计。本仓库这一版冻结，匠心的新登录页跑通以前照旧是缺省，跑通以后退不退到时报产品负责人 | 认证页的路径 `/login`、`/login/select-tenant`（身份服务的 `Accounts.SelectionPath` 要在 `/login` 下）；导入名 `@qian/identity-ui` | 新方向；二表 3；「跑通以前照旧是缺省」是读法 |
| `packages/qian-shell-kit`（`@qian/shell-kit`） | 外壳专有接线：运行时配置、网关会话、bootstrap、防伪、能力导航，以及认证页用的同源 AuthProvider | `apps/qian-shell`（7）、`packages/qian-identity-ui`（4）、`tools/Kun.Dev.Shell`（2）、工作流 `qian-shell-auth.yml` | 留在 Kun 冻结。认证页还依赖它，随认证页留到新登录页跑通 | 导入名 `@qian/shell-kit` | 二表 5；新方向；随认证页留着是读法 |
| `packages/qian-tenancy-ui`（`@qian/tenancy-ui`） | 平台控制台里的租户运营页面，以远程模块挂进外壳 | `apps/qian-shell`（5）、`packages/qian-tenancy-signup-ui`（1）、`tools/Kun.Dev.Shell`（2）、工作流 `qian-tenant-ui.yml`、`qian-ui.yml`、`qian-shell-auth.yml` | 页面去匠心重新设计（匠心的管理端）。本仓库的代码冻结，不再进镜像和发布链 | 组件 ID `qian.tenancy` | 一表 29；二表 5 |
| `packages/qian-tenancy-signup-ui`（`@qian/tenancy-signup-ui`） | 注册页（申请人公开页 `/signup`、`/signup/status`），加平台运营的开通申请页（远程模块） | 平台镜像的 `/app/web/signup/`；`apps/qian-shell` 的注册主线验收；`tools/Kun.Dev.Shell`（2）；工作流 `qian-ui.yml`、`qian-shell-auth.yml` | 页面去匠心重新设计。注册页这一版冻结、新页面跑通以前照旧用；开通申请页随平台控制台不再进镜像 | 申请人接口的路径 `/api/signup/*`（契约在 `plugins/qian.tenancy.signup/contracts/`） | 新方向；一表 29；二表 5；注册页归进「产品页面」是读法 |
| `apps/qian-shell`（`qian-shell`） | 旧外壳：远程模块的宿主、网关会话、bootstrap、能力导航、租户切换 | 网关镜像的 `/app/web/shell/`（`deploy/images/README.md`）；网关宿主给了 `Shell:Root` 时托管它；`tools/Kun.Dev.Shell`（3）、`tests/Kun.Shell.TestHost`；`website/e2e` 借用它的一个测试辅助文件；工作流 `qian-shell-auth.yml`、`qian-shell-federation.yml`、`qian-ui.yml` | 留在 Kun 冻结；不再进镜像和发布链；只为它跑的检查停掉（[S1-QA-033](../pm/tasks/S1-QA-033.md)） | — | 一表 29；二表 5 |
| `samples/qian-sample-crm-ui`（`@qian/sample-crm-ui`） | 参考应用 `qian.sample.crm` 的远程模块：客户列表与新建客户两页，挂在旧外壳里 | `apps/qian-shell`（2）、`samples/Kun.Sample.Crm`（1）、`tests/Kun.Sample.Crm.Tests`（1）、`tools/Kun.Dev.Shell`（3）、`tools/Kun.Dev.TenantOps`（1）、工作流 `qian-ui.yml`、`qian-shell-auth.yml` | 留在 Kun 冻结。参考应用的界面以后在匠心那一侧做演示 | 组件 ID `qian.sample.crm` | 二表 5（挂在旧外壳里）；匠心做演示是提议 |
| `tools/openapi-codegen`（`@qian/openapi-codegen`） | 从 OpenAPI 契约生成 TypeScript 客户端 | 根 `package.json` 的 `openapi:generate`；`@qian/tenancy-ui`、`@qian/tenancy-signup-ui` 的 `contracts:generate`；工作流 `qian-ui.yml`、`qian-tenant-ui.yml`、`qian-directory-e2e.yml` 的区域 | 留在 Kun 照旧维护。匠心照本仓库的契约生成客户端时可以用 | — | 提议 |
| `tests/directory-e2e`（`@qian/directory-e2e`） | 目录选择器的浏览器验收：用前端框架业务包的组织与人员选择器，经真实身份宿主的目录接口 | 工作流 `qian-directory-e2e.yml`、`qian-ui.yml`；`tests/README.md` | 留在 Kun 冻结；工作流停掉（S1-QA-033） | — | 二表 5；归到「成员一类页面的检查」是读法 |
| `website`（`website`） | Kun 网站（Rspress），内容见第 7 节 | 工作流 `qian-docs-site.yml`；`deploy/docs-site/` | 留在 Kun 照旧维护；页面照第 7 节更新 | 站内的旧地址 `/components/qian/*` 落到说明页 | S1-DEC-047；新方向 |
| 根 `package.json`（`qian-workspace`）的前端脚本 | `ui:*`、`shell:federation`、`shell:auth`、`docs:site`、`openapi:generate` | 工作流 `qian-ui.yml`、`qian-shell-auth.yml`、`qian-shell-federation.yml` | 文字更新：`ui:*` 收窄到认证页与注册页，`shell:federation` 随工作流停掉（S1-QA-033） | 脚本名 | 二表 5 |
| `pnpm-workspace.yaml` 的版本目录 | 前端依赖的版本只钉一次，10 个 `@cavailab/qian-*` 钉在 `0.2.1` | 上面各前端项目的 `package.json` 写 `catalog:` | 留在 Kun 照旧维护。旧外壳与界面包退出检查以后，版本目录里的 `@cavailab/qian-*` 只剩认证页、注册页与网站用到的几个 | `@cavailab/qian-*` 的包名与 `0.2.1` | 归档 D1；收窄是提议 |
| `eslint.config.mjs` | 前端项目的 lint 规则 | 根 `package.json` 的 `ui:lint` | 文字更新：随 S1-QA-033 收窄范围 | — | 二表 5 |
| `eslint.boundaries.config.mjs` | 前端分层的依赖边界 | `ui:boundaries` | 文字更新：同上 | — | 二表 5 |
| `eslint.i18n.mjs` | 界面文字的国际化检查 | `eslint.config.mjs` | 文字更新：同上 | — | 二表 5 |
| `.dependency-cruiser.cjs` | 前端循环依赖检查 | `ui:circular` | 文字更新：同上 | — | 二表 5 |
| `vitest.config.ts` | 前端单测与门禁的测试配置 | `ui:test` | 文字更新：同上 | — | 二表 5 |
| `turbo.json` | 前端构建与检查的任务编排 | `ui:typecheck`、`ui:build`、工作流 `qian-shell-auth.yml`、`qian-tenant-ui.yml` | 文字更新：同上 | — | 二表 5 |
| `tsconfig.base.json` | 前端项目共用的 TypeScript 设置 | 各前端项目的 `tsconfig.json` | 留在 Kun 照旧维护 | — | 提议 |
| `scripts/ui-tokens.mjs` | 界面包只用设计令牌的检查 | `ui:lint` | 文字更新：随 S1-QA-033 收窄范围 | — | 二表 5 |
| `tests/gates/ui/` | 前端门禁的自测与版本钉住的检查 | `ui:typecheck`、工作流 `qian-ui.yml`、`qian-directory-e2e.yml` | 文字更新：随 S1-QA-033 收窄范围 | — | 二表 5 |

## 3 跟页面绑在一起的 .NET 部分

| 项 | 是什么 | 谁在用 | 去向 | 要保持兼容的名字 | 依据 |
|---|---|---|---|---|---|
| `src/Kun.Gateway.Shell`（NuGet 包 `Kun.Gateway.Shell`） | 网关托管外壳的静态文件、`/app-config.json` 与 bootstrap 聚合 | `hosts/Kun.Gateway.Host`（3）、`plugins/qian.gateway`（2）、`src/Kun.Gateway`、`tests/Kun.Gateway.IntegrationTests`（3）、`tests/Kun.Sample.Crm.Tests`（5）、`tests/Kun.AotSmoke`、`tools/Kun.Dev.Shell`（4） | 代码留在 Kun 冻结。这个包已经发布过，停不停发新版要产品负责人定（第 10 节） | 包名 `Kun.Gateway.Shell` | 二表 5；要定 |
| `src/Kun.Identity.Server` 里的认证页托管（`IdentityPagesHosting`、`UseKunIdentityPages`） | 在 `/login` 下同源托管认证页的构建产物，带缓存与安全头 | `hosts/Kun.Platform.Host/PlatformIdentity.cs` | 留在 Kun 照旧维护。页面改由匠心提供以后怎么接，由 [S1-IDP-032](../pm/tasks/S1-IDP-032.md) 先出设计 | `/login` | 新方向（Kun 继续负责认证与会话）；S1-IDP-032 |
| `src/Kun.Capabilities.AspNetCore` 的 `GET /api/capabilities/shell`、`/navigation`、`/console` | 给外壳与平台控制台的目录与导航接口 | `packages/qian-shell-kit`（`navigation.ts`）、`apps/qian-shell` 的验收、`samples/Kun.Sample.Crm/PlatformSurfaceHost.cs` | 留在 Kun 照旧维护（接口）。匠心的管理端与工作区要不要照它取导航，随匠心的设计 | 路径 | 新方向（Kun 只出接口）；提议 |
| `plugins/qian.shell`（组件 ID `qian.shell`） | 外壳组件的清单 | shell 组合包；`scripts/ci_changes.py` 的 `shell-federation` 区域 | 留在 Kun 冻结 | `qian.shell` | 二表 5；字面保留 |
| `plugins/qian.bundle.shell`（组件 ID `qian.bundle.shell`） | shell 组合包：把外壳的静态制品交给网关 | `profiles/platform/qian.profile.yml`、`docs/generated`、`tests/Kun.Sample.Crm.Tests` | 留在 Kun 冻结。要不要从 platform profile 的缺省装配里拿掉，S1-QA-033 开工时核对影响再定 | `qian.bundle.shell` | 二表 5 |
| `hosts/Kun.Gateway.Host` 的外壳托管 | 给了 `Shell:Root` 才托管外壳，不给就不托管 | 网关镜像 | 留在 Kun 照旧维护（宿主）；镜像不再送外壳的制品（S1-QA-033） | 配置键 `Shell:Root` | 二表 5 |
| `tools/Kun.Dev.Shell` 与 `scripts/dev-shell.sh` | 本机起外壳、身份服务与参考应用的手测与验收宿主 | `apps/qian-shell` 的登录验收（`pnpm shell:auth`）、工作流 `qian-shell-auth.yml` 的区域 | 留在 Kun 冻结。旧登录页的验收还用它，留到新登录页跑通 | — | 二表 5 |
| `tests/Kun.Shell.TestHost` | 外壳联邦验收的测试宿主 | 工作流 `qian-shell-federation.yml` | 留在 Kun 冻结；工作流停掉（S1-QA-033） | — | 二表 5 |
| `tools/Kun.Dev.TenantOps` 与 `scripts/dev-tenant-ops.sh` | 本机的租户运营宿主，用开发令牌调运营接口，不起页面 | 工作流 `qian-dev-tenant-ops.yml`、`qian-tenant-ui.yml`、`qian-directory-e2e.yml` | 留在 Kun 照旧维护（是后端） | — | 二表 5（后端照做） |

## 4 示例与样板

| 项 | 是什么 | 谁在用 | 去向 | 要保持兼容的名字 | 依据 |
|---|---|---|---|---|---|
| `samples/Kun.Sample.AppHost` | Aspire 编排入口，拉起目录与订单两个示例服务 | `Kun.slnx`、`docs/getting-started.md`、`docs/guides`（2）、`samples/README.md` | 留在 Kun 照旧维护 | — | 提议 |
| `samples/Kun.Sample.Catalog` | 商品目录服务：领域模型、命令查询分离、Outbox、gRPC | `samples/Kun.Sample.Order`（3）、`samples/Kun.Sample.Tests`（2）、`tests/Kun.IntegrationTests`（3）、`docs/guides`（3） | 留在 Kun 照旧维护 | — | 提议 |
| `samples/Kun.Sample.Order` | 订单服务：下单、经 gRPC 预留库存、经 Outbox 发集成事件 | `samples/Kun.Sample.AppHost`、`samples/Kun.Sample.Tests`（2）、`docs/guides` | 留在 Kun 照旧维护 | — | 提议 |
| `samples/Kun.Sample.Shared` | 两个示例服务共用的类型 | `samples/Kun.Sample.Order`（2）、`samples/Kun.Sample.Tests`、`docs/adr`、`docs/design`、`docs/guides` | 留在 Kun 照旧维护 | — | 提议 |
| `samples/Kun.Sample.Tests` | 示例的测试，要 Aspire 的服务发现环境变量，CI 不跑 | `scripts/dotnet_ci.py` 的 `NOT_IN_CI`、`tests/gates` | 留在 Kun 照旧维护 | — | 提议 |
| `samples/Kun.Sample.Minimal` | 最小宿主，用来做 NativeAOT 发布与运行 | `tests/Kun.Integration.AotTests`（2）、工作流 `ci.yml` 的 `sample-aot` 作业 | 留在 Kun 照旧维护 | — | 提议 |
| `samples/Kun.Sample.Storage` | 对象存储的示例宿主 | 只有 `Kun.slnx` 引用 | 留在 Kun 照旧维护；没有文档指到它，补一句说明是文字更新 | — | 提议 |
| `samples/Kun.Sample.Crm` | 参考应用 `qian.sample.crm` 的后端，连同租户运营宿主与平台表面宿主 | 平台宿主（3）、几个组件的测试与说明、`tools/Kun.Dev.TenantOps`、工作流 `ci.yml`；共 99 个文件 | 留在 Kun 照旧维护（后端）。它的远程模块见第 2 节 | 组件 ID `qian.sample.crm` | 二表 5（后端照做）；字面保留 |
| `samples/docs/getting-started` | 网站「上手」一篇的代码 | `scripts/docs_examples.py`、`ci.yml` 的 `docs-examples` 作业、`release.yml` | 留在 Kun 照旧维护 | — | [S1-DOCS-012](../pm/tasks/S1-DOCS-012.md) |
| `samples/docs/write-a-component` | 网站「写一个组件」一篇的代码 | 同上 | 留在 Kun 照旧维护 | — | S1-DOCS-012 |
| `samples/docs/authorization-dotnet` | 网站「在 .NET 里接权限」一篇的代码 | 同上 | 留在 Kun 照旧维护 | — | S1-DOCS-012 |
| `samples/local/catalog-credentials.cs` | 本机跑目录示例时生成演示用凭据的单文件程序（引用 `Kun.Gateway`） | `docs/getting-started.md` | 留在 Kun 照旧维护 | — | 提议 |
| `tck/marketplaces` | 目录市场文件的一致性样例 | `packages/qian-surfaces`、`src/Kun.Surfaces`、`tests/Kun.Surfaces.Tests`、`schemas/v2` | 留在 Kun 照旧维护 | `.qian/marketplace.json` 的字段 | 字面保留 |
| `tck/profiles` | profile 与 patch 合成的一致性样例 | `src/Kun.Profiles`（2）、`packages/qian-surfaces`（2）、`packages/qian-cli`、几个 .NET 测试 | 留在 Kun 照旧维护 | `qian.profile.yml`、`qian.patch.yml` | 字面保留 |
| `tck/registry-artifacts` | 注册表制品的一致性样例 | `src/Kun.Registry`、`packages/qian-cli`、`tests/Kun.Registry.Tests` | 留在 Kun 照旧维护 | — | 提议 |
| `tck/runtime` | 运行时语义的一致性场景（.NET 与 TypeScript 两边跑同一套） | `src/Kun.Runtime`（2）、`packages/qian-cli`（3）、几个 .NET 测试 | 留在 Kun 照旧维护 | — | 提议 |
| `tck/surfaces` | 声明式表面的解析与校验样例 | `packages/qian-surfaces`（2）、`packages/qian-cli`（2）、`src/Kun.Surfaces` 等、工作流 `qian-surfaces.yml` | 留在 Kun 照旧维护 | — | 提议 |

`qian init` 现在只生成不带代码的组件骨架（`packages/qian-cli/src/init-docs.ts`），随命令行照旧维护。

## 5 已发布的包

### 5.1 npm：`@cavailab/qian-*`

前端框架的 10 个包，发在 GitHub Packages（`npm.pkg.github.com`），关联的是已归档的 `CavaiLAB/qian`，每个包都有 `0.2.0` 与 `0.2.1` 两个版本。只读查的清单，2026-11-27。

| 包 | 本仓库里谁在用 | 去向 | 依据 |
|---|---|---|---|
| `@cavailab/qian-tokens` | `packages/qian-identity-ui`、`packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell`、`samples/qian-sample-crm-ui`、`tests/directory-e2e`、`website`、根 `package.json` | 归档（已归档，留着能装）；本仓库照旧装 `0.2.1` | 归档 D1 |
| `@cavailab/qian-contracts` | `packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell` | 同上 | 归档 D1 |
| `@cavailab/qian-sdk` | `packages/qian-identity-ui`、`packages/qian-shell-kit`、`packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell`、`samples/qian-sample-crm-ui`、`tests/directory-e2e` | 同上 | 归档 D1 |
| `@cavailab/qian-core` | `packages/qian-identity-ui`、`packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell`、`samples/qian-sample-crm-ui`、`tests/directory-e2e` | 同上 | 归档 D1 |
| `@cavailab/qian-form` | `packages/qian-identity-ui`、`packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell` | 同上 | 归档 D1 |
| `@cavailab/qian-data` | `packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell`、`tests/directory-e2e` | 同上 | 归档 D1 |
| `@cavailab/qian-shell` | `packages/qian-identity-ui`、`packages/qian-shell-kit`、`packages/qian-tenancy-signup-ui`、`apps/qian-shell` | 同上 | 归档 D1 |
| `@cavailab/qian-biz` | `apps/qian-shell`、`tests/directory-e2e`；`tests/Kun.Identity.Server.Tests/Contracts/biz.openapi.json` 是它的契约的副本 | 同上 | 归档 D1 |
| `@cavailab/qian-plugin-sdk` | `packages/qian-shell-kit`、`packages/qian-tenancy-signup-ui`、`packages/qian-tenancy-ui`、`apps/qian-shell`、`samples/qian-sample-crm-ui` | 同上 | 归档 D1 |
| `@cavailab/qian-runtime` | `packages/qian-cli` 与 `packages/qian-surfaces` 的测试（开发依赖） | 同上 | 归档 D1 |

旧外壳与界面包冻结以后，还要装这些包的是认证页、注册页、网站（`qian-tokens`）与命令行、校验库的测试（`qian-runtime`）。删包、取消归档、给包标弃用，都不在这里定（第 10 节）。

### 5.2 NuGet：Kun 的包

85 个包，都在 `Kun.slnx` 里、由 `release.yml` 发布（`python3 scripts/release_packages.py list` 列出的名单，内核档 42 个、平台档 43 个）。GitHub Packages 上每个包都有，关联的是本仓库；内核档另发 nuget.org，平台档只在 GitHub Packages，装的时候要带 `read:packages` 的令牌（[S1-DEC-047](../pm/tasks/S1-DEC-047.md) D4）。平台档里只有 `Kun.Audit.EntityFrameworkCore` 在 nuget.org 上也有。「谁在用」：都是本仓库的宿主、组件与测试经项目引用在用，匠心以后照包装；逐包的引用不在这里列。只读查的清单，2026-11-27。

| 包 | 档位 | 角色 | 发在哪 | 去向 | 依据 |
|---|---|---|---|---|---|
| `Kun.AspNetCore` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Audit.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Audit.Postgres` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Authorization` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Caching` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Configuration` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Configuration.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Configuration.Apollo` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Configuration.Secrets` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Core` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Data.Native.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Data.Native.Postgres` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.EntityFrameworkCore` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.EntityFrameworkCore.PostgreSQL` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Errors` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Errors.AspNetCore` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.EventBus` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.EventBus.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.EventBus.EntityFrameworkCore` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.EventBus.RabbitMQ` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Gateway` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Gateway.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Grpc` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Identity` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Identity.AspNetCore` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Mediator` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.MultiTenancy` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Observability` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Plugin.Sdk` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Plugin.Sdk.Generators` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.RateLimit` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Runtime` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Security` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Security.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Storage.Abstractions` | 内核档 | 契约 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Storage.Local` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Storage.Oss` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Storage.S3` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Testing` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Testing.Containers` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Testing.EntityFrameworkCore` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Validation` | 内核档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Audit.EntityFrameworkCore` | 平台档 | 提供者 | GitHub Packages、nuget.org | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Authorization.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Authorization.Rbac` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Authorization.Rbac.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Capabilities` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Capabilities.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Capabilities.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Configuration.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Configuration.Platform` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Data` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Data.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Data.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Data.Dialect.Pg` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Docs` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Docs.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Files` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Files.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Files.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Gateway.Shell` | 平台档 | 提供者 | GitHub Packages | 代码留在 Kun 冻结；停不停发新版要产品负责人定（第 10 节） | 二表 5；要定 |
| `Kun.Identity.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Identity.Client` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Identity.Connector.Oidc` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Identity.Server` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Lifecycle` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Lifecycle.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Lifecycle.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Plugin.Sdk.AspNetCore` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Plugin.Sdk.Commands` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Profiles` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Profiles.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Registry` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Registry.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Registry.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Scheduler` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Scheduler.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Surfaces` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Surfaces.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Surfaces.Hosting` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Tenancy` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Tenancy.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Tenancy.AspNetCore` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Tenancy.Signup` | 平台档 | 提供者 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |
| `Kun.Tenancy.Signup.Abstractions` | 平台档 | 契约 | GitHub Packages | 留在 Kun 照旧维护 | 提议（照现行发布规则） |

### 5.3 早先发过、仓库里已经没有的包

本仓库已经不发、也不引用，但包源上还在的包。只读查的清单，2026-11-27。

nuget.org 上停发的 8 个旧线 `Kun.*`：

| 包 | 在哪 | 谁在用 | 为什么不在仓库里 | 去向 |
|---|---|---|---|---|
| `Kun.Audit` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 旧线的审计包，命令管道与链存储归档，`CHANGELOG.md` 记的停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.Auditing` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 并入后包名取消（S1-KUN-025），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.Dapper` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 归档的模块（D23.9），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.Infrastructure` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 归档的模块（D23.9），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.MultiTenancy.EntityFrameworkCore` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 并入 `Kun.EntityFrameworkCore` 后停发（S1-KUN-024），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.Saga` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 归档的模块（D23.9），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.Sandbox` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 归档的模块（D23.9，S1-KUN-028），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |
| `Kun.Storage` | nuget.org，最后一个版本 `2026.10.25` | 不在仓库里，本仓库不引用 | 旧线的存储包，归档的模块（D23.9），停发 8 个旧包之一 | 归档：已发布的版本不动；标不标弃用要产品负责人定（第 10 节） |

另外，合并以前在别的包源位置发过一批旧包，本仓库已经不引用。其中平台 0.x 的 54 个 NuGet 包，`CHANGELOG.md` 2026.11.21 对外写过「停更并保留全部历史版本」。这批包的清单不在这里列，删不删要产品负责人定（第 10 节第 3 项）。

## 6 `docs/` 下的文档

| 项 | 是什么 | 谁在用 | 去向 | 依据 |
|---|---|---|---|---|
| `docs/architecture.md` | Kun 框架架构，讲现状 | `README.md`、网站「Kun 框架」小节 | 留在 Kun 照旧维护（讲现状，随改随写） | 提议 |
| `docs/agent-support.md` | 对智能体的支撑：Agent 接入与模型服务，讲现状 | `README.md`、网站「Kun 框架」小节 | 留在 Kun 照旧维护 | 提议 |
| `docs/getting-started.md` | Kun 框架入门（目录与订单示例） | `README.md`、网站「Kun 框架」小节 | 留在 Kun 照旧维护 | 提议 |
| `docs/package-and-docs-disposition.md` | 本文 | `README.md`、[S1-DEC-051](../pm/tasks/S1-DEC-051.md) | 留在 Kun 照旧维护，随整理改写 | 新方向 |
| `docs/platform-overview.md` | 原来的平台总览 | 网站「设计与路线」的文档索引 | 删掉，已定没有做 | 丙批删 |
| `docs/platform-roadmap.md` | 原来的平台路线 | 网站「设计与路线」 | 删掉，已定没有做 | 丙批删 |
| `docs/adr/`（ADR-001 至 ADR-024 与 `decisions.md`） | 本仓库已接受的决策记录 | 任务、需求文档、网站「设计与路线」 | 留在 Kun 照旧维护：已接受的原文不改，后来的裁定加带日期的标注 | 提议（照现行写法） |
| `docs/adr/ADR-013-frontend-framework.md` | 前端与前端框架的决策 | 同上 | 留在 Kun 冻结（当时的决策）；2026-11-27 的去向加在它的 §9 | 一表 29；二表 3、5；新方向 |
| `docs/adr/ADR-021-product-family-structure.md` | 产品族结构（三层口径） | 同上 | 留在 Kun 冻结（当时的决策，已部分被 ADR-023 取代） | 提议 |
| `docs/adr/ADR-022-rspress-unified-component-docs.md` | 用 Rspress 统一组件文档 | 同上 | 留在 Kun 冻结；组件页那部分已由站 D2 改了 | 站 D2 |
| `docs/adr/kun/` | Kun 框架的决策记录（引用时写「Kun ADR-0xx」） | 网站「Kun 框架」小节按发布版镜像 | 留在 Kun 照旧维护 | 提议 |
| `docs/design/`（`module-map.md`、`why-cqrs.md`、`why-outbox.md`、`when-to-use-currentuser.md`） | Kun 框架的设计说明 | 网站「Kun 框架」小节 | 留在 Kun 照旧维护 | 提议 |
| `docs/design/whitepaper.md` | 原来的设计白皮书 | 网站「Kun 框架」小节 | 删掉，已定没有做 | 丙批删 |
| `docs/generated/` | 组件、事件、seam 与配置的派生目录 | 工作流 `qian-documentation.yml` 核对新鲜度；`tests/Kun.Tenancy.Tests` 读配置目录 | 留在 Kun 照旧维护（由命令行生成） | 提议 |
| `docs/guides/` | Kun 框架的指南（限流、身份客户端、Outbox、读侧写法、多租户等） | 网站「Kun 框架」小节 | 留在 Kun 照旧维护 | 提议 |
| `docs/ops/configuration-runbook.md` | 配置的运维手册 | `tests/Kun.Sample.Crm.Tests` 的 `CapabilityInvokeTests` 读它 | 留在 Kun 照旧维护 | 提议 |
| `docs/ops/k3s-control-plane-recovery.md` | 集群控制面恢复 | 网站「设计与路线」 | 删掉，已定没有做 | 丙批删 |
| `docs/ops/staging-first-deploy.md` | 预发环境首次部署 | 网站「设计与路线」 | 删掉，已定没有做 | 丙批删 |
| `docs/prd/`（第 01 至 17 章与 `README.md`） | 需求文档 | 任务、决策记录、网站「设计与路线」 | 留在 Kun 照旧维护：已接受的原文不改，加带日期的标注 | 提议（照现行写法） |
| `docs/prd/07-s1-foundation.md` 的 7.15 节 | 外壳与前端框架的 S1 最小集 | 同上 | 留在 Kun 冻结；7.15.1 加了 2026-11-27 的标注 | 一表 29；二表 5；新方向 |
| `docs/prd/11-ux-glossary.md` | 界面词表 | 认证页规格等 | 文字更新：匠心自己定设计规范以后，词表里只讲页面的部分去匠心，讲接口与错误文案的部分留 | 新方向；分法是提议 |
| `docs/prd/specs/component-model.md` | 组件模型规格，网站「CLI 与规格」由它生成 | `website/scripts/sync-specs.ts` | 留在 Kun 照旧维护 | 字面保留 |
| `docs/prd/specs/plugin-manifest.md` | 组件清单规格 | 组件模型规格、校验库 | 留在 Kun 照旧维护 | 字面保留 |
| `docs/prd/specs/data-contracts.md` | 数据契约规格 | 数据组件 | 留在 Kun 照旧维护 | 提议 |
| `docs/prd/specs/authz-remote-interface.md` | 远程授权接口的契约草案 | S1-DEC-045、S1-AZ 各任务 | 留在 Kun 照旧维护 | 提议 |
| `docs/prd/specs/model-service-interface.md` | 模型服务接口的契约草案 | S1-DEC-044、S1-MGW 各任务 | 留在 Kun 照旧维护 | 提议 |
| `docs/prd/specs/auth-pages.md` | 本仓库现在这一版认证页的设计 | 认证页、S1-IDP 各任务 | 留在 Kun 冻结，作匠心重做时的口径参考；页面去匠心重新设计 | 新方向；二表 3 |
| `docs/prd/specs/tenant-signup.md` | 租户自助开通：申请、审核、开通的接口与页面 | `plugins/qian.tenancy.signup`、S1-TEN 各任务 | 文字更新：接口与流程留，讲页面的部分标成去匠心 | 新方向；分法是提议 |
| `docs/prd/specs/frontend.md` | 前端框架的规格 | ADR-013、需求文档 7.15 | 归档：前端框架已归档，规格留着可查 | 归档 D1；提议 |
| `docs/prd/specs/docs-site.md` | 文档站的规格 | `website/` | 文字更新：组件页那部分已拿掉，导航与对外的地址随 [S1-DOCS-013](../pm/tasks/S1-DOCS-013.md) 与站点导航的任务改 | 站 D2；提议 |
| `docs/research/baseline-digest.md` | 调研基线摘要 | 网站「设计与路线」 | 删掉，已定没有做 | 丙批删 |
| `docs/security/` | 审计日志、多租户行级安全、服务间令牌 | 网站「Kun 框架」小节 | 留在 Kun 照旧维护 | 提议 |
| `docs/_meta.json` | 原来给平台文档站排侧栏，现在没有读它的地方 | — | 留着，新文档照旧列进去；以后留不留，S1-DEC-049 记的没有定 | `AGENTS.md`「要改文档」第 6 条 |

## 7 网站

### 7.1 顶栏导航（`website/docs/_nav.json`）

| 导航项 | 指到 | 现在讲什么 | 去向 | 依据 |
|---|---|---|---|---|
| 开始 | `/start/` | 概览、上手、30 分钟上手、宿主与框架、产品族、版本矩阵、核心概念、指南 | 留在 Kun 照旧维护；讲旧外壳与前端框架的几页文字更新（见 7.2） | 新方向 |
| 组件目录 | `/components/` | 平台组件的规划页（由 `website/scripts/collect-components.ts` 生成）与前端框架组件页的说明页 | 留在 Kun 照旧维护；`qian.shell` 一页标成冻结 | 站 D2；二表 5 |
| SDK | `/sdk/` | Kun 框架、后端 SDK、前端 SDK、AI SDK、API 参考 | 留在 Kun 照旧维护；前端 SDK 归档，AI SDK 里讲模型网关的两页文字更新 | 新方向；ADR-024 |
| CLI 与规格 | `/cli/` | 由组件模型规格按章节生成的命令与文件格式 | 留在 Kun 照旧维护 | 字面保留 |
| 运维 | `/ops/` | 部署、发布、密钥、模型网关接线 | 留在 Kun 照旧维护；模型网关接线一页文字更新 | ADR-024 |
| 市场与 ISV | `/marketplace/` | 上架规则 | 文字更新：补上私有市场由本企业的审核人批准、公共市场由平台审核 | 第二张表第 4 条，照建议（S1-DEC-050） |
| 设计与路线 | `/design/` | 逐文件镜像仓库的 `docs/`（由 `website/scripts/sync-specs.ts` 生成） | 留在 Kun 照旧维护，跟着 `docs/` 走（第 6 节）；本文也在这里 | 提议 |
| 产品 · 用 AI 构建 | `/sdk/ai/` | AI SDK 概览 | 文字更新：随 AI SDK 改 | 提议 |
| 产品 · 构建应用 | `/start/hosts` | 宿主与框架，讲旧外壳 | 文字更新：改指上手与写一个组件，讲旧外壳的部分去掉 | 提议 |
| 产品 · 多租户平台 | `/components/platform/` | 平台组件目录 | 留在 Kun 照旧维护 | 提议 |
| 产品 · 保护应用 | `/components/platform/qian.gateway` | 网关组件页 | 留在 Kun 照旧维护 | 提议 |
| 产品 · 部署与运维 | `/ops/` | 运维 | 留在 Kun 照旧维护 | 提议 |

「产品」这一组菜单整体怎么排、要不要加「给开发者与 Agent」的入口，在站点导航的任务里提，见第 11 节。

### 7.2 页面来源

手写的页面（`website/docs/` 下，进 Git）：

| 页面 | 现在讲什么 | 去向 | 依据 |
|---|---|---|---|
| `index.mdx` | 首页：标语、卡片（含「Qian 技能」「Qian MCP」两张）、经模型网关发一条请求 | 文字更新：卡片与模型网关的说法照现行裁定改 | 新方向；ADR-024 |
| `sitemap.mdx` | 站点地图 | 留在 Kun 照旧维护 | 提议 |
| `marketplace/index.md` | 上架规则 | 文字更新（见 7.1） | 第二张表第 4 条 |
| `ops/index.md` | 运维概览，写「Qian 沿用现有 K3s……」与模型网关接线 | 文字更新 | ADR-024；提议 |
| `ops/deploy.md` | GitOps 部署，写「Qian 用独立的 `qian-<env>` 命名空间」 | 文字更新 | 提议 |
| `ops/release.md` | 发布、灰度与回切 | 留在 Kun 照旧维护 | 提议 |
| `ops/secrets.md` | 密钥 | 留在 Kun 照旧维护 | 提议 |
| `ops/model-gateway.md` | 模型网关接线（推理服务经 Higress） | 文字更新：照 ADR-024 重写 | ADR-024；S1-DEC-049「后续」第 11 项 |
| `sdk/index.md` | SDK 概览，前端 SDK 一行列 `@cavailab/qian-*` | 文字更新 | 归档 D1 |
| `sdk/kun/with-qian.mdx` | 「Kun 框架」小节的手写说明 | 文字更新：讲前端框架的部分改成已归档 | 归档 D1 |
| `sdk/dotnet/index.md` | 后端 SDK 的编程模型 | 留在 Kun 照旧维护 | 提议 |
| `sdk/dotnet/capabilities.md` | 能力与函数 | 留在 Kun 照旧维护 | 提议 |
| `sdk/dotnet/events.md` | 事件与中间件 | 留在 Kun 照旧维护 | 提议 |
| `sdk/frontend/index.md` | 前端 SDK：外壳插件树、远程模块、插槽 | 归档 | 归档 D1；一表 29；二表 5 |
| `sdk/ai/index.md` | AI SDK 概览 | 文字更新：「Qian 不写 Agent loop」等说法与模型网关照现行改 | ADR-024；提议 |
| `sdk/ai/gateway.md` | 模型网关（Higress 方案） | 文字更新：照 ADR-024 重写 | ADR-024；S1-DEC-049「后续」第 11 项 |
| `sdk/ai/agent-runtime.md` | Agent 运行时对接（下游智能体平台、经 Higress 的 MCP 路由） | 文字更新：运行时用 DSH、MCP 那条路按 ADR-024 提议七 | S1-DEC-043 D2；ADR-024 |
| `sdk/ai/mcp.md` | MCP：组件的 `.mcp.json`、文档检索、每租户门面 | 文字更新：每租户门面不再托管在 Higress | ADR-024 |
| `sdk/ai/sandbox.md` | 沙箱 | 文字更新：「Qian 自己不写沙箱」的主语改成 Kun | 提议 |
| `start/index.md` | 「开始」概览 | 文字更新：宿主与框架一行随该页改 | 提议 |
| `start/getting-started.md` | 上手：在 ASP.NET Core 里用上 Kun | 留在 Kun 照旧维护 | S1-DOCS-010、S1-DOCS-012 |
| `start/quickstart.md` | 30 分钟上手 | 留在 Kun 照旧维护；与「上手」怎么分工在站点导航的任务里提 | 提议 |
| `start/hosts.md` | 宿主与框架，讲旧外壳与前端框架 0.2.1 | 文字更新：讲旧外壳的部分改成冻结、指到匠心 | 一表 29；二表 5；新方向 |
| `start/family.md` | 产品族：源码、版本、许可 | 文字更新：旧外壳那句随本文改 | 二表 5 |
| `start/concepts/component.md` | 组件 | 文字更新：包名写成 `Kun.<X>` | 提议 |
| `start/concepts/profile.md` | profile 与 patch | 文字更新：「运行中的 Qian」改成 Kun | 提议 |
| `start/concepts/scope.md` | scope | 留在 Kun 照旧维护 | 提议 |
| `start/concepts/seam.md` | seam | 文字更新：包名写成 `Kun.<X>` | 提议 |
| `start/concepts/surface.md` | 表面 | 文字更新：`ui` 表面一行的「前端外壳」改写 | 二表 5 |
| `start/guides/index.md` | 指南总览，含 2026-10 写的几节 | 文字更新：模型服务与 Agent 那几节照现行改 | ADR-024；提议 |
| `start/guides/write-a-component.md` | 写一个组件 | 留在 Kun 照旧维护 | S1-DOCS-010、S1-DOCS-012 |
| `start/guides/authorization-dotnet.md` | 在 .NET 里接权限 | 留在 Kun 照旧维护 | S1-DOCS-010、S1-DOCS-012 |

构建前由脚本生成的部分（不进 Git，`website/scripts/sources-plugin.ts` 调用）：

| 生成的目录 | 来源 | 去向 | 依据 |
|---|---|---|---|
| `components/` | `website/scripts/collect-components.ts` 里的平台组件清单，加前端框架组件页的说明页 | 留在 Kun 照旧维护 | 站 D2 |
| `cli/` | `docs/prd/specs/component-model.md` 按章节拆 | 留在 Kun 照旧维护 | 字面保留 |
| `design/` | 逐文件镜像 `docs/` | 留在 Kun 照旧维护 | 提议 |
| `sdk/kun/` | Kun 最新发布版的标签上白名单内的文档 | 留在 Kun 照旧维护 | S1-DOCS-005 |
| `sdk/api/` | Kun 最新发布版的 API 参考 | 留在 Kun 照旧维护 | S1-DOCS-005 |
| `start/versions.md` | 版本矩阵，从 `pnpm-workspace.yaml` 的版本目录与发布版读 | 文字更新：`@cavailab/qian-*` 标成已归档 | 归档 D1 |

### 7.3 给机器读的产物

| 产物 | 现在有什么 | 去向 | 依据 |
|---|---|---|---|
| `llms.txt`、`llms-full.txt` | 构建时生成；有给大模型的读法与「从这里开始」一节，按项目与部分分节 | 留在 Kun 照旧维护；先不接进 DSH，等 Web 端的工作区服务 | S1-DEC-047 D3；第二张表第 7 条，照建议（S1-DEC-050） |
| 每页的 `.md` | 构建时把每页渲染成 Markdown | 留在 Kun 照旧维护 | 提议 |
| `kb-index.json`、`sitemap.xml` | 构建后产出，带绝对地址 | 留在 Kun 照旧维护；地址随 S1-DOCS-013 换成对外的域名 | S1-DOCS-013 |

## 8 工作流

逐个写在 [S1-QA-033](../pm/tasks/S1-QA-033.md) 里，这里只列去向：

| 工作流 | 测的是什么 | 去向 | 依据 |
|---|---|---|---|
| `ci.yml` | 根工作流，调用其余各个；「CI 汇总」是必需检查 | 留在 Kun 照旧维护；去掉停掉的那几个作业，「CI 汇总」不改名 | 二表 5 |
| `release.yml` | 发 NuGet 包与 GitHub Release | 留在 Kun 照旧维护 | 提议 |
| `qian-docs-and-pm.yml` | 文档门禁、任务文件、看板 | 留在 Kun 照旧维护 | 提议 |
| `qian-docs-site.yml` | 网站的构建、检查与发布 | 留在 Kun 照旧维护 | 提议 |
| `qian-documentation.yml` | 事件声明、命令行的文档模板、派生文档的新鲜度 | 留在 Kun 照旧维护 | 字面保留 |
| `qian-surfaces.yml` | 两边的声明式表面契约、profile 装载 | 留在 Kun 照旧维护 | 字面保留 |
| `qian-dev-tenant-ops.yml` | 租户运营宿主的后端走查 | 留在 Kun 照旧维护 | 二表 5（后端照做） |
| `qian-directory-e2e.yml` | 目录选择器的浏览器验收 | 留在 Kun 冻结，不再由 `ci.yml` 调用 | 二表 5 |
| `qian-tenant-ui.yml` | 租户运营页面的七步验收 | 留在 Kun 冻结，不再由 `ci.yml` 调用 | 二表 5 |
| `qian-shell-federation.yml` | 外壳挂载远程模块 | 留在 Kun 冻结，不再由 `ci.yml` 调用 | 二表 5 |
| `qian-shell-auth.yml` | 真实身份服务上的登录、选择租户、连接器登录、注册主线，加上外壳与平台控制台的用例 | 文字更新：只留登录与注册的用例，留到新登录页跑通 | 二表 5；新方向 |
| `qian-ui.yml` | 外壳与界面包的 lint、类型、单测、边界、构建 | 文字更新：只查认证页、注册页与它们的依赖 | 二表 5 |

工作流的文件名都带 `qian-` 前缀，改名会动「CI 汇总」以外的检查名，这次不改；要改另提。

## 9 仍在使用、要保持兼容的名字

| 名字 | 在哪 | 依据 |
|---|---|---|
| 命令 `qian` 与子命令 `init`、`add`、`list`、`remove`、`doctor`、`docs generate` | `packages/qian-cli`；工作流与 .NET 测试里调用 | 新方向；字面保留 |
| 导入名 `@qian/*`（`@qian/surfaces`、`@qian/identity-ui`、`@qian/shell-kit`、`@qian/tenancy-ui`、`@qian/tenancy-signup-ui`、`@qian/sample-crm-ui`、`@qian/openapi-codegen`、`@qian/directory-e2e`） | 工作区包名 | 字面保留 |
| 包名 `@cavailab/qian-*`（10 个） | 已发布，见 5.1 | 归档 D1 |
| 包名 `Kun.*`（85 个） | 已发布，见 5.2 | 提议（照现行发布规则） |
| 组件 ID `qian.*` | `plugins/` 下的组件目录，`qian.sample.crm` | 字面保留 |
| 清单与配置的文件名 `.qian-plugin/plugin.json`、`qian.profile.yml`、`qian.patch.yml`、`.qian/marketplace.json`、`ui/manifest.json` | 组件、profile、市场 | 字面保留 |
| 契约里的扩展字段 `x-qian-side-effect`、`x-qian-capability` 等，UI 清单的 `engines.qian` | `contracts/`、`plugins/*/contracts/`、组件的 UI 清单 | 字面保留 |
| 运行期的环境变量、镜像标签、Cookie 名里的 `qian` 字样（如 `QIAN_ROOT`、`io.qian.*`、`qian.identity.signin`） | 宿主、镜像、身份服务 | 字面保留 |
| 认证页与接口的路径 `/login`、`/api/identity/*`、`/api/signup/*`、`/auth/*` | 身份服务、注册接口、依赖方会话 | 新方向（Kun 继续负责认证与会话）；S1-IDP-031 |

## 10 要产品负责人定的

下面几件动了会改变对外承诺，或者现有的裁定没有说到，整理时不定，攒进下一张要产品负责人定的表：

1. **NuGet 包 `Kun.Gateway.Shell` 停不停发新版。** 它托管的是旧外壳，第二张表第 5 条定了旧外壳不再进镜像和发布链；可它是一个已经发布过的包（GitHub Packages 上有 `2026.11.20-beta.1`、`2026.11.21`、`2026.11.24` 三个版本），停发会让照它装的宿主拿不到新版。整理时的建议：从下一个版本起停发，已发布的版本不动，在 `CHANGELOG.md` 里写明。
2. **10 个 `@cavailab/qian-*` 要不要在包源上标弃用。** S1-DEC-043 D1 定的是包留着、能装、不再发新版，没有说标不标弃用。标了，装的时候会出提示，本仓库的认证页与网站照样能装。整理时的建议：先不标，等匠心的新登录页跑通、本仓库不再装它们时再问。
3. **合并以前在别的包源位置发过的旧包删不删。** 本仓库已经不引用它们，删是不可逆的；其中平台 0.x 的 54 个 NuGet 包，`CHANGELOG.md` 2026.11.21 对外写过「停更并保留全部历史版本」，删了就违背这句。整理时的建议：先不删。
4. **nuget.org 上停发的 8 个旧线 `Kun.*` 包要不要标弃用。** 它们最后一个版本是 `2026.10.25`，此前的做法是「nuget.org 上已发布的版本不动」（`CHANGELOG.md`）。整理时的建议：标弃用并指到取代它的包，不下架。
5. **旧登录页与旧注册页什么时候退。** 新方向说旧布局和样式可以淘汰；第二张表第 3 条说冻结的那一版留作缺省、新页面上线以前照冻结的样子用。整理时读作新页面跑通以前旧页面照旧用，跑通以后退不退、什么时候退，到时再报。

## 11 接下来的任务

先列清去向是第一步。后两步要做的活建成了任务，只建、没有做，估时都是整理时的估计：

| 任务 | 做什么 |
|---|---|
| [S1-DOCS-014](../pm/tasks/S1-DOCS-014.md) | 修订文档、归档过时的 Qian 内容：照第 6 节与 7.2 节逐项改写或归档 |
| [S1-DOCS-015](../pm/tasks/S1-DOCS-015.md) | 网站的导航：照第 7.1 节改顶栏与「产品」菜单，加给开发者与 Agent 的入口 |
| [S1-DOCS-016](../pm/tasks/S1-DOCS-016.md) | 给开发者与 Agent 的文档、MCP 与命令行入口（Kun 这一侧）：让人找得到 Kun 的接口与能跑的示例 |
| [S1-IDP-033](../pm/tasks/S1-IDP-033.md) | 用登录页跑通前后端流程时 Kun 这一侧要补的：接口进公开契约以后，给匠心一个能跑的示例与本机起身份服务的办法 |
| [S1-QA-033](../pm/tasks/S1-QA-033.md) | 停掉只为旧外壳、租户与成员页、平台控制台跑的 CI，从镜像与发布链里拿掉 |
| [S1-IDP-031](../pm/tasks/S1-IDP-031.md) | 登录与会话的接口进公开契约 |
| [S1-IDP-032](../pm/tasks/S1-IDP-032.md) | 设计：登录页与认证页改由匠心提供以后，身份服务与这些页面怎么接 |
