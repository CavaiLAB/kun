# 文档站上 K3s

文档站（`website/`，S1-DOCS-001）的托管方式：**CI 构建，git-sync 拉取，nginx 提供**。与[任务看板](../pm-board/README.md)同一套做法，不构建镜像：

```text
main 的 CI ──构建──> 推到 CavaiLAB/kun 的 docs-site 分支 ──git-sync 每 60s──> emptyDir /git/current ──nginx──> Service ──> 浏览器
```

- 合并到 `main`：发布正式站点，挂在分支根目录。
- PR 预览：迁到 `CavaiLAB/kun` 之后暂不发布，见下面「迁到 CavaiLAB/kun」。nginx 与 `publish.sh` 仍支持 `pr-preview/pr-<n>/`，接回只改工作流。
- `docs-site` 分支只有一个提交，每次发布在当前内容上改完后强推（`--force-with-lease` 防并发覆盖，冲突自动重试），构建产物不在仓库里累积历史。

`repo` 检出缓存固定在 qd，使用上限 4Gi 的内存 `emptyDir`；git-sync 请求 2Gi、上限 5Gi，CPU 预算保持 10m / 300m。2026-10-15 实测原缓存约 990Mi，qd 可用内存约 332Gi。该隔离用于减少与 etcd 共用根盘的批量检出写入，是否消除慢请求仍按 S1-DEP-007 的实际采样验收。内存卷占用计入写入容器的内存额度，见 [Kubernetes emptyDir 文档](https://kubernetes.io/docs/concepts/storage/volumes/#emptydir)。Pod 重建后从 `docs-site` 分支重新拉取，沿用现有 `Recreate` 策略，文档站会在首次同步期间短暂不可用。

git-sync 经 HTTPS（`github.com:443`）匿名拉取公开仓库 `CavaiLAB/kun` 的 `docs-site` 分支，不挂 deploy key。原来经 SSH 拉 `idadawn/qian` 时，2026-10-15 现场 22 端口多次下载超时、检出目录被重置，改走过 `ssh.github.com:443`；HTTPS 同样只用 443。

## 迁到 CavaiLAB/kun

原 Qian 仓库归档之后（迁移执行计划 B2），文档站的源码在 `CavaiLAB/kun` 的过渡目录 `_import/qian/` 下，由仓库根的 `qian-docs-site` 工作流构建与发布：

- `main` 上改到文档站相关路径时，构建并把正式站点推到 `CavaiLAB/kun` 的 `docs-site` 分支（作业权限 `contents: write`，只在 `main` 上跑）。运行摘要只写发布的路径与来源提交，不写访问入口：仓库是公开的，内网地址不写进工作流。
- PR 上只构建与检查，构建仍带 `pr-preview/pr-<n>/` 前缀，前缀检查照跑；预览不发布，PR 里也不留评论。原来的评论靠两个内网入口才有用，公开仓库的 PR 评论人人可见，要不要接预览、评论里写什么，由产品负责人定。接回时照原件 `.github/workflows/docs-site.yml` 加回 PR 的发布条件、`pull-requests: write`、`closed` 触发类型与「删除预览」作业。
- 原仓库 `docs-site` 分支上的旧预览不搬过来。

为什么不用 Cloudflare Pages：文档面向内部团队，现有网络入口与看板一致（WireGuard 与公司内网），不需要再开外部账号与访问控制；以后要公网访问时，在边缘 nginx 加一条到 Ingress 域名的转发即可。

## 文件

| 文件 | 作用 |
|---|---|
| `kustomization.yaml` | 命名空间 `qian-tools`（由 `deploy/pm-board` 创建并持有，这里不重复声明） |
| `deployment.yaml` | `git-sync`（`GITSYNC_REF=docs-site`）+ `nginx-unprivileged`，非 root；经 HTTPS 匿名拉取，不挂 Secret |
| `nginx-configmap.yaml` | 根目录与 `pr-preview/pr-<n>/` 都可打开；`/a/b`、`/a/b.html`、`/a/b/` 三种写法等价；带哈希的 `static/` 长缓存，页面协商缓存；隐藏点文件 |
| `service.yaml`、`ingress.yaml` | LoadBalancer（MetalLB 固定 192.168.1.211）+ 固定 NodePort 30820；Traefik Ingress 域名占位 `qian-docs.deepsingularity.cn` |
| `networkpolicy.yaml` | 只放行入站 8080 与出站 DNS / 443 |
| `publish.sh` | 把构建产物发布到 `docs-site` 分支（CI 调用，本机也能用；在 `_import/qian/` 下运行） |
| `argocd-application.example.yaml` | Argo CD Application 的样例；实际生效的在部署仓库里 |

## 访问入口

| 入口 | 正式站点 | PR 预览 |
|---|---|---|
| WireGuard 侧（Mac 直接可用） | <http://10.78.1.161:30820/> | `http://10.78.1.161:30820/pr-preview/pr-<n>/` |
| 公司内网 | <http://192.168.1.211/> | `http://192.168.1.211/pr-preview/pr-<n>/` |
| Traefik Host 路由 | `qian-docs.deepsingularity.cn`（占位） | 同上加路径 |

仓库根的 `qian-docs-site` 工作流不写访问入口（公开仓库），改入口只改本文与 `service.yaml`、`ingress.yaml`。

## 首次部署

前提：看板已部署（命名空间 `qian-tools` 由它创建），`CavaiLAB/kun` 的 `docs-site` 分支已由 CI 发布过至少一次。在 `_import/qian/` 下运行：

```bash
kubectl kustomize deploy/docs-site | ssh qd 'kubectl apply -f -'
```

核验：

```bash
ssh qd 'kubectl -n qian-tools rollout status deploy/qian-docs-site && kubectl -n qian-tools get svc qian-docs-site'
curl -s http://10.78.1.161:30820/build-info.json
```

`build-info.json` 记录当前正式站点对应的 `main` 提交；每个预览目录下也有一份。

## 手动发布

一般不需要；CI 不可用时可以在本机构建后发布（需要对 `CavaiLAB/kun` 有推送权限；在 `_import/qian/` 下运行）：

```bash
pnpm --filter website build
deploy/docs-site/publish.sh main website/doc_build
```

PR 预览要用对应的前缀构建：`DOCS_BASE=/pr-preview/pr-<n>/ pnpm --filter website build`，再 `deploy/docs-site/publish.sh preview <n> website/doc_build`。

## 之后

- 已交给 Argo CD（2026-10-08）：部署仓库里的 Application `qian-docs-site` 跟踪 `CavaiLAB/kun` 的 `main` 上的 `_import/qian/deploy/docs-site`（自动同步，`selfHeal: true`，`prune: false`），公开仓库，不需要仓库凭据。此后清单改动合并到 `main` 即自动同步，不再手动 `kubectl apply`；上面「首次部署」一节只在重建集群时用。
- 接回预览之后：每个预览约 58 MB（gzip 后约 12 MB，nginx 已开 gzip；S1-DOCS-005 加入 Kun 小节与 Kun API 参考前约 17 MB，S1-DOCS-003 加入每页 .md、llms.txt 与知识库索引前约 48 MB），`docs-site` 分支随打开的 PR 数增长；PR 关闭即删除。产物检查有 100 MB 的总预算（R55）。
