# 原 Kun 的 `.github/` 原件

这个目录保留基准测试、对象存储实测和问题模板等归档文件。CI 与发布已迁至仓库根的 `.github/workflows/ci.yml` 和 `.github/workflows/release.yml`。旧的 CI 和发布工作流、release-drafter 与发布说明模板可在历史提交 `51b480775c` 查看。

这里的文件 GitHub 既不执行也不读：工作流只从 `.github/workflows/` 这一层读；问题与 PR 模板只认 `.github/ISSUE_TEMPLATE/`、`.github/PULL_REQUEST_TEMPLATE.md` 这些固定位置；release-drafter 只读 `.github/release-drafter.yml`。不要把它们原样放回那些位置：工作流原件跑在自托管 runner 上，`release.yml` 一推 `v*` 标签就发包，模板与 release-drafter 的配置会改变本仓库的行为。

| 文件 | 是什么 | 现状 | 以后怎么接 |
|---|---|---|---|
| `workflows/benchmarks.yml` | BenchmarkDotNet 基准：每周一次、手动，以及带 `benchmarks` 标签的 PR | 不接：只在自托管 runner 上跑，本仓库不接自托管 runner（S1-DEC-038 D21） | 在本机跑，命令见仓库根 `AGENTS.md` 的「构建与测试」 |
| `workflows/oss-live-smoke.yml` | 对象存储实测：手动与每晚一次 | 不接：要自托管 runner 与 `OSS_SMOKE_*` 密钥（S1-DEC-038 D21） | 在本机设好四个 `OSS_SMOKE_*` 环境变量后跑，命令见仓库根 `AGENTS.md` 的「构建与测试」；CI 里没有这些变量，这组用例在测试分片 k 里跳过 |
| `ISSUE_TEMPLATE/bug_report.md`、`ISSUE_TEMPLATE/feature_request.md`、`PULL_REQUEST_TEMPLATE.md` | 问题与 PR 模板 | 没启用：放进 `.github/` 会改变本仓库新建问题与 PR 时的表单 | 要不要启用、启用哪一份，由产品负责人定；原 Qian 没有这类模板 |

平台项目的构建、文档与部署检查由仓库根的 [CI 工作流](../../workflows/ci.yml) 及其调用的工作流执行。
