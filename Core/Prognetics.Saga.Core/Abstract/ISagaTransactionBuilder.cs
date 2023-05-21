namespace Prognetics.Saga.Core.Abstract;

public interface ISagaTransactionBuilder
{
    ISagaTransactionBuilder AddStep(
        string from,
        string to,
        string compensation);
}