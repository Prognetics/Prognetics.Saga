namespace Prognetics.Saga.Orchestrator;

public interface ISagaHost : IDisposable
{
    public Task Start(CancellationToken cancellationToken = default);
}

public class SagaHost : ISagaHost
{
    private readonly List<ISagaClient> _clients;
    private readonly ISagaOrchestratorFactory _orchestratorFactory;
    private ISagaOrchestrator? _orchestrator;

    public SagaHost(
        IEnumerable<ISagaClient> clients,
        ISagaOrchestratorFactory orchestratorFactory)
    {
        _clients = clients.ToList();
        _orchestratorFactory = orchestratorFactory;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        if (_orchestrator != null) {
            throw new InvalidOperationException("Host is already running");
        }

        _orchestrator = await _orchestratorFactory.Create(cancellationToken);

        await Task.WhenAll(
            _clients.Select(c =>
                c.Start(_orchestrator, cancellationToken)));
    }

    public void Dispose()
    {
        _clients.ForEach(c => c.Dispose());
        _clients.Clear();
        _orchestrator?.Dispose();
    }
}
