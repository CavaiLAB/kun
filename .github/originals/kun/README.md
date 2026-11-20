# 原 Kun 的 `.github/` 原件

这个目录放原 Kun（已归档的 `idadawn/kun`）仓库根上 `.github/` 里的全部文件，内容与归档时的最终头 `51b480775c` 相同，一字未改。B2 把原 Kun 提到仓库根时，从 `_import/kun/.github/` 挪到了这里（git 识别为改名，`git log --follow -m` 看得到原来的历史）。本说明是挪过来时新加的。

这里的文件 GitHub 既不执行也不读：工作流只从 `.github/workflows/` 这一层读；问题与 PR 模板只认 `.github/ISSUE_TEMPLATE/`、`.github/PULL_REQUEST_TEMPLATE.md` 这些固定位置；release-drafter 只读 `.github/release-drafter.yml`。不要把它们原样放回那些位置：工作流原件跑在自托管 runner 上，`release.yml` 一推 `v*` 标签就发包，模板与 release-drafter 的配置会改变本仓库的行为。

| 文件 | 是什么 | 现状 | 以后怎么接 |
|---|---|---|---|
| `workflows/ci.yml` | 还原、构建、单元与集成测试、AOT 审计、样例 AOT 集成测试、格式与分层检查 | 已接：并进仓库根的 `.github/workflows/ci.yml`（S1-KUN-020），相对原件改了什么写在它的文件头 | 已接完 |
| `workflows/benchmarks.yml` | BenchmarkDotNet 基准：每周一次、手动，以及带 `benchmarks` 标签的 PR | 不接：只在自托管 runner 上跑，本仓库不接自托管 runner（S1-DEC-038 D21） | 在本机跑，命令见仓库根 `AGENTS.md` 的「构建与测试」 |
| `workflows/oss-live-smoke.yml` | 对象存储实测：手动与每晚一次 | 不接：要自托管 runner 与 `OSS_SMOKE_*` 密钥（S1-DEC-038 D21） | 在本机设好四个 `OSS_SMOKE_*` 环境变量后跑，命令见仓库根 `AGENTS.md` 的「构建与测试」；CI 里没有这些变量，这组用例在测试分片 k 里跳过 |
| `workflows/release.yml` | 推 `v*` 标签触发：校验标签、构建、测试、AOT 核验、打包，推 nuget.org 与 GitHub Packages，建 GitHub Release | 没接 | B12（首个合并版）之前由产品负责人定。合并蓝图 D23.12 第 4 点要求发版改为手动触发、要求同一提交的检查已通过；包源密钥、GitHub Packages 归谁、用哪种 runner 也在那时定。原件写的是原仓库的路径与包源，不能直接用 |
| `workflows/release-drafter.yml`、`release-drafter.yml` | 按 PR 标签维护版本说明草稿；后者是它的配置 | 没接 | B12 之前定。配置按标签推算语义化版本号，与日历版本冲突；放回 `.github/release-drafter.yml` 就会被读到 |
| `RELEASE_TEMPLATE.md` | 手写版本说明的模板 | GitHub 不认这个文件 | 随发版一起定 |
| `ISSUE_TEMPLATE/bug_report.md`、`ISSUE_TEMPLATE/feature_request.md`、`PULL_REQUEST_TEMPLATE.md` | 问题与 PR 模板 | 没启用：放进 `.github/` 会改变本仓库新建问题与 PR 时的表单 | 要不要启用、启用哪一份，由产品负责人定；原 Qian 没有这类模板 |

平台项目的构建、文档与部署检查由仓库根的 [CI 工作流](../../workflows/ci.yml) 及其调用的工作流执行。
