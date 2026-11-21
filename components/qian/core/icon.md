> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Icon 图标

通过统一入口使用图标并区分装饰与语义用途。


本页交互预览支持状态、主题切换与本地代码编辑。示例源码见下方「用法与源码」。


## 安装

```bash
pnpm add @cavailab/qian-core @cavailab/qian-tokens
```

包发在 GitHub Packages 上，包源与令牌的配置见 [qian 组件库](/components/qian/index.md)。宿主导入 `@cavailab/qian-tokens/theme.css`，并用 Tailwind v4 的 `@source` 扫描安装目录里组件包的 `dist`；React 19 为 peer 依赖。

## 用法与源码

下面代码与交互预览来自同一份组件示例；可以复制到宿主，或在上方展开编辑器修改并运行。

```tsx
import { useId, useState } from 'react';
import * as C from '@cavailab/qian-core';

interface DemoProps { disabled?: boolean; invalid?: boolean; rootProps?: Record<string, unknown> }

export default function Example({ rootProps }: DemoProps) { return <C.Icon name="check" label="已完成" size={24} {...rootProps}/>; }
```

## 组合结构

- `Icon`

## 状态与交互

预览工具栏可切换默认、暗色场景；切换场景会重置示例。悬停状态需将指针实际移入目标；弹层通过示例里的触发器打开与关闭。点击、编辑或使用键盘直接操作组件，编辑代码后点击「运行」更新预览。

## API

以下属性直接取自公开 TypeScript 类型，包含继承属性和回调。默认值取源码解构或 JSDoc；「未指定」表示当前声明未提供默认值。

### Icon

| 属性 | 类型 | 必填 | 默认值 | 说明 |
| --------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -- | --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ref | `Ref<SVGSVGElement> \| undefined` | 否 | 未指定 | Allows getting a ref to the component instance. Once the component unmounts, React will set \`ref.current\` to \`null\` (or call the ref with \`null\` if you passed a callback ref). |
| children | `ReactNode` | 否 | 未指定 | 以 TypeScript 类型约束为准。 |
| className | `string \| undefined` | 否 | 未指定 | 以 TypeScript 类型约束为准。 |
| testId | `string \| undefined` | 否 | 未指定 | 覆盖默认的 \`data-testid\`（默认 \`qian-\<组件>-\<部件>\`）。 |
| name | `"search" \| "key" \| "alert" \| "home" \| "user" \| "arrowUp" \| "arrowRight" \| "check" \| "chevronDown" \| "chevronLeft" \| "chevronRight" \| "chevronsUpDown" \| "circleCheck" \| "circleX" \| "help" \| "eye" \| "eyeOff" \| "info" \| "spinner" \| "maximize" \| "minimize" \| "minus" \| "plus" \| "close" \| "dashboard" \| "building" \| "users" \| "inbox" \| "clipboardCheck" \| "settings" \| "package" \| "fileText" \| "refresh" \| "more" \| "columns" \| "pencil" \| "undo" \| "logOut" \| "bell" \| "sun" \| "moon" \| "monitor" \| "languages" \| "shieldCheck" \| "clock" \| "calendar" \| "mail" \| "phone" \| "externalLink" \| "panelLeftClose" \| "panelLeftOpen" \| "globe" \| "lock" \| "message" \| "network" \| "shield" \| "sliders" \| "layoutGrid"` | 是 | 未指定 | 以 TypeScript 类型约束为准。 |
| label | `string \| undefined` | 否 | 未指定 | 以 TypeScript 类型约束为准。 |
| size | `number \| undefined` | 否 | 16 | 以 TypeScript 类型约束为准。 |
| title | `string \| undefined` | 否 | 未指定 | 透传 SVG 的 title 属性；可访问名称由 label 提供。 |

[继承的 DOM / ARIA / 事件属性（484 项）](/components/qian/api/native-f312f5257ffa.md)：从相同的 TypeScript 声明提取，完整类型与说明只存一份。

## 键盘与无障碍

展示内容不接管键盘焦点。

为交互控件提供有意义的名称；弹层关闭后恢复触发器焦点，验证明暗两态下的键盘与读屏路径。

## 模型体验

组件清单通过 `@cavailab/qian-core/manifest.json` 分发；模型选择已登记属性、slots 与 events，函数回调由宿主提供。

## 已知限制与延期项

示例为本地交互，不连接业务后端。文案由消费方本地化；浏览器能力（如全屏）受宿主环境约束，失败通过组件回调处理。

## 变更

本页对应 `@cavailab/qian-core@0.2.1`，由 CavaiLAB/qian 该版本标签上的源码生成；组件行为与示例以该版本为准。
