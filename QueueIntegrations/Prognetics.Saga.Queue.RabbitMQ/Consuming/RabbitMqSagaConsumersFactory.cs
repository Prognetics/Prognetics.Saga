using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

class RabbitMqSagaConsumersFactory : IRabbitMqSagaConsumersFactory
{
    private readonly ISagaModel _sagaModel;
    private readonly IRabbitMqSagaConsumerFactory _rabbitMqSagaConsumerFactory;

    public RabbitMqSagaConsumersFactory(
        ISagaModel sagaModel,
        IRabbitMqSagaConsumerFactory rabbitMqSagaConsumerFactory)
    {
        _sagaModel = sagaModel;
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
    }

    public IReadOnlyList<RabbitMqConsumer> Create(
        IModel channel,
        ISagaQueue sagaQueue)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaQueue);

        return _sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMqConsumer
            {
                Queue = x.To,
                BasicConsumer = consumer
            })
            .ToList();
    }
}

