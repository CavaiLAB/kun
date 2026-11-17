# AGENTS.md：给编码代理的入口说明

这个仓库是 **Kun** 的新家：匠心智能体平台的企业级后端框架（.NET 10）。**目前只有框架级文档，没有代码。**

## 要改代码

迁移已于 2026-11-17 启动，分批的做法见[迁移执行计划](docs/migration-plan.md)。

历史导入的那一批（计划里的 B1）合入之前，代码仍在两个原仓库里，去那边改，按那边的 `AGENTS.md` 办：

- 原 Kun 框架：`idadawn/kun`
- 原 Qian 平台的后端：`idadawn/qian`

除迁移批次的 PR 之外，不要在本仓库里新建代码目录，也不要把原仓库的代码复制过来。迁移批次由计划指定的执行会话做：一批一个 PR，用合并提交合入，不压缩、不变基，否则导入的历史会丢。

目标仓库已定为本仓库（产品负责人，2026-11-17）。合并蓝图里写的 `idadawn/kun` 以[迁移与归并](docs/migration.md)第 6 节与迁移执行计划为准。

## 要改文档

1. 先读 [README.md](README.md) 与平台总览仓库的[名称对照](https://github.com/CavaiLAB/jiangxin/blob/main/docs/glossary.md)。「Qian」换了所指：以前指基于 Kun 的平台层，现在只作前端框架的名字；原来叫 Qian 的后端并入 Kun。组件 id 前缀 `qian.`、清单文件名这些运行期字面当作历史字面保留；`qian` 命令行与 `@qian/*` 导入名本次不改。
2. 合并蓝图是 `idadawn/qian` 主干上的 `docs/adr/ADR-023-kun-qian-merge-blueprint.md`，2026-11-17 接受，裁定记在同一仓库的 `pm/tasks/S1-DEC-036.md`（D1–D14）。写到归并的细节时以它为准，蓝图里的条目按已定写。被之后的裁定改写的几项（目标仓库、仓库划分、包的作用域、总名）见[迁移与归并](docs/migration.md)第 6 节。
3. 文档用中文，不写参考平台和对标项目的名字。
4. 分清「已定」「提议」「待定」。说现状的时候写清是「已有」（在原仓库里已经实现）还是「规划」。
5. 平台级的口径（名称、三个框架的分工、Agent 协同）在 [jiangxin](https://github.com/CavaiLAB/jiangxin) 里改，这里只引用。
6. 在 `docs/` 下新增文档，要在 `docs/_meta.json` 里加上它的文件名（不带扩展名）。这个文件定这些文档在平台文档站侧栏里的顺序；漏列的话，文档站升级到这个版本时构建会报错。
7. 改动走分支和 PR，不直接推 `main`。建仓期间的初始文档除外，见平台总览的[协作约定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/07-governance.md)第 2 节。
