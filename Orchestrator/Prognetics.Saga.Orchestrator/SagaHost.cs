using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator;

public class SagaHost : ISagaHost
{
    private readonly ISagaClient _client;
    private IStartableSagaOrchestrator _orchestrator;

    public SagaHost(
        ISagaClient client,
        IStartableSagaOrchestrator orchestrator)
    {
        _client = client;
        _orchestrator = orchestrator;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        if (_orchestrator.IsStarted) {
            throw new InvalidOperationException("Orchestrator has been already run");
        }

        await _client.Initialize();
        var subscriber = await _client.GetSubscriber();
        _orchestrator.Start(subscriber);
        await _client.Consume(_orchestrator);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) {
            _client.Dispose();
        }
    }
}
