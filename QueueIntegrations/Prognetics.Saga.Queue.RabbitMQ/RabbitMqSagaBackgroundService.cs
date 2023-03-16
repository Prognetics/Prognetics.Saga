using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Prognetics.Saga.Queue.RabbitMQ;
internal class RabbitMqSagaBackgroundService
{
    private readonly IRabbitMqConnectionFactory _rabbitMqChannelFactory;
    private readonly ISagaModel _sagaModel;
    private readonly ISagaQueue _sagaQueue;

    public RabbitMqSagaBackgroundService(
        IRabbitMqConnectionFactory rabbitMqChannelFactory,
        ISagaModel sagaModel,
        ISagaQueue sagaQueue)
    {
        _rabbitMqChannelFactory = rabbitMqChannelFactory;
        _sagaModel = sagaModel;
        _sagaQueue = sagaQueue;
    }
    public async Task Listen(CancellationToken cancellationToken)
    {
        using var connection = _rabbitMqChannelFactory.Create();
        using var channel = connection.CreateModel();

        _sagaModel.Transactions.SelectMany(t => t.Steps)
            .ToList()
            .ForEach(s =>
            {
                channel.QueueDeclare(queue: s.From);
                channel.QueueDeclare(queue: s.To);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, e) =>
                {
                    var queueName = s.From;
                    var messageBytes = e.Body.ToArray();
                    var messageString = Encoding.UTF8.GetString(messageBytes);
                    var rabbitMqMessage = JsonSerializer.Deserialize<RabbitMqInputMessage>(messageString);

                    if (rabbitMqMessage is null)
                    {
                        return;
                    }

                    (var transactionId, var payload, var compensation) = rabbitMqMessage;

                    await _sagaQueue.Push(new(transactionId, queueName, payload, compensation));
                };

                channel.BasicConsume(s.From, true, consumer);
            });

        var properties = channel.CreateBasicProperties();
        properties.ContentType = "application/json";

        _sagaQueue.Subscribe(m => {
            var serializedMessage = JsonSerializer.Serialize(m);
            var messageBytes = Encoding.UTF8.GetBytes(serializedMessage);
            channel.BasicPublish(string.Empty, m.Name, properties, messageBytes);
            return Task.CompletedTask;
        });

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}

record RabbitMqInputMessage(
    string TransactionId,
    object Payload,
    object? Compensation);