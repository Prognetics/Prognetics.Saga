using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;

public class DelegateSagaModelSource : ITransactionLedgerSource
{
    private readonly Func<TransactionsLedger> _factory;

    public DelegateSagaModelSource(Func<TransactionsLedger> factory)
    {
        _factory = factory;
    }

    public DelegateSagaModelSource(Action<TransactionLedgerBuilder> configure)
    {
        _factory = () =>
        {
            var builder = new TransactionLedgerBuilder();
            configure(builder);
            return builder.Build();
        };
    }

    public Task<TransactionsLedger> GetTransactionLedger(CancellationToken cancellation = default)
        => Task.FromResult(_factory());
}