---
id: S1-DOCS-010
title: Kun 文档站第一步：盘点与结构提议，写上手、写一个组件、在 .NET 里接权限三篇，llms.txt 加给大模型的读法与入口
status: doing
stage: S1
area: DOCS
wave: 4
priority: P1
type: doc
owner: "claude"
estimate: 2d
depends: [S1-DEC-043]
refs:
  - pm/tasks/S1-DEC-043.md
  - docs/prd/specs/docs-site.md
  - docs/adr/ADR-022-rspress-unified-component-docs.md
  - pm/tasks/S1-DOCS-002.md
updated: 2026-11-26
---

## 背景

产品负责人 2026-11-26 定的 P82（本仓库的登记见 S1-DEC-043 D4）：Kun 要有单独的网站和使用文档，方便其他人使用，并有给大模型用的 llms.txt 这类文档，方便调用。这一步盘点现有文档站离别人能照着用还差什么，写第一批使用文档，并让 llms.txt 能把大模型带到怎么调用。对外托管的地址、前端框架组件页的去留、llms.txt 还要补什么，产品负责人没有定，本任务只单列问题，不定。

## 验收

- [ ] 盘点现有文档站：页面与来源、新人照着能走到哪一步、每项使用文档的现状；读者看得到的部分写成指南页的「使用文档」表，已有的写明核对日期与 Kun 版本，写不了的写现状与入口
- [ ] 写三篇使用文档：上手（只用 nuget.org 上的内核包，不要令牌）、写一个组件、在 .NET 里接权限。每一步用 2026.11.24 的已发布包、在干净的包缓存里照着跑通，页里的输出是实测的；卡住的地方写进页面；没实现的能力只写现状与入口
- [ ] 开始页、指南页与「30 分钟上手」加上三篇的入口，侧栏顺序跟着改
- [ ] llms.txt 在标题与一句话定位之后加给大模型的读法与「从这里开始」一节，列的页站里没有即构建失败，有单测；docs-site.md 追加 §5.2，原文不改；网站 README 跟着改
- [ ] 结构提议与要产品负责人定的事单列，各给选项、影响与建议，本任务不定：对外托管的地址、前端框架组件页的去留、llms.txt 还要补什么
- [ ] `python3 scripts/check_docs.py`（含生成目录）、`python3 scripts/pm.py validate`、`pnpm --filter website typecheck`、`test`、`build`、`check`、`e2e` 在本地通过

## 进度日志

- 2026-11-26 claude：创建任务
- 2026-11-26 claude：开工：先盘点现有页面与来源，三篇使用文档逐步在本机用已发布包跑通再写
