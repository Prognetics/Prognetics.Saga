namespace Prognetics.Saga.Orchestrator.DTO;

public record InputMessage(
    string? TransactionId,
    object Payload,
    object? Compensation);