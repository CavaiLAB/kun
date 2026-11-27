---
id: S1-DOCS-016
title: 给开发者与 Agent 的入口（Kun 这一侧）：文档、MCP 与命令行，让人找得到 Kun 的接口与能跑的示例
status: todo
stage: S1
area: DOCS
wave: 3
priority: P1
type: doc
owner: ""
estimate: 3d
depends: []
refs:
  - docs/package-and-docs-disposition.md
  - docs/agent-support.md
  - website/docs/sdk/ai/mcp.md
updated: 2026-11-27
---

## 背景

产品负责人 2026-11-27 发来的新方向（登记在 [S1-DEC-051](S1-DEC-051.md)）里有「前后端都要有面向开发者和 Agent 的文档、MCP 和 CLI 入口，让人能找到匠心组件、Kun 接口和可运行的示例。」本任务做 Kun 这一侧：让人和 Agent 找得到 Kun 的接口与能跑的示例。匠心组件那一侧在匠心仓库里做。

现在已经有的（对着 2026-11-27 的主干读的）：

- 给大模型读的：网站构建时生成 `llms.txt` 与 `llms-full.txt`，带给大模型的读法与「从这里开始」一节，每页另有 `.md`（[S1-DOCS-003](S1-DOCS-003.md)、[S1-DOCS-010](S1-DOCS-010.md)）。先不接进 DSH，等 Web 端的工作区服务（第二张表第 7 条，照建议，[S1-DEC-050](S1-DEC-050.md)）。
- 能跑的示例：网站「开始」里三篇使用文档的代码在 `samples/docs/`，平时跟着源码编译运行，发版时用刚发布的包再跑一遍（[S1-DOCS-012](S1-DOCS-012.md)）；`samples/` 下还有目录、订单与参考应用几个示例。
- 公开契约：`contracts/identity/v1/` 的目录查询与新增成员，`plugins/qian.tenancy`、`plugins/qian.tenancy.signup`、`plugins/qian.registry` 的 `contracts/http-v1.openapi.json`，`contracts/v0/` 的错误、事件信封与能力目录的 Schema。登录与会话的接口还没有进契约（[S1-IDP-031](S1-IDP-031.md)）。
- 能力目录：运行时的 `GET /api/capabilities` 与 `GET /api/capabilities/openapi.json`，列出已装组件的能力并聚合成 OpenAPI。
- 命令行：`qian`（`packages/qian-cli`），没有发布到 npm，要从本仓库构建（网站「写一个组件」一篇写了怎么构建）。
- MCP：组件可以带 `.mcp.json` 表面，随组件安装；本仓库自己没有 MCP 服务器。网站 MCP 一页写的「文档检索 MCP」是 S1 可选，没有做；每租户的能力门面排在 S4。

本任务先把上面这些整理成一个入口（网站上的一页，`llms.txt` 里对应一节），再列出缺什么、各自建任务。整理时看到的缺口，开工时核对：

- 契约散在 `contracts/` 与各组件的 `contracts/` 下，没有一页总目录；网站上没有接口参考（排在 [S1-DOCS-002](S1-DOCS-002.md) 之后，S1-DEC-047 D3 的第④项）。
- 命令行不发包，外面的人与 Agent 拿不到；要不要发到 npmjs，与第一张表第 41 条（以后的客户端包一做出来就公开）怎么对上，要问产品负责人。
- 给 Agent 用的 MCP 入口：本仓库要不要出一个（文档检索，或者把能力目录与契约暴露成工具），与 ADR-024 提议七、Web 端的工作区服务怎么对上，要先出方案。

估时是整理时的估计。

## 验收

- [ ] 网站上有一页给开发者与 Agent 的入口：Kun 的公开契约总目录、能力目录接口、能跑的示例、命令行怎么拿到，`llms.txt` 里有对应的一节
- [ ] 缺口逐项列清，每项有任务或进了要产品负责人定的表；命令行发不发包、MCP 入口出不出，不在本任务里定
- [ ] 与匠心仓库里讲匠心组件的文档互相指到（整理时的提议）

## 进度日志

- 2026-11-27 claude：创建任务
