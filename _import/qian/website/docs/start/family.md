---
title: 产品族
description: Kun、Qian 与 qian 组件库三层与周边仓库（官方市场、Agent 运行时、部署与模型服务）：各是什么、在哪个仓库、版本与许可、文档在哪。
part: start
project: qian
audience: [developer, operator, isv, decision-maker]
updated: 2026-11-19
---

# 产品族

> **合并后的去处（2026-11-17 起）**：Qian 并入 Kun（[ADR-023](/design/adr/adr-023-kun-qian-merge-blueprint)，S1-DEC-038 D17、D18），合并前 Kun 与 Qian 各自的仓库当天归档为只读。
>
> - 两边的后端都带历史迁入 [`CavaiLAB/kun`](https://github.com/CavaiLAB/kun)：Kun 的代码在仓库根；Qian 的 .NET 代码、外壳应用、本站、命令行、契约、部署清单、任务与看板、决策记录与设计文档暂放在 `_import/qian/` 下，由后续批次提出过渡目录，`Qian.*` 改名为 `Kun.*` 也在这些批次里做。本站与任务看板现在由 `CavaiLAB/kun` 的主干发布。
> - 前端框架库 qian（八个组件包、组件演练场、前端插件开发包、TypeScript 运行时）迁往 [`CavaiLAB/qian`](https://github.com/CavaiLAB/qian)。「Qian」以后只作前端框架的名字。
> - 迁移怎么分批，见 `CavaiLAB/kun` 的[迁移执行计划](https://github.com/CavaiLAB/kun/blob/main/docs/migration-plan.md)。
>
> 下面的表格与「四样东西把它们关联起来」一节是合并之前的原状，表里写「本仓库」的指合并前 Qian 自己的仓库。

三个名字、三层，不是一个大单仓（[ADR-021](/design/adr/adr-021-product-family-structure)）。对外一句话：**Qian 平台基于 Kun 框架构建，前端使用 qian 组件库**。Kun 不是 Qian 的子项目（它有自己的下游与发布节奏），Qian 也不是 Kun 的扩展（它有自己的运行时、插件与产品面）。

```text
qian 组件库  前端层   React 组件库与外壳（@cavailab/qian-*）
Qian         平台层   Qian.Runtime + 平台插件 + profile + qian CLI + 目录市场
Kun          框架层   .NET 10 微服务框架（Kun.*），Qian 的底座
```

| 名字 | 层 | 是什么 | 仓库 | 版本 · 许可 | 文档在哪 |
|---|---|---|---|---|---|
| Kun | 框架层（底座） | .NET 10 云原生微服务框架，32 个 `Kun.*` 模块：Mediator、Outbox、Saga、多租户与 RLS、网关与 S2S、存储、缓存、可观测 | `idadawn/kun`（2026-10-08 从 CavaiLAB 迁回，旧地址重定向） | CalVer · MIT · 正式包（含同日修复版 `-hotfix.N`）在 nuget.org | [Kun 框架](/sdk/kun/)小节（按锁定版本镜像）与 [Kun API 参考](/sdk/api/kun/) |
| Qian | 平台层 | 基于 Kun 的多租户 PaaS：`Qian.Runtime` + 平台插件 + profile + `qian` CLI + 目录市场 | `idadawn/qian`（本仓库） | SemVer 锁步 · Apache-2.0 | 本站全部七个部分；设计文档在[设计与路线](/design/) |
| yue | 前端层 | React 组件库与外壳，现名 qian，`@cavailab/qian-*` | `CavaiLAB/qian`（2026-11 迁出；本仓库装 GitHub Packages 上的已发布包） | SemVer · MIT | [组件目录](/components/yue/)（活演示 + props 表）与[前端 SDK](/sdk/frontend/) |
| `qian-plugins` | 市场 | 官方目录市场 `.qian/marketplace.json` | `idadawn/qian-plugins`（S1 私有，待建） | — | [市场与 ISV](/marketplace/) |
| iClaw | 兄弟系统 | Agent 运行时与 AI 资产控制面；Qian 不写 Agent loop（[ADR-016](/design/adr/adr-016-iclaw-boundary)） | [`idadawn/iclaw`](https://github.com/idadawn/iclaw) | — | AI SDK 的 [Agent 运行时对接](/sdk/ai/agent-runtime) |
| 部署与模型服务 | 运维 | GitOps 部署仓库（[ADR-017](/design/adr/adr-017-deployment)）；GPUStack + Higress 模型服务（[ADR-015](/design/adr/adr-015-model-gateway)） | [`idadawn/iclaw-deploy`](https://github.com/idadawn/iclaw-deploy)、[`idadawn/MaaS`](https://github.com/idadawn/MaaS) | — | [运维](/ops/) |

## 四样东西把它们关联起来

不建大单仓、不建 submodule 元仓库（ADR-021 D21.2）：

1. **一个 GitHub 所有者**：产品族全部仓库同一所有者，一个 GitHub Packages 命名空间、一段源配置。Kun 正式版的主渠道是公开的 nuget.org。
2. **一个文档站**：就是本站。Kun 是第 3 部分 SDK 的第一节，内容按 Qian 锁定的 Kun 版本从 kun 仓库镜像。
3. **一张版本矩阵**：[版本矩阵](/start/versions)由仓库钉版文件生成——哪个 Qian 配哪个 Kun、.NET SDK、Node、K3s 基线。
4. **一个看板**：跨仓库工作在 Qian 仓库的 `pm/` 跟踪，kun 仓库的 PR 回链到任务。

Kun 的版本只在 `Directory.Packages.props` 的 `KunVersion` 写一次；升级 Kun = 改这一处、跑 TCK、文档自动跟随。要改 Kun 的代码或文档，去 kun 仓库提 PR，Qian 的任务里只回链。本页与仓库 README 的「产品族」表同一内容，改一处要同步另一处（有单测核对）。
