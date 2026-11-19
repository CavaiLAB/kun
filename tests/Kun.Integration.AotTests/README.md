# AOT 发布回归

`SampleMinimalAotPublishTests` 保留框架示例的构建与原生执行门禁。`GatewayPackageAotPublishTests` 补充普通 NuGet 制品被独立宿主消费的路径。`GatewayStandaloneAotPublishTests` 原生发布并运行网关的独立宿主 `hosts/Kun.Gateway.Standalone`。三者均由 CI 的 `sample-aot-integration` 作业执行。

在已安装 .NET 10 SDK、clang 和 zlib 开发包的 Linux 主机上，从仓库根运行：

```bash
dotnet test tests/Kun.Integration.AotTests/Kun.Integration.AotTests.csproj -c Release \
  --filter FullyQualifiedName~GatewayPackageAotPublishTests
```

该用例在非 Linux 系统明确跳过，不把本机托管测试当作原生证据。Linux 缺少编译器、发布失败或任何 `warning/error IL####` 都会失败，不能以诊断抑制替代修复。

回归先将 `Kun.Errors`、`Kun.Core`、`Kun.MultiTenancy`、`Kun.Security.Abstractions`、`Kun.Security` 和 `Kun.Gateway` 按普通类库打包到临时源，使用隔离的 obj/bin；宿主模板再复制到仓库外，通过唯一版本的 PackageReference 消费这些包。源映射限定 Kun 包来自本次制品，assets 文件不得含项目引用，发布文件必须是 ELF 且不存在托管入口 DLL。

原生宿主启动真实 HTTP 监听并请求公开／内部 JWKS：回退授权策略下匿名可达，生产环境未选择启用内部端点时为 404，两个 additionalGate 均能返回 404；成功响应带 no-store、源生成 JSON 与不含私钥的 RSA 公钥。原子替换 PEM 后，两端点都同时发布新旧公钥，超过重叠期后同步移除旧钥。JSON 反射与宿主 RDG 也关闭，避免消费者偶然补偿库制品的缺陷。

用例结束会停止宿主并删除临时密钥、包与构建目录。测试失败信息保留子进程输出；进程有超时和进程树清理。此测试只证明 JWKS 包消费链路，不将 EF 适配器或未执行的网关路径宣称为 AOT 兼容。

## 独立宿主的原生发布

`GatewayStandaloneAotPublishTests` 在 Linux 与 macOS 上按本机 RID 原生发布 `hosts/Kun.Gateway.Standalone`（隔离的 obj/bin，ILC 有任何警告即失败），以发布目录为内容根启动原生可执行文件：监听地址由系统分配、从启动日志读出；`/healthz` 有上限地轮询到 200 之后，核对 `/readyz` 为 200、JWKS 匿名可达且只含公钥并带 no-store、要认证的代理路由对匿名请求返回 401。签名密钥是用例生成的临时 PEM，可观测性的导出关掉。用例结束停止宿主并删除临时目录；失败信息带上宿主的标准错误与标准输出。AOT 冒烟宿主整体扎根的是网关的库，宿主才用到的 JwtBearer、gRPC-Web、健康检查与可观测性由这条用例过 ILC。
