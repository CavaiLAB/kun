---
title: 30 分钟上手
description: 构建 Kun 命令行，在独立 ASP.NET Core 宿主验收组件的还原、挂载和卸载。
part: start
project: platform
audience: [developer]
updated: 2026-11-20
---

# 30 分钟上手

在自己的 ASP.NET Core 宿主中安装身份、授权和文件组件是独立模式的目标。CLI 随 Kun 源码提供，本页按仓库中可运行的安装验收说明准备环境。[CLI 说明](https://github.com/CavaiLAB/kun/tree/main/packages/qian-cli)

## 1. 构建命令行

在 Kun 仓库根目录按 README 配置前端包源的读取凭据，然后执行：

```bash
pnpm install --frozen-lockfile
pnpm --filter qian... build
pnpm exec qian --profile platform --dump-config --no-home
```

最后一条命令读平台 profile，并打印解析后的组件树。

## 2. 在空白宿主验收安装

仓库的安装验收为每个场景建立独立目录，从包源还原组件并运行消费者。六个组件的清单钉住同一稳定版本。内核包从 nuget.org 还原，平台包从 GitHub Packages 还原。设置 `KUN_PLATFORM_FEED_USER`（GitHub 用户名）和 `KUN_PLATFORM_FEED_TOKEN`（有读包权限的令牌），然后在仓库根运行以下命令。

```bash
pnpm --filter qian test:feed
```

验收覆盖单独安装身份客户端、安装数据与文件组件，以及组件还原和卸载。身份宿主需提供 issuer 与 clientId，文件宿主需选择存储后端。CLI 将安装归属与版本写入本地锁文件。已有宿主采用相同的安装入口，参数和配置示例见 [CLI 说明](https://github.com/CavaiLAB/kun/tree/main/packages/qian-cli)。

## 下一步

- 三个组件各自怎么配、暴露了什么：[qian.identity](/components/platform/qian.identity)、[qian.authz](/components/platform/qian.authz)、[qian.files](/components/platform/qian.files)
- 同一组包放进平台运行：[宿主与框架](/start/hosts)
- profile 与 patch 怎么叠加：[profile 与 patch](/start/concepts/profile)
