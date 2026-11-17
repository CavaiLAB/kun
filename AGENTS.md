# AGENTS.md：给编码代理的入口说明

这个仓库是 **Kun** 的新家：匠心智能体平台的企业级后端框架（.NET 10）。**两个原仓库的代码已带历史导入过渡目录 `_import/`，原仓库已归档为只读；过渡目录里的内容在本仓库改，正按迁移执行计划分批整理、并轨与归并。**

## 要改代码

迁移已于 2026-11-17 启动，分批的做法见[迁移执行计划](docs/migration-plan.md)。历史导入（计划里的 B1）把两个原仓库的 `main` 带历史导入了两个过渡目录，原提交的 SHA 原样保留：

| 目录 | 内容 | 怎么构建与测试 |
|---|---|---|
| `_import/kun/` | 原 Kun 框架（`idadawn/kun`） | 按 `_import/kun/CLAUDE.md` 与 `_import/kun/AGENTS.md`，命令在 `_import/kun/` 下运行 |
| `_import/qian/` | 原 Qian 的整棵树（`idadawn/qian`）：.NET 包、前端工作区、文档站、任务看板 | 按 `_import/qian/AGENTS.md`，命令在 `_import/qian/` 下运行 |

两棵树各用各的构建基线（`global.json`、`Directory.*.props`、`NuGet.config`、pnpm 工作区），脚本都把自己所在的目录当作根。仓库根上没有构建文件，不要在仓库根上直接跑 `dotnet` 或 `pnpm`。

两边的检查接在仓库根的 `.github/workflows/` 里：`kun-*.yml` 对应原 Kun 的，`qian-*.yml` 对应原 Qian 的，都在各自的过渡目录里运行。过渡目录里的 `.github/` 是原件，GitHub 不读它。每个根工作流的开头写着它对应哪个原件、相对原件改了什么；没有接的工作流与原因列在 B1 的 PR 里。

原仓库归档之后的规矩：

- 两个原仓库 2026-11-17 冻结，【待写实：归档日期】归档为只读。最后一次同步的来源头是 `idadawn/kun` 的 【待写实：原 Kun 最终头】 与 `idadawn/qian` 的 【待写实：原 Qian 最终头】，`_import/kun/`、`_import/qian/` 在同步时与这两个头的树逐字节相同。原 Kun 与原 Qian 的源码、文档与任务文件从此以本仓库为准，原仓库不再接受改动。
- 过渡目录里的内容从此在本仓库改，在本仓库开 PR。改的是文件内容；过渡目录本身的搬动与拆分属于后面的批次（见最后一条），不要顺手做。
- 两份 `AGENTS.md` 里构建、测试与代码约定的部分照用。任务文件照原 Qian 的 `pm/README.md` 改：任务文件在 `_import/qian/pm/tasks/` 下，`pm.py` 在 `_import/qian/` 下运行（如 `cd _import/qian && python3 scripts/pm.py validate`），`set` 与 `add` 要带 `--by`，生成物不进 git。迁移任务（S1-KUN-018–022）的文件也在这里改（迁移执行计划第 3 节第 7 条）。两份 `AGENTS.md` 里讲提 PR、发版、发布文档站的部分说的是原仓库，在本仓库以本文件与迁移执行计划为准。
- 两个原仓库冻结时写的去处说明随最后一次同步带了进来，在本仓库不适用：`_import/kun/` 的 `README.md`、`AGENTS.md`、`CLAUDE.md` 开头那一段，`_import/qian/` 的 `README.md` 开头那一段与 `AGENTS.md` 的「本仓库已冻结」一节。原 Kun 的自管任务队列（`_import/kun/.autonomy/`）与第三方生成的代理配置（如 `_import/kun/.claude/CLAUDE.md`）按合并蓝图在 B2 的整理里删除，在那之前也不要照着用。
- 归前端仓库 `CavaiLAB/qian` 的路径在这里只读，改前端去那边改：`_import/qian/packages/yue/`、`_import/qian/apps/yue-playground/`、`_import/qian/packages/qian-plugin-sdk/`、`_import/qian/packages/qian-runtime/`、`_import/qian/tests/gates/yue/`、`_import/qian/scripts/yue-visual.mjs`、`_import/qian/.changeset/`，以及原 Qian 的 yue、runtime-ts、runtime-ts-release、plugin-sdk-release 四个工作流（仓库根上对应 `qian-yue.yml` 与 `qian-runtime-ts.yml`，两个发布的没有接）。它们在 B11 由前端计划的执行会话删除。外壳应用与外壳工具包、三个界面包、参考应用的远程模块、文档站、命令行、声明式表面的校验库留在本仓库。
- 一批一个 PR。本仓库的 PR 一律用合并提交合入，不压缩、不变基，否则导入的历史会丢。除迁移批次的 PR 之外，不要在过渡目录之外新建代码目录，也不要把代码搬出过渡目录。
- 不要抢跑：B2 余下的几步——修绿原 Kun 的检查、看板与文档站改指本仓库、整理（原 Kun 提到仓库根、改写历史 PR 编号与地址、统一许可证等）——由计划指定的执行会话做；构建并轨是 B3；包名、命名空间与类型名的改名是 B4–B10。

目标仓库已定为本仓库（产品负责人，2026-11-17）。合并蓝图里写的 `idadawn/kun` 以[迁移与归并](docs/migration.md)第 6 节与迁移执行计划为准。

## 要改文档

1. 先读 [README.md](README.md) 与平台总览仓库的[名称对照](https://github.com/CavaiLAB/jiangxin/blob/main/docs/glossary.md)。「Qian」换了所指：以前指基于 Kun 的平台层，现在只作前端框架的名字；原来叫 Qian 的后端并入 Kun。组件 id 前缀 `qian.`、清单文件名这些运行期字面当作历史字面保留；`qian` 命令行与 `@qian/*` 导入名本次不改。
2. 合并蓝图是 `idadawn/qian` 主干上的 `docs/adr/ADR-023-kun-qian-merge-blueprint.md`，2026-11-17 接受，裁定记在同一仓库的 `pm/tasks/S1-DEC-036.md`（D1–D14）。写到归并的细节时以它为准，蓝图里的条目按已定写。被之后的裁定改写的几项（目标仓库、仓库划分、包的作用域、总名）见[迁移与归并](docs/migration.md)第 6 节。
3. 文档用中文，不写参考平台和对标项目的名字。
4. 分清「已定」「提议」「待定」。说现状的时候写清是「已有」（在原仓库里已经实现）还是「规划」。
5. 平台级的口径（名称、三个框架的分工、Agent 协同）在 [jiangxin](https://github.com/CavaiLAB/jiangxin) 里改，这里只引用。
6. 在 `docs/` 下新增文档，要在 `docs/_meta.json` 里加上它的文件名（不带扩展名）。这个文件定这些文档在平台文档站侧栏里的顺序；漏列的话，文档站升级到这个版本时构建会报错。
7. 改动走分支和 PR，不直接推 `main`。建仓期间的初始文档除外，见平台总览的[协作约定](https://github.com/CavaiLAB/jiangxin/blob/main/docs/07-governance.md)第 2 节。
