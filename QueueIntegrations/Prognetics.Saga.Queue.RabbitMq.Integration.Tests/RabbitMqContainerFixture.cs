using NSubstitute;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Prognetics.Saga.Queue.RabbitMq.Integration.Tests;

public class RabbitMqContainerFixture : IAsyncLifetime
{
    public RabbitMqContainer Container { get; }

    public RabbitMqContainerFixture()
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
