using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ;

public interface IRabbitMqConnectionFactory
{
    IConnection Create();
}

class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly RabbitMqSagSettings _settings;

    public RabbitMqConnectionFactory(RabbitMqSagSettings settings)
	{
        _settings = settings;
    }

    public IConnection Create()
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(_settings.ConnectionString),
            DispatchConsumersAsync = true,
        };
        return connectionFactory.CreateConnection();
    }
}