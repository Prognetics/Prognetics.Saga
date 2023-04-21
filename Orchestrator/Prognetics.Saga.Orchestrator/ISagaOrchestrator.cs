using Prognetics.Saga.Orchestrator.DTO;

namespace Prognetics.Saga.Orchestrator;

public interface ISagaOrchestrator :
    ISagaOutput,
    ISagaInput,
    IDisposable
{ }

public interface ISagaOutput
{
    void Subscribe(ISagaSubscriber sagaSubscriber);
}

public interface ISagaInput
{
    Task Push(string queueName, InputMessage inputMessage);
}