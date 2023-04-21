namespace Prognetics.Saga.Orchestrator.SagaModel;

public interface ISagaModelBuilder
{
    ISagaModelBuilder From(SagaModel sagaModel);
    ISagaModelBuilder AddTransaction(Action<ISagaTransactionBuilder> builderAction);
}

public interface ISagaTransactionBuilder
{
    ISagaTransactionBuilder AddStep(string from, string to);
}