namespace Prognetics.Saga.Core.Model;

public class SagaModel
{
    public IReadOnlyList<SagaTransactionModel> Transactions { get; init; } = new List<SagaTransactionModel>();
}