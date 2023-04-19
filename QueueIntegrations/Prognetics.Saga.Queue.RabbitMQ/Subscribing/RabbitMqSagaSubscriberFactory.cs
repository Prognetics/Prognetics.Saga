using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Subscribing;

public class RabbitMQSagaSubscriberFactory : IRabbitMQSagaSubscriberFactory
{
    private readonly IRabbitMQSagaSerializer _serializer;
    private readonly RabbitMQSagaOptions _options;

    public RabbitMQSagaSubscriberFactory(
        IRabbitMQSagaSerializer serializer,
        RabbitMQSagaOptions options)
    {
        _serializer = serializer;
        _options = options;
    }

    public ISagaSubscriber Create(IModel model)
        => new RabbitMQSagaSubscriber(
            _serializer,
            model,
            _options);
}
