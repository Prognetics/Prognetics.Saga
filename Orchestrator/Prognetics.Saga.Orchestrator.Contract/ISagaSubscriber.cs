using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;
public interface ISagaSubscriber
{
    Task OnMessage(string queueName, OutputMessage message);
}
