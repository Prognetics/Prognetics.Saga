using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Core.DependencyInjection;

public class SagaBackgroundService : BackgroundService
{
    private readonly IInitializableTransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CancellationTokenSource _ctsOnStarted = new();
    private CancellationTokenSource? _ctsOnRestart;

    public SagaBackgroundService(
        IInitializableTransactionLedgerAccessor transactionLedgerAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _transactionLedgerAccessor = transactionLedgerAccessor;
    }

    public CancellationToken OnStarted => _ctsOnStarted.Token;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _transactionLedgerAccessor.Initialize(
            () => _ctsOnRestart?.Cancel(),
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested){
            _ctsOnRestart = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            using var scope = _serviceScopeFactory.CreateScope();
            using var orchestrator = scope.ServiceProvider.GetRequiredService<IStartableSagaOrchestrator>();
            await orchestrator.Start(_ctsOnRestart.Token);
            if (!_ctsOnStarted.IsCancellationRequested)
            {
                _ctsOnStarted.Cancel();
            }
            await _ctsOnRestart.Token.Wait();
        }
    }
}
