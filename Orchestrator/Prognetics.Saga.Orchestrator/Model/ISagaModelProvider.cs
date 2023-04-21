namespace Prognetics.Saga.Orchestrator.Model;

public interface ISagaModelProvider
{
    ValueTask<SagaModel> GetModel();
}