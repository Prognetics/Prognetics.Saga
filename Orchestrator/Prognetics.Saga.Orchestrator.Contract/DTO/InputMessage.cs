namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public record InputMessage(
    string? TransactionId,
    object Payload,
    string? Compensation);