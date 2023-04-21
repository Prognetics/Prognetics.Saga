namespace Prognetics.Saga.Orchestrator;

public interface ISagaClient : IDisposable
{
    ISagaSubscriber Subscriber { get; }

    void UseInput(ISagaInput sagaEntryPoint);

    Task Start(CancellationToken cancellationToken = default);
}