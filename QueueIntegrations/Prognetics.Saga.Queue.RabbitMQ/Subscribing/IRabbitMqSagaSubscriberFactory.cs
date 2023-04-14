using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Subscribing;

public interface IRabbitMQSagaSubscriberFactory
{
    ISagaSubscriber Create(IModel model);
}