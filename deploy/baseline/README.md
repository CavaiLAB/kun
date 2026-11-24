# 部署基线

部署基线是平台在 K3s 上运行所需的基础设施清单：staging 与生产两个环境各自的命名空间、PostgreSQL 与 Redis、网络策略和资源约束。范围与验收见任务 [S1-DEP-001](../../pm/tasks/S1-DEP-001.md)，分阶段验收的裁定见 [S1-DEC-016](../../pm/tasks/S1-DEC-016.md)。

## 部署资产在哪里

生效的部署清单只放在部署仓库，由 Argo CD 按固定版本同步。本仓库不复制一份，只保留：

- [`deploy/baseline.json`](../baseline.json)：K3s 的钉版。文档站的版本矩阵（`/start/versions`）读它，集群升级时改这里。
- 这份说明，以及上面两个任务文件。

## 基线包含什么

- 两个环境的命名空间相互独立，各有 PostgreSQL 16、Redis 7 和独立的持久卷；镜像按多架构摘要固定。
- 入站与出站默认拒绝，只放行明确的应用与迁移 Pod 访问同环境的 PostgreSQL、Redis 与集群 DNS。RabbitMQ、Apollo 在专属实例与标签就绪后再加入放行。
- 资源配额、默认资源限制、PDB、非 root、只读根文件系统、资源上限与认证探针；生产的数据服务固定调度到专用节点。
- Secret 引导脚本只创建缺失的条目，不覆盖、不打印；清单里没有秘密值。
- PostgreSQL 的应用角色不能绕过行级安全，租户表由各服务的迁移启用并强制行级安全。

基线只覆盖基础设施。平台应用访问 PostgreSQL、Redis、RabbitMQ、Apollo 的协议联调按 S1-DEC-016 由 [S1-DEP-005](../../pm/tasks/S1-DEP-005.md) 承接，不在这份基线里。
