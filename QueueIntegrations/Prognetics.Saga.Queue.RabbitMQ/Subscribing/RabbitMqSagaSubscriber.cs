﻿using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Orchestrator.DTO;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Subscribing;

internal class RabbitMQSagaSubscriber : ISagaSubscriber
{
    private readonly IRabbitMQSagaSerializer _serializer;
    private readonly IModel _model;
    private readonly IBasicProperties _properties;
    private readonly string _exchange;

    public RabbitMQSagaSubscriber(
        IRabbitMQSagaSerializer serializer,
        IModel model,
        RabbitMQSagaOptions options)
    {
        _serializer = serializer;
        _model = model;
        _properties = model.CreateBasicProperties();
        _properties.ContentType = options.ContentType;
        _exchange = options.Exchange;
    }

    public Task OnMessage(string queueName, OutputMessage message)
    {
        var messageBytes = _serializer.Serialize(message);
        _model.BasicPublish(_exchange, queueName, _properties, messageBytes);
        return Task.CompletedTask;
    }
}