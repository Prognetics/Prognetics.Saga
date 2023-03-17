using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

class RabbitMqChannelFactory : IRabbitMqChannelFactory
{
    private readonly RabbitMqSagaOptions _options;
    private readonly IRabbitMqSagaQueuesProvider _queuesFactory;

    public RabbitMqChannelFactory(
        RabbitMqSagaOptions options,
        IRabbitMqSagaQueuesProvider queuesFactory)
    {
        _options = options;
        _queuesFactory = queuesFactory;
    }

    public IModel Create()
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            DispatchConsumersAsync = _options.DispatchConsumersAsync,
        };

        var connection = connectionFactory.CreateConnection();
        var channel = connection.CreateModel();
        var queues = _queuesFactory.Queues;

        foreach (var queue in queues)
        {
            channel.QueueDeclare(
                queue.Name,
                queue.Durable,
                queue.Exclusive,
                queue.AutoDelete,
                queue.Arguments);
        }

        return channel;
    }
}