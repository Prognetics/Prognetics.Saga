namespace Prognetics.Saga.Orchestrator;
public static class SagaOrchestratorBuilder
{
    public static ISagaOrchestrator Build(SagaModel model)
        => new SagaOrchestrator(model);
}