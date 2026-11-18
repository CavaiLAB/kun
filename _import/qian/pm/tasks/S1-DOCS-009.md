---
id: S1-DOCS-009
title: 文档站改指本仓库：仓库链接与「Kun 框架」小节的来源，清掉同批页面里旧名字的描述
status: doing
stage: S1
area: DOCS
wave: 4
priority: P1
type: doc
owner: "claude"
estimate: 1d
depends: []
refs:
  - pm/tasks/S1-KUN-019.md
  - docs/prd/specs/docs-site.md
updated: 2026-11-19
---

## 背景

迁移执行计划 B2 剩下的文档站两项（S1-KUN-019）：页面上的仓库链接改指本仓库；「Kun 框架」小节改从本仓库取同名标签。两个旧仓库的历史与版本标签已带着原提交号导入本仓库，同名标签指向相同的提交，版本仍由 `Directory.Packages.props` 的 `KunVersion` 决定。顺带按产品负责人 2026-11-19 定的规矩，清掉 `website/` 下手写页、README、脚本与测试里旧名字的描述：前端旧名、旧仓库地址、「原 Kun」「原 Qian」这类来源说法、讲改名经过的句子。决策记录与历史文档、迁移指南、改名与导入工具保留；源码里带旧名的标识、文件名与文档站路由不动。范围是 `_import/qian/website/`，以及 `.github/workflows/qian-docs-site.yml` 里取 Kun 来源的几行。

## 验收

- [ ] `theme/site.ts` 的 `REPO_URL` 指本仓库，脚本拼出的 GitHub 地址与 `source` 带上 `_import/qian/` 前缀；页脚「Kun」一项指本仓库，市场仓库那条不写旧地址
- [ ] 「Kun 框架」小节与 Kun API 参考按 `KunVersion` 从本仓库的同名标签生成：本地默认读本仓库，缺标签时从 origin 只取这一个；CI 不再另外检出旧仓库
- [ ] 改动前后各生成一次「Kun 框架」小节比对，除链接地址外内容相同，有差别逐条写在 PR 正文
- [ ] 手写页、README、脚本与测试里指向两个旧仓库的文字和链接改掉；指向历史的改成本仓库里同一提交的固定链接
- [ ] 同批页面里旧名字的描述按规矩清掉，只改措辞、不改意思；改完再扫一遍，剩下的每一处在 PR 正文里说明为什么保留
- [ ] `pnpm --filter website test`、`python3 scripts/check_docs.py`、`python3 scripts/pm.py validate` 通过，PR 的 CI 全绿

## 进度日志

- 2026-11-19 claude：创建任务
- 2026-11-19 claude：开工：先给「Kun 框架」小节、Kun API 参考、设计与 CLI 镜像页拍改动前的基线，再改 site.ts、repo.ts、kun.ts 与工作流
