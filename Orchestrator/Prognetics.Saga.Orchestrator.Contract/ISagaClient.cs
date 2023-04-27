namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaClient : IDisposable
{
    Task Start(
        ISagaOrchestrator orchestrator,
        CancellationToken cancellationToken = default);
}