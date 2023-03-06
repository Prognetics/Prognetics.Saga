namespace Prognetics.Saga.Orchestrator;

public interface ISagaModel
{
    IReadOnlyList<SagaTransaction> Transactions { get; }
}


public class SagaTransaction
{
    public IReadOnlyList<SagaTransactionStep> Steps { get; set; }
}

public class SagaTransactionStep
{
    public string From { get; set; }
    public string To { get; set; }
}
