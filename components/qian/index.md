> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# 前端框架 Qian 的组件页已从本站拿掉

本站 2026-11-26 起不再收录前端框架 Qian 的组件页（产品负责人定，登记在任务 [S1-DEC-047](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-047.md) D2）。原来 `/components/qian/` 下的 91 页，连同更早的 `/components/yue/` 书签（91 个），现在都落到这一页。

前端框架 Qian 的仓库 [CavaiLAB/qian](https://github.com/CavaiLAB/qian) 2026-11-25 已归档，只读，不再发新版；已发布的 `@cavailab/qian-*` 留在 GitHub Packages，继续可以安装。

## 组件文档在哪里

原来的组件页是构建时从归档仓库的标签 `@cavailab/qian-core@0.2.1`（提交 `e4becd2e1c3c`）生成的，源都在这个标签里：

| 原来的页面                  | 原来的地址                                                                                           | 归档仓库里的源                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| ---------------------- | ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 基础组件（52 个入口）与通用上下文 API | `/components/qian/core/<组件名>`、`/components/qian/core/shared-api`                                | [packages/qian/core/docs/](https://github.com/CavaiLAB/qian/tree/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/core/docs)：用途说明 `catalog.json`、示例 `examples/catalog.tsx`、状态清单 `scenarios.json`；属性表取自 [packages/qian/core/src/](https://github.com/CavaiLAB/qian/tree/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/core/src) 的 TypeScript 类型                                                                                                                                                            |
| 继承的 DOM、ARIA 与事件属性     | `/components/qian/api/native-<编号>`                                                              | 同上，取自组件属性继承的 React 类型声明                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| 设计令牌                   | `/components/qian/tokens`                                                                       | [packages/qian/tokens/](https://github.com/CavaiLAB/qian/tree/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/tokens)                                                                                                                                                                                                                                                                                                                                                                                             |
| 表单与弹层、数据组件、外壳与认证、业务选择器 | `/components/qian/form`、`/components/qian/data`、`/components/qian/shell`、`/components/qian/biz` | 各包的 `docs/index.mdx`：[form](https://github.com/CavaiLAB/qian/blob/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/form/docs/index.mdx)、[data](https://github.com/CavaiLAB/qian/blob/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/data/docs/index.mdx)、[shell](https://github.com/CavaiLAB/qian/blob/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/shell/docs/index.mdx)、[biz](https://github.com/CavaiLAB/qian/blob/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/packages/qian/biz/docs/index.mdx) |

包名、包源与令牌的配置见归档仓库的[安装与升级说明](https://github.com/CavaiLAB/qian/blob/e4becd2e1c3c2ff471a6bfce671f86b0ef0df786/docs/migrating-from-yue.md)。

## 本站还有哪些与它有关

- [前端 SDK](/sdk/frontend/index.md)：旧外壳的插件树、远程模块与插槽。
- [宿主与框架](/start/hosts.md)：旧外壳的归档源码。
- [前端框架规格](/design/prd/specs/frontend.md)：设计与阶段规划。
- [版本矩阵](/start/versions.md)：本仓库钉住的 `@cavailab/qian-*` 版本。

本仓库里的旧外壳、三个界面包与参考应用的远程模块还在用 Qian 已发布的包；本站的主题也用它的令牌包 `@cavailab/qian-tokens`。
