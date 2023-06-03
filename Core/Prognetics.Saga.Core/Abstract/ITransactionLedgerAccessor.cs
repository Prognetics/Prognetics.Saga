using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionLedgerAccessor
{
    Task Initialize(CancellationToken cancellation = default);
    TransactionsLedger TransactionsLedger { get; }
}