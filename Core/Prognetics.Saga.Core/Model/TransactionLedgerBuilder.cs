using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

public class TransactionLedgerBuilder
{
    private readonly List<Transaction> _transactions = new();
        
    public TransactionLedgerBuilder FromLedger(TransactionsLedger sagaModel)
    {
        _transactions.AddRange(sagaModel.Transactions.ToList());
        return this;
    }

    public TransactionLedgerBuilder AddTransaction(string name, Action<ITransactionBuilder> builderAction)
    {
        var transactionBuilder = new TransactionBuilder();
        builderAction(transactionBuilder);
        _transactions.Add(transactionBuilder.Build(name));
        return this;
    }

    public TransactionsLedger Build()
        => new()
        {
            Transactions = _transactions.ToList(),
        };
}


