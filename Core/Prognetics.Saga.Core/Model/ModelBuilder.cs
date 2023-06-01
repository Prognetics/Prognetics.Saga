using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

public class ModelBuilder
{
    private readonly List<Transaction> _transactions = new();

    // todo rename
    public ModelBuilder From(TransactionsLedger sagaModel)
    {
        _transactions.AddRange(sagaModel.Transactions.ToList());
        return this;
    }

    public ModelBuilder AddTransaction(string name, Action<ITransactionBuilder> builderAction)
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


