using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMQConnectionFactory
{
    IConnection Create();
}
