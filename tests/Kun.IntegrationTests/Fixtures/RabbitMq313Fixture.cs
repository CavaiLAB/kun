using Kun.Testing.Containers;

namespace Kun.IntegrationTests.Fixtures;

/// <summary>使用 RabbitMQ 3.13 的框架兼容性套件。</summary>
public sealed class RabbitMq313Fixture : RabbitMqFixture
{
    public RabbitMq313Fixture() => Image = ContainerImages.RabbitMq313;
}
