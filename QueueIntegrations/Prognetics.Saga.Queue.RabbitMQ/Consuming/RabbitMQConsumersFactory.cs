using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQConsumersFactory : IRabbitMQConsumersFactory
{
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IRabbitMQConsumerFactory _consumerFactory;
    private readonly IRabbitMQDlxConsumerFactory _dlxConsumerFactory;
    private readonly IOptions<RabbitMQSagaOptions> _options;

    public RabbitMQConsumersFactory(
        ITransactionLedgerAccessor transactionLedgerAccessor,
        IRabbitMQConsumerFactory consumerFactory,
        IRabbitMQDlxConsumerFactory dlxConsumerFactory,
        IOptions<RabbitMQSagaOptions> options)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _consumerFactory = consumerFactory;
        _dlxConsumerFactory = dlxConsumerFactory;
        _options = options;
    }

    public IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator)
    {
        var consumer = _consumerFactory.Create(
            channel,
            sagaOrchestrator);

        var dlxConsumer = _dlxConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return _transactionLedgerAccessor.TransactionsLedger.Transactions
            .Aggregate(new List<RabbitMQConsumer>{
                new()
                {
                    Queue = _options.Value.DlxQueue,
                    BasicConsumer = dlxConsumer,
                }},
                (consumers, transaction) =>
                {
                    consumers.AddRange(
                        transaction.Steps.Select(x => new RabbitMQConsumer
                        {
                            Queue = x.CompletionEventName,
                            BasicConsumer = consumer
                        }));
                    return consumers;
                });
    }
}

