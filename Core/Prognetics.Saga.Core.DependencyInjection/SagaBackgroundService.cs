using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Core.DependencyInjection;

public class SagaBackgroundService : BackgroundService
{
    private readonly IInitializableTransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private CancellationTokenSource? _cts;

    public SagaBackgroundService(
        IInitializableTransactionLedgerAccessor transactionLedgerAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _transactionLedgerAccessor = transactionLedgerAccessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _transactionLedgerAccessor.Initialize(
            () => _cts?.Cancel(),
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested){
            _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            using var scope = _serviceScopeFactory.CreateScope();
            using var orchestrator = scope.ServiceProvider.GetRequiredService<IStartableSagaOrchestrator>();
            await orchestrator.Start(_cts.Token);
            await Task.FromCanceled(_cts.Token);
        }
    }
}
