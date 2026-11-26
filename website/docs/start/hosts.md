---
title: 宿主与框架
description: 在独立宿主或平台宿主中装配 Kun 组件，了解旧 Kun 外壳归档源码如何通过 Qian 0.2.1 加载界面。
part: start
project: platform
audience: [developer, operator]
updated: 2026-11-26
---

# 宿主与框架

## 两种模式，同一组包

| 模式 | 宿主 | 装什么 | 适合 |
|---|---|---|---|
| 独立模式 | 你自己的 ASP.NET Core 项目（standalone profile） | `Kun.Runtime` + 所选组件的 Definition / Provider 包；配置走标准 `IConfiguration`，租户默认单租户 | 只想用某几项能力，由自己的进程承载 |
| 平台模式 | `platform` profile 的宿主 | base 组合包里的全部平台插件，含注册表、生命周期、能力目录三件套 | 多租户平台本体 |

独立与平台宿主共用组件契约和 TCK。组件通过 `Kun.Runtime` 登记、获取服务，两档的依赖边界由构建门禁校验。[组件接口](/start/concepts/seam)

## 五个 profile

可部署单元是 profile。S1 随发行版交付五个：

| profile | 用途 |
|---|---|
| `platform` | 平台本体：base 组合包 + 前端外壳，可选模型网关；不承诺 AOT |
| `gateway` | 业务 API 网关边缘，可 AOT |
| `identity` | 身份服务（唯一 issuer），独立安全边界 |
| `standalone` | 消费者宿主安装的最小组合包，`qian init --template dotnet-webapi` 的默认 |
| `minimal` | 测试与 SDK 用的最小插件树：TCK 宿主、CLI 自身运行时 |

组合包清单见 [profile、组合包与 patch](/cli/profile-patch)。

## 前端

Kun 旧应用外壳的归档源码使用 React 和 [Qian 0.2.1 组件库](/components/qian/)。业务预发保留原有部署和数据。匠心工业智能体平台的 Web 与桌面端采用 DSH 外壳。

归档外壳 [qian.shell](/components/platform/qian.shell) 是 MF2 宿主：插件的界面以远程模块按需加载，挂到版本化插槽上（[ADR-013](/design/adr/adr-013-frontend-framework)）。前端外壳自身也是一棵由 `@cavailab/qian-runtime` 装载的插件树，编程模型见 [前端 SDK](/sdk/frontend/)。

## 与 Kun 的关系

Kun 的内核档提供可单独安装的通用机制，平台档在其上装配租户、身份、权限等组件。Qian 提供 React 组件、TypeScript 运行时与前端插件 SDK。[产品族](/start/family)

[Kun 框架](/sdk/kun/)和 [API 参考](/sdk/api/kun/)按最新正式版本生成。同一组件在两类宿主中的装配关系见[内核与平台组件](/sdk/kun/with-qian)。
