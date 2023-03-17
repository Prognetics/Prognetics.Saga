using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

internal interface IRabbitMqSagaConsumerFactory
{
    IBasicConsumer Create(
        IModel channel,
        ISagaQueue sagaQueue);
}