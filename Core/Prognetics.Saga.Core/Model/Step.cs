namespace Prognetics.Saga.Core.Model;

public class Step
{
    public required string CompletionEventName { get; init; }
    public required string NextEventName { get; init; }
    public required string CompensationEventName { get; init; }
    
}