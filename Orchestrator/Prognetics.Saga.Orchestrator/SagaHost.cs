using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator;

public class SagaHost : ISagaHost
{
    private readonly IReadOnlyList<ISagaModelSource> _sources;
    private readonly ISagaClient _client;
    private IStartableSagaOrchestrator _orchestrator;

    public SagaHost(
        IEnumerable<ISagaModelSource> sources,
        ISagaClient client,
        IStartableSagaOrchestrator orchestrator)
    {
        _sources = sources.ToList();
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
