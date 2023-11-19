namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionBuilder
{
    ITransactionBuilder AddStep(
        string completionEventName,
        string nextEventName,
        string compensationEventName);
}