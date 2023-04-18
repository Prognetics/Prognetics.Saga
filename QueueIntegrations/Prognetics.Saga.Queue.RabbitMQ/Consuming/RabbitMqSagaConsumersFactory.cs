using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

class RabbitMQSagaConsumersFactory : IRabbitMQSagaConsumersFactory
{
    private readonly ISagaModelProvider _modelProvider;
    private readonly IRabbitMQSagaConsumerFactory _rabbitMqSagaConsumerFactory;

    public RabbitMQSagaConsumersFactory(
        ISagaModelProvider modelProvider,
        IRabbitMQSagaConsumerFactory rabbitMqSagaConsumerFactory)
    {
        _modelProvider = modelProvider;
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
    }

    public IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return _modelProvider.Model.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMQConsumer
            {
                Queue = x.From,
                BasicConsumer = consumer
            })
            .ToList();
    }
}

