namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaHost
{
    public Task Start(CancellationToken cancellationToken = default);
}
