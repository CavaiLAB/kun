---
id: S1-DOCS-012
title: 使用文档的代码做成示例：平时跟着源码编译并运行，每次发版用刚发布的包照文档再跑一遍
status: review
stage: S1
area: DOCS
wave: 4
priority: P1
type: infra
owner: "claude"
estimate: 2d
depends: [S1-DOCS-010]
refs:
  - website/docs/start/getting-started.md
  - website/docs/start/guides/write-a-component.md
  - website/docs/start/guides/authorization-dotnet.md
  - pm/tasks/S1-DOCS-010.md
  - docs/prd/specs/docs-site.md
updated: 2026-11-26
---

## 背景

产品负责人 2026-11-26 定：给大模型的入口先补「使用文档的代码收进仓库，每次发版编译并跑一遍」，并说这个入口最终是给 DSH（平台的 Agent 运行时）用的；四条网站裁定的登记见 S1-DEC-047。目的是让文档站「开始」里的三篇使用文档（上手、写一个组件、在 .NET 里接权限）一直照着能跑，页面上的代码和输出不随版本走样。

本任务把三篇里读者照着敲的代码做成仓库里的示例：平时跟着当前源码编译并运行，发版时用刚发布的包、照页面上的装包方式再跑一遍，并保证页面上的代码块与示例文件一致。

## 验收

- [x] 三篇使用文档里读者照着敲的代码收进仓库的示例目录；示例的说明写清放在这里的理由、怎么跑、文档里的版本号怎么跟着发版走
- [x] 页面上的代码块与示例文件逐字一致，三篇页面里的 Kun 版本号处处相同；有比对检查和它的自测，选这种做法的理由写在示例说明里
- [x] 平时：CI 用当前源码按占位版本打出示例要装的包，照页面上的命令建项目、装包、编译、运行，核对页面贴出的状态码、响应体的形状与错误码；接口改了，示例在 CI 里失败
- [x] 发版时：release.yml 在包发布之后，用刚发布的版本、照页面上的装包方式再跑一遍（内核档从 nuget.org；平台档用发布页的包源映射，读包令牌照现有做法取，不写进文件）；失败不撤回已经完成的发布，在运行里报出来
- [x] ci.yml 的作业、scripts/ci_changes.py 的区域表和它们的自测照现有模式改；「CI 汇总」的名字与作业 id 不变
- [x] 示例在本机用 .NET SDK 10 跑通；check_docs、pm validate、门禁自测、scripts/tests 本地通过

## 进度日志

- 2026-11-26 claude：创建任务
- 2026-11-26 claude：开工：先把三篇页面里的代码与命令逐条对上，做成示例并在本机跑通，再接 CI 与发版工作流
- 2026-11-26 claude：转 review：三篇使用文档里读者写进文件的代码收进 samples/docs，scripts/docs_examples.py 逐字比对页面并照页面建项目、装包、运行、核对输出；ci.yml 加作业「使用文档示例」与区域 docs-examples（用当前源码按占位版本打本地包），release.yml 在发布之后用刚发布的版本照页面再跑一遍，不挡发布；页面里的版本号发版后用 bump 改。本机两种模式三篇都跑通；check_docs、pm validate、门禁自测、scripts/tests、站点 typecheck、test、generate、build、check 通过。两个工作流没有在 GitHub 上真跑过
