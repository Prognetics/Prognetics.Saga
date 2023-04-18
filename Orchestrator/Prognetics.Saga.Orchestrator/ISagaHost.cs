namespace Prognetics.Saga.Orchestrator;

public interface ISagaHost : IDisposable
{
    Task Start(
        ISagaOrchestrator orchestrator,
        CancellationToken cancellationToken = default);
}