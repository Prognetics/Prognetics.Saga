namespace Prognetics.Saga.Orchestrator.SagaModel;

public class EmptySagaModelProvider : ISagaModelProvider
{
    private EmptySagaModelProvider()
    { }

    public static EmptySagaModelProvider Instance { get; } = new EmptySagaModelProvider();

    public ValueTask<SagaModel> GetModel()
        => ValueTask.FromResult(new SagaModel());
}
