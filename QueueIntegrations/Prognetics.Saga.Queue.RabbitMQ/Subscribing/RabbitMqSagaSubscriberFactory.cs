using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Subscribing;

class RabbitMqSagaSubscriberFactory : IRabbitMqSagaSubscriberFactory
{
    private readonly IRabbitMqSagaSerializer _serializer;
    private readonly RabbitMqSagaOptions _options;

    public RabbitMqSagaSubscriberFactory(
        IRabbitMqSagaSerializer serializer,
        RabbitMqSagaOptions options)
    {
        _serializer = serializer;
        _options = options;
    }

    public ISagaSubscriber Create(IModel model)
        => new RabbitMqSagaSubscriber(
            _serializer,
            model,
            _options);
}
