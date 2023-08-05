namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public enum TransactionState
{
    Active,
    Rollback,
    Finished,
    Failed,
}