namespace Prognetics.Saga.Orchestrator.Contract;

public interface IStartableSagaOrchestrator : ISagaOrchestrator
{
    Task Start(CancellationToken cancellationToken = default);
}