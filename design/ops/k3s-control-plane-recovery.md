> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# K3s 控制面故障处置（S1-DEP-007）

> 本页由构建脚本镜像自仓库 [`docs/ops/k3s-control-plane-recovery.md`](https://github.com/CavaiLAB/kun/blob/main/docs/ops/k3s-control-plane-recovery.md)，请修改源文件，不要改本页。

2026-10 试点集群的控制面出现崩溃循环：控制面节点上的 k3s 反复退出重启，apiserver 间歇返回 ServiceUnavailable。本文记录现象、根因判断、方案取舍、通用的处置步骤与结论；执行与验收的记录见 [S1-DEP-007](https://github.com/CavaiLAB/kun/blob/main/pm/tasks/S1-DEP-007.md)。

## 1 现象

- 每次退出前 kube-controller-manager 报 `leaderelection lost`，k3s 进程以状态 1 退出，由 systemd 拉起。
- etcd 读 lease 键的耗时远超预期，日志成片出现 `apply request took too long`、`no leader`、`leader changed`。
- 控制面节点看到另外两个 etcd 成员的 2380 端口反复拒绝连接：对端虚机在线，但 etcd 不在监听，说明那两台上的 k3s 也在反复重启。三个成员中任意两个同时不在，集群就失去多数，apiserver 不可用，controller-manager 续约失败，三台 server 一起退出、拉起，循环往复。
- 节点间网络正常；控制面节点本机的 I/O 等待与内存压力在当时的即时采样里正常。

## 2 根因判断

- 结构性问题：etcd 三个成员里有两个跑在同一台虚拟化宿主机的虚机上，宿主机资源接近上限。这样的三成员 etcd 不提供宿主级高可用，反而把一台宿主机的压力放大成整个集群的故障。
- 最可能的触发：宿主机压力拖慢虚机，其上的 etcd 成员跟不上心跳，触发选举。处置时测得宿主机内存余量并不紧张，压力主要在机械阵列的 I/O 上，「内存不足」这一推断没有得到确认。
- 加剧因素：控制面节点同时承载不设上限的 CI runner 和一个常驻满占一核的进程。
- 改造之后仍出现过成片慢请求，同时段控制面节点根盘（etcd 所在）的同步写延迟升高，与文档站检出缓存的批量写入在时间上相邻（见第 7 节）。相关性不等于唯一根因。

方案的选择不依赖触发原因的确认：无论触发原因是什么，都要把控制面从同宿主的虚机上拿走。

## 3 方案与取舍

| 方案                  | 做法                                                        | 能否彻底解决                        | 代价                       |
| ------------------- | --------------------------------------------------------- | ----------------------------- | ------------------------ |
| A 保持三成员，扩宿主机        | 给宿主机加内存，调大两台工作节点的资源，关闭 balloon                            | 不能：两台工作节点仍同宿主，宿主机任何抖动仍会带崩控制面  | 采购或停机加内存                 |
| B 单控制面加三个 agent（采用） | 控制面节点独占 etcd、apiserver 与 controller；两台工作节点与生产专用节点都作 agent | 能：控制面落在最稳的物理机上，不再跨虚机做 raft 共识 | 一次维护窗口；失去控制面高可用，但现状本来就没有 |
| C 三个成员分到三台物理机       | 需要两台额外的物理机                                                | 能                             | 当时没有可用的物理机               |

采用 B。试点集群里 apiserver 短暂不可用不影响已运行的 Pod；改造之后，工作节点的虚机抖动只影响它自己的 Pod。

## 4 处置步骤

需要控制面节点、宿主机与两台工作节点的 root，并经产品负责人审核；执行期间暂停控制面节点上的 CI runner。

### 阶段 0：止血，不改拓扑

1. 在宿主机上记录内存、交换与 I/O 压力和备份计划，有整点备份或快照计划的先停。把工作节点的 k3s 日志与宿主机的压力、OOM、I/O 记录按时间对齐，确认或推翻触发假设。

2. 控制面节点上的 CI runner 放进同一个 systemd slice，限制合计用量（各设一份上限时，合计仍可占满整机），阶段 1 结束前停掉：

   ```bash
   printf '[Slice]\nCPUQuota=<合计 CPU 上限>%%\nMemoryMax=<合计内存上限>\nIOWeight=50\n' > /etc/systemd/system/actions-runners.slice
   # 每个 runner 的 unit 加 drop-in：[Service] Slice=actions-runners.slice
   systemctl daemon-reload
   ```

3. 常驻的未知进程先弄清是什么、谁在用，不要盲目 kill；需要保留就限额。

4. 保存 etcd 快照，连同 server token 与配置一起打包、记录 SHA-256，复制到控制面节点之外（不进 Git）。按 K3s 官方说明，把快照恢复到新节点时必须使用原集群的 token，缺 token 的快照恢复不了：

   ```bash
   k3s etcd-snapshot save --name pre-reset
   tar -czf /root/pre-reset-backup.tgz <快照文件> /var/lib/rancher/k3s/server/token /var/lib/rancher/k3s/server/node-token /etc/rancher/k3s/config.yaml
   chmod 0600 /root/pre-reset-backup.tgz; sha256sum <快照文件> /root/pre-reset-backup.tgz
   ```

### 阶段 1：改为单控制面

1. 前置核对。在每台工作节点上核实 k3s 的 `data-dir`、卸载脚本的删除范围、PV 的真实路径与挂载来源（包括符号链接与 bind mount）；业务卷落在清理范围内、与基线不符或备份不完整时立即停止。动节点之前先备份业务数据：PostgreSQL 逻辑导出、Redis 的 RDB、本地卷副本与 k3s 配置。按两台工作节点上的全部工作负载评估维护窗口并通知各负责人；有按主机名钉定的数据卷时，节点名必须保持不变。

2. 在两台工作节点上共同暂停旧的 server 服务，以便重置控制面的成员列表。`stop` 不等于移除 etcd 成员；此时不运行卸载脚本、不删除 Node 对象：

   ```bash
   systemctl stop k3s
   systemctl mask --runtime k3s
   ```

3. 在控制面节点上把 etcd 重置为单成员，再正常启动。重置时保留原 server unit 的必要参数（secrets-encryption、node-ip、advertise-address、tls-san），不能假定参数都在 `config.yaml` 里；用带超时的 readyz 与 etcd 成员检查确认健康，失败按第 5 节回退并停止后续步骤：

   ```bash
   systemctl stop k3s && k3s server --cluster-reset && systemctl start k3s && kubectl get --raw /readyz && kubectl get nodes
   ```

4. 两台工作节点逐台改为 agent，前一台的 Pod 与数据卷恢复后再做下一台。优先就地切换到既有的 agent unit，保留原节点名、node-ip、node password、Node UID、镜像缓存与业务卷；已有 unit 带 node-ip 时不要在 YAML 里重复写，否则可能合并出两个地址。切换后只去掉 control-plane / etcd 角色标签，其他标签、污点与调度状态保留。版本固定为集群现有的版本，不顺手升级。

5. 确认 agent 回来、数据卷重新绑定、工作负载恢复：

   ```bash
   kubectl get nodes -o wide; kubectl get pv; kubectl get pods -A -o wide | grep -vE 'Running|Completed'
   ```

6. 恢复 CI runner。

### 阶段 2：加固

- 控制面节点的 kubelet 加系统预留：先核对现有 Pod 的请求与可分配资源，再定预留量，不挤掉既有 Pod 的准入。
- runner 的共同 slice 转为长期配置；长期看把 runner 迁出控制面节点。
- 宿主机上虚机的内存分配总和留出余量，关闭虚机的 balloon。
- etcd 快照每天落到控制面节点之外，连同 token 与配置加密传输，并验证取回、解密与离线恢复结构。
- 控制面节点上加一个周期性的只读健康检查定时器，记录 `NRestarts`、`/readyz` 与 etcd 慢请求，连续异常时通知值班人。

### 阶段 3：验收

连续 24 小时内 k3s 的 `NRestarts` 不再增长，`/readyz` 全部成功，etcd 日志不再成片出现 `apply request took too long`。健康脚本的结果只覆盖控制面与节点状态，不含慢请求，慢请求要另查 etcd 指标与有界的 journal。出现重启、健康失败或成片慢请求时，从恢复后的首个正常采样重新计时。

## 5 回退

- 控制面重置失败：先停 k3s 并确认没有 server 进程在运行，核对备份的 SHA-256、原 token、配置与 unit 参数，再用阶段 0 的快照恢复：`k3s server --cluster-reset --cluster-reset-restore-path=<快照路径>`，然后正常启动。
- 工作节点加不进来：控制面已独立运行，不影响生产；agent 可以反复重装，业务卷不受影响。
- 任何时候都不要把工作节点重新装回 server，那等于回到故障前的拓扑。

## 6 禁止事项

- 故障期间不升级 k3s，不轮换 secrets-encryption 密钥，不改入口与负载均衡组件。
- 不删除虚机，不清空数据目录；与故障无关的节点不用 `kubectl delete node` 处理。
- 不在控制面节点上不加时间窗地查 `journalctl -u k3s`：日志量大，会长时间满占一核。

## 7 文档站缓存隔离试验

文档站的 git-sync 检出缓存可以从 `docs-site` 分支重建，原先是控制面节点根盘上的磁盘 `emptyDir`，与 etcd 共用一块盘。采样里它的批量写入与 etcd 慢请求时间上相邻，经产品负责人批准做了隔离试验：

- 检出缓存改为内存 `emptyDir`（设 `sizeLimit`），同时上调 git-sync 的内存请求与上限：内存卷的占用计入写入容器的内存额度，只改 `medium` 不够，见 [Kubernetes emptyDir 文档](https://kubernetes.io/docs/concepts/storage/volumes/#emptydir)。
- 沿用 `Recreate`，首次同步期间文档站短暂不可用；出现 OOM、同步失败或站点不能恢复时经 GitOps 撤回。
- 试验期间 git 经 22 端口取数反复超时，git-sync 与 Argo CD 的仓库连接都改走 SSH over 443。
- 生效后从恢复后的首个正常采样重新计 24 小时，不以 Pod 就绪代替验收。

现行配置见 [deploy/docs-site](https://github.com/CavaiLAB/kun/blob/main/deploy/docs-site/README.md)。

## 8 结论（2026-10-16）

- 方案 B 已落地：单控制面、单成员 etcd，两台工作节点与生产专用节点为 agent；改装前后 Node 身份与 PVC 绑定不变，生产与 staging 的数据协议和业务健康抽查通过。
- 24 小时稳定性验收的前几轮都因成片慢请求不合格、重新计时。最后一轮连续观察约 20 小时、全部健康时，产品负责人在知晓未满 24 小时的情况下决定提前关闭；原 24 小时标准没有记为完整通过。
- 旧磁盘缓存的写入与慢请求相关，但不等于唯一根因；当时的即时压力正常，也不能排除历史上的本机 I/O 停顿。
- 控制面节点的周期健康采样与每日异机加密备份继续运行；文件校验不代替恢复演练。此次关闭不改变 S1-DEP-001 的基线验收边界与 S1-DEC-019 的既有裁决。
