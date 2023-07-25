namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public readonly record struct Compensation(
    string TransactionId,
    string EventName,
    object Content);