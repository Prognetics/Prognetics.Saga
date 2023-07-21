using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionLedgerSource
{
    Task<TransactionsLedger> GetTransactionLedger(CancellationToken cancellation = default);
}
