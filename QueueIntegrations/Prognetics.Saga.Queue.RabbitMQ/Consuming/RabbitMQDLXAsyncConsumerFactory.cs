﻿using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQDLXAsyncConsumerFactory : IRabbitMQDLXConsumerFactory
{
    private readonly IRabbitMQSagaSerializer _serializer;

    public RabbitMQDLXAsyncConsumerFactory(IRabbitMQSagaSerializer serializer)
    {
        _serializer = serializer;
    }

    public IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator orchestrator)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (sender, e) =>
        {
            var message = _serializer.Deserialize<OutputMessage>(e.Body);

            if (message is null)
            {
                return;
            }

            await orchestrator.Rollback(message.TransactionId);
            channel.BasicAck(e.DeliveryTag, false);
        };

        return consumer;
    }
}