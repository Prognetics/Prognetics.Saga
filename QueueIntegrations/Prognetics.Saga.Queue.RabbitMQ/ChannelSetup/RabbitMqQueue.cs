namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMqQueue
{
    public required string Name { get; init; }

    public bool Durable { get; init; }

    public bool Exclusive { get; init; }

    public bool AutoDelete { get; init; }

    public IDictionary<string, object> Arguments { get; init; } = new Dictionary<string, object>();
}
