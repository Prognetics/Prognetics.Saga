namespace Prognetics.Saga.Core.Model;

public class Step
{
    public required string EventName { get; init; }

    public required string CompletionEventName { get; init; }

    public required string CompensationEventName { get; init; }
    
}