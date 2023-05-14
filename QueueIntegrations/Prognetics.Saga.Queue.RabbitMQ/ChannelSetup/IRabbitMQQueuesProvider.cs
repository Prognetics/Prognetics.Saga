using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public interface IRabbitMQQueuesProvider
{
    IReadOnlyList<RabbitMQQueue> GetQueues(TransactionsLedger sagaModel);
}