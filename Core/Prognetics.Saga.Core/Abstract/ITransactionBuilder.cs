namespace Prognetics.Saga.Core.Abstract;

public interface ITransactionBuilder
{
    ITransactionBuilder AddStep(string eventName, string completionEventName, string compensationEventName);
}