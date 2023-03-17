using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMqChannelFactory
{
    IModel Create();
}
