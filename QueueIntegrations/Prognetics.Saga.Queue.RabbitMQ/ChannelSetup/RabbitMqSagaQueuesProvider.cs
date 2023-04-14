using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

class RabbitMQSagaQueuesProvider : IRabbitMQSagaQueuesProvider
{
    public RabbitMQSagaQueuesProvider(
        SagaModel sagaModel,
        RabbitMQSagaOptions options)
    {
        Queues = sagaModel.Transactions
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