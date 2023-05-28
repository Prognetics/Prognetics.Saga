namespace Prognetics.Saga.Core.Model;

public record SagaTransactionStepModel
{
    public required int Order { get; set; }
    public required string EventName { get; init; }
    public required string CompletionEventName { get; init; }
    public required string CompensationEventName { get; init; }
    public SagaTransactionStepModel? Next { get; init; }
}