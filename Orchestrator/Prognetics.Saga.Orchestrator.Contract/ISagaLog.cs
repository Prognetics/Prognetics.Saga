using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;
public interface ISagaLog
{
    Task SaveTransactionState(
        TransactionLog transactionLog,
        CancellationToken cancellationToken = default);

    Task<TransactionLog> GetTransactionState(
        string transactionId,
        CancellationToken cancellationToken = default);
}
