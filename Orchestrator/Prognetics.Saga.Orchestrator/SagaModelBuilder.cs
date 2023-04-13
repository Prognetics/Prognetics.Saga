namespace Prognetics.Saga.Orchestrator;

public interface ISagaTransactionBuilder
{
    ISagaTransactionBuilder AddStep(string from, string to);
}

public class SagaModelBuilder
{
    private readonly List<SagaTransactionModel> transactions = new();

    public SagaModelBuilder AddTransaction(Action<ISagaTransactionBuilder> builderAction)
    {
        var transactionBuilder = new SagaTransactionBuilder();
        builderAction(transactionBuilder);
        transactions.Add(transactionBuilder.Build());
        return this;
    }

    public SagaModel Build()
        => new()
        {
            Transactions = transactions.ToList(),
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


