namespace Prognetics.Saga.Common.Model;

public interface ISagaModelSource
{
    Task<SagaModel> GetSagaModel(CancellationToken cancellation = default);
}
