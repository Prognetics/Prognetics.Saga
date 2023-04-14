using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQSagaConsumerFactory
{
    IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator sagaQueue);
}