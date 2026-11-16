# AGENTS.md：给编码代理的入口说明

这个仓库是 **Kun** 的新家：匠心智能体平台的企业级后端框架（.NET 10）。**目前只有框架级文档，没有代码。**

## 要改代码

代码还在两个原仓库里，去那边改，按那边的 `AGENTS.md` 办：

- 原 Kun 框架：`idadawn/kun`
- 原 Qian 平台的后端：`idadawn/qian`

不要在本仓库里新建代码目录，也不要把原仓库的代码复制过来。代码按合并蓝图分批迁入；蓝图整体确认之前，不动代码和仓库结构。

## 要改文档

1. 先读 [README.md](README.md) 与平台总览仓库的[名称对照](https://github.com/CavaiLAB/jiangxin/blob/main/docs/glossary.md)。「Qian」换了所指：以前指基于 Kun 的平台层，现在只作前端框架的名字；原来叫 Qian 的后端并入 Kun。组件 id 前缀 `qian.`、清单文件名、`qian` 命令行这些字面保留不改。
2. 合并蓝图是 `idadawn/qian` 仓库的 ADR-023。写到归并的细节时以它为准；它里面标了已裁的条目可以写成已定，其余按提议写。
3. 文档用中文，不写参考平台和对标项目的名字。
4. 分清「已定」「提议」「待定」。说现状的时候写清是「已有」（在原仓库里已经实现）还是「规划」。
5. 平台级的口径（名称、三个框架的分工、Agent 协同）在 [jiangxin](https://github.com/CavaiLAB/jiangxin) 里改，这里只引用。
6. 改动走分支和 PR，不直接推 `main`。
