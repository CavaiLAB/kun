---
title: 产品族
description: Kun 内核与平台组件、Qian 前端框架的源码、版本、许可与文档。
part: start
project: qian
audience: [developer, operator, isv, decision-maker]
updated: 2026-11-19
---

# 产品族

Kun 在一个 .NET 源码树中提供内核与平台组件。前端宿主和插件从 Qian 获取 React 组件、前端 SDK 与运行时。

| 名字 | 层 | 是什么 | 仓库 | 版本 · 许可 | 文档在哪 |
|---|---|---|---|---|---|
| Kun | 内核 | .NET 10 的命令调度、租户上下文、数据访问、事件和可观测模块 | `CavaiLAB/kun` | CalVer · MIT | [框架文档](/sdk/kun/) |
| Kun | 平台组件 | 组件运行时、宿主、租户、身份、授权、事件、审计与能力目录 | `CavaiLAB/kun` | CalVer · MIT | [平台设计](/design/) |
| Qian | 前端 | React 组件、外壳、TypeScript 运行时与前端插件 SDK | `CavaiLAB/qian` | SemVer · MIT | [组件目录](/components/yue/)与[前端 SDK](/sdk/frontend/) |

正式版内核包发布到 nuget.org，平台包的可用渠道见[版本矩阵](/start/versions)。本站从发布标签构建 Kun 框架和前端组件参考，从后端仓库的当前文档构建[设计与路线](/design/)。开发任务在 [Kun 任务目录](https://github.com/CavaiLAB/kun/tree/main/_import/qian/pm/tasks)中维护。
