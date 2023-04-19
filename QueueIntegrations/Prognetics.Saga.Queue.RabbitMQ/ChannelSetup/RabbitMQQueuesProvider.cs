using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQQueuesProvider : IRabbitMQQueuesProvider
{
    public RabbitMQQueuesProvider(
        ISagaModelProvider sagaModelProvider,
        RabbitMQSagaOptions options)
    {
        Queues = sagaModelProvider.Model.Transactions
            .SelectMany(x => x.Steps)
            .Aggregate(
                new List<RabbitMQQueue>(),
                (acc, step) =>
                {
                    acc.Add(new RabbitMQQueue { Name = step.From });
                    acc.Add(new RabbitMQQueue { Name = step.To });
                    return acc;
                });
        Exchange = options.Exchange;
    }

    public IReadOnlyList<RabbitMQQueue> Queues { get; }
    public string Exchange { get; }
}