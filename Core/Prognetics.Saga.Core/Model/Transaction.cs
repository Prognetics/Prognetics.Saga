namespace Prognetics.Saga.Core.Model;

public class Transaction    
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Step> Steps { get; init; } = new List<Step>();

    public StepRecord? GetStepByCompletionEventName(string completionEventName)
        => Steps
            .Select((x, i) => new StepRecord(i, x))
            .SingleOrDefault(x => x.Step.CompletionEventName.ToLowerInvariant() == completionEventName.ToLowerInvariant());
    
    public Step? GetStepByOrderNumber(int orderNumber)
        => orderNumber < Steps.Count ? Steps[orderNumber] : null;    
}