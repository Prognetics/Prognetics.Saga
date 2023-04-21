namespace Prognetics.Saga.Orchestrator.Model;

public class SagaModel
{
    public IReadOnlyList<SagaTransactionModel> Transactions { get; init; } = new List<SagaTransactionModel>();
}

public class SagaTransactionModel
{
    public IReadOnlyList<SagaTransactionStepModel> Steps { get; init; } = new List<SagaTransactionStepModel>();
}

public class SagaTransactionStepModel
{
    public required string From { get; init; }
    public required string To { get; init; }
}