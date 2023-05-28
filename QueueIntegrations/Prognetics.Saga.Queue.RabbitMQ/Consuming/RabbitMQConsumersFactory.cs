using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQConsumersFactory : IRabbitMQConsumersFactory
{
    private readonly IRabbitMQConsumerFactory _rabbitMqSagaConsumerFactory;

    public RabbitMQConsumersFactory(IRabbitMQConsumerFactory rabbitMqSagaConsumerFactory)
    {
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
    }

    public IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator,
        SagaModel model)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return model.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMQConsumer
            {
                Queue = x.EventName,
                BasicConsumer = consumer
            })
            .ToList();
    }
}

