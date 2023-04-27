namespace Prognetics.Saga.Orchestrator;

public interface ISagaClient : IDisposable
{
    Task Start(
        ISagaOrchestrator orchestrator,
        CancellationToken cancellationToken = default);
}