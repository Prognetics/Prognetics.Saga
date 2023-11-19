using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator;

public class SagaHost : ISagaHost
{
    private readonly IStartableSagaOrchestrator _orchestrator;
    private readonly IInitializableTransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly ISagaClient _client;

    public SagaHost(
        IInitializableTransactionLedgerAccessor transactionLedgerAccessor,
        ISagaClient client,
        IStartableSagaOrchestrator orchestrator)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _client = client;
        _orchestrator = orchestrator;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            while (cancellationTokenSource.IsCancellationRequested)
            {
                if (_orchestrator.IsStarted)
                {
                    await Task.Delay(1000);
                    continue;
                }

                await _transactionLedgerAccessor.Initialize(cancellationToken);
                await _client.Initialize();
                var subscriber = await _client.GetSubscriber();
                _orchestrator.Start(subscriber, cancellationTokenSource.Cancel);
                await _client.Consume(_orchestrator);
            }
        }
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
