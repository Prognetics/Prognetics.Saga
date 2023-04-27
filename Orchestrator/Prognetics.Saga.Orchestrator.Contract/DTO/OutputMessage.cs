namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public record OutputMessage(
    string TransactionId,
    object Payload);