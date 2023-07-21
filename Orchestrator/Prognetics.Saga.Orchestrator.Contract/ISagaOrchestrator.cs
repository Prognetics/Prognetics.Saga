using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaOrchestrator : IDisposable
{
    void Subscribe(ISagaSubscriber sagaSubscriber);

    Task Push(string queueName, InputMessage inputMessage);
}