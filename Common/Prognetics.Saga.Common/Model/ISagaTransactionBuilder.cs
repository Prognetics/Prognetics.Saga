namespace Prognetics.Saga.Common.Model;

public interface ISagaTransactionBuilder
{
    ISagaTransactionBuilder AddStep(string from, string to);
}