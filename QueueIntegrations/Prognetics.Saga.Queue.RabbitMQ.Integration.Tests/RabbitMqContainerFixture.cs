using RabbitMQ.Client;
using System.ComponentModel;
using System.Data.Common;
using Testcontainers.RabbitMq;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;

public class RabbitMQContainerFixture : IAsyncLifetime
{
    private IConnection _connection = null!;
    public RabbitMqContainer Container { get; }

    public RabbitMQContainerFixture()
    {
        Container = new RabbitMqBuilder().Build();
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

    }

    public IConnection Connection =>
        new ConnectionFactory
        {
            Uri = new Uri(Container.GetConnectionString())
        }.CreateConnection();

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}
 