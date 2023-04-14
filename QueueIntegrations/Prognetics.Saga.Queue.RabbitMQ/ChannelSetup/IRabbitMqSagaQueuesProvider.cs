namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMQSagaQueuesProvider
{
    public IReadOnlyList<RabbitMQQueue> Queues { get; }
    public string Exchange { get; }
}