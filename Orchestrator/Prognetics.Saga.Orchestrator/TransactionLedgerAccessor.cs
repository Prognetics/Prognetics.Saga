using Microsoft.Extensions.Logging;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator;

public class TransactionLedgerAccessor : IInitializableTransactionLedgerAccessor
{
    private TransactionsLedger? _sagaModel;
    private readonly ITransactionLedgerSource[] _sources;
    private readonly TransactionsLedger[] _transactionLedgers;
    private readonly ILogger<TransactionLedgerAccessor> _logger;
    private readonly object _lock = new ();

    public TransactionLedgerAccessor(
        IEnumerable<ITransactionLedgerSource> sources,
        ILogger<TransactionLedgerAccessor> logger)
    {
        _sources = sources.ToArray();
        _transactionLedgers = new TransactionsLedger[_sources.Length];
        _logger = logger;
    }

    public async Task Initialize(
        Action onUpdate,
        CancellationToken cancellation = default)
    {
        try
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                var sourceTransactionLedger = await source.GetTransactionLedger(cancellation);
                _transactionLedgers[i] = sourceTransactionLedger;
                await source.TrackTransactionLedger(
                    (tl) =>
                    {
                        lock (_lock)
                        {
                            _transactionLedgers[i] = tl;
                            UpdateTransactionLedger();
                            onUpdate();
                        }
                    },
                    (exception) =>
                    {
                        _logger.LogError(
                            exception,
                            "Error during an update of the transaction ledger");
                    },
                    cancellation);
            }
            UpdateTransactionLedger();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error during an initialization of the transaction ledger accessor.");
        }
    }

    private void UpdateTransactionLedger()
    {
        _sagaModel = _transactionLedgers
            .Where(x => x != null)
            .Aggregate(
                new TransactionLedgerBuilder(),
                (builder, model) => builder.FromLedger(model))
            .Build();
    }

    public TransactionsLedger TransactionsLedger
        => _sagaModel
        ?? throw new InvalidOperationException("Transaction ledger has not been initialized");
}