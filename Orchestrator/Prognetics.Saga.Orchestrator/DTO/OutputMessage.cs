namespace Prognetics.Saga.Orchestrator.DTO;

public record OutputMessage(
    string TransactionId,
    object Payload);