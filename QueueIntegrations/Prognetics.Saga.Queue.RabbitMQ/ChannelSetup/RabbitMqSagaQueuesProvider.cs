using Prognetics.Saga.Orchestrator;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

class RabbitMqSagaQueuesProvider : IRabbitMqSagaQueuesProvider
{
    public RabbitMqSagaQueuesProvider(ISagaModel sagaModel)
    {
        Queues = sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .Aggregate(
                new List<RabbitMqQueue>(),
                (acc, step) =>
                {
                    acc.Add(new RabbitMqQueue { Name = step.From });
                    acc.Add(new RabbitMqQueue { Name = step.To });
                    return acc;
                });
    }

    public IReadOnlyList<RabbitMqQueue> Queues { get; }
}