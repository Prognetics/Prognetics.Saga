namespace Prognetics.Saga.Orchestrator;

public interface ISagaOrchestratorFactory
{
    Task<ISagaOrchestrator> Create(CancellationToken cancellation);
}