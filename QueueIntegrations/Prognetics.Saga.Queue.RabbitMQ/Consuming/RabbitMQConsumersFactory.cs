using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;
using System.Linq;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQConsumersFactory : IRabbitMQConsumersFactory
{
    private readonly IRabbitMQConsumerFactory _rabbitMqSagaConsumerFactory;
    private readonly IRabbitMQDLXConsumerFactory _rabbitMqDLXConsumerFactory;
    private readonly RabbitMQSagaOptions _options;

    public RabbitMQConsumersFactory(
        IRabbitMQConsumerFactory rabbitMqSagaConsumerFactory,
        IRabbitMQDLXConsumerFactory rabbitMqDLXConsumerFactory,
        RabbitMQSagaOptions options)
    {
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
        _rabbitMqDLXConsumerFactory = rabbitMqDLXConsumerFactory;
        _options = options;
    }

    public IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator,
        TransactionsLedger model)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        var dlxConsumer = _rabbitMqDLXConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return model.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMQConsumer
            {
                Queue = x.EventName,
                BasicConsumer = consumer
            })
            .Concat(new[]{ new RabbitMQConsumer{
                Queue = _options.DLXQueue,
                BasicConsumer = dlxConsumer
            }})
            .ToList();
    }
}

