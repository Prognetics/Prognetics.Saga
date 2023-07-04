using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQQueuesProvider : IRabbitMQQueuesProvider
{
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly RabbitMQSagaOptions _rabbitMQOptions;

    public RabbitMQQueuesProvider(
        ITransactionLedgerAccessor transactionLedgerAccessor,
        RabbitMQSagaOptions rabbitMQOptions)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _rabbitMQOptions = rabbitMQOptions;
    }

    public IReadOnlyList<RabbitMQQueue> GetQueues()
    {
        var dlxQueue = new RabbitMQQueue
        {
            Name = _rabbitMQOptions.DLXQueue,
            Exchange = _rabbitMQOptions.DLXExchange,
        };

        var queues = new List<RabbitMQQueue> { dlxQueue };

        var steps = _transactionLedgerAccessor
            .TransactionsLedger
            .Transactions
            .SelectMany(x => x.Steps);

        foreach (var step in steps)
        {
            queues.Add(new RabbitMQQueue
            {
                Name = step.EventName,
                Exchange = _rabbitMQOptions.Exchange,
            });
            queues.Add(new RabbitMQQueue
            {
                Name = step.CompletionEventName,
                Exchange = _rabbitMQOptions.Exchange,
                Arguments = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", dlxQueue.Exchange }
                }
            });
        }

        return queues;
    }
}