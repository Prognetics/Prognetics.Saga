using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionLedgerBuilder
{
    ITransactionLedgerBuilder FromLedger(TransactionsLedger transactionsLedger);
    ITransactionLedgerBuilder AddTransaction(Action<ITransactionBuilder> builderAction);
}
