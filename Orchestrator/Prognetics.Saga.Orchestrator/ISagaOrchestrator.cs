namespace Prognetics.Saga.Orchestrator;

public interface ISagaOrchestrator
{
    Task Push(string queueName, InputMessage inputMessage);
    void Subscribe(ISagaSubscriber sagaSubscriber);
}