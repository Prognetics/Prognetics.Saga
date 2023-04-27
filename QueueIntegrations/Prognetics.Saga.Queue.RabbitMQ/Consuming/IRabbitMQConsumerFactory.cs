using Prognetics.Saga.Orchestrator.Contract;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQConsumerFactory
{
    IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator sagaQueue);
}