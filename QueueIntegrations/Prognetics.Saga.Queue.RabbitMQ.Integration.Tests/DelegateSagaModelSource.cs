using Prognetics.Saga.Orchestrator.Model;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;

internal class DelegateSagaModelSource : ISagaModelSource
{
    private readonly Action<ISagaModelBuilder> _configure;

    public DelegateSagaModelSource(Action<ISagaModelBuilder> configure)
    {
        _configure = configure;
    }

    public Task<SagaModel> GetSagaModel(CancellationToken cancellation = default)
    {
        var builder = new SagaModelBuilder();
        _configure(builder);
        return Task.FromResult(builder.Build());
    }
}