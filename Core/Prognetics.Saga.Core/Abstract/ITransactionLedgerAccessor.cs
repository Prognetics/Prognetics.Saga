using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionLedgerAccessor
{
    TransactionsLedger TransactionsLedger { get; }
}