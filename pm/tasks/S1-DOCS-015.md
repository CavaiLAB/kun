---
id: S1-DOCS-015
title: 网站的导航：照去向表改顶栏与「产品」菜单，前端 SDK 移出主导航，加给开发者与 Agent 的入口
status: todo
stage: S1
area: DOCS
wave: 3
priority: P1
type: doc
owner: ""
estimate: 1d
depends: [S1-DOCS-014]
refs:
  - docs/package-and-docs-disposition.md
  - website/docs/_nav.json
updated: 2026-11-27
---

## 背景

产品负责人 2026-11-27 发来的新方向（登记在 [S1-DEC-051](S1-DEC-051.md)）要整理「站点导航」，还要「前后端都要有面向开发者和 Agent 的文档、MCP 和 CLI 入口，让人能找到匠心组件、Kun 接口和可运行的示例。」本任务照[去向表](../../docs/package-and-docs-disposition.md)第 7.1 节改 Kun 网站的导航。

- 顶栏（`website/docs/_nav.json`）与各部分的侧栏（`_meta.json`）：前端 SDK 归档以后移出主导航；「产品」菜单里指到旧外壳的「构建应用」改指上手与写一个组件；要不要整组重排，开工时给出方案。
- 加一个给开发者与 Agent 的入口，指到 [S1-DOCS-016](S1-DOCS-016.md) 整理出来的接口、契约、能跑的示例与命令行；让人找得到匠心组件的文档与入口在匠心仓库，两边的入口互相指到，是整理时的提议。
- 「设计与路线」镜像 `docs/` 时，根目录文档的侧栏名字写在 `website/scripts/sync-specs.ts` 的 `DESIGN_ROOT_FILES`；去向表要不要在那里给一个名字，随这次一起定。
- 「开始」里「上手」与「30 分钟上手」怎么分工，一并提。
- 对外的域名与入口是 [S1-DOCS-013](S1-DOCS-013.md)，不在本任务。

估时是整理时的估计。

## 验收

- [ ] 顶栏与侧栏照去向表第 7.1 节改完，前端 SDK 不在主导航里，旧地址有落点
- [ ] 有一个给开发者与 Agent 的入口，指到 Kun 的接口、契约、能跑的示例与命令行；指到匠心组件的文档（整理时的提议）
- [ ] 网站的构建、检查（死链、锚点）与浏览器用例通过；`llms.txt` 的「从这里开始」一节跟着改
- [ ] 去向表第 7.1 节照实改写

## 进度日志

- 2026-11-27 claude：创建任务
