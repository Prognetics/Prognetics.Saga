namespace Prognetics.Saga.Orchestrator;

public record InputMessage(
    string? TransactionId,
    object Payload,
    object? Compensation);