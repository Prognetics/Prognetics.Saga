using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

class TransactionBuilder : ITransactionBuilder
{
    private readonly List<Step> _steps = new();

    public ITransactionBuilder AddStep(
        string completionEventName,
        string nextEventName,
        string compensationEventName)
    {
        _steps.Add(new Step
        {
            CompletionEventName = completionEventName,
            NextEventName = nextEventName,
            CompensationEventName= compensationEventName
        });
        return this;
    }

    public Transaction Build(string name)
        => new()
        {
            Name = name,
            Steps = _steps.ToList(),
        };
}