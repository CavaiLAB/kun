# Kun

[匠心智能体平台](https://github.com/CavaiLAB/jiangxin)的企业级后端框架：为 Agent 时代构建的多租户平台框架，.NET 10。

## 定位

已定的表述（2026-11-17）：

> Kun 是为 Agent 时代构建的多租户平台框架（.NET 10）。它由三部分组成：一组不带产品语义、可以单独安装的内核包；一组以组件形式装配的平台组件（租户、身份、权限、数据、文件、配置、事件、审计、能力目录）；以及组件运行时、命令行与目录市场。

在平台的理念「三元全智生命体」里，Kun 是「三元」的后端一半，也就是躯体。它为智能体提供身份与能力目录；模型服务也由它提供（已定）；Agent 之间的协同与人审提议也由它承担。人审的规则已定：发给 Agent 的内容、Agent 发出去的内容都要由人审核，起步出站、入站两道都必审，见平台总览的 [Agent 协同](https://github.com/CavaiLAB/jiangxin/blob/main/docs/04-agent-collaboration.md)第 4.2 节。它不写智能体的推理循环，那是 [SanYuanAgent](https://github.com/CavaiLAB/sanyuan-agent) 的事。

## 产品族

| 名字 | 层 | 是什么 | 仓库 | 版本 · 许可 | 文档在哪 |
|---|---|---|---|---|---|
| Kun | 内核 | .NET 10 的命令调度、租户上下文、数据访问、事件和可观测模块 | `CavaiLAB/kun` | CalVer · MIT | [架构](docs/architecture.md) |
| Kun | 平台组件 | 组件运行时、宿主、租户、身份、授权、事件、审计与能力目录 | `CavaiLAB/kun` | CalVer · MIT | [平台概览](docs/platform-overview.md) |
| Qian | 前端 | React 组件和外壳库、TypeScript 运行时与前端插件 SDK | `CavaiLAB/qian` | SemVer · MIT | [前端框架](docs/prd/specs/frontend.md) |

Kun 仓库的 `qian` 命令行、外壳应用、组件清单与契约共同组成平台开发工作区。任务、看板构建方法与维护流程见[任务说明](pm/README.md)。

## 两档

| 档位 | 是什么 | 规则 |
|---|---|---|
| 内核档 | 通用机制：错误模型、命令与查询调度、租户上下文、事件总线、数据访问约定、令牌校验、网关、存储后端、配置源。它不知道租户目录、人员、角色、组件清单这些产品概念 | 可以在任意 .NET 10 应用里单独安装使用；不得引用平台档的包 |
| 平台档 | 带产品语义：租户主档与开通、身份服务、权限引擎、文件服务、能力目录、注册表、生命周期、调度 | 以组件形式装配；只依赖内核档，以及别的平台组件的契约包 |

本次合并版的 NuGet 清单包含 85 个包，内核档 42 个，平台档 43 个。内核依赖边界由构建门禁校验。首个合并版发布后，可从该版本的 GitHub Release 下载逐包清单与用于还原的 `NuGet.config`。[包迁移指南](docs/guides/package-migration.md)

## 它负责什么

| 能力域 | 内容 |
|---|---|
| 运行时与组件 | 上下文、服务注册、类型化事件、可撤销的副作用、装载器；profile、组合包与补丁；注册表与生命周期 |
| 命令行与目录市场 | 组件的安装、移除、列出、健康检查、打印配置树；目录市场的清单格式 |
| 多租户 | 租户上下文贯穿全栈，数据按租户行级隔离；租户主档与开通；租户范围的组件启用与定制 |
| 身份 | 单点登录的签发方与客户端，外部身份源连接器，会话 |
| 权限 | 权限点与角色，数据与字段级判定，入口缺省拒绝的强制执行点 |
| 数据 | 数据访问约定，行级隔离，不经 ORM 的原生通道，数据库方言与迁移 |
| 事件 | 领域事件与集成事件，同事务落库后可靠投递，消费端幂等 |
| 审计 | 审计记录与业务同事务，串成哈希链，事后可核验 |
| 配置与密钥 | 强类型配置，配置中心接入，密钥只以引用出现 |
| 存储与文件 | 存储后端（本地、对象存储）；带配额与策略的文件服务 |
| 网关 | 反向代理，外部令牌校验，内部短期令牌签发，限流 |
| 调度 | 定时与后台作业 |
| 能力目录 | 每项能力登记输入输出 Schema、副作用等级、所需权限、是否对 Agent 暴露 |
| 缓存与可观测 | 统一缓存；追踪、指标、日志 |
| 对智能体的支撑 | `agent` 主体与委托链、Agent 归属与目录、协同信箱与人审、把能力目录暴露成工具；接住从设备上连进来的 Agent。大部分是规划：路线把 `agent` 主体与能力目录给 Agent 用排在里程碑 M1，协同与人审排在 M7，设备接入排在 M8（已定）；协同服务放在 Kun 是提议。见[对智能体的支撑](docs/agent-support.md) |
| 模型服务 | 模型目录、调用入口、租户路由、配额与计量。由 Kun 提供是已定的，怎么做是规划；模型请求的转发仍由模型网关承担。见[对智能体的支撑](docs/agent-support.md) |

## 设计原则

1. **没有特权内核。** 运行时只有一个很小的核心，租户、身份、权限这些平台能力都是组件，任何一项都能从配置里替换。
2. **契约与实现分包。** 一项能力拆成契约包（接口与词汇）和提供方包（实现）。使用方只依赖契约包，换一个实现不影响使用方。
3. **装一个包就能用。** 内核档的包装一个就能在别的 .NET 项目里独立使用，不要求先部署整个平台。平台档的组件以组件形式装进使用方自己的进程即可使用，同样不要求部署整套平台。
4. **多租户贯穿。** 租户上下文只来自令牌，缺失即拒绝。租户号统一用 Guid。
5. **入口缺省拒绝。** HTTP、命令、gRPC 入口都经过强制执行点，没有声明权限的入口不放行。
6. **跨服务契约冻结后只做加法。** 服务之间共用的契约冻结之后，只增加字段，不改、不删。
7. **门禁把关。** 公共 API 清单、包的档位与依赖方向、一致性测试都由 CI 检查。

## 已定与待定

已定（产品负责人，2026-11-17 起）：

- 合并蓝图和分阶段记录见 [ADR-023](docs/adr/ADR-023-kun-qian-merge-blueprint.md)。
- 内核与平台的 .NET 包均使用 `Kun.*` 包名与日历版本。
- 租户号统一用 Guid，去掉缺省租户的回落；租户上下文端口里租户号的属性类型也改成 Guid。
- 许可：.NET 包族、命令行与前端包全部用 MIT。
- 合并版的发布规则：beta 的两个档位均从 CavaiLAB 的 GitHub Packages 还原。稳定版内核档从 nuget.org 还原，平台档从 CavaiLAB 的 GitHub Packages 还原。发布流程逐包核对组织包的公开可见性及仓库关联。各版本的源映射见[包迁移指南](docs/guides/package-migration.md)。
- 已归档五个模块。`Kun.Saga`、`Kun.Sandbox`、`Kun.Dapper`、`Kun.Infrastructure` 与旧线的 `Kun.Storage` 已移出解决方案和发布清单。
- 「Qian」只作前端框架的名字。组件 id 前缀 `qian.`、清单与配置的文件名这些运行期字面当作历史字面保留，不再表示产品名；`qian` 命令行与 `@qian/*` 导入名本次不改。
- 迁移 2026-11-17 开始，不等别的里程碑（S1-DEC-038 D17）；原来「冻结窗口放在原仓库当前在途的里程碑收口之后」的说法由此改写。
- 用源码引用 Kun 的下游，在归并开始之前先钉住合并前的最后一个 Kun 版本。
- 合并后的 Kun 放在本仓库：Kun 框架与平台后端都迁入这里，取代蓝图里写的目标仓库（S1-DEC-038 D17）。先建仓库写文档，代码已带历史导入（B1，CavaiLAB/kun#3）。
- 前端框架库迁往 `CavaiLAB/qian`，只迁框架库（S1-DEC-038 D18）：TypeScript 运行时与前端插件开发包随前端仓库；外壳应用与外壳工具包、三个组件界面包、参考应用的远程模块、文档站、命令行与声明式表面的校验库留在本仓库。
- 模型服务由 Kun 提供。SanYuanAgent 是独立运行的 SDK，每个端都要用到它，包括机器人这类边缘设备；机器人本地可能还有小模型。
- 2026-11-19 的平台级裁定（P22–P33，全文见平台总览的 [2026-11-19 平台级裁定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/decisions/2026-11-19-platform.md)）里与 Kun 直接相关的：应用之间可以互相通讯，缺省是目录里看得到、调用与订阅按声明经批准放行（P26）；历史系统经连接器接进企业级框架，在企业内网的，连接器的执行部分装在内网、只向外连（P27）；每个 Agent 发出去的内容与发给 Agent 的内容都要人审，起步两道都必审（P28）；加一种 Agent 一级的范围（P29）；组件里新增「知识库」内容（P30）；路线重排，`agent` 主体在里程碑 M1 启用（P31）；交付给客户的可以是多租户的版本，也可以是单租户的版本（P33）。与平台原决策记录的出入登记在 S1-DEC-038 D22–D26。

待定：

- 任务、看板和新决策记录的长期归属见 S1-DEC-038 Q2。本仓库现行任务与看板的入口是 [pm/](pm/README.md)。

## 前端依赖与包源

Kun 旧应用外壳按 [Qian](https://github.com/CavaiLAB/qian) 0.2.1 归档，组件文档从 Qian 的对应发布标签生成。业务预发保留原有部署和数据。匠心工业智能体平台的 Web 与桌面端采用 DSH 外壳。[外壳归档裁定 P42](https://github.com/CavaiLAB/jiangxin/blob/5ca1442b04eb7c4812314664ab06a9d7feba6a22/docs/decisions/2026-11-21-shell-migration.md) · [产品裁定 P38](https://github.com/CavaiLAB/jiangxin/blob/main/docs/decisions/2026-11-20-product.md)

本仓库的旧外壳源码、三个界面包、外壳接线、参考应用的远程模块、文档站与真实目录验收页引用 Qian 发布在 GitHub Packages 上的 `@cavailab/qian-*`：版本只在 `pnpm-workspace.yaml` 的版本目录里钉一次（精确版本，升级走 PR）；命令行与校验库的测试用的 `@cavailab/qian-runtime` 写在它们各自的 `package.json` 里。

GitHub Packages 的 npm 源连公开包也要令牌（有 `read:packages` 即可）。仓库里的 `.npmrc` 只写作用域 `@cavailab:registry=https://npm.pkg.github.com`；令牌行要放用户级配置，因为 10.34.2 起的 pnpm 10、11.5.3 起的 pnpm 11，以及 pnpm 12 不展开仓库 `.npmrc` 认证行里的环境变量（安全公告 GHSA-3qhv-2rgh-x77r；11.0.0–11.5.2 仍会展开），会提示 `Ignored project-level auth setting … environment variables are not expanded in repository-controlled registry credentials`，接着 401。本机的一种做法：在 `~/.npmrc`（或 `NPM_CONFIG_USERCONFIG` 指向的文件）里写一行引用环境变量、不写令牌本身：

```ini
//npm.pkg.github.com/:_authToken=${NODE_AUTH_TOKEN}
```

```bash
NODE_AUTH_TOKEN=$(gh auth token) pnpm install
```

CI 里由 `actions/setup-node` 的 `registry-url` 写同样的用户级配置，`pnpm install` 带 `NODE_AUTH_TOKEN`（本次运行的 `GITHUB_TOKEN`），作业权限 `packages: read`。

钉版涉及版本目录中的 9 条、`minimumReleaseAgeExclude` 里同名的 10 条、`packages/qian-cli` 与 `packages/qian-surfaces` 的 `@cavailab/qian-runtime`。pnpm 12 默认拦发布不满一天的包（`minimumReleaseAge`），刚发的版本靠 `minimumReleaseAgeExclude` 放行；`pnpm install` 会先按新的排除清单检查旧锁文件，旧版本发布也不满一天时报 `ERR_PNPM_MINIMUM_RELEASE_AGE_VIOLATION`。这时解析期间先留着旧版本的排除条目，装完删掉，再跑 `pnpm install --frozen-lockfile` 确认新锁文件过策略。文档站读版本目录检出同名标签，不用另改。

## 文档

- [框架架构](docs/architecture.md)：两档分层、运行时模型、包的形态与命名、请求怎么走、能力域、对智能体的支撑。
- [对智能体的支撑：Agent 接入与模型服务](docs/agent-support.md)：怎么接住集群里和设备上的 Agent，模型服务怎么提供，设备上的本机模型怎么算。
- [迁移与归并](docs/migration.md)：两边各有什么，哪些已定、哪些待定，分几步并。
- Kun 框架的文档（多数是英文）：[入门](docs/getting-started.md)、[模块地图](docs/design/module-map.md)、[设计白皮书](docs/design/whitepaper.md)；指南、安全与设计说明分别在 `docs/guides/`、`docs/security/`、`docs/design/`，Kun 框架的决策记录在 `docs/adr/kun/`，引用时写「Kun ADR-0xx」。
- [平台总览](https://github.com/CavaiLAB/jiangxin)：理念、角色与产品形态、总体架构、插件模型、应用互通、历史系统接入、Agent 协同、路线、平台级裁定。

合并蓝图见 [ADR-023](docs/adr/ADR-023-kun-qian-merge-blueprint.md)，裁定见 [S1-DEC-036](pm/tasks/S1-DEC-036.md)。[平台决策目录](docs/adr/decisions.md)列出各能力域的设计与规范。
