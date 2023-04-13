namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMqSagaQueuesProvider
{
    public IReadOnlyList<RabbitMqQueue> Queues { get; }
    public string Exchange { get; }
}