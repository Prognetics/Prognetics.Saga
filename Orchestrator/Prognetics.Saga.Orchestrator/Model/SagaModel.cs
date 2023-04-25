namespace Prognetics.Saga.Orchestrator.Model;

public class SagaModel
{
    public IReadOnlyList<SagaTransactionModel> Transactions { get; init; } = new List<SagaTransactionModel>();
}