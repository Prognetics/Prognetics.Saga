namespace Prognetics.Saga.Orchestrator;
public interface ISagaLog
{
    Task<TransactionState?> GetState(string transactionId);
    Task SetState(TransactionState transactionState);
}