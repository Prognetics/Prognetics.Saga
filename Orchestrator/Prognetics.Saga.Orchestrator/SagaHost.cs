namespace Prognetics.Saga.Orchestrator;

public interface ISagaHost : IDisposable
{
    public void Start();
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

    public void Start()
        => _clients.ForEach(c =>
        {
            c.UseInput(_orchestrator);
            _orchestrator.Subscribe(c.Subscriber);
        });

    public void Dispose()
    {
        _clients.Clear();
        _orchestrator.Dispose();
    }
}
