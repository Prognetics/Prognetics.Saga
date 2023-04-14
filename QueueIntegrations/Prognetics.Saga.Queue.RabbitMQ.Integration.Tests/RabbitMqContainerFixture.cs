using Testcontainers.RabbitMq;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;

public class RabbitMQContainerFixture : IAsyncLifetime
{
    public RabbitMqContainer Container { get; }

    public RabbitMQContainerFixture()
    {
        Container = new RabbitMqBuilder().Build();
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}
 