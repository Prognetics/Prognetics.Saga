using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMqSagaConsumersFactory
{
    IReadOnlyList<RabbitMqConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaQueue);
}