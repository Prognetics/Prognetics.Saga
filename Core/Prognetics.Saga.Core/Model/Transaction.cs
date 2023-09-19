namespace Prognetics.Saga.Core.Model;

public class Transaction    
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Step> Steps { get; init; } = new List<Step>();

    public TransactionStep? GetStepByCompletionEventNameOrDefault(string completionEventName)
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            if (Steps[i].CompletionEventName.ToLowerInvariant() == completionEventName.ToLowerInvariant())
            {
                return new TransactionStep(i, Steps[i], this);
            }
        }
        return null;
    }
    
    public Step? GetStepByOrderNumber(int orderNumber)
        => orderNumber < Steps.Count ? Steps[orderNumber] : null;
}