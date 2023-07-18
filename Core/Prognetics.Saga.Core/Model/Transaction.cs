namespace Prognetics.Saga.Core.Model;

public class Transaction    
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Step> Steps { get; init; } = new List<Step>();

    public StepRecord? GetStepByEventName(string eventName)
        => Steps
            .Select((x, i) => new StepRecord(i, x))
            .SingleOrDefault(x => x.Step.EventName.ToLowerInvariant() == eventName.ToLowerInvariant());
    
    public Step? GetStepByOrderNumber(int orderNumber)
        => orderNumber < Steps.Count ? Steps[orderNumber] : null;    
}