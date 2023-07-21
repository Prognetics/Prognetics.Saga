namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaHost : IDisposable
{
    public Task Start(CancellationToken cancellationToken = default);
}
