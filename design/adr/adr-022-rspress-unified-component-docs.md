> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# ADR-022 统一 Rspress 组件文档与验收

> 本页由构建脚本镜像自仓库 [`docs/adr/ADR-022-rspress-unified-component-docs.md`](https://github.com/CavaiLAB/kun/blob/main/docs/adr/ADR-022-rspress-unified-component-docs.md)，请修改源文件，不要改本页。

> 本文保留当时的决策。方括号中的能力描述用于标出整理时替换的引擎或下游名称。

> 修订（2026-11-26，[S1-DEC-043](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-043.md) D1、D4）：前端框架 Qian 的仓库 2026-11-25 归档（D1）；产品负责人 2026-11-26 说 Kun 要增加单独的网站，放使用文档与给大模型用的文档（D4）。D22.1 的「唯一文档与演示体系」受影响，范围与没有定的见 §9。正文未改，D22.1 前加了标注。

> 修订（2026-11-26，[S1-DEC-043](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-043.md) D8）：平台文档站（`CavaiLAB/jiangxin`）照同一天的平台级裁定 P87 改成只放匠心自己的文档，不再聚本仓库的文档；本仓库的文档只在本仓库和本仓库的文档站里，见 §9 的最后一条。正文未改。

> 修订（2026-11-26，[S1-DEC-047](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-047.md)）：产品负责人 2026-11-26 答了 Kun 网站的四个问题：网站用他提供的域名部署到 k3s（D1）；前端框架的组件页从站里拿掉（D2）；给大模型的入口照建议补（D3）；平台档的包维持现状（D4）。D22.1 至 D22.3 受 D2 影响，见 §10。正文未改，这三条前加了标注。

## 1 背景与问题（Context）

S1-YUE-005 按原验收交付了 51 个组件入口、manifest 和 Storybook；平台文档已使用 Rspress 2.0.23。两套文档入口使使用者需要切换导航、搜索和预览。2026-10-14 产品负责人明确要求「统一使用 Rspress，不再 Storybook」，并要求先评估能力。本 ADR 记录这次已授权修订，不重新讨论其它前端选型。

## 2 决策（Decision）

- 【经 S1-DEC-043 D1、D4 标注：前端框架的仓库已归档；Kun 要增加单独的网站。站里前端框架的组件页怎么办没有定，见 §9】【经 S1-DEC-047 D2 标注：前端框架的组件页 2026-11-26 起从站里拿掉，`/components/qian/` 只留一页说明，原来的地址都落到这一页，见 §10；本条不改】**D22.1 唯一文档与演示体系**：Qian、Kun、［前端框架］使用现有 `website/` Rspress 站点。［前端框架］页面归 `/components/yue/`；按产品负责人补充的组件页参考组织预览、源码、安装、用法、组合结构、场景示例与 API，全部在现有 Rspress 一个入口中完成；移除 Storybook 应用、CSF、依赖、命令与 CI 入口，不保留内部 Storybook。组件文档源仍在组件目录，不手写生成目录。
- 【经 S1-DEC-047 D2 标注：本站 2026-11-26 起没有前端框架的组件页，本条说的在 Rspress 构建页面上做的操作断言与发布后复验，在本站没有了对应的页面，见 §10；本条不改】**D22.2 测试独立**：Rspress 承担文档和演示；Vitest 浏览器模式、SSR 测试、Playwright、axe 与 Lighthouse 承担原有质量门禁。共用普通 React 示例，迁移 Storybook 状态与交互覆盖；可交互组件必须在 Rspress 实际构建页面上完成操作与结果断言，发布后复验代表性交互，不把删除测试或仅渲染视为完成替换。
- 【经 S1-DEC-047 D2 标注：本站 2026-11-26 起不再生成前端框架的属性表，见 §10；本条不改】**D22.3 完整 API**：React Props 从真实 TypeScript 类型生成，清单的 JSON Schema 继续负责机器可序列化契约。使用插件或底层解析器时都必须有属性完整性检查，缺失函数、继承属性或组合部件即失败。配置、示例与迁移验收以 [文档站规范 §3.1](/design/prd/specs/docs-site.md#31-前端框架组件文档与演示) 为准。
- **D22.4 任务边界**：重新打开 S1-YUE-005，负责 core 文档接入、测试迁移与 Storybook 清理。S1-DOCS-002 继续依赖它，复用接线并完成通用收集器及平台、TS、.NET、HTTP API 文档；不得反向新增依赖造成循环。本 ADR 初次交付为评估与任务/规格修订；替换已于 2026-10-14 随 PR idadawn/qian#88、idadawn/qian#90、idadawn/qian#91 完成，S1-YUE-005 已完成正式站交互、离线编辑与 API 验收。

## 3 理由与能力评估（Rationale）

**结论：Rspress 能满足目标，但默认 API 插件和在线编辑器不能不经适配就作为验收结果。** 无需为了替换文档工具重写组件或更换现有测试框架。

评估日期为 2026-10-14，基线是本仓库 Rspress 2.0.23、React 19.3.0、TypeScript 6.0.3。npm 公开元数据确认 `plugin-preview`、`plugin-api-docgen`、`plugin-playground` 均存在 2.0.23；preview/playground 的 React peer 为 ≥18，core peer 为 ^2.0.10；api-docgen 的 TypeScript peer 为 ^6.0.3，与当前工作区匹配。

| 需求           | 证据与结论                                                                                                                                                       | 实施约束                                                                       |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| React 组件活演示  | 官方 preview 支持 MDX 与本地 TSX 文件；隔离试构建直接引用现有 Button、Dialog 成功，浏览器按钮计数 0→1、弹层打开/关闭通过                                                                             | 全量组件、Tailwind v4、Portal、明暗隔离仍需迁移验收；iframe 模式需独立配置样式与构建                     |
| 完整 Props 表   | 2.0.23 默认 API 表只显示 Button 的 className/testId，Dialog 根表为空；底层 react-docgen-typescript 2.4.0 能解析 variant/size/loading/disabled 与 open/defaultOpen/onOpenChange | 默认过滤会排除 node_modules 声明，并隐藏多数无注释属性；调整过滤、补源码 JSDoc，必要时直接用解析结果生成表，逐组件核对完整性   |
| 文档与机器输出      | 同一试构建成功生成 index.html、index.md、llms.txt、llms-full.txt；现有主站已有导航、搜索与主题                                                                                         | 对 API、代码块、演示说明检查 HTML/SSG-MD 一致；纯交互控件不得污染 Markdown                         |
| 在线编辑         | 官方 playground 支持浏览器编译，可预打包依赖；不支持任意本地模块导入，也不执行 TS 类型检查                                                                                                       | 示例独立 typecheck；显式登记允许的依赖，不能把任意源路径当成运行时模块                                   |
| 离线部署         | 官方 playground 默认 Babel 与 Monaco 使用 CDN，支持配置资源地址                                                                                                             | 编译器、编辑器、worker 资源同源托管并懒加载；断外网后实测编辑；本次未把该项记为已通过                             |
| a11y、交互、视觉测试 | 仓库已有独立 Vitest、Playwright、axe 和 Lighthouse；这些门禁不依赖文档框架                                                                                                       | 迁移原 126 条 Storybook 用例的语义与状态覆盖，保留现有 SSR/键盘/E2E；Rspress 构建页面及预览 iframe 加入测试 |

**试验方法**：在仓库外的临时 npm 工程安装上述固定版本的 core、preview、api-docgen；MDX 以 `file` preview 引用仓库真实 Button/Dialog 组合示例，API entries 指向组件源码并使用 core 的 tsconfig；执行 `rspress build`，静态服务器加载产物，检查点击/弹层及生成 Markdown；再直接调用相同版本 `react-docgen-typescript.withCustomConfig(...).parse(...)` 对比解析属性与默认表。检查发布包 `plugin-api-docgen/dist/index.js` 的 propFilter 和 generateTable 确认过滤原因。试验未修改组件源码，未验证全量 51 入口、最终主题样式、离线编辑器及部署；这些保持为任务未勾选项。

## 4 备选方案（Alternatives）

- **Rspress + Storybook 双入口**：原实现可复用测试，但不符合此次产品负责人要求，不再采用。
- **只用 Rspress、删除独立测试**：文档构建不能证明键盘、焦点、SSR 与可访问性，拒绝降低门禁。
- **由 manifest 直接代替 Props 表**：manifest 只描述可序列化属性，不能覆盖函数、ref 与 ReactNode，不能满足完整 API 文档要求。

## 5 后果与代价（Consequences）

统一读者入口与版本、搜索、主题、llms 产物。新增工作集中在示例接线、Props 完整性、在线编辑器本地资源、测试迁移和旧工具清理。全站仍遵守 R55 产物预算（最初 100 MB，S1-QA-010 于 2026-10-15 按 R55 调整为 120 MB），组件靶场的 Lighthouse 预算保持原口径；不能把编辑器打入每页首屏。

## 6 对各阶段的影响

- S1：YUE-005 回到 todo，保留已合并组件与 manifest 的验收记录；新增 Rspress 接入与清理验收，全部通过后才恢复 done。DOCS-002 复用成果，继续完成更广的文档管线。
- S2–S4：新增组件沿用普通 React 示例、Rspress 文档与独立测试，不再引入 CSF 或 Storybook。

## 7 待确认事项

工具统一方向已由产品负责人明确授权。Props 适配、离线编辑器和全量页面属于实施验收，不另设选型审批；若实测发现必须改变产品或安全边界的问题，再按任务流程记录具体决策。

## 8 参考

- [产品负责人指定的组件页参考](https://ui.shadcn.com/docs/components/aria/alert-dialog)：借鉴页面编排、可操作预览、对应源码与场景示例，示例实现使用本仓库 API。

- [Rspress 官方 preview](https://rspress.rs/zh/plugin/official-plugins/preview)：本地文件预览、iframe 独立构建与样式。

- [Rspress 官方 playground](https://rspress.rs/zh/plugin/official-plugins/playground)：编辑器、模块限制及 Babel/Monaco 配置。

- [Rspress 官方 api-docgen](https://rspress.rs/zh/plugin/official-plugins/api-docgen)：解析入口与 TypeScript 配置。

- [Rspress api-docgen 2.0.23 发布源码](https://unpkg.com/@rspress/plugin-api-docgen@2.0.23/dist/index.js)：本次验证的过滤行为。

- [原前端决策](/design/adr/adr-013-frontend-framework.md)、[文档站规范](/design/prd/specs/docs-site.md)、[YUE-005](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-YUE-005.md)、[DOCS-002](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DOCS-002.md)。

## 9 Kun 增加单独的网站（2026-11-26）

产品负责人 2026-11-26 说（[S1-DEC-043](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-043.md) D4；平台级裁定 P82，原文在 `CavaiLAB/jiangxin` 的 [2026-11-26 平台级裁定](https://github.com/CavaiLAB/jiangxin/blob/c7b32a4dea2d1455fd2b9835e1cc0848c9d036c2/docs/decisions/2026-11-26-platform.md)）：「Kun 增加单独网站增加使用文档和说明。方便后续其他人使用，增加给大模型使用的类似 llm.txt 这种文档。方便进行调用。」

- 已有：本仓库的 `website/` 就是 D22.1 说的那个站，构建时产出 llms.txt 与 llms-full.txt（[S1-DOCS-003](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DOCS-003.md)）。它只部署在内部，Kun 的文档和前端框架的组件页放在一起。
- 读法（整理者的读法，产品负责人没有逐句确认）：「增加单独网站」读作把这个站做成 Kun 自己的、别人打得开的站，补上讲怎么使用的文档；「类似 llm.txt 这种文档」读作 llms.txt 这一类给大模型读的入口。
- 与 D22.1 的关系：D22.1 让 Kun 与前端框架共用一个站。前端框架的仓库 2026-11-25 已归档，不再发新版（S1-DEC-043 D1；平台级裁定 P77）；站里它的组件页留着、移走还是撤掉，没有定。D22.2 至 D22.4 讲的测试、API 生成与任务边界不变。
- 没有定的：网站对外放在哪；已有的 llms.txt 够不够、还要补什么；使用文档先写哪些。
- 不变的：平台文档站在 `CavaiLAB/jiangxin`（[S1-DEC-038](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-038.md) D8），组件站其余的 API 参考与示例先不并进平台站（D43）。那个站以后还聚不聚本仓库的文档，是那边的提议，没有定。
- 看板上相关的任务（[S1-DOCS-002](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DOCS-002.md)、[S1-QA-006](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-QA-006.md)）没有动，没有新建任务。
- 平台文档站还聚不聚本仓库的文档，同一天稍后有了答案（S1-DEC-043 D8；平台级裁定 P87）：那个站改成只放匠心自己的文档，不再聚本仓库的文档，导航里的「Kun」先指向本仓库，等本仓库的网站有了对外的地址再改。本仓库的文档只在本仓库和本仓库的文档站里。上面「没有定」的那一句由这一条回答；网站对外放在哪，仍没有定。

## 10 Kun 网站的四条裁定（2026-11-26）

产品负责人 2026-11-26 答了 Kun 网站的四个问题（[S1-DEC-047](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEC-047.md)；四个问题、当时的选项与建议都记在那份任务里）。原话：

> 先确定 Kun 的网站
> 1.我提供域名，部署到 k3s
> 2.C，从站里拿掉。
> 3.按照你的建议把，最终肯定要给 DSH 使用的
> 4.A

第 3 条的「把」按「吧」读。下面的读法都是整理者的读法，产品负责人没有逐句确认。

- D1 网站对外：用他提供的域名，部署到 k3s。已定，没有实现：域名还没有给，站点照旧只在内部能打开。读法：k3s 是现在放着内部文档站的那个集群（`deploy/docs-site/`）；内部的入口留不留，他没有说。§9「网站对外放在哪」由这一条回答。
- D2 前端框架的组件页：从站里拿掉。当时的建议是留在站里、移出主导航，他没有采纳。读法：组件页不再生成，旧地址落到一页说明上，说明页指向已归档的仓库与它的标签；站里别的与前端框架有关的页面（前端 SDK、讲旧外壳的几页）不在这一问里，没有定。由 [S1-DOCS-011](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DOCS-011.md) 落地：`/components/qian/` 下原来的 91 页、同源的交互演示、离线编辑器与组件交互的浏览器用例都不再生成，构建不再检出前端框架的仓库；`/components/qian/` 留一页说明，原来的地址连同 `/components/yue/` 的旧书签都落到这一页。与本 ADR 的关系：D22.1 让前端框架的页面在本站一个入口里完成，到此为止；D22.2 说的在构建页面上做操作断言与发布后复验、D22.3 说的完整属性表，在本站没有了对应的页面。前端框架的组件文档以归档仓库的标签 `@cavailab/qian-core@0.2.1` 为准。§9「站里它的组件页留着、移走还是撤掉，没有定」由这一条回答。
- D3 给大模型的入口：照建议补。先把使用文档的代码收进仓库、每次发版编译并跑一遍；再把 `llms.txt` 里的 API 参考收成每个包一行；平台 HTTP 接口的 OpenAPI 导出进站点排在接口参考（[S1-DOCS-002](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DOCS-002.md)）之后，但要排上。他另说「最终肯定要给 DSH 使用的」，读作这个入口最终的使用方是 DSH，也就是平台的 Agent 运行时；DSH 怎么用上它，没有定。已定，没有实现。§9「已有的 llms.txt 够不够、还要补什么」由这一条回答。
- D4 平台档的包：维持现状，装包要 GitHub 令牌。建议里「以后的客户端包从一开始就公开发布」那半句不算已定。
