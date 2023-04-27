﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public class SagaBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private IServiceScope? _scope;
    private ISagaHost? _host;
    private bool _isRunning;

    public SagaBackgroundService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_isRunning)
        {
            throw new InvalidOperationException($"{nameof(SagaBackgroundService)} is running");
        }

        _scope = _serviceProvider.CreateScope();
        _host = _serviceProvider.GetRequiredService<ISagaHost>();
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
            _scope?.Dispose();
            base.Dispose();
        }

        _isRunning = false;
    }
}
