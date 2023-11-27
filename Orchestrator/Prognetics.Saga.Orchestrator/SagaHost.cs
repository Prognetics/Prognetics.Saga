using Microsoft.Extensions.DependencyInjection;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator;

public class SagaHost : ISagaHost
{
    private readonly IInitializableTransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IServiceScopeFactory _serviceScopeProvider;

    public SagaHost(
        IInitializableTransactionLedgerAccessor transactionLedgerAccessor,
        IServiceScopeFactory serviceScopeProvider)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _serviceScopeProvider = serviceScopeProvider;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var restartCts = new CancellationTokenSource();
            var restartToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                restartCts.Token).Token;

            await _transactionLedgerAccessor.Initialize(
                restartCts.Cancel,
                restartToken);

            using var scope = _serviceScopeProvider.CreateAsyncScope();
            using var orchestrator = scope.ServiceProvider.GetRequiredService<IStartableSagaOrchestrator>();
            await orchestrator.Start(restartToken);

            while (!restartToken.IsCancellationRequested)
            {
                await Task.Delay(
                    1000,
                    restartToken);
            }
        }
    }
}
