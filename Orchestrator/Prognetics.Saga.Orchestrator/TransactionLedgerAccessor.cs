using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator;

public class TransactionLedgerAccessor : IInitializableTransactionLedgerAccessor
{
    private TransactionsLedger? _sagaModel;
    private readonly IEnumerable<ITransactionLedgerSource> _sources;

    public TransactionLedgerAccessor(IEnumerable<ITransactionLedgerSource> sources)
    {
        _sources = sources;
    }

    public async Task Initialize(CancellationToken cancellation = default)
    {
        _sagaModel = (await Task.WhenAll(
            _sources.Select(s => s.GetTransactionLedger(cancellation))))
            .Aggregate(
                new TransactionLedgerBuilder(),
                (builder, model) => builder.FromLedger(model))
            .Build();
    }

    public TransactionsLedger TransactionsLedger
        => _sagaModel
        ?? throw new InvalidOperationException("Transaction ledger has not been initialized");
}