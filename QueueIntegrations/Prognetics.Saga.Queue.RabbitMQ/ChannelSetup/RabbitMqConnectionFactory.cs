using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly RabbitMqSagaOptions _options;

    public RabbitMqConnectionFactory(RabbitMqSagaOptions options)
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