using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

class RabbitMqSagaConsumerFactory : IRabbitMqSagaConsumerFactory
{
    private readonly RabbitMqSagaOptions _options;
    private readonly IRabbitMqSagaSerializer _serializer;

    public RabbitMqSagaConsumerFactory(
        RabbitMqSagaOptions options,
        IRabbitMqSagaSerializer serializer)
    {
        _options = options;
        _serializer = serializer;
    }

    public IBasicConsumer Create(
        IModel channel,
        ISagaQueue sagaQueue)
        => _options.DispatchConsumersAsync
        ? CreateAsyncConsumer(channel, sagaQueue)
        : CreateBasicConsumer(channel, sagaQueue);

    public IBasicConsumer CreateAsyncConsumer(
        IModel channel,
        ISagaQueue sagaQueue)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (sender, e) =>
        {
            var inputMessage = _serializer.Deserialize(e.Body);

            if (inputMessage is null)
            {
                return;
            }

            await sagaQueue.Push(inputMessage);
        };

        return consumer;
    }

    public IBasicConsumer CreateBasicConsumer(
        IModel channel,
        ISagaQueue sagaQueue)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var inputMessage = _serializer.Deserialize(e.Body);

            if (inputMessage is null)
            {
                return;
            }

            sagaQueue.Push(inputMessage).GetAwaiter().GetResult();
        };

        return consumer;
    }
}

