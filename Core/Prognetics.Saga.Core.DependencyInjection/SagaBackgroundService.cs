using Microsoft.Extensions.Hosting;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Core.DependencyInjection;
public class SagaBackgroundService : BackgroundService
{
    private readonly ISagaHost _host;
    private bool _isRunning;

    public SagaBackgroundService(ISagaHost sagaHost)
    {
        _host = sagaHost;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_isRunning)
        {
            throw new InvalidOperationException($"{nameof(SagaBackgroundService)} is running");
        }

        await _host.Start(stoppingToken);
        _isRunning = true;
    }

    public override async Task StopAsync(CancellationToken cancellation)
    {
        await base.StopAsync(cancellation);
        _isRunning = false;
    }

    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _host?.Dispose();
            base.Dispose();
        }

        _isRunning = false;
    }
}
