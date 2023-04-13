using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

class RabbitMqSagaConsumersFactory : IRabbitMqSagaConsumersFactory
{
    private readonly SagaModel _sagaModel;
    private readonly IRabbitMqSagaConsumerFactory _rabbitMqSagaConsumerFactory;

    public RabbitMqSagaConsumersFactory(
        SagaModel sagaModel,
        IRabbitMqSagaConsumerFactory rabbitMqSagaConsumerFactory)
    {
        _sagaModel = sagaModel;
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
    }

    public IReadOnlyList<RabbitMqConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return _sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMqConsumer
            {
                Queue = x.From,
                BasicConsumer = consumer
            })
            .ToList();
    }
}

