namespace Prognetics.Saga.Orchestrator.SagaModel;

public interface ISagaModelProvider
{
    ValueTask<SagaModel> GetModel();
}