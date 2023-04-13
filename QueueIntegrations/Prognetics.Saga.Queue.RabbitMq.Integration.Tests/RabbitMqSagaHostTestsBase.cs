using Microsoft.Extensions.Logging;
using NSubstitute;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMq.Integration.Tests;

internal class RabbitMqSagaHostTestsBuilder
{
    public SagaModel SagaModel { get; set; } = new();
    public RabbitMqSagaOptions RabbitMqSagaOptions { get; set; } = new();
    public IRabbitMqSagaSerializer? RabbitMqSagaSerializer { get; set; }
    public IRabbitMqConnectionFactory? RabbitMqConnectionFactory { get; set; }
    public ISagaOrchestrator? SagaOrchestrator { get; set; }
    public IRabbitMqSagaQueuesProvider? QueuesProvider { get; set; }
    public IRabbitMqSagaConsumersFactory? RabbitMqSagaConsumersFactory { get; set; }
    public IRabbitMqSagaConsumerFactory? RabbitMqSagaConsumerFactory { get; set; }
    public IRabbitMqSagaSubscriberFactory? SagaSubscriberFactory { get; set; }
    public EventHandler<CallbackExceptionEventArgs> OnExceptionHandler { get; set; } = (_, _) => { };
    public EventHandler<ShutdownEventArgs> OnShutdownHandler { get;set; } = (_, _) => { };

    public (RabbitMqSagaHost Host, RabbitMqSagaHostDependencies Dependencies) Build()
    {
        var serializer = RabbitMqSagaSerializer ?? new RabbitMqSagaJsonSerializer();
        var consumerFactory = RabbitMqSagaConsumerFactory ?? new RabbitMqSagaConsumerFactory(
                    RabbitMqSagaOptions,
                    serializer);

        var dependencies = new RabbitMqSagaHostDependencies
        {
            SagaModel = SagaModel,
            RabbitMqSagaOptions = RabbitMqSagaOptions,
            RabbitMqSagaSerializer = serializer,
            RabbitMqConnectionFactory = RabbitMqConnectionFactory ?? new RabbitMqConnectionFactory(RabbitMqSagaOptions),
            QueuesProvider = QueuesProvider ?? new RabbitMqSagaQueuesProvider(SagaModel, RabbitMqSagaOptions),
            SagaOrchestrator = SagaOrchestrator ?? SagaOrchestratorBuilder.Build(SagaModel),
            RabbitMqSagaConsumerFactory = consumerFactory,
            RabbitMqSagaConsumersFactory = RabbitMqSagaConsumersFactory ?? new RabbitMqSagaConsumersFactory(
                SagaModel,
                consumerFactory),
            SagaSubscriberFactory = SagaSubscriberFactory ?? new RabbitMqSagaSubscriberFactory(
                serializer,
                RabbitMqSagaOptions),
            OnExceptionHandler = OnExceptionHandler,
            OnShutdownHandler = OnShutdownHandler,
    };

        return (
            new(
                dependencies.RabbitMqConnectionFactory,
                dependencies.QueuesProvider,
                dependencies.SagaOrchestrator,
                dependencies.RabbitMqSagaConsumersFactory,
                dependencies.SagaSubscriberFactory,
                dependencies.OnExceptionHandler,
                dependencies.OnShutdownHandler),
            dependencies);
    }
}

internal class RabbitMqSagaHostDependencies
{
    public required SagaModel SagaModel { get; init; }
    public required RabbitMqSagaOptions RabbitMqSagaOptions { get; init; }
    public required IRabbitMqSagaSerializer RabbitMqSagaSerializer { get; init; }
    public required IRabbitMqConnectionFactory RabbitMqConnectionFactory { get; init; }
    public required ISagaOrchestrator SagaOrchestrator { get; init; }
    public required IRabbitMqSagaQueuesProvider QueuesProvider { get; init; }
    public required IRabbitMqSagaConsumersFactory RabbitMqSagaConsumersFactory { get; init; }
    public required IRabbitMqSagaConsumerFactory RabbitMqSagaConsumerFactory { get; init; }
    public required IRabbitMqSagaSubscriberFactory SagaSubscriberFactory { get; init; }
    public required EventHandler<CallbackExceptionEventArgs> OnExceptionHandler { get; init; }
    public required EventHandler<ShutdownEventArgs> OnShutdownHandler { get; init; }
}