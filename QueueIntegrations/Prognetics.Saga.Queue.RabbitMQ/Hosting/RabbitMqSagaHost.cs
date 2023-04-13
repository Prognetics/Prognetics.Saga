using Microsoft.Extensions.Logging;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

class RabbitMqSagaHost : IRabbitMqSagaHost
{
    private readonly IRabbitMqConnectionFactory _rabbitMqConnectionFactory;
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly IRabbitMqSagaQueuesProvider _queuesProvider;
    private readonly IRabbitMqSagaConsumersFactory _rabbitMqSagaConsumersFactory;
    private readonly IRabbitMqSagaSubscriberFactory _sagaSubscriberFactory;
    private readonly ILogger<IRabbitMqSagaHost> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqSagaHost(
        IRabbitMqConnectionFactory rabbitMqConnectionFactory,
        IRabbitMqSagaQueuesProvider queuesProvider,
        ISagaOrchestrator sagaOrchestrator,
        IRabbitMqSagaConsumersFactory rabbitMqSagaConsumersFactory,
        IRabbitMqSagaSubscriberFactory sagaSubscriberFactory,
        ILogger<IRabbitMqSagaHost> logger)
    {
        _queuesProvider = queuesProvider;
        _rabbitMqSagaConsumersFactory = rabbitMqSagaConsumersFactory;
        _sagaSubscriberFactory = sagaSubscriberFactory;
        _logger = logger;
        _sagaOrchestrator = sagaOrchestrator;
        _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
    }

    public void Start()
    {
        _connection = _rabbitMqConnectionFactory.Create();
        _connection.CallbackException += OnExceptionHandler;
        _connection.ConnectionShutdown += OnShutdownHandler;
        _channel = _connection.CreateModel();
        _channel.CallbackException += OnExceptionHandler;

        var exchange = _queuesProvider.Exchange;

        if (!string.IsNullOrEmpty(exchange))
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Direct);
        }

        foreach (var queue in _queuesProvider.Queues)
        {
            _channel.QueueDeclare(
                queue.Name,
                queue.Durable,
                queue.Exclusive,
                queue.AutoDelete,
                queue.Arguments);

            if (!string.IsNullOrEmpty(exchange))
            {
                _channel.QueueBind(
                    queue.Name,
                    exchange,
                    string.Empty,
                    null);
            }
        }

        var consumers = _rabbitMqSagaConsumersFactory.Create(
            _channel,
            _sagaOrchestrator);

        foreach (var consumer in consumers)
        {
            _channel.BasicConsume(
                consumer.BasicConsumer,
                consumer.Queue,
                consumer.AutoAck,
                consumer.ConsumerTag,
                consumer.NoLocal,
                consumer.Exclusive,
                consumer.Arguments);
        }

        var sagaSubscriber = _sagaSubscriberFactory.Create(_channel);
        _sagaOrchestrator.Subscribe(sagaSubscriber);
    }

    private void OnShutdownHandler(object? sender, ShutdownEventArgs e)
    {
        _logger.LogError("A shutdown occured: {ReplyText}, code: {ReplyCode}", e.ReplyText, e.ReplyCode);
    }

    private void OnExceptionHandler(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "An exception occured during handling the message: {Detail}", e.Detail);
    }

    public void Dispose()
    {
        if (_connection?.IsOpen == true)
        {
            _connection.Close();
        }

        if (_channel?.IsOpen == true)
        {
            _channel.Close();
        }
    }
}
