# 任务看板上 K3s

在集群里提供任务看板（看板页、`STATUS.md`、`tasks.json`），随时用浏览器看进度。**不构建镜像、不改 CI**：一个 Pod 里三个容器：

- `git-sync` 每 60 秒拉一次仓库（`CavaiLAB/kun`）的 `main` 分支；
- `builder` 每换一个提交，就用该提交自己的 `<前缀>/scripts/pm.py build --out` 生成看板（S1-PM-004）；
- `nginx` 提供生成结果与任务源文件。

看板只取决于 `main` 上的任务文件，任务改动合入 `main` 后，一分钟左右看板就会更新。

```text
GitHub CavaiLAB/kun (main) --git-sync 每 60s--> /git/current --builder（<前缀>/scripts/pm.py build）--> /site/current --nginx--> Service --Traefik Ingress--> 浏览器
```

## 仓库与前缀

原 Qian 仓库（`idadawn/qian`）归档之后，任务文件在 `CavaiLAB/kun` 的过渡目录 `_import/qian/pm/tasks/` 下改（迁移执行计划 B2），看板跟着改拉 `CavaiLAB/kun`。它是公开仓库，git-sync 经 HTTPS 匿名拉取，ArgoCD 也匿名读取，都不需要 deploy key 与 Secret。

**前缀**是 `scripts/pm.py` 与 `pm/tasks/` 在检出里所在的目录，现在是 `_import/qian`，空串表示检出根。它只写在一处：`deployment.yaml` 里 builder 容器的环境变量 `BOARD_PREFIX`。builder 按它找 `pm.py`，启动时建 `/site/source` → `/git/current/<前缀>`，nginx 的 `/tasks/` 经这个链接取任务源文件，nginx 配置里不写前缀。`pm/` 与 `scripts/pm.py` 提出过渡目录时，只改 `BOARD_PREFIX`（提到仓库根就改为空串），与搬动它们的改动在同一个 PR 里合入。

清单本身的位置（ArgoCD Application 的 `path`，现在是 `_import/qian/deploy/pm-board`）是另一回事：`deploy/` 搬走时在部署仓库里改 Application。

## 文件

| 文件 | 作用 |
|---|---|
| `kustomization.yaml` | 命名空间 `qian-tools`（开发期工具，与平台本体 `qian` 分开）。`builder.py` 生成为带哈希后缀的 ConfigMap，并带 ArgoCD 的 `IgnoreExtraneous` 注解：改脚本后留下的旧 ConfigMap 不会让应用一直显示 OutOfSync（应用没开 prune，旧的不会被自动删除） |
| `deployment.yaml` | `git-sync` + `builder`（官方 `python:3.13-alpine`）+ `nginx-unprivileged` 三个容器，非 root；git-sync 经 HTTPS 匿名拉取，不挂 Secret；builder 的根文件系统只读，只写 `/site`，前缀写在它的 `BOARD_PREFIX` 里。首版看板生成前 Pod 不就绪。与文档站一样固定在 qd：worker 节点经镜像代理拉 `registry.k8s.io` 的 git-sync 会失败（2026-10-15 遇到 502）。滚动更新（S1-PM-005）：新 Pod 就绪后才停旧 Pod，新 Pod 起不来时旧版照常提供看板；nginx 停止前先等 5 秒（`preStop`），等各节点的转发规则摘掉旧 Pod，切换瞬间不丢请求；为此三个容器各只请求 1m CPU，新旧两个 Pod 才放得进 qd 的余量（2026-11-17 qd 已请求 7995m / 8000m、只剩 5m，产品负责人定为 1m，原为 5m；实测 git-sync 0m、builder 1m、nginx 1m），CPU 上限不变 |
| `builder.py` | builder 的主循环：启动时建 `/site/source` → `/git/current/<前缀>`；发现新提交就用 `<前缀>/scripts/pm.py` 生成到 `/site/<提交>`，原子切换 `/site/current`，删掉旧版。生成失败（任务文件校验不过）时继续提供上一版，只记日志；生成途中 git-sync 已切到更新的提交（旧检出随之被删）不算失败，只记一条跳过说明，下一轮生成新提交。收到 SIGTERM 立即退出：它在容器里是 1 号进程，不处理的话 SIGTERM 会被忽略，旧 Pod 要等 30 秒宽限期满才被强杀。kustomize 把它生成为带哈希后缀的 ConfigMap，改脚本即滚动更新 |
| `test_builder.py` | builder 的自测（只用标准库）：正常生成、任务文件校验不过、生成途中切换提交、收到 SIGTERM 立即退出、`/site/source` 链接与前缀规整，前缀为空与 `_import/qian` 各跑一遍。在 `_import/qian/` 下运行 `python3 -m unittest discover -s deploy/pm-board -p 'test_*.py'`，仓库根的 qian-docs-and-pm 工作流也会跑 |
| `nginx-configmap.yaml` | 只暴露 `/`、`/tasks.json`、`/STATUS.md`（取自 `/site/current`）与 `/tasks/*`（经 `/site/source` 取最新的 git 检出），禁缓存 |
| `service.yaml`、`ingress.yaml` | Service 为 LoadBalancer（MetalLB 固定 192.168.1.210）+ 固定 NodePort 30810；Traefik Ingress 域名占位 `qian-pm.deepsingularity.cn` |
| `networkpolicy.yaml` | 只放行入站 8080 与出站 DNS / 443 |
| `bootstrap.sh` | 初始化（apply → 核验）；支持 `--kube-ssh` 与 `--check` |
| `argocd-application.example.yaml` | 交给 ArgoCD 托管时用的样例，实际生效的在部署仓库里 |

