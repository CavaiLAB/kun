> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# qian 组件库

Kun 旧应用外壳按 Qian 0.2.1 归档，业务预发保留原有部署和数据。归档源码使用 Qian 的 React 组件、TypeScript 运行时和前端插件 SDK（[ADR-013](/design/adr/adr-013-frontend-framework.md)、[组件库专题规格](/design/prd/specs/frontend.md)），源码在 [CavaiLAB/qian](https://github.com/CavaiLAB/qian)；本小节由它的标签 `@cavailab/qian-core@0.2.1` 生成（版本钉在 `pnpm-workspace.yaml` 的版本目录）。包发在 GitHub Packages，包名 `@cavailab/qian-<包>`；设计文档里沿用旧包名，对应关系见 [CavaiLAB/qian 迁移说明第 2 节](https://github.com/CavaiLAB/qian/blob/main/docs/migrating-from-yue.md#2-包名与安装)。core 提供 52 个组件入口，每页包含真实交互演示、源码与完整 Props。

安装前在项目的 `.npmrc` 写 `@cavailab:registry=https://npm.pkg.github.com`；令牌行 `//npm.pkg.github.com/:_authToken=${NODE_AUTH_TOKEN}` 写进用户级配置（`~/.npmrc`）——10.34.2 起的 pnpm 10、11.5.3 起的 pnpm 11，以及 pnpm 12 不展开仓库里 `.npmrc` 认证行的环境变量（安全公告 GHSA-3qhv-2rgh-x77r；11.0.0–11.5.2 仍会展开）——再把有 `read:packages` 权限的个人访问令牌（classic）放进环境变量 `NODE_AUTH_TOKEN`。从这个包源安装公开包也要令牌，详见 [CavaiLAB/qian 迁移说明第 2 节](https://github.com/CavaiLAB/qian/blob/main/docs/migrating-from-yue.md#2-包名与安装)。下表是 S1 各包的范围。

| 包 | 阶段 | 内容 |
| ----------------------- | -- | ---------------------------------------------------- |
| `@cavailab/qian-tokens` | S1 | 三层设计令牌（primitive → semantic → component）、明暗两态与首帧防闪烁。 |
| `@cavailab/qian-core` | S1 | 52 个基础组件：按钮、输入、选择、弹层、反馈、导航等。 |
| `@cavailab/qian-shell` | S1 | AppShell 单一侧栏布局、页面骨架、租户切换器与兜底页。 |
| `@cavailab/qian-data` | S1 | DataTable 基础版、树与级联、日期时间、分片上传。 |
| `@cavailab/qian-form` | S1 | SchemaForm 基础版、校验与基础联动、弹层 API。 |
| `@cavailab/qian-biz` | S1 | 业务选择器（远程选项、字典、组织、用户、角色）、菜单与按钮授权树、IconPicker。 |
| `@cavailab/qian-sdk` | S1 | 注册表 API、PlatformClient、请求拦截器链与 i18n。 |

## 基础组件

- [Button 按钮](/components/qian/core/button.md)：触发保存等操作，支持尺寸、强调程度、加载和禁用状态。

- [IconButton 图标按钮](/components/qian/core/iconbutton.md)：用于紧凑操作，必须提供可访问名称。

- [ButtonGroup 按钮组](/components/qian/core/buttongroup.md)：把相关操作放在一起，保持各按钮的独立语义。

- [Input 文本输入](/components/qian/core/input.md)：录入单行文本，支持原生表单属性与错误状态。

- [Textarea 多行输入](/components/qian/core/textarea.md)：录入说明等多行文本，保留原生编辑与表单语义。

- [InputNumber 数字输入](/components/qian/core/inputnumber.md)：通过键盘或增减按钮输入数字，并遵守范围限制。

- [InputPassword 密码输入](/components/qian/core/inputpassword.md)：在保留输入值的同时切换密码显示状态。

- [PinInput 验证码输入](/components/qian/core/pininput.md)：分格录入验证码，完成一格后自动移动焦点。

- [Checkbox 复选框](/components/qian/core/checkbox.md)：独立勾选选项，支持键盘和受控状态。

- [Radio 单选项](/components/qian/core/radio.md)：与 RadioGroup 组合，在一组选项中选择一项。

- [RadioGroup 单选组](/components/qian/core/radiogroup.md)：管理一组互斥选择，方向键可切换选项。

- [Switch 开关](/components/qian/core/switch.md)：控制一个立即生效的二态设置。

- [Toggle 切换按钮](/components/qian/core/toggle.md)：用按下状态表达格式或工具的选择。

- [Segmented 分段选择](/components/qian/core/segmented.md)：在少量互斥模式之间切换。

- [Select 下拉选择](/components/qian/core/select.md)：从预设选项中选取值，显示对应标签。

- [Combobox 可搜索选择](/components/qian/core/combobox.md)：在弹层内部搜索并选择选项。

- [Label 标签](/components/qian/core/label.md)：用原生关联把说明与输入控件连接起来。

- [Field 表单字段](/components/qian/core/field.md)：组合标签、描述、输入和校验反馈。

- [Dialog 对话框](/components/qian/core/dialog.md)：在模态弹层中完成编辑，关闭后恢复焦点。

- [AlertDialog 确认对话框](/components/qian/core/alertdialog.md)：对重要操作提供明确的确认和取消选择。

- [Sheet 侧栏弹层](/components/qian/core/sheet.md)：从侧边展示筛选或补充信息。

- [Drawer 抽屉](/components/qian/core/drawer.md)：在抽屉中承载更多选项与补充操作。

- [Popover 浮层](/components/qian/core/popover.md)：在触发器旁展示短小的设置或操作内容。

- [Tooltip 提示](/components/qian/core/tooltip.md)：通过悬停或键盘焦点显示补充说明。

- [HoverCard 悬浮卡片](/components/qian/core/hovercard.md)：悬停查看对象的补充资料。

- [Menu 菜单](/components/qian/core/menu.md)：通过键盘或指针选择操作，禁用项保持可辨识。

- [DropdownMenu 下拉菜单](/components/qian/core/dropdownmenu.md)：以 Menu 的兼容名称组合下拉操作。

- [ContextMenu 右键菜单](/components/qian/core/contextmenu.md)：通过右键或菜单快捷键打开上下文操作。

- [Tabs 标签页](/components/qian/core/tabs.md)：在多个内容面板之间切换，并维护焦点语义。

- [Accordion 折叠面板](/components/qian/core/accordion.md)：按需展开问题说明或分组内容。

- [Card 卡片](/components/qian/core/card.md)：组合标题、说明、内容和操作区域。

- [Separator 分隔线](/components/qian/core/separator.md)：表达相邻内容的视觉或语义分隔。

- [ScrollArea 滚动区域](/components/qian/core/scrollarea.md)：在受限区域中展示长内容，并提供可访问名称。

- [Resizable 可调整面板](/components/qian/core/resizable.md)：通过拖动或键盘调整相邻面板的宽度。

- [Badge 徽标](/components/qian/core/badge.md)：用简短文本表达状态或分类。

- [Tag 标签](/components/qian/core/tag.md)：展示分类并通过有名称的移除按钮删除标签。

- [Avatar 头像](/components/qian/core/avatar.md)：显示头像或后备文字，提供对象名称。

- [Skeleton 骨架屏](/components/qian/core/skeleton.md)：在内容载入时展示布局占位。

- [Spinner 加载指示](/components/qian/core/spinner.md)：表达正在处理的状态，并提供读屏名称。

- [Progress 进度](/components/qian/core/progress.md)：显示已知比例的任务进度。

- [Alert 提示信息](/components/qian/core/alert.md)：用语义色和文本展示需要用户注意的信息。

- [Toast 短消息](/components/qian/core/toast.md)：通过共享消息队列显示短时反馈并允许关闭。

- [Notification 通知](/components/qian/core/notification.md)：通过共享消息队列展示较完整的通知内容。

- [Empty 空状态](/components/qian/core/empty.md)：解释暂无内容的原因并提供后续操作。

- [Result 结果页](/components/qian/core/result.md)：展示操作结果及下一步入口。

- [Breadcrumb 面包屑](/components/qian/core/breadcrumb.md)：展示当前位置与上级路径。

- [Pagination 分页](/components/qian/core/pagination.md)：在结果页之间切换，并约束首尾边界。

- [BackTop 返回顶部](/components/qian/core/backtop.md)：在长内容中快速回到顶部。

- [FullScreen 全屏](/components/qian/core/fullscreen.md)：调用浏览器全屏 API，并向宿主报告失败。

- [Icon 图标](/components/qian/core/icon.md)：通过统一入口使用图标并区分装饰与语义用途。

- [Descriptions 描述列表](/components/qian/core/descriptions.md)：展示多项字段名称与对应内容。

- [Steps 步骤条](/components/qian/core/steps.md)：展示多步流程的进度：已完成、进行中、未通过与未开始。

- [设计令牌](/components/qian/tokens.md)

- [SchemaForm 表单与弹层](/components/qian/form.md)

- [数据组件](/components/qian/data.md)

- [外壳与认证](/components/qian/shell.md)

- [业务选择器](/components/qian/biz.md)
