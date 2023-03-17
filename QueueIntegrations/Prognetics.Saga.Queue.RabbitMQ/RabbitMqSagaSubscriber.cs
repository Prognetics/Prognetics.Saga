﻿using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ;

internal class RabbitMqSagaSubscriber : ISagaSubscriber
{
    private readonly IRabbitMqSagaSerializer _serializer;
    private readonly IModel _model;
    private IBasicProperties _properties;
    private readonly string _exchange;

    public RabbitMqSagaSubscriber(
        IRabbitMqSagaSerializer serializer,
        IModel model,
        RabbitMqSagaOptions options)
    {
        _serializer = serializer;
        _model = model;
        _properties = model.CreateBasicProperties();
        _properties.ContentType = options.ContentType;
        _exchange = options.Exchange;
    }

    public Task Subscribe(OutputMessage message)
    {
        var messageBytes = _serializer.Serialize(message);
        _model.BasicPublish(_exchange, message.Name, _properties, messageBytes);
        return Task.CompletedTask;
    }
}