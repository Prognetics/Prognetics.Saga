using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMqConnectionFactory
{
    IConnection Create();
}
