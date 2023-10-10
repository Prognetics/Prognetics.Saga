namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMQQueuesProvider
{
    IReadOnlyList<RabbitMQQueue> GetQueues();
}