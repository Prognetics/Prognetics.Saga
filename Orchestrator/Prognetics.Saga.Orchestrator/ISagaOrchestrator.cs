using Prognetics.Saga.Orchestrator.DTO;

namespace Prognetics.Saga.Orchestrator;

public interface ISagaOrchestrator
{
    Task Push(string queueName, InputMessage inputMessage);
    void Subscribe(ISagaSubscriber sagaSubscriber);
}