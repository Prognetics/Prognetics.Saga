using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

class SagaTransactionBuilder : ISagaTransactionBuilder
{
    private readonly List<SagaTransactionStepModel> _steps = new();

    public ISagaTransactionBuilder AddStep(
        string from,
        string to,
        string compensation)
    {
        _steps.Add(new SagaTransactionStepModel
        {
            Order = _steps.Count(),
            EventName = from,
            CompletionEventName = to,
            CompensationEventName = compensation
        });
        return this;
    }

    public SagaTransactionModel Build(string name)
        => new()
        {
            Name = name,
            Steps = _steps.ToList(),
        };
}