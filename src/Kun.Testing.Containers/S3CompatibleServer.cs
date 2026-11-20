using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Kun.Testing.Containers;

/// <summary>
/// 存储集成测试用的 S3 兼容服务容器：RustFS（Apache-2.0）。
/// </summary>
/// <remarks>
/// <para>
/// 镜像钉到版本与摘要。RustFS 项目自己在 GHCR 与 Docker Hub 发布同一个摘要，两处都能匿名拉取，
/// 这里用 GHCR。原来用的 <c>minio/minio</c> 已经从 Docker Hub 删除，Quay 上的同名镜像要登录才能拉。
/// </para>
/// <para>
/// 存储测试用到的 S3 能力它都支持并照规范校验：SigV4 头签名（含带尾部校验和的 aws-chunked 流式上传）、
/// 预签名 PUT 与 GET（AWSSDK 对自定义端点生成的是 SigV2 查询签名；含 <c>response-content-disposition</c>
/// 覆盖）、对象的 Content-Type 与 Content-Disposition、ListObjectsV2，以及浏览器表单上传（POST Object）
/// 的 SigV4 签名策略与 <c>content-length-range</c> 条件：超限时返回 400（EntityTooLarge），对象不落盘。
/// </para>
/// </remarks>
public sealed class S3CompatibleServer : IAsyncDisposable
{
    /// <summary>RustFS 1.0.6，多架构索引的摘要（linux/amd64、linux/arm64）。</summary>
    public static string Image => ContainerImages.RustFs;

    /// <summary>根用户的访问密钥。</summary>
    public const string AccessKey = "kunadmin";

    /// <summary>根用户的私有密钥。</summary>
    public const string SecretKey = "kunadmin-secret";

    private const ushort S3Port = 9000;

    private readonly IContainer _container;

    private S3CompatibleServer(IContainer container) => _container = container;

    /// <summary>S3 端点，形如 <c>http://127.0.0.1:端口/</c>。启动之后才有效。</summary>
    public string Endpoint =>
        new UriBuilder(Uri.UriSchemeHttp, _container.Hostname, _container.GetMappedPublicPort(S3Port)).ToString();

    /// <summary>
    /// 构建容器，不启动。本机没有容器环境时，Testcontainers 在这里抛出消息含 Docker 的
    /// <see cref="ArgumentException"/>，调用方据此交给 <see cref="DockerRequirement.ReportUnavailable"/>。
    /// </summary>
    public static S3CompatibleServer Create() =>
        new(new ContainerBuilder(Image)
            .WithPortBinding(S3Port, assignRandomHostPort: true)
            .WithEnvironment("RUSTFS_ACCESS_KEY", AccessKey)
            .WithEnvironment("RUSTFS_SECRET_KEY", SecretKey)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPort(S3Port).ForPath("/health/ready")))
            .Build());

    /// <summary>启动容器并等到服务就绪。拉不到镜像或起不来时照常抛出，不跳过。</summary>
    public Task StartAsync(CancellationToken cancellationToken) => _container.StartAsync(cancellationToken);

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
