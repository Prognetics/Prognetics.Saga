using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ;
public class RabbitMqSagaHostingService
{
    private readonly IRabbitMqChannelFactory _rabbitMqChannelFactory;
    private readonly ISagaQueue _sagaQueue;
    private readonly IRabbitMqSagaConsumersFactory _rabbitMqSagaConsumersFactory;
    private readonly IRabbitMqSagaSubscriberFactory _sagaSubscriberFactory;

    public RabbitMqSagaHostingService(
        IRabbitMqChannelFactory rabbitMqChannelFactory,
        ISagaQueue sagaQueue,
        IRabbitMqSagaConsumersFactory rabbitMqSagaConsumersFactory,
        IRabbitMqSagaSubscriberFactory sagaSubscriberFactory)
    {
        _rabbitMqChannelFactory = rabbitMqChannelFactory;
        _sagaQueue = sagaQueue;
        _rabbitMqSagaConsumersFactory = rabbitMqSagaConsumersFactory;
        _sagaSubscriberFactory = sagaSubscriberFactory;
    }

    public async Task Listen(CancellationToken cancellationToken)
    {
        using var channel = _rabbitMqChannelFactory.Create();
        var consumers = _rabbitMqSagaConsumersFactory.Create(channel, _sagaQueue);

        foreach (var consumer in consumers)
        {
            channel.BasicConsume(
                consumer.BasicConsumer,
                consumer.Queue,
                consumer.AutoAck,
                consumer.ConsumerTag,
                consumer.NoLocal,
                consumer.Exclusive,
                consumer.Arguments);
        }

        var sagaSubscriber = _sagaSubscriberFactory.Create(channel);
        _sagaQueue.Subscribe(sagaSubscriber);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}