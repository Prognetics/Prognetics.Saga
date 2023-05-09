namespace Prognetics.Saga.Core.Model;

public class SagaTransactionModel
{
    public IReadOnlyList<SagaTransactionStepModel> Steps { get; init; } = new List<SagaTransactionStepModel>();
}
