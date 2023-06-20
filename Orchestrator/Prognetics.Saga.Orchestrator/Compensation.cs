namespace Prognetics.Saga.Orchestrator;

public readonly record struct Compensation(
    string TransactionId,
    string EventName,
    object Content);