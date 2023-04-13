namespace Prognetics.Saga.Orchestrator;

public interface ISagaOrchestrator
{
    Task Push(InputMessage inputMessage);
    void Subscribe(ISagaSubscriber sagaSubscriber);
}