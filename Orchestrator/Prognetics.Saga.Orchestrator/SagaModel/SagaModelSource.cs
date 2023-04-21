namespace Prognetics.Saga.Orchestrator.SagaModel;

public interface ISagaModelSource
{
    Task<SagaModel> GetSagaModel(CancellationToken cancellation = default);
}

public class DelegateSagaModelSource : ISagaModelSource
{
    private readonly Func<SagaModel> _factory;

    public DelegateSagaModelSource(Func<SagaModel> factory)
    {
        _factory = factory;
    }

    public Task<SagaModel> GetSagaModel(CancellationToken cancellation = default)
        => Task.FromResult(_factory());
}