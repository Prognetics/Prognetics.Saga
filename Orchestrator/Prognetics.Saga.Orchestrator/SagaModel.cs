namespace Prognetics.Saga.Orchestrator;

public class SagaModel
{
    public IReadOnlyList<SagaTransactionModel> Transactions { get; set; }
}

public class SagaTransactionModel
{
    public required IReadOnlyList<SagaTransactionStepModel> Steps { get; init; }
}

public class SagaTransactionStepModel
{
    public required string From { get; init; }
    public required string To { get; init; }
}