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

    public RabbitMQSagaHostBuilder SetLogger(ILogger<IRabbitMQSagaHost> logger)
    {
        _logger = logger;
        return this;
    }

    public ISagaClient Build(RabbitMQSagaOptions? options = default)
    {
        options ??= RabbitMQSagaOptions.Default;

        var serializer = options.ContentType == "application/json"
            ? new RabbitMQSagaJsonSerializer()
            : throw new NotSupportedException($"Provided content type not supported: {options.ContentType}");

        return new RabbitMQSagaClient(
            new RabbitMQConnectionFactory(options),
            new RabbitMQQueuesProvider(),
            new RabbitMQConsumersFactory(
                options.DispatchConsumersAsync 
                    ? new RabbitMQAsyncConsumerFactory(serializer)
                    : new RabbitMQConsumerFactory(serializer)),
            new RabbitMQSagaSubscriberFactory(serializer, options),
            options,
            _logger);
    }
}