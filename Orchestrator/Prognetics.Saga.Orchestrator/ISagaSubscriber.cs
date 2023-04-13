namespace Prognetics.Saga.Orchestrator;
public interface ISagaSubscriber
{
    Task OnMessage(OutputMessage message);
}
