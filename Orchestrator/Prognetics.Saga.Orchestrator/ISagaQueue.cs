namespace Prognetics.Saga.Orchestrator;

public interface ISagaQueue
{
    Task Push(InputMessage inputMessage);
    void Subscribe(ISagaSubscriber sagaSubscriber);
}