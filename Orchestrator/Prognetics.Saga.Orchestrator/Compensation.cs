namespace Prognetics.Saga.Orchestrator;

public readonly record struct Compensation(
    object TransactionId,
    string EventName,
    object Content);