namespace Prognetics.Saga.Orchestrator.Model;

public interface ISagaModelBuilder
{
    ISagaModelBuilder From(SagaModel sagaModel);
    ISagaModelBuilder AddTransaction(Action<ISagaTransactionBuilder> builderAction);
}
