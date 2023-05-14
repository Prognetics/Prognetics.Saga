using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;

internal class DelegateSagaModelSource : IModelSource
{
    private readonly Action<IModelBuilder> _configure;

    public DelegateSagaModelSource(Action<IModelBuilder> configure)
    {
        _configure = configure;
    }

    public Task<TransactionsLedger> GetModel(CancellationToken cancellation = default)
    {
        var builder = new ModelBuilder();
        _configure(builder);
        return Task.FromResult(builder.Build());
    }
}