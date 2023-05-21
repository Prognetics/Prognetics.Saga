using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaOrchestrator
{
    Task Push(string queueName, InputMessage inputMessage);
}

public interface IStartableSagaOrchestrator : ISagaOrchestrator
{
    public bool IsStarted { get; }

    void Start(ISagaSubscriber sagaSubscriber);
}