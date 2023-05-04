using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ISagaModelBuilder
{
    ISagaModelBuilder From(SagaModel sagaModel);
    ISagaModelBuilder AddTransaction(Action<ISagaTransactionBuilder> builderAction);
}
