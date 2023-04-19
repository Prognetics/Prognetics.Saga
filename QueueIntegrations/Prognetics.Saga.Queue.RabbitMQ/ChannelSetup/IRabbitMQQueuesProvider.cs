namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMQQueuesProvider
{
    public IReadOnlyList<RabbitMQQueue> Queues { get; }
    public string Exchange { get; }
}