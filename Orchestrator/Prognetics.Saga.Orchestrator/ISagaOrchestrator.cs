using Prognetics.Saga.Orchestrator.DTO;
using Prognetics.Saga.Orchestrator.Model;

namespace Prognetics.Saga.Orchestrator;

public interface ISagaOrchestrator : IDisposable
{
    SagaModel Model { get; }

    void Subscribe(ISagaSubscriber sagaSubscriber);

    Task Push(string queueName, InputMessage inputMessage);
}