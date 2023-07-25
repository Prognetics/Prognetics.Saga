﻿using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQQueuesProvider : IRabbitMQQueuesProvider
{
    public IReadOnlyList<RabbitMQQueue> GetQueues(TransactionsLedger sagaModel)
        => sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .Aggregate(
                new List<RabbitMQQueue>(),
                (acc, step) =>
                {
                    acc.Add(new RabbitMQQueue { Name = step.CompletionEventName });
                    acc.Add(new RabbitMQQueue { Name = step.NextEventName });
                    return acc;
                });
}