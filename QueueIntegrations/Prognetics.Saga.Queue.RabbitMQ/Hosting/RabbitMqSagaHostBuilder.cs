using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public class RabbitMQSagaHostBuilder : IRabbitMQSagaHostBuilder
{
    private RabbitMQSagaOptions _options = new();
    private ILogger<IRabbitMQSagaHost>? _logger;

    public RabbitMQSagaHostBuilder With(RabbitMQSagaOptions options)
    {
        _options = options;
        return this;
    }

    public RabbitMQSagaHostBuilder With(ILogger<IRabbitMQSagaHost> logger)
    {
        _logger = logger;
        return this;
    }

    public IRabbitMQSagaHost Build(SagaModel sagaModel)
    {
        var sagaOrchestrator = SagaOrchestratorBuilder.Build(sagaModel);
        var serializer = _options.ContentType == "application/json"
            ? new RabbitMQSagaJsonSerializer()
            : throw new NotSupportedException($"Provided content type not supported: {_options.ContentType}");

        return new RabbitMQSagaHost(
            new RabbitMQConnectionFactory(_options),
            new RabbitMQSagaQueuesProvider(sagaModel, _options),
            sagaOrchestrator,
            new RabbitMQSagaConsumersFactory(
                sagaModel,
                new RabbitMQSagaConsumerFactory(
                    _options,
                    serializer)),
            new RabbitMQSagaSubscriberFactory(serializer, _options),
            _logger ?? NullLogger<IRabbitMQSagaHost>.Instance);
    }
}