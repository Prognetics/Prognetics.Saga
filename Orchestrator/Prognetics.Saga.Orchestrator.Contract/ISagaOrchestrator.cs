using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaOrchestrator
{
    Task Push(string queueName, InputMessage inputMessage);
}