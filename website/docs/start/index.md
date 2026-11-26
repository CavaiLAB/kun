---
title: 开始
description: 从空白 ASP.NET Core 项目开始使用 Kun 组件，了解组件、表面、scope、seam 与 profile。
part: start
project: platform
audience: [developer]
updated: 2026-11-26
---

# 开始

Kun 平台以**组件**交付能力。组件目录包含清单、声明式表面与可选代码。应用可以将所需组件装入自己的宿主，或通过 `platform` profile 组装多租户平台。

## 从这里开始

- [上手](/start/getting-started)：空的 ASP.NET Core 项目里装两个 nuget.org 上的内核包，写一个命令，出错时返回统一的 ErrorDetail；不需要令牌。
- [写一个组件](/start/guides/write-a-component)：声明服务与能力，用清单工具从代码派生清单，再装进宿主。
- [在 .NET 里接权限](/start/guides/authorization-dotnet)：给命令与 HTTP 端点声明权限点，在业务代码之前判定。
- 哪些使用文档已经能照着做、哪些还写不了：[指南](/start/guides/#usage)。
- [30 分钟上手](/start/quickstart)：从源码构建 `qian` 命令行，用仓库的安装验收从包源装组件；要 GitHub Packages 的读包令牌。
- [宿主与框架](/start/hosts)：独立模式与平台模式、五个 profile、前端外壳、与 Kun 的关系。

## 核心概念

| 概念 | 一句话 |
|---|---|
| [组件](/start/concepts/component) | 分发与安装的单位：文件夹 + 清单 + 声明式表面 + 可选代码；运行时形态叫插件 |
| [表面](/start/concepts/surface) | 组件对外暴露的一类声明式内容：skills、agents、commands、hooks、MCP、tools、ui 等 |
| [scope](/start/concepts/scope) | 注册的归属：全局，或一个租户 / 应用 / Agent 会话；两层扁平、不继承 |
| [seam](/start/concepts/seam) | 一项能力的缝合点：Definition + Provider + Consumer，换 Provider 不改调用方 |
| [profile 与 patch](/start/concepts/profile) | 具名组装与分层覆盖：环境 = profile + 分层 patch |

## 再往后

- [指南](/start/guides/)：使用文档的现状，以及 Agent 栈、后端模式、多 profile 组合、安全、连接器与集成的要点。
- 组件怎么装、怎么配：[组件目录](/components/)；编程模型与 API：[SDK](/sdk/)；命令与清单字段：[CLI 与规格](/cli/)。
