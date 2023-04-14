using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQSagaConsumersFactory
{
    IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaQueue);
}