using Prognetics.Saga.Orchestrator.Contract;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQDlxConsumerFactory
{
    IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator sagaQueue);
}