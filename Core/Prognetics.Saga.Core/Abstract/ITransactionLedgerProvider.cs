using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionLedgerProvider
{
    Task<TransactionsLedger> Get();
}