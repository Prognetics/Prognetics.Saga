using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQConsumerFactory : IRabbitMQConsumerFactory
{
    private readonly IRabbitMQSagaSerializer _serializer;

    public RabbitMQConsumerFactory(IRabbitMQSagaSerializer serializer)
    {
        _serializer = serializer;
    }

    public IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator orchestrator)
    { 
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var inputMessage = _serializer.Deserialize<RabbitMqInputMessage>(e.Body);

            if (inputMessage is null)
            {
                return;
            }

            orchestrator.Push(e.RoutingKey, new(
                    inputMessage.TransactionId,
                    inputMessage.Payload,
                    inputMessage.Compensation?.ToString()))
                .GetAwaiter()
                .GetResult();
            channel.BasicAck(e.DeliveryTag, false);
        };

        return consumer;
    }
}