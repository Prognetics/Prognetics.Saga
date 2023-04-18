using Microsoft.Extensions.Hosting;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
internal class SagaBackgroundService : BackgroundService
{
    private readonly IReadOnlyList<ISagaHost> _sagaHosts;
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public SagaBackgroundService(
        ISagaOrchestrator sagaOrchestrator,
        IEnumerable<ISagaHost> sagaHosts)
    {
        _sagaHosts = sagaHosts.ToList();
        _sagaOrchestrator = sagaOrchestrator;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.WhenAll(
            _sagaHosts.Select(x => x
                .Start(_sagaOrchestrator, stoppingToken)));

    public override void Dispose()
    {
        foreach (var host in _sagaHosts)
        {
            host.Dispose();
        }

        base.Dispose();
    }
}
