namespace Prognetics.Saga.Core.Model;

public enum TransactionState
{
    Active,
    Finished,
    Rollback,
    Failed,
}