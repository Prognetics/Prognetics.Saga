using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

class RabbitMQSagaConsumersFactory : IRabbitMQSagaConsumersFactory
{
    private readonly SagaModel _sagaModel;
    private readonly IRabbitMQSagaConsumerFactory _rabbitMqSagaConsumerFactory;

    public RabbitMQSagaConsumersFactory(
        SagaModel sagaModel,
        IRabbitMQSagaConsumerFactory rabbitMqSagaConsumerFactory)
    {
        _sagaModel = sagaModel;
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
    }

    public IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return _sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMQConsumer
            {
                Queue = x.From,
                BasicConsumer = consumer
            })
            .ToList();
    }
}

