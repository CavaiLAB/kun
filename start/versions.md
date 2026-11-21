> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# 版本矩阵

> 本页由 `website/scripts/build-versions.ts` 从仓库钉版文件生成（[ADR-021](/design/adr/adr-021-product-family-structure.md) D21.2 第 3 条），不要手改；改版本去改下表列出的源文件。

平台包清单列出仓库根 `src/` 下平台档项目的开发版本。使用方安装合并版时，以相应 GitHub Release 附带的 `release-packages.json` 为准。Kun 一列取仓库根 `CHANGELOG.md` 中最新的正式版本。[Kun 框架](/sdk/kun/index.md)小节与 [Kun API 参考](/sdk/api/kun/index.md)都按它镜像。

Kun 的同日修复版 `-hotfix.N` 是正式修复：最新的发布版按 `CHANGELOG.md` 里的先后算，不按 NuGet 的预发布排序。

| 文档版本      | Kun        | 平台包（源码开发版本）  | 前端版本目录      | .NET SDK                            | Node / pnpm                     | K3s   |
| --------- | ---------- | ------------ | ----------- | ----------------------------------- | ------------------------------- | ----- |
| main（开发版） | 2026.10.25 | 0.1.0（43 个包） | 0.2.1（9 个包） | 10.0.100（rollForward latestFeature） | ^22.12.0 \|\| >=24.0.0 / 12.9.1 | v1.35 |

本站当前展示 `main` 文档。Qian 组件参考从前端仓库对应的发布标签生成。

## 来源

| 项                 | 钉版文件                   | 说明                                                               |
| ----------------- | ---------------------- | ---------------------------------------------------------------- |
| Kun               | 仓库根的 `CHANGELOG.md`    | 最新的发布段 `## [版本] - 日期`，CalVer；发布工作流核对包源并建立 `v<版本>` 标签，随后合入版本日志 PR |
| 平台包               | `src/*/*.csproj`       | 平台档可打包项目。版本优先取项目文件，默认值来自 `src/Directory.Build.props`             |
| @cavailab/qian-\* | `pnpm-workspace.yaml`  | 版本目录（catalog）里的前端框架包，写精确版本；源码在 CavaiLAB/qian                     |
| .NET SDK          | 仓库根的 `global.json`     | `sdk.version` 与 `rollForward`                                    |
| Node / pnpm       | `package.json`         | `engines.node` 与 `packageManager`                                |
| K3s               | `deploy/baseline.json` | 平台部署采用的 Kubernetes 基线版本                                          |

## 平台包清单

- `Kun.Audit.EntityFrameworkCore` 0.1.0
- `Kun.Authorization.AspNetCore` 0.1.0
- `Kun.Authorization.Rbac` 0.1.0
- `Kun.Authorization.Rbac.Abstractions` 0.1.0
- `Kun.Capabilities` 0.1.0
- `Kun.Capabilities.Abstractions` 0.1.0
- `Kun.Capabilities.AspNetCore` 0.1.0
- `Kun.Configuration.AspNetCore` 0.1.0
- `Kun.Configuration.Platform` 0.1.0
- `Kun.Data` 0.1.0
- `Kun.Data.Abstractions` 0.1.0
- `Kun.Data.AspNetCore` 0.1.0
- `Kun.Data.Dialect.Pg` 0.1.0
- `Kun.Docs` 0.1.0
- `Kun.Docs.Abstractions` 0.1.0
- `Kun.Files` 0.1.0
- `Kun.Files.Abstractions` 0.1.0
- `Kun.Files.AspNetCore` 0.1.0
- `Kun.Gateway.Shell` 0.1.0
- `Kun.Identity.Abstractions` 0.1.0
- `Kun.Identity.Client` 0.1.0
- `Kun.Identity.Connector.Oidc` 0.1.0
- `Kun.Identity.Server` 0.1.0
- `Kun.Lifecycle` 0.1.0
- `Kun.Lifecycle.Abstractions` 0.1.0
- `Kun.Lifecycle.AspNetCore` 0.1.0
- `Kun.Plugin.Sdk.AspNetCore` 0.1.0
- `Kun.Plugin.Sdk.Commands` 0.1.0
- `Kun.Profiles` 0.1.0
- `Kun.Profiles.Abstractions` 0.1.0
- `Kun.Registry` 0.1.0
- `Kun.Registry.Abstractions` 0.1.0
- `Kun.Registry.AspNetCore` 0.1.0
- `Kun.Scheduler` 0.1.0
- `Kun.Scheduler.Abstractions` 0.1.0
- `Kun.Surfaces` 0.1.0
- `Kun.Surfaces.Abstractions` 0.1.0
- `Kun.Surfaces.Hosting` 0.1.0
- `Kun.Tenancy` 0.1.0
- `Kun.Tenancy.Abstractions` 0.1.0
- `Kun.Tenancy.AspNetCore` 0.1.0
- `Kun.Tenancy.Signup` 0.1.0
- `Kun.Tenancy.Signup.Abstractions` 0.1.0

## 前端版本目录中的框架包

Kun 旧应用外壳按 Qian 0.2.1 归档，组件文档取对应发布标签。业务预发保留原有部署和数据。[外壳归档裁定 P42](https://github.com/CavaiLAB/jiangxin/blob/5ca1442b04eb7c4812314664ab06a9d7feba6a22/docs/decisions/2026-11-21-shell-migration.md) · [产品裁定 P38](https://github.com/CavaiLAB/jiangxin/blob/main/docs/decisions/2026-11-20-product.md)

Qian 的 8 个层包统一定版本。TypeScript 运行时 `@cavailab/qian-runtime` 与前端插件开发包 `@cavailab/qian-plugin-sdk` 各自定版本。见 [Qian 的版本策略](https://github.com/CavaiLAB/qian#现状)。

下列项目来自根版本目录。`@cavailab/qian-runtime` 的版本由 CLI 与声明式表面校验库的 `package.json` 单独钉住，见[仓库的前端依赖说明](https://github.com/CavaiLAB/kun#前端依赖与包源)。

- `@cavailab/qian-biz` 0.2.1
- `@cavailab/qian-contracts` 0.2.1
- `@cavailab/qian-core` 0.2.1
- `@cavailab/qian-data` 0.2.1
- `@cavailab/qian-form` 0.2.1
- `@cavailab/qian-plugin-sdk` 0.2.1
- `@cavailab/qian-sdk` 0.2.1
- `@cavailab/qian-shell` 0.2.1
- `@cavailab/qian-tokens` 0.2.1
