using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;

public class DelegateSagaModelSource : IModelSource
{
    private readonly Func<TransactionsLedger> _factory;

    public DelegateSagaModelSource(Func<TransactionsLedger> factory)
    {
        _factory = factory;
    }

    public DelegateSagaModelSource(Action<ModelBuilder> configure)
    {
        _factory = () =>
        {
            var builder = new ModelBuilder();
            configure(builder);
            return builder.Build();
        };
    }

    public Task<TransactionsLedger> GetModel(CancellationToken cancellation = default)
        => Task.FromResult(_factory());
}