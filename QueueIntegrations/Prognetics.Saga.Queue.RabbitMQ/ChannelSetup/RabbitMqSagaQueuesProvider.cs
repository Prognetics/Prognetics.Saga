using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

class RabbitMqSagaQueuesProvider : IRabbitMqSagaQueuesProvider
{
    public RabbitMqSagaQueuesProvider(
        SagaModel sagaModel,
        RabbitMqSagaOptions options)
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
        Exchange = options.Exchange;
    }

    public IReadOnlyList<RabbitMqQueue> Queues { get; }
    public string Exchange { get; }
}