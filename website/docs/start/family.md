---
title: 产品族
description: Kun 内核与平台组件、Qian 前端框架的源码、版本、许可与文档。
part: start
project: platform
audience: [developer, operator, isv, decision-maker]
updated: 2026-11-21
---

# 产品族

Kun 在一个 .NET 源码树中提供内核与平台组件。Kun 旧应用外壳按 Qian 0.2.1 归档，业务预发保留原有部署和数据。匠心工业智能体平台的 Web 与桌面端采用 DSH 外壳。[外壳归档裁定 P42](https://github.com/CavaiLAB/jiangxin/blob/5ca1442b04eb7c4812314664ab06a9d7feba6a22/docs/decisions/2026-11-21-shell-migration.md) · [产品裁定 P38](https://github.com/CavaiLAB/jiangxin/blob/main/docs/decisions/2026-11-20-product.md)

| 名字 | 层 | 是什么 | 仓库 | 版本 · 许可 | 文档在哪 |
|---|---|---|---|---|---|
| Kun | 内核 | .NET 10 的命令调度、租户上下文、数据访问、事件和可观测模块 | `CavaiLAB/kun` | CalVer · MIT | [框架文档](/sdk/kun/) |
| Kun | 平台组件 | 组件运行时、宿主、租户、身份、授权、事件、审计与能力目录 | `CavaiLAB/kun` | CalVer · MIT | [平台设计](/design/) |
| Qian | 前端 | React 组件和外壳库、TypeScript 运行时与前端插件 SDK | `CavaiLAB/qian` | SemVer · MIT | [组件目录](/components/qian/)与[前端 SDK](/sdk/frontend/) |

首个合并版发布后，beta 两档均从 CavaiLAB 的 GitHub Packages 还原。稳定版内核包从 nuget.org 还原，平台包从 GitHub Packages 还原。逐包清单随该版本的 GitHub Release 发布；站点收录的版本见[版本矩阵](/start/versions)。本站从发布标签构建 Kun 框架和前端组件参考，从后端仓库的当前文档构建[设计与路线](/design/)。开发任务在 [Kun 任务目录](https://github.com/CavaiLAB/kun/tree/main/pm/tasks)中维护。
