using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQConsumersFactory
{
    IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator orchestrator,
        SagaModel sagaModel);
}