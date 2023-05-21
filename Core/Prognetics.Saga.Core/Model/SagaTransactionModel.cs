namespace Prognetics.Saga.Core.Model;

public class SagaTransactionModel
{
    public required string Name { get; init; }
    public IReadOnlyList<SagaTransactionStepModel> Steps { get; init; } = new List<SagaTransactionStepModel>();
}
