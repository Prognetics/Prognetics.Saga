namespace Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;

public class RabbitMQQueue
{
    public required string Name { get; init; }

    public string? Exchange { get; init; }

    public bool Durable { get; init; } = true;

    public bool Exclusive { get; init; }

    public bool AutoDelete { get; init; }

    public IDictionary<string, object> Arguments { get; init; } = new Dictionary<string, object>();
}
