namespace Prognetics.Saga.Core.Model;

public record SagaTransactionModel
{
    private readonly IReadOnlyDictionary<string, SagaTransactionStepModel> _eventNameToStep;
    public SagaTransactionModel(
        string name,
        IEnumerable<SagaTransactionStepModel> steps)
    {
        Name = name;
        Steps = steps.OrderBy(s => s.Order).ToList();
        _eventNameToStep = steps.ToDictionary(s => s.EventName.ToUpper());
    }

    public string Name { get; }
    public IReadOnlyList<SagaTransactionStepModel> Steps { get; }

    public SagaTransactionStepModel? GetOperationByEventName(string eventName)
        => _eventNameToStep.GetValueOrDefault(eventName.ToUpper());
}
