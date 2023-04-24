using Prognetics.Saga.Orchestrator.Model;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQQueuesProvider : IRabbitMQQueuesProvider
{
    public IReadOnlyList<RabbitMQQueue> GetQueues(SagaModel sagaModel)
        => sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .Aggregate(
                new List<RabbitMQQueue>(),
                (acc, step) =>
                {
                    acc.Add(new RabbitMQQueue { Name = step.From });
                    acc.Add(new RabbitMQQueue { Name = step.To });
                    return acc;
                });
}