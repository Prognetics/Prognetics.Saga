namespace Prognetics.Saga.Orchestrator;

public interface ISagaHost : IDisposable
{
    public Task Start(CancellationToken cancellationToken = default);
}
