using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;

public class DelegateSagaModelSource : ISagaModelSource
{
    private readonly Func<SagaModel> _factory;

    public DelegateSagaModelSource(Func<SagaModel> factory)
    {
        _factory = factory;
    }

    public DelegateSagaModelSource(Action<SagaModelBuilder> configure)
    {
        _factory = () =>
        {
            var builder = new SagaModelBuilder();
            configure(builder);
            return builder.Build();
        };
    }

    public Task<SagaModel> GetSagaModel(CancellationToken cancellation = default)
        => Task.FromResult(_factory());
}