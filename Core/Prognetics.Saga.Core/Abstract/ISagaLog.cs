using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;
public interface ISagaLog
{
    Task AddTransaction(
        TransactionLog transactionLog,
        CancellationToken cancellationToken = default);

    Task<TransactionLog> GetTransaction(
        string transactionId,
        CancellationToken cancellationToken = default);

    Task UpdateTransaction(
        TransactionLog transactionLog,
        CancellationToken cancellationToken = default);
}