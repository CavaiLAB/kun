---
id: S1-DOCS-008
title: 现行文档移除旧名字的描述（CavaiLAB/kun）：前端旧名、旧仓库地址、旧的来源说法与讲改名经过的句子
status: done
stage: S1
area: DOCS
wave: 4
priority: P1
type: doc
owner: "claude"
estimate: 1d
depends: []
refs: []
updated: 2026-11-19
---

## 背景

产品负责人 2026-11-19 要求在四个公开仓库里移除讲旧名字的描述，本任务只做 CavaiLAB/kun。要移除四类：前端旧名；旧仓库地址，包括文字和链接；「原 Kun」「原 Qian」这类来源说法；讲改名经过的句子。范围是现行文档：仓库根的说明文件、`docs/`、模块目录里的说明文件，以及构建文件注释与包元数据里的旧仓库地址。决策记录 `docs/adr/**`、迁移执行计划 `docs/migration-plan.md` 与迁移指南 `docs/migration.md` 保留；过渡目录 `_import/**`、`.github/originals/**`、提交历史与源码里带旧名的标识不动；`.github/workflows/**` 由 B3 的 PR-3（S1-KUN-020）按同一规矩改。

## 验收

- [x] 现行文档里不再出现前端旧名与旧仓库地址；指向历史文件的链接改成本仓库里同一提交的固定链接，链接指向的文件在那个提交里存在
- [x] 旧的来源说法改成路径或现在的名字，讲改名经过的句子删掉或改写；只改措辞，不改意思，表格与目录说明和代码现状一致
- [x] `src/Directory.Build.props` 的项目地址与仓库地址改指本仓库
- [x] 改完按四类说法再扫一遍，剩下的每一处在 PR 正文里按「保留 / 不动」说明
- [x] `python3 scripts/check_docs.py` 与 `python3 scripts/pm.py validate` 通过，PR 的 CI 全绿
- [x] 只改文字和链接：文件名、目录名与文档站路由都不改；名字里带旧名的，当作带旧名的标识，跟以后的批次一起改

## 进度日志

- 2026-11-19 claude：创建任务
- 2026-11-19 claude：开工：扫出 24 个文件，逐处改写
- 2026-11-19 claude：转 review：CavaiLAB/kun#28 开出。23 个文件只改措辞，包元数据与 KUN0002 的帮助链接改指本仓库；固定链接逐个核对过，剩余命中按保留 / 不动写在 PR 正文。验收前四条勾上，第五条里 check_docs 与 validate 已过，CI 结果待补
- 2026-11-19 claude：补两件，都是计划会话提的。一是只改文字和链接、文件名、目录名与路由不改：本 PR 没有改名或搬动；5 个标题锚点随标题变（AGENTS.md 讲约定的那一节，CHANGELOG 4 个带旧 PR 编号的标题），查过 CavaiLAB/qian、CavaiLAB/jiangxin 与 GitHub 代码搜索，没有外部引用。二是 AGENTS.md 指向平台总览 docs/glossary.md 的链接文字改为「术语表」，跟 CavaiLAB/jiangxin#21 改过的页面标题一致，地址不变
- 2026-11-19 claude：置 done，完成：产品负责人 2026-11-19 发话，与 S1-DOCS-009 一并置 done。CavaiLAB/kun#28 于 2026-11-18 17:18 UTC 合入主干 831bbac998。验收第 5 条勾上：PR 头 4c9a83e089 上 35 项检查全部通过，其中 `qian-docs-and-pm` 的 check 作业跑 `python3 scripts/check_docs.py` 与 `python3 scripts/pm.py validate`。另记一笔：合入后主干上那次运行（44357152614）里，测试分片 a 有一条调度器接管用例失败（`SchedulerTakeoverTests` 里讲正常停止后由另一个 worker 接管的那条，断言期望 2、实得 3）；本 PR 没有碰调度器，同一分片在 PR 头与下一个主干提交 daf1bbd2ac 上都通过，按偶发看，这条用例此前的偶发记在 S1-QA-023
