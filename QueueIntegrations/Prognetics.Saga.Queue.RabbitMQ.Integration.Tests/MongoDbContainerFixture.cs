using Testcontainers.MongoDb;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;

public class MongoDbContainerFixture : IAsyncLifetime
{
    public MongoDbContainer Container { get; }

    public MongoDbContainerFixture()
    {
        Container = new MongoDbBuilder().Build();
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