---
id: S1-DOCS-011
title: 文档站拿掉前端框架 Qian 的组件页：不再生成组件页与演示，旧地址落到说明页，站名、首页、llms.txt 与测试跟着改
status: doing
stage: S1
area: DOCS
wave: 4
priority: P1
type: doc
owner: "claude"
estimate: 1d
depends: [S1-DEC-047]
refs:
  - pm/tasks/S1-DEC-047.md
  - docs/prd/specs/docs-site.md
  - docs/adr/ADR-022-rspress-unified-component-docs.md
  - website/README.md
updated: 2026-11-26
---

## 背景

产品负责人 2026-11-26 定：前端框架 Qian 的组件页从站里拿掉（[S1-DEC-047](S1-DEC-047.md) D2）。这些页面在 `/components/qian/` 下，共 91 页，构建时从已归档的 `CavaiLAB/qian` 的标签检出源码生成，带同源的交互演示、离线编辑器与完整属性表。本任务落地这一条：组件页与只给它们用的演示不再生成，构建不再检出 Qian 仓库；旧地址落到一页说明上；站里讲到组件页的地方改到与事实一致。

边界：他答的是「91 个组件页」这一问。站里别的与 Qian 有关的内容不在这一问里，不动：前端 SDK 的页面、讲旧外壳的页面、首页页脚的仓库与许可链接、版本矩阵里的前端包、「设计与路线」里的前端框架规格；Kun 自己的旧外壳与界面包还在用 Qian 的包，文档站的主题也用它的令牌包。拿不准算不算的不动，列进交接说明等裁定。

## 验收

- [ ] 文档站不再生成 Qian 的组件页（基础组件、设计令牌、表单、数据、外壳、业务选择器、通用 API 与继承属性页），也不再带只给它们用的演示、离线编辑器与组件交互的浏览器用例；构建、类型检查与单测不再检出 `CavaiLAB/qian`
- [ ] 旧地址不变成 404：`/components/qian/` 下原来的 91 页与 `/components/yue/` 下的旧书签，HTML 与 Markdown 两种地址都落到 `/components/qian/` 的说明页；说明页写明前端框架 Qian 的仓库已归档、组件文档在归档仓库的哪个标签里；产物检查与浏览器用例逐个核对
- [ ] 站名、首页、组件目录、产品族页、llms.txt 的分节与读法、版本矩阵的说明跟着改到与事实一致；知识库索引与站点地图随页面变化；单测与浏览器用例跟着改
- [ ] 站里别的与 Qian 有关的内容不动（见背景的边界）；拿不准的列进交接说明
- [ ] 记录类只加带日期的标注或追加小节：`docs/prd/specs/docs-site.md` 里讲组件页的地方与修订历史；现行说明照实改：`website/README.md`、`website/AGENTS.md`、仓库 README 讲文档站的地方、文档站工作流
- [ ] 门禁：`python3 scripts/check_docs.py`（含生成目录那一遍）、`python3 scripts/pm.py validate`、门禁自测；`website/` 的 typecheck、test、generate 与 build、check、e2e 通过；记下拿掉前后的页数、产物大小与构建耗时

## 进度日志

- 2026-11-26 claude：创建任务
- 2026-11-26 claude：开始：先构建一遍拿掉之前的站点，记下页数、产物大小、构建耗时与 91 个旧地址
