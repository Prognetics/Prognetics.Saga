using Prognetics.Saga.Common.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaOrchestrator : IDisposable
{
    SagaModel Model { get; }

    void Subscribe(ISagaSubscriber sagaSubscriber);

    Task Push(string queueName, InputMessage inputMessage);
}