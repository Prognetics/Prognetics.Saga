using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
    private readonly RabbitMQSagaOptions _options;

    public RabbitMQConnectionFactory(RabbitMQSagaOptions options)
        => _options = options;

    public IConnection Create()
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            DispatchConsumersAsync = _options.DispatchConsumersAsync,
        };

        return connectionFactory.CreateConnection();
    }
}