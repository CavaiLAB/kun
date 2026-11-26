# gateway 完整依赖 NativeAOT 技术探针

本目录是 S1-GW-001 对 S1-DEC-029 的接续验证，不是替代网关 E2E 的空宿主。普通构建通过不代表 NativeAOT 发布通过；发布失败时不能执行或宣称原生运行成功。

在仓库根执行，镜像使用该目录作为构建上下文，本目录的项目从源码编译仓库根 `src/` 下的 Kun 项目：

```bash
dotnet build tools/Kun.Gateway.AotProbe -c Release
docker build --progress=plain -f tools/Kun.Gateway.AotProbe/Dockerfile -t qian-gateway-aot-probe:gw001 .
```

Dockerfile 固定 SDK 镜像摘要，按 Linux arm64 发布，启用 `IlcTreatWarningsAsErrors` 并逐条输出裁剪诊断。运行入口确实连接 Apollo、Redis、受限 PostgreSQL，挂载 Config / Identity / Data / Audit 并执行审计写入、读取和验链；运行配置由测试环境提供，不记录凭据。

2026-10-23 实测：普通 Release 构建成功，严格 Linux NativeAOT 发布失败，原生运行尚未执行。定位、边界与当时的几条路径见 [S1-DEC-030](../../pm/tasks/S1-DEC-030.md)；产品负责人选了新增原生 Data / Audit Provider。探针不进入正常解决方案的发布集合，避免把已知失败的诊断路径伪装成已交付的业务宿主。
