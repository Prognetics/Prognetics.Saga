namespace Prognetics.Saga.Orchestrator;
public interface ISagaSubscriber
{
    Task Subscribe(OutputMessage message);
}
