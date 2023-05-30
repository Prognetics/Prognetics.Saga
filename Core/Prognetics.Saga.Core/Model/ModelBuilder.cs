using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

public class ModelBuilder : IModelBuilder
{
    private readonly List<Transaction> _transactions = new();

    // todo rename
    public IModelBuilder From(TransactionsLedger sagaModel)
    {
        _transactions.AddRange(sagaModel.Transactions.ToList());
        return this;
    }

    public IModelBuilder AddTransaction(Action<ITransactionBuilder> builderAction)
    {
        var transactionBuilder = new TransactionBuilder();
        builderAction(transactionBuilder);
        _transactions.Add(transactionBuilder.Build());
        return this;
    }

    public TransactionsLedger Build()
        => new()
        {
            Transactions = _transactions.ToList(),
        };
}


