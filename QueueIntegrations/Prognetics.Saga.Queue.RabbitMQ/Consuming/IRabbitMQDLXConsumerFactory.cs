using Prognetics.Saga.Orchestrator.Contract;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public interface IRabbitMQDLXConsumerFactory
{
    IBasicConsumer Create(IModel channel, ISagaOrchestrator orchestrator);
}
