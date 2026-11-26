---
id: S1-DOCS-011
title: 文档站拿掉前端框架 Qian 的组件页：不再生成组件页与演示，旧地址落到说明页，站名、首页、llms.txt 与测试跟着改
status: review
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

- [x] 文档站不再生成 Qian 的组件页（基础组件、设计令牌、表单、数据、外壳、业务选择器、通用 API 与继承属性页），也不再带只给它们用的演示、离线编辑器与组件交互的浏览器用例；构建、类型检查与单测不再检出 `CavaiLAB/qian`
- [x] 旧地址不变成 404：`/components/qian/` 下原来的 91 页与 `/components/yue/` 下的旧书签，HTML 与 Markdown 两种地址都落到 `/components/qian/` 的说明页；说明页写明前端框架 Qian 的仓库已归档、组件文档在归档仓库的哪个标签里；产物检查与浏览器用例逐个核对
- [x] 站名、首页、组件目录、产品族页、llms.txt 的分节与读法、版本矩阵的说明跟着改到与事实一致；知识库索引与站点地图随页面变化；单测与浏览器用例跟着改
- [x] 站里别的与 Qian 有关的内容不动（见背景的边界）；拿不准的列进交接说明
- [x] 记录类只加带日期的标注或追加小节：`docs/prd/specs/docs-site.md` 里讲组件页的地方与修订历史；现行说明照实改：`website/README.md`、`website/AGENTS.md`、仓库 README 讲文档站的地方、文档站工作流
- [x] 门禁：`python3 scripts/check_docs.py`（含生成目录那一遍）、`python3 scripts/pm.py validate`、门禁自测；`website/` 的 typecheck、test、generate 与 build、check、e2e 通过；记下拿掉前后的页数、产物大小与构建耗时

## 进度日志

- 2026-11-26 claude：创建任务
- 2026-11-26 claude：开始：先构建一遍拿掉之前的站点，记下页数、产物大小、构建耗时与 91 个旧地址
- 2026-11-26 claude：转 review：组件页、同源演示、离线编辑器与组件交互的浏览器用例不再生成，构建、类型检查与单测不再检出 CavaiLAB/qian；/components/qian/ 留一页说明，原来的 90 页与 /components/yue/ 的 91 个旧书签（HTML、Markdown 各一）都落到说明页，产物检查与浏览器用例核对；站名改成「Kun 开发文档」，首页、组件目录、产品族页、llms.txt 的读法、版本矩阵的说明跟着改；说明文档照实改，docs-site.md 只加标注。拿掉前后：知识库 780 → 690 页，产物 103.7 → 69.2 MB，热构建约 15 → 8 秒，浏览器用例 210 → 8。check_docs（含生成目录）、pm validate、门禁自测 191 个、站点 typecheck、test 97 个、build、check、e2e 本地通过，带 PR 前缀的构建也通过。另有一处：scripts/ci_changes.py 的 ui 区域还列着已删掉的 website/demo/**，区域表自测会报它，不在本任务改的文件里，合并前要删掉这一项
- 2026-11-26 claude：写交接说明前复查，补改一处：版本矩阵页开头还写着「Qian 组件参考从前端仓库对应的发布标签生成」，删掉这一句，单测加断言；站点 typecheck、test 97 个、build、check、e2e 与 check_docs（含生成目录）、pm validate 重跑通过
- 2026-11-26 claude：复核后补两处：scripts/ci_changes.py 的 ui 区域删掉已经没有文件的 website/demo/** 一项，区域表自测恢复全过；前端 SDK 页讲组件界面的那一句不再说本站有组件演示，改为指到 /components/qian/ 的说明页。这一句是拿掉组件页直接造成的不实，照实改；前端 SDK 页的其余内容与背景里列的其余各处照旧不动
