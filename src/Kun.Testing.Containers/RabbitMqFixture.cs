using System.Net;
using System.Net.Sockets;
using Testcontainers.RabbitMq;

namespace Kun.Testing.Containers;

/// <summary>RabbitMQ 容器。韧性测试可经 <see cref="ContainerFixture{TContainer}.Container"/> 停启 broker。</summary>
public class RabbitMqFixture : ContainerFixture<RabbitMqContainer>
{
    /// <summary>本次运行的 RabbitMQ 镜像，默认取 ContainerImages.RabbitMq。</summary>
    public string Image { get; init; } = ContainerImages.RabbitMq;

    /// <summary>重启同一实例时保持宿主映射端口。</summary>
    public bool PreservePortAcrossRestarts { get; init; }

    /// <summary>AMQP 连接串（<c>amqp://user:password@host:port/</c>）。</summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <inheritdoc />
    protected override RabbitMqContainer CreateContainer()
    {
        var builder = new RabbitMqBuilder(Image).WithUsername("qian").WithPassword(NewSecret());
        if (PreservePortAcrossRestarts)
        {
            using var reservation = new TcpListener(IPAddress.Loopback, 0);
            reservation.Start();
            var port = ((IPEndPoint)reservation.LocalEndpoint).Port;
            reservation.Stop();
            builder = builder.WithPortBinding(port, 5672);
        }
        return builder.Build();
    }

    /// <inheritdoc />
    protected override ValueTask OnStartedAsync(RabbitMqContainer container, CancellationToken cancellationToken)
    {
        ConnectionString = container.GetConnectionString();
        return ValueTask.CompletedTask;
    }
}
