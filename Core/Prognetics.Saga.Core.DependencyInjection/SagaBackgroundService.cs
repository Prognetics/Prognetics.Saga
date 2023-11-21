using Microsoft.Extensions.Hosting;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Core.DependencyInjection;
public class SagaBackgroundService : BackgroundService
{
    private readonly ISagaHost _host;

    public SagaBackgroundService(ISagaHost sagaHost)
    {
        _host = sagaHost;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _host.Start(stoppingToken);
}
