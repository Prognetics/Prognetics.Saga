namespace Prognetics.Saga.Orchestrator.Model;

public interface ISagaTransactionBuilder
{
    ISagaTransactionBuilder AddStep(string from, string to);
}