namespace Prognetics.Saga.Orchestrator;

public record OutputMessage(
    string TransactionId,
    object Payload);