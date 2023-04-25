using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Orchestrator.DTO;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public class RabbitMQAsyncConsumerFactory : IRabbitMQConsumerFactory
{
    private readonly IRabbitMQSagaSerializer _serializer;

    public RabbitMQAsyncConsumerFactory(
        IRabbitMQSagaSerializer serializer)
        => _serializer = serializer;

    public IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator orchestrator)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (sender, e) =>
        {
            var inputMessage = _serializer.Deserialize<InputMessage>(e.Body);

            if (inputMessage is null)
            {
                return;
            }

            await orchestrator.Push(e.RoutingKey, inputMessage);
            channel.BasicAck(e.DeliveryTag, false);
        };

        return consumer;
    }
}

