using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator;

public class TransactionLedgerProvider : ITransactionLedgerProvider
{
    private readonly Lazy<Task<SagaModel>> _sagaModel;
    public TransactionLedgerProvider(IEnumerable<ISagaModelSource> sources)
    {
        _sagaModel = new Lazy<Task<SagaModel>>(async () => 
            (await Task.WhenAll(
                sources.Select(s => s.GetSagaModel())))
            .Aggregate(
                new SagaModelBuilder(),
                (builder, model) => builder.From(model))
            .Build());
    }

    public Task<SagaModel> Get()
        => _sagaModel.Value;
}