## 当前状态（2026-10-07 已部署）

| 入口 | 地址 | 适用 |
|---|---|---|
| WireGuard 侧（Mac 直接可用） | <http://10.78.1.161:30810/> | 固定 NodePort，经节点别名 10.78.1.161 |
| 公司内网 | <http://192.168.1.210/> | MetalLB 固定地址，仅 192.168.1.0/24 内可达 |
| Traefik Host 路由 | `qian-pm.deepsingularity.cn` → 192.168.1.200 | 公网接入前置：公网 DNS 与边缘 nginx 转发尚未配置 |

切到 `CavaiLAB/kun` 之后不再用 deploy key。原来为拉 `idadawn/qian` 建的 Secret `qian-tools/qian-pm-board-git`（文档站也用它）、ArgoCD 读原仓库的仓库凭据与原仓库上的只读 deploy key 留作回退；看板与文档站都切换并核验之后，由产品负责人决定何时清理。ArgoCD Application 在部署仓库里。

## 首次部署

在 `_import/qian/` 下运行：

```bash
bash deploy/pm-board/bootstrap.sh                    # 本机有 kubectl
bash deploy/pm-board/bootstrap.sh --kube-ssh qd      # kubectl 在 ssh 主机 qd（本环境的实际用法）
bash deploy/pm-board/bootstrap.sh --check --kube-ssh qd   # 只核验：就绪、仓库与前缀、同步提交 == main、生成提交 == 同步提交、任务源文件、入口
```

脚本幂等，只 apply 清单并核验，不在 GitHub 与集群里建凭据。公网访问需要在边缘 nginx 加一条到该域名的转发（现网 TLS 在边缘终止）；内网直连 Traefik 或 `kubectl port-forward` 不需要。

## 之后

- 已交给 ArgoCD：Application 在部署仓库里，跟踪 `CavaiLAB/kun` 的 `main` 上的 `_import/qian/deploy/pm-board`（样例见 `argocd-application.example.yaml`），不需要仓库凭据。此后清单改动随 `main` 自动同步。
- 换分支或缩短刷新间隔：改 `deployment.yaml` 的 `GITSYNC_REF`、`GITSYNC_PERIOD`。
- 换前缀：只改 `deployment.yaml` 里 builder 的 `BOARD_PREFIX`，见上面「仓库与前缀」。前缀配错时 builder 在检出里找不到 `pm.py`，不生成，看板停在上一版，builder 日志里有一条「检出 … 里没有 …/scripts/pm.py」；核验用 `bootstrap.sh --check`。
- 看板没更新：先看 `bootstrap.sh --check` 的两项提交对比，再看 builder 日志（`kubectl -n qian-tools logs deploy/qian-pm-board -c builder`）。日志里「生成失败」说明该提交的任务文件校验不过，看板仍是上一版；「跳过这一版」说明生成途中 git-sync 已同步到更新的提交，下一轮会生成，不用处理。
- 改了清单却一直没换上新 Pod：多半是新 Pod 停在 Pending（`kubectl -n qian-tools describe pod` 的事件里有 `Insufficient cpu`），即 qd 的 CPU 请求余量放不下第二个 Pod。旧 Pod 仍在提供看板；腾出余量后会自动继续，超过 10 分钟 Deployment 会标为 `ProgressDeadlineExceeded`。
- ArgoCD 显示 OutOfSync，差异只有 `qian-pm-board-builder-<哈希>`（待清理）：这是带 `IgnoreExtraneous` 注解之前生成的旧 ConfigMap，只被缩到 0 的旧 ReplicaSet 引用，可以 `kubectl -n qian-tools delete configmap <名字>` 删掉。
- 文档站（S1-DOCS-001）上线后可以复用同一套方式先做预览，再决定是否走 Cloudflare Pages。
