namespace Prognetics.Saga.Orchestrator.Model;

class SagaTransactionBuilder : ISagaTransactionBuilder
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
            Steps = _steps.ToList(),
        };
}