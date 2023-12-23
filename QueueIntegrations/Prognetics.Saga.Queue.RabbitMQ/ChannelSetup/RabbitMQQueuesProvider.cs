using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQQueuesProvider : IRabbitMQQueuesProvider
{
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IOptions<RabbitMQSagaOptions> _options;

    public RabbitMQQueuesProvider(
        ITransactionLedgerAccessor transactionLedgerAccessor,
        IOptions<RabbitMQSagaOptions> options)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _options = options;
    }

    public IReadOnlyList<RabbitMQQueue> GetQueues()
        => _transactionLedgerAccessor.TransactionsLedger.Transactions
            .SelectMany(x => x.Steps)
            .Aggregate(
                new List<RabbitMQQueue>
                {
                    new()
                    {
                        Name = _options.Value.DlxQueue,
                        Exchange = _options.Value.DlxExchange,
                    }
                },
                (acc, step) =>
                {
                    acc.Add(new() 
                    {
                        Name = step.CompletionEventName,
                        Exchange = _options.Value.Exchange,
                    });

                    acc.Add(new()
                    {
                        Name = step.NextEventName,
                        Exchange = _options.Value.Exchange,
                        Arguments = new Dictionary<string, object>
                        {
                            { RabbitMqConsts.DLX_EXCHANGE_QUEUE_HEADER_NAME, _options.Value.DlxExchange },
                            { RabbitMqConsts.DLX_ROUTING_KEY_QUEUE_HEADER_NAME, _options.Value.DlxQueue },
                        }
                    });

                    acc.Add(new()
                    {
                        Name = step.CompensationEventName,
                        Exchange = _options.Value.Exchange,
                    });
                    return acc;
                });
}