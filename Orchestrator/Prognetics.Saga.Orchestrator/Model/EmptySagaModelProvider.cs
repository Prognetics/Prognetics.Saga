namespace Prognetics.Saga.Orchestrator.Model;

public class EmptySagaModelProvider : ISagaModelProvider
{
    private EmptySagaModelProvider()
    { }

    public static EmptySagaModelProvider Instance { get; } = new EmptySagaModelProvider();

    public ValueTask<SagaModel> GetModel()
        => ValueTask.FromResult(new SagaModel());
}
