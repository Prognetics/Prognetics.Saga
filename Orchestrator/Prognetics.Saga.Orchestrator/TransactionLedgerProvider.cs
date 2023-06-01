using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator;

public class TransactionLedgerProvider : ITransactionLedgerProvider
{
    private readonly Lazy<Task<TransactionsLedger>> _sagaModel;
    public TransactionLedgerProvider(IEnumerable<IModelSource> sources)
    {
        _sagaModel = new Lazy<Task<TransactionsLedger>>(async () => 
            (await Task.WhenAll(
                sources.Select(s => s.GetModel())))
            .Aggregate(
                new ModelBuilder(),
                (builder, model) => builder.From(model))
            .Build());
    }

    public Task<TransactionsLedger> Get()
        => _sagaModel.Value;
}