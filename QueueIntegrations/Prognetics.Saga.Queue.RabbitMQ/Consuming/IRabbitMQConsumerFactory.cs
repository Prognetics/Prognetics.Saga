using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQConsumerFactory
{
    IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator sagaQueue);
}