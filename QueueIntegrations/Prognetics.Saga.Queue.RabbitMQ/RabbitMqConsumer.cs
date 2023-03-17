using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ;

public class RabbitMqConsumer
{
    public required string Queue { get; init; }
    public bool AutoAck { get; init; }
    public string ConsumerTag { get; init; } = string.Empty;
    public bool NoLocal { get; init; }
    public bool Exclusive { get; init; }
    public IDictionary<string, object>? Arguments { get; init; } = null;
    public required IBasicConsumer BasicConsumer { get; init; }
}