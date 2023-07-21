using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQConsumersFactory : IRabbitMQConsumersFactory
{
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IRabbitMQConsumerFactory _rabbitMqSagaConsumerFactory;

    public RabbitMQConsumersFactory(
        ITransactionLedgerAccessor transactionLedgerAccessor,
        IRabbitMQConsumerFactory rabbitMqSagaConsumerFactory)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _rabbitMqSagaConsumerFactory = rabbitMqSagaConsumerFactory;
    }

    public IReadOnlyList<RabbitMQConsumer> Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator)
    {
        var consumer = _rabbitMqSagaConsumerFactory.Create(
            channel,
            sagaOrchestrator);

        return _transactionLedgerAccessor.TransactionsLedger.Transactions
            .SelectMany(x => x.Steps)
            .Select(x => new RabbitMQConsumer
            {
                Queue = x.EventName,
                BasicConsumer = consumer
            })
            .ToList();
    }
}

