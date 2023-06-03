﻿using Microsoft.Extensions.Logging;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public class RabbitMQSagaClient : ISagaClient
{
    private readonly ITransactionLedgerProvider _transactionLedgerProvider;
    private readonly IRabbitMQConnectionFactory _rabbitMqConnectionFactory;
    private readonly IRabbitMQQueuesProvider _queuesProvider;
    private readonly IRabbitMQConsumersFactory _rabbitMqSagaConsumersFactory;
    private readonly IRabbitMQSagaSubscriberFactory _sagaSubscriberFactory;
    private readonly RabbitMQSagaOptions _options;
    private readonly ILogger<IRabbitMQSagaHost> _logger;
    private TransactionsLedger? _sagaModel;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQSagaClient(
        ITransactionLedgerProvider transactionLedgerProvider,
        IRabbitMQConnectionFactory rabbitMqConnectionFactory,
        IRabbitMQQueuesProvider queuesProvider,
        IRabbitMQConsumersFactory rabbitMqSagaConsumersFactory,
        IRabbitMQSagaSubscriberFactory sagaSubscriberFactory,
        RabbitMQSagaOptions options,
        ILogger<IRabbitMQSagaHost> logger)
    {
        _transactionLedgerProvider = transactionLedgerProvider;
        _queuesProvider = queuesProvider;
        _rabbitMqSagaConsumersFactory = rabbitMqSagaConsumersFactory;
        _sagaSubscriberFactory = sagaSubscriberFactory;
        _options = options;
        _logger = logger;
        _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
    }

    public async Task Initialize(){
        _sagaModel = await _transactionLedgerProvider.Get();
        _connection = _rabbitMqConnectionFactory.Create();
        _connection.CallbackException += OnExceptionHandler;
        _connection.ConnectionShutdown += OnShutdownHandler;
        _channel = _connection.CreateModel();
        _channel.CallbackException += OnExceptionHandler;

        var exchange = _options.Exchange;

        if (!string.IsNullOrEmpty(exchange))
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Direct);
        }

        foreach (var queue in _queuesProvider.GetQueues(_sagaModel))
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
                    queue.Name,
                    null);
            }
        }
    }

    public Task<ISagaSubscriber> GetSubscriber(){
        if (_channel is null)
        {
            throw new InvalidOperationException("Client has not been initialized");
        }

        return Task.FromResult(_sagaSubscriberFactory.Create(_channel));
    }

    public Task Consume(ISagaOrchestrator orchestrator){
        if (_channel is null || _sagaModel is null)
        {
            throw new InvalidOperationException("Client has not been initialized");
        }

        var consumers = _rabbitMqSagaConsumersFactory.Create(
            _channel,
            orchestrator,
            _sagaModel);

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

        return Task.CompletedTask;
    }

    private void OnShutdownHandler(
        object? sender,
        ShutdownEventArgs e)
    {
        _logger.LogError("A shutdown occured: {ReplyText}, code: {ReplyCode}", e.ReplyText, e.ReplyCode);
    }

    private void OnExceptionHandler(
        object? sender,
        CallbackExceptionEventArgs e)
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
