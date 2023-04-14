namespace Prognetics.Saga.Orchestrator;
public interface ISagaSubscriber
{
    Task OnMessage(string queueName, OutputMessage message);
}
