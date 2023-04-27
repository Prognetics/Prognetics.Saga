namespace Prognetics.Saga.Orchestrator.Model;

public interface ISagaModelSource
{
    Task<SagaModel> GetSagaModel(CancellationToken cancellation = default);
}
