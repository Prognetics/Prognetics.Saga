using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public class RabbitMQSagaHostBuilder
{
    private ILogger<IRabbitMQSagaHost> _logger = NullLogger<IRabbitMQSagaHost>.Instance;
    private ISagaModelProvider _modelProvider = EmptySagaModelProvider.Instance;

    public RabbitMQSagaHostBuilder SetModelProvider(ISagaModelProvider modelProvider)
    {
        _modelProvider = modelProvider;
        return this;
    }

    public RabbitMQSagaHostBuilder SetLogger(ILogger<IRabbitMQSagaHost> logger)
    {
        _logger = logger;
        return this;
    }

    public ISagaHost Build(RabbitMQSagaOptions? options = default)
    {
        options ??= RabbitMQSagaOptions.Default;

        var serializer = options.ContentType == "application/json"
            ? new RabbitMQSagaJsonSerializer()
            : throw new NotSupportedException($"Provided content type not supported: {options.ContentType}");

        return new RabbitMQSagaHost(
            new RabbitMQConnectionFactory(options),
            new RabbitMQSagaQueuesProvider(_modelProvider, options),
            new RabbitMQSagaConsumersFactory(
                _modelProvider,
                new RabbitMQSagaConsumerFactory(
                    options,
                    serializer)),
            new RabbitMQSagaSubscriberFactory(serializer, options),
            _logger);
    }
}