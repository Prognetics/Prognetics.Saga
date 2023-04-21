namespace Prognetics.Saga.Orchestrator;

public interface ISagaHost : IDisposable
{
    public Task Start(CancellationToken cancellationToken = default);
}

public class SagaHost : ISagaHost
{
    private readonly List<ISagaClient> _clients;
    private readonly ISagaOrchestrator _orchestrator;

    public SagaHost(
        IEnumerable<ISagaClient> clients,
        ISagaOrchestrator orchestrator)
    {
        _clients = clients.ToList();
        _orchestrator = orchestrator;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        _clients.ForEach(c =>
        {
            c.UseInput(_orchestrator);
            _orchestrator.Subscribe(c.Subscriber);
        });

        await Task.WhenAll(
            _clients.Select(c =>
                c.Start(cancellationToken)));
    }

    public void Dispose()
    {
        _clients.Clear();
        _orchestrator.Dispose();
    }
}
