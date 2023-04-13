namespace Prognetics.Saga.Orchestrator;

public record OutputMessage(
    string TransactionId,
    string Name,
    object Payload);