namespace Prognetics.Saga.Orchestrator;

public interface ISagaTransactionBuilder
{
    ISagaTransactionBuilder AddStep(string from, string to);
}

public class SagaModelBuilder
{
    private readonly List<SagaTransactionModel> _transactions = new();

    public SagaModelBuilder From(SagaModel sagaModel)
    {
        _transactions.AddRange(sagaModel.Transactions.ToList());
        return this;
    }

    public SagaModelBuilder AddTransaction(Action<ISagaTransactionBuilder> builderAction)
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

    private class SagaTransactionBuilder : ISagaTransactionBuilder
    {
        private readonly List<SagaTransactionStepModel> _steps = new();

        public ISagaTransactionBuilder AddStep(string from, string to)
        {
            _steps.Add(new SagaTransactionStepModel
            {
                From = from,
                To = to
            });
            return this;
        }

        public SagaTransactionModel Build()
            => new()
            {
                Steps = _steps.ToList()
            };
    }
}


