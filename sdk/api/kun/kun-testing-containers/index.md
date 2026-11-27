> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Testing.Containers

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[ApolloFixture](/sdk/api/kun/kun-testing-containers/apollofixture.md)

Apollo 配置服务的协议替身（PRD §14.2：集成测试层的 Apollo 用 mock server）。
容器内运行 `ApolloMock/apollo_mock.py`：实现客户端用到的 `/configs`、`/configfiles/json`、
`/notifications/v2` 长轮询、Meta Server 与访问密钥签名；测试经本类的方法发布或撤下配置。
覆盖范围见脚本文件头注释与 tests/README.md。

[ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)

一次发布的结果。

[AuthentikFixture](/sdk/api/kun/kun-testing-containers/authentikfixture.md)

真实 Authentik（server + worker + 专属 PostgreSQL），作为 OIDC 连接器的上游身份源（S1-IDP-007 验收 1）。
启动后用引导令牌经 REST API 开 OAuth2 客户端与用户；[AuthentikFixture.SignInAsync](/sdk/api/kun/kun-testing-containers/authentikfixture.md#Kun_Testing_Containers_AuthentikFixture_SignInAsync_System_Uri_Kun_Testing_Containers_AuthentikFixture_User_System_Uri_System_Threading_CancellationToken_) 像浏览器一样走 Authentik 的登录流程（flow executor），
返回 Authentik 回跳到依赖方回调地址的完整 URL。口令与令牌每次运行随机生成。

[ContainerFixture\<TContainer>](/sdk/api/kun/kun-testing-containers/containerfixture-1.md)

单个容器 fixture 的公共骨架：启动容器（瞬时失败时有界重试）、按 [DockerRequirement](/sdk/api/kun/kun-testing-containers/dockerrequirement.md) 处理 Docker 缺失、随 fixture 一起释放。
直接用作 xUnit 的 class / collection / assembly fixture。

[ContainerImages](/sdk/api/kun/kun-testing-containers/containerimages.md)

集成测试使用的镜像。版本与部署基线一致：PostgreSQL 16、Redis 7（ADR-017 D17.2）；
RabbitMQ 取当前 4.x 稳定线（PRD §14.6）。每个镜像都可用环境变量覆盖，便于与 staging 对齐或换内网镜像源。

[DexFixture](/sdk/api/kun/kun-testing-containers/dexfixture.md)

独立实现的外部 OIDC 发行方（CNCF Dex），用来验证依赖方能对接 Kun 身份服务之外的任意发行方。内存存储、本地密码库里一个用户、一个启用 PKCE 的公开客户端，
跳过授权确认页。Dex 的 issuer 必须在启动前写进配置，所以宿主端口与 RP 回调端口都在启动前选定；口令每次运行随机生成，配置里只有 bcrypt 哈希。

[DockerRequirement](/sdk/api/kun/kun-testing-containers/dockerrequirement.md)

没有 Docker 时集成测试怎么办：本机跳过；CI 上直接失败，因为 PR 级门禁要求集成测试必须真跑（PRD §14.4 C10）。

[InfrastructureFixture](/sdk/api/kun/kun-testing-containers/infrastructurefixture.md)

一次拉起全部四个依赖（PostgreSQL / Redis / RabbitMQ / Apollo 替身），并行启动。
平台插件的集成测试通常同时需要它们，作为 collection 或 assembly fixture 使用，整组测试共享一套容器。

[K3sFixture](/sdk/api/kun/kun-testing-containers/k3sfixture.md)

K3s 单节点集群（特权容器）。[K3sFixture.Kubeconfig](/sdk/api/kun/kun-testing-containers/k3sfixture.md#Kun_Testing_Containers_K3sFixture_Kubeconfig) 里的 API Server 已映射到宿主端口；容器内自带 kubectl（含 kustomize），
可用 [K3sFixture.KubectlAsync](/sdk/api/kun/kun-testing-containers/k3sfixture.md#Kun_Testing_Containers_K3sFixture_KubectlAsync_System_Threading_CancellationToken_System_String___) 代替 Argo 应用部署仓库里的片段。

[AuthentikFixture.OAuthClient](/sdk/api/kun/kun-testing-containers/authentikfixture-oauthclient.md)

为一个依赖方开出的 OAuth2 客户端。

[PgBouncerPostgresFixture](/sdk/api/kun/kun-testing-containers/pgbouncerpostgresfixture.md)

Spins up a Postgres 16 container fronted by `edoburu/pgbouncer` in
`POOL_MODE=transaction`. Exposes both direct and bouncer-fronted connection strings so
tests can assert that `SET LOCAL` / `set_config(..., true)` semantics survive
PgBouncer's backend-connection recycling between transactions.

[PostgresFixture](/sdk/api/kun/kun-testing-containers/postgresfixture.md)

PostgreSQL 16 容器。提供两条连接串：
[PostgresFixture.ConnectionString](/sdk/api/kun/kun-testing-containers/postgresfixture.md#Kun_Testing_Containers_PostgresFixture_ConnectionString) 是超级用户，用于建表、迁移与造数；
[PostgresFixture.AppConnectionString](/sdk/api/kun/kun-testing-containers/postgresfixture.md#Kun_Testing_Containers_PostgresFixture_AppConnectionString) 是 `NOSUPERUSER NOBYPASSRLS` 的应用角色，验证 RLS 的测试必须用它——
超级用户与 `BYPASSRLS` 角色会绕过 `FORCE ROW LEVEL SECURITY`，策略对它们不可见（ADR-004）。

[RabbitMqFixture](/sdk/api/kun/kun-testing-containers/rabbitmqfixture.md)

RabbitMQ 容器。韧性测试可经 [ContainerFixture\<TContainer>.Container](/sdk/api/kun/kun-testing-containers/containerfixture-1.md#Kun_Testing_Containers_ContainerFixture_1_Container) 停启 broker。

[RedisFixture](/sdk/api/kun/kun-testing-containers/redisfixture.md)

Redis 7 容器。

[S3CompatibleServer](/sdk/api/kun/kun-testing-containers/s3compatibleserver.md)

存储集成测试用的 S3 兼容服务容器：RustFS（Apache-2.0）。

[AuthentikFixture.User](/sdk/api/kun/kun-testing-containers/authentikfixture-user.md)

Authentik 里的一个本地用户。
