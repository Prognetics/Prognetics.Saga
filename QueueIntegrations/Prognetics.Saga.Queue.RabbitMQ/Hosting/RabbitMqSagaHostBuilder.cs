using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public class RabbitMqSagaHostBuilder : IRabbitMqSagaHostBuilder
{
    private RabbitMqSagaOptions _options = new();
    private ILogger<IRabbitMqSagaHost>? _logger;

    public RabbitMqSagaHostBuilder With(RabbitMqSagaOptions options)
    {
        _options = options;
        return this;
    }

    public RabbitMqSagaHostBuilder With(ILogger<IRabbitMqSagaHost> logger)
    {
        _logger = logger;
        return this;
    }

    public IRabbitMqSagaHost Build(SagaModel sagaModel)
    {
        var sagaOrchestrator = SagaOrchestratorBuilder.Build(sagaModel);
        var serializer = _options.ContentType == "application/json"
            ? new RabbitMqSagaJsonSerializer()
            : throw new NotSupportedException($"Provided content type not supported: {_options.ContentType}");

        return new RabbitMqSagaHost(
            new RabbitMqConnectionFactory(_options),
            new RabbitMqSagaQueuesProvider(sagaModel, _options),
            sagaOrchestrator,
            new RabbitMqSagaConsumersFactory(
                sagaModel,
                new RabbitMqSagaConsumerFactory(
                    _options,
                    serializer)),
            new RabbitMqSagaSubscriberFactory(serializer, _options),
            _logger ?? NullLogger<IRabbitMqSagaHost>.Instance);
    }
}