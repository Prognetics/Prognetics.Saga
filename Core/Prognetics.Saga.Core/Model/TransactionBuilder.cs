using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Core.Model;

class TransactionBuilder : ITransactionBuilder
{
    private readonly List<Step> _steps = new();

    public ITransactionBuilder AddStep(string eventName, string completionEventName, string compensationEventName)
    {
        _steps.Add(new Step
        {
            EventName = eventName,
            CompletionEventName = completionEventName,
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