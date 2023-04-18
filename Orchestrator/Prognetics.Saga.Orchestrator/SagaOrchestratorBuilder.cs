namespace Prognetics.Saga.Orchestrator;
public class SagaOrchestratorBuilder
{
    private ISagaModelProvider _modelProvider = new EmptySagaModelProvider();

    public SagaOrchestratorBuilder With(ISagaModelProvider modelProvider)
    {
        _modelProvider = modelProvider;
        return this;
    }

    public ISagaOrchestrator Build() => new SagaOrchestrator(_modelProvider);
}