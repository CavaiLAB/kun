---
title: 前端 SDK
description: "@cavailab/qian-runtime 与 @cavailab/qian-plugin-sdk 的编程模型：外壳是一棵插件树，插件界面以 MF2 远程模块加载并挂到版本化插槽。"
part: sdk
project: platform
audience: [developer]
updated: 2026-11-26
---

# 前端 SDK

本页说明 Kun 旧应用外壳归档源码使用的 Qian 0.2.1 SDK。业务预发保留原有部署和数据。匠心工业智能体平台的 Web 与桌面端采用 DSH 外壳。

旧外壳是一棵由 `@cavailab/qian-runtime` 装载的插件树，与后端同一套语义：上下文、服务注册、类型化事件、可逆副作用、scope。

| 包 | 内容 |
|---|---|
| `@cavailab/qian-runtime` | 运行时：上下文 `ctx`、`provide` / `inject`、事件、副作用、装载器 |
| `@cavailab/qian-plugin-sdk` | 插件开发包的前端部分：与外壳交互的桥接 API（随 qian.shell 提供，PRD §7.15） |
| `@cavailab/qian-sdk` | 注册表 API、`PlatformClient`、请求拦截器链（鉴权刷新、租户头）、i18n |

- **服务键**：TS 侧小驼峰（`files`），.NET 侧大驼峰（`Files`），两侧各自声明常量，文档生成器把它们对应到同一 seam。`ctx.files` 是代理属性，等价于 `ctx.inject("files")`。
- **远程模块**：外壳 [qian.shell](/components/platform/qian.shell) 是 MF2 宿主，按需加载插件的远程模块；`react`、`react-dom`、`@cavailab/qian-*`（含 `@cavailab/qian-plugin-sdk`）作为共享单例。
- **插槽**：导航、设置、用户菜单、命令、预览等插槽都带版本号与输入数据 Schema。

## API 草图

节选自[组件模型规格](/design/prd/specs/component-model) §5.3（以规格为准）：

```ts
export type ScopeKey = "global" | `${"tenant" | "app" | "agent-session"}:${string}`;
export type DispatchMode = "emit" | "waterfall" | "parallel" | "serial" | "bail";

export interface QianPlugin<C = unknown> {
  readonly name: string; // 组件 id
  readonly inject?: readonly ServiceKey[];
  apply(ctx: Context, config: C): void | Promise<void>;
}

export interface Context {
  readonly scope: ScopeKey;
  provide<T>(key: ServiceKey<T>, value: T): Disposer;
  inject<T>(key: ServiceKey<T>): T;
  on<P, R>(evt: QianEvent<P, R>, mode: DispatchMode, handler: HandlerFor<P, R>, priority?: number): Disposer;
  dispatch<P, R>(evt: QianEvent<P, R>, payload: P): Promise<R>;
  scoped(scope: ScopeKey): Context;
}
```

组件的界面怎么写，见前端框架 Qian 的组件文档（在归档仓库的标签里，本站只有一页[说明](/components/qian/)）；API 参考（TypeDoc 生成）随 S1-DOCS-002 上线。
