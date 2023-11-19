namespace Prognetics.Saga.Orchestrator.Contract;

public interface IStartableSagaOrchestrator : ISagaOrchestrator
{
    public bool IsStarted { get; }

    void Start(ISagaSubscriber sagaSubscriber, Action onUpdate);
}