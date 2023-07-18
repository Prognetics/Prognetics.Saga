using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;
public interface ISagaLog
{
    Task<TransactionState?> GetState(string transactionId);
    Task SetState(TransactionState transactionState);
}