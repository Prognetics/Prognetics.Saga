using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

public class SagaModelBuilder
{
    private readonly List<SagaTransactionModel> _transactions = new();

    public SagaModelBuilder From(SagaModel sagaModel)
    {
        _transactions.AddRange(sagaModel.Transactions.ToList());
        return this;
    }

    public SagaModelBuilder AddTransaction(
        string name,
        Action<ISagaTransactionBuilder> builderAction)
    {
        var transactionBuilder = new SagaTransactionBuilder();
        builderAction(transactionBuilder);
        _transactions.Add(transactionBuilder.Build(name));
        return this;
    }

    public SagaModel Build()
        => new(_transactions.ToList());
}


