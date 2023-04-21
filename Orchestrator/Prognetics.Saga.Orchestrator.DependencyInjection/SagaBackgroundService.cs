using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public class SagaBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private IServiceScope? _scope;
    private ISagaHost? _host;

    public SagaBackgroundService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_scope != null)
        {
            throw new InvalidOperationException($"{nameof(SagaBackgroundService)} is running");
        }

        _scope = _serviceProvider.CreateScope();
        _host = _serviceProvider.GetRequiredService<ISagaHost>();
        _host.Start();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _host?.Dispose();
        _host = null;
        _scope?.Dispose();
        _scope = null;
        base.Dispose();
    }
}
