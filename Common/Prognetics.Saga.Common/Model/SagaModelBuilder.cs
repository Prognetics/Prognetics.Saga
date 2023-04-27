namespace Prognetics.Saga.Common.Model;

public class SagaModelBuilder : ISagaModelBuilder
{
    private readonly List<SagaTransactionModel> _transactions = new();

    public ISagaModelBuilder From(SagaModel sagaModel)
    {
        _transactions.AddRange(sagaModel.Transactions.ToList());
        return this;
    }

    public ISagaModelBuilder AddTransaction(Action<ISagaTransactionBuilder> builderAction)
    {
        var transactionBuilder = new SagaTransactionBuilder();
        builderAction(transactionBuilder);
        _transactions.Add(transactionBuilder.Build());
        return this;
    }

    public SagaModel Build()
        => new()
        {
            Transactions = _transactions.ToList(),
        };
}


