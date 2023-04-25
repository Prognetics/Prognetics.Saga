namespace Prognetics.Saga.Orchestrator.Model;

public class SagaTransactionModel
{
    public IReadOnlyList<SagaTransactionStepModel> Steps { get; init; } = new List<SagaTransactionStepModel>();
}
