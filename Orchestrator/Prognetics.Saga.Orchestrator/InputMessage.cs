namespace Prognetics.Saga.Orchestrator;

public record InputMessage(
    string TransactionId,
    string Name,
    object Payload,
    object Compensation);