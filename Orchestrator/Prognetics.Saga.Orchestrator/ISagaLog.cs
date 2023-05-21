namespace Prognetics.Saga.Orchestrator;
public interface ISagaLog
{
    Task<IReadOnlyDictionary<string, object>> GetCompensations(string transactionId);
    Task<TransactionState?> GetState(string transactionId);
    void SaveCompensation(object transactionId, string compensation1, object compensation2);
    Task SetState(TransactionState transactionState);
}