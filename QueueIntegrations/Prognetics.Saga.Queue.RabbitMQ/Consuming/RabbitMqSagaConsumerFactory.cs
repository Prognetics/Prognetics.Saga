using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

class RabbitMQSagaConsumerFactory : IRabbitMQSagaConsumerFactory
{
    private readonly RabbitMQSagaOptions _options;
    private readonly IRabbitMQSagaSerializer _serializer;

    public RabbitMQSagaConsumerFactory(
        RabbitMQSagaOptions options,
        IRabbitMQSagaSerializer serializer)
    {
        _options = options;
        _serializer = serializer;
    }

    public IBasicConsumer Create(
        IModel channel,
        ISagaOrchestrator sagaOrchestrator)
        => _options.DispatchConsumersAsync
        ? CreateAsyncConsumer(channel, sagaOrchestrator)
        : CreateBasicConsumer(channel, sagaOrchestrator);

    public IBasicConsumer CreateAsyncConsumer(
        IModel channel,
        ISagaOrchestrator sagaQueue)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (sender, e) =>
        {
            var inputMessage = _serializer.Deserialize<InputMessage>(e.Body);

            if (inputMessage is null)
            {
                return;
            }
            
            await sagaQueue.Push(e.RoutingKey, inputMessage);
            channel.BasicAck(e.DeliveryTag, false);
        };

        return consumer;
    }

    public IBasicConsumer CreateBasicConsumer(
        IModel channel,
        ISagaOrchestrator sagaQueue)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var inputMessage = _serializer.Deserialize<InputMessage>(e.Body);

            if (inputMessage is null)
            {
                return;
            }

            sagaQueue.Push(e.RoutingKey, inputMessage).GetAwaiter().GetResult();
            channel.BasicAck(e.DeliveryTag, false);
        };

        return consumer;
    }
}

