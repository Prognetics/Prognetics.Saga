using Microsoft.Extensions.Options;
using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace Prognetics.Saga.Queue.RabbitMQ;

public interface IRabbitMqSagaSubscriberFactory
{
    ISagaSubscriber Create(IModel model);
}

public class RabbitMqSagaSubscriberFactory : IRabbitMqSagaSubscriberFactory
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
