namespace Prognetics.Saga.Queue.RabbitMQ.Consuming;

public record RabbitMqInputMessage(
    string? TransactionId,
    object Payload,
    object? Compensation);
