using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ISagaModelSource
{
    Task<SagaModel> GetSagaModel(CancellationToken cancellation = default);
}
