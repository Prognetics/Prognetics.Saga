using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface IModelBuilder
{
    IModelBuilder From(TransactionsLedger transactionsLedger);
    IModelBuilder AddTransaction(Action<ITransactionBuilder> builderAction);
}
