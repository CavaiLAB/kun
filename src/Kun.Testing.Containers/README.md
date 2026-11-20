# Kun.Testing.Containers

在 xUnit v3 测试中以类夹具或集合夹具启动和释放 Testcontainers。项目需引用此包与 xUnit v3。宿主需有 Docker。CI 或设置 `QIAN_REQUIRE_DOCKER=1` 的环境会将 Docker 缺失视为失败。容器启动、认证、镜像下载和就绪检查的错误会传播到测试。

```csharp
using Kun.Testing.Containers;
using Xunit;

public class DatabaseTests(PostgresFixture database) : IClassFixture<PostgresFixture>
{
    [Fact]
    public void ApplicationConnectionIsAvailable() =>
        Assert.False(string.IsNullOrEmpty(database.AppConnectionString));
}
```

`PostgresFixture.ConnectionString` 用于建表和准备数据。应用侧使用 `AppConnectionString`，该角色具有 DML 权限并受行级安全约束。每个夹具实例生成独立口令。`RabbitMqFixture.Image` 可为单次测试指定兼容版本。`PreservePortAcrossRestarts` 在需要验证同一个 broker 重启时保留宿主端口。

| 夹具或服务 | 默认镜像 | 镜像覆盖环境变量 |
|---|---|---|
| PostgreSQL | postgres:16-alpine | QIAN_TEST_IMAGE_POSTGRES |
| Redis | redis:7-alpine | QIAN_TEST_IMAGE_REDIS |
| RabbitMQ | rabbitmq:4.3-alpine | QIAN_TEST_IMAGE_RABBITMQ |
| RabbitMQ 3.13 兼容性测试 | rabbitmq:3.13-management | QIAN_TEST_IMAGE_RABBITMQ_313 |
| PgBouncer | edoburu/pgbouncer，按清单摘要固定 | QIAN_TEST_IMAGE_PGBOUNCER |
| S3 兼容服务 | ghcr.io/rustfs/rustfs:1.0.6，按清单摘要固定 | QIAN_TEST_IMAGE_RUSTFS |
| K3s | rancher/k3s:v1.31.5-k3s1 | QIAN_TEST_IMAGE_K3S |
| Authentik | ghcr.io/goauthentik/server:2026.8.5 | QIAN_TEST_IMAGE_AUTHENTIK |
| Apollo 协议替身 | python:3.13-alpine | QIAN_TEST_IMAGE_APOLLO_MOCK |
| Dex | ghcr.io/dexidp/dex:v2.41.1 | QIAN_TEST_IMAGE_DEX |

只在实例化相应夹具时拉取镜像。`InfrastructureFixture` 同时使用 PostgreSQL、Redis、RabbitMQ 和 Apollo 协议替身。真实 Apollo 验收与第三方 SQL 资源位于仓库的 `tests/Kun.Testing.PlatformFixtures`。
