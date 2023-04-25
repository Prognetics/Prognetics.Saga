namespace Prognetics.Saga.Orchestrator.Model;

public class SagaTransactionStepModel
{
    public required string From { get; init; }
    public required string To { get; init; }
}