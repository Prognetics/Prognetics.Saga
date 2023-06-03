namespace Prognetics.Saga.Core.Model;

public class Transaction    
{
    private IReadOnlyDictionary<string, StepRecord>? _eventNameToStep;

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Step> Steps { get; init; } = new List<Step>();

    public StepRecord? GetStepByEventName(string eventName){
        if (_eventNameToStep is null)
        {
            _eventNameToStep = Steps
                .Select((x, i) => new StepRecord(i, x))
                .ToDictionary(s => s.Step.EventName.ToUpper());
        }
        return _eventNameToStep.GetValueOrDefault(eventName.ToUpper());
    }
    
    public Step? GetStepByOrderNumber(int orderNumber)
        => orderNumber < Steps.Count ? Steps[orderNumber] : null;    
}