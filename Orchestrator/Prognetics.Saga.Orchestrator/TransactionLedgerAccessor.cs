using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator;

public class TransactionLedgerAccessor : ITransactionLedgerAccessor
{
    private TransactionsLedger? _sagaModel;
    private readonly IEnumerable<IModelSource> _sources;

    public TransactionLedgerAccessor(IEnumerable<IModelSource> sources)
    {
        _sources = sources;
    }

    public async Task Initialize(CancellationToken cancellation = default)
    {
        _sagaModel = (await Task.WhenAll(
            _sources.Select(s => s.GetModel(cancellation))))
            .Aggregate(
                new ModelBuilder(),
                (builder, model) => builder.From(model))
            .Build();
    }

    public TransactionsLedger TransactionsLedger
        => _sagaModel
        ?? throw new InvalidOperationException("Transaction ledger has not been initialized");
